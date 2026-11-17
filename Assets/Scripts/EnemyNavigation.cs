using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
    public const float DeathSpeed = 1.65f;
    public const float DeathStart = 0.38f;
    public const float DeathBlend = 0.07f;
    public NavMeshAgent agent;
    public Transform target;
    public Animator enemyAnimator;
    public bool isDead;
    public float attackRange = 1.6f;
    public int attackDamage = 20;
    public float attackInterval = 1f;
    [System.NonSerialized] public DatosVentana ventanaEntrada;
    public bool Entrando;
    float nextAttack;
    float nextPath;
    bool cruzandoEscalera;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent) agent.autoTraverseOffMeshLink = false;
        enemyAnimator = GetComponentInChildren<Animator>();
        if (enemyAnimator) enemyAnimator.applyRootMotion = false;
    }

    IEnumerator Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) target = player.transform;
        if (!GetComponent<ReciveDamage>()) gameObject.AddComponent<ReciveDamage>();
        if (agent) agent.stoppingDistance = attackRange * 0.75f;
        MatchGame.Instance?.Register(this);
        if (ventanaEntrada != null) { Entrando = true; yield return StartCoroutine(ventanaEntrada.Entrar(this)); }
        else if (agent && !isDead) agent.enabled = true;
    }

    void Update()
    {
        if (isDead || Entrando || cruzandoEscalera || !target || (MatchGame.Instance && MatchGame.Instance.Finished)) return;
        if (agent && agent.enabled && agent.isOnNavMesh)
        {
            if (agent.isOnOffMeshLink) { StartCoroutine(SubirEscalon()); return; }
            if (Time.time >= nextPath)
            {
                agent.SetDestination(target.position);
                nextPath = Time.time + 0.2f;
            }
            if (enemyAnimator) enemyAnimator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.01f);
        }
        if (Time.time >= nextAttack && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            // La línea de visión evita golpear al jugador a través de las paredes.
            Vector3 origin = transform.position + Vector3.up * 0.4f;
            Vector3 destination = target.position + Vector3.up * 0.8f;
            if (!Physics.Linecast(origin, destination, out var hit) || hit.transform == target || hit.transform.IsChildOf(target))
            {
                if (enemyAnimator) enemyAnimator.SetTrigger("Golpear");
                target.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
                nextAttack = Time.time + attackInterval;
            }
        }
    }

    // Los enlaces conectan los escalones altos que el bake deja separados.
    IEnumerator SubirEscalon()
    {
        cruzandoEscalera = true;
        Vector3 inicio = transform.position;
        Vector3 fin = agent.currentOffMeshLinkData.endPos + Vector3.up * agent.baseOffset;
        agent.updatePosition = false;
        if (enemyAnimator) enemyAnimator.SetBool("SaltarVentana", true);
        float avance = 0;
        while (avance < 1 && !isDead)
        {
            avance += Time.deltaTime / 0.8f;
            transform.position = Vector3.Lerp(inicio, fin, Mathf.Clamp01(avance)) + Vector3.up * Mathf.Sin(Mathf.Clamp01(avance) * Mathf.PI) * 0.2f;
            yield return null;
        }
        if (enemyAnimator) enemyAnimator.SetBool("SaltarVentana", false);
        if (!isDead && agent && agent.enabled) { agent.CompleteOffMeshLink(); agent.updatePosition = true; }
        cruzandoEscalera = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        if (enemyAnimator) enemyAnimator.SetBool("Muerto", true);
        CombatSfx.EnemyDeath(transform.position + Vector3.up * 0.5f);
        if (agent && agent.enabled)
        {
            if (agent.isOnNavMesh) { agent.isStopped = true; agent.ResetPath(); }
            agent.enabled = false;
        }
        foreach (var body in GetComponentsInChildren<Rigidbody>()) { body.isKinematic = true; }
        foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = false;
        foreach (var canvas in GetComponentsInChildren<Canvas>()) canvas.enabled = false;
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        float duration = 0.15f;
        if (enemyAnimator && enemyAnimator.runtimeAnimatorController)
        {
            enemyAnimator.SetBool("IsMoving", false);
            enemyAnimator.SetTrigger("Die");
            foreach (var clip in enemyAnimator.runtimeAnimatorController.animationClips)
                if (clip.name.ToLowerInvariant().Contains("dying")) duration = Mathf.Max(duration, clip.length * (1 - DeathStart) / DeathSpeed + DeathBlend);
        }
        var skins = GetComponentsInChildren<SkinnedMeshRenderer>();
        var baked = new Mesh();
        var vertices = new List<Vector3>(10000);
        float elapsed = 0, nextGroundSample = DeathBlend, targetY = transform.position.y;
        while (elapsed < duration)
        {
            if (elapsed >= nextGroundSample)
            {
                targetY = CorpseGroundHeight(skins, baked, vertices);
                nextGroundSample = elapsed + 0.05f;
            }
            var position = transform.position;
            position.y = Mathf.Lerp(position.y, targetY, 1 - Mathf.Exp(-40 * Time.deltaTime));
            transform.position = position;
            yield return null;
            elapsed += Time.deltaTime;
        }
        var finalPosition = transform.position;
        finalPosition.y = CorpseGroundHeight(skins, baked, vertices);
        transform.position = finalPosition;
        Destroy(baked);
        yield return new WaitForSeconds(0.15f);
        MatchGame.Instance?.EnemyDefeated(this);
        Destroy(gameObject);
    }
    void OnDestroy() { MatchGame.Instance?.Retirar(this); }
    float CorpseGroundHeight(SkinnedMeshRenderer[] skins, Mesh baked, List<Vector3> vertices)
    {
        Vector3 lowest = new Vector3(0, float.PositiveInfinity, 0);
        foreach (var skin in skins)
        {
            skin.BakeMesh(baked); baked.GetVertices(vertices);
            var matrix = skin.localToWorldMatrix;
            foreach (var vertex in vertices)
            {
                var world = matrix.MultiplyPoint3x4(vertex);
                if (world.y < lowest.y) lowest = world;
            }
        }
        if (float.IsInfinity(lowest.y)) return transform.position.y;
        var hits = Physics.RaycastAll(lowest + Vector3.up * 1.5f, Vector3.down, 3f, ~0, QueryTriggerInteraction.Ignore);
        float floor = float.NegativeInfinity;
        foreach (var hit in hits)
        {
            if (hit.normal.y < 0.7f || hit.transform.IsChildOf(transform) || hit.collider.attachedRigidbody) continue;
            if (hit.point.y > floor) floor = hit.point.y;
        }
        return float.IsInfinity(floor) ? transform.position.y : transform.position.y + floor + 0.01f - lowest.y;
    }
}

// Las ventanas son datos del escenario; comparten la lógica de entrada del enemigo.
[System.Serializable]
public class DatosVentana
{
    public Transform raiz;
    public Transform aparicion;
    public Transform entrada;
    public Transform salida;
    public GameObject[] tablas;
    public BoxCollider limiteJugador;
    public AudioSource sonido;
    public AudioClip romper;
    public AudioClip reparar;
    public bool PuedeRecibir => ocupante == null;
    EnemyNavigation ocupante;
    float proximaReparacion;
    int reparacionesPremiadas;
    public int TablasRestantes
    {
        get { int cantidad = 0; foreach (var tabla in tablas) if (tabla && tabla.activeSelf) cantidad++; return cantidad; }
    }
    public IEnumerator Entrar(EnemyNavigation zombi)
    {
        ocupante = zombi;
        if (zombi.agent) zombi.agent.enabled = false;
        Vector3 inicio = zombi.transform.position;
        float recorrido = 0;
        while (recorrido < 1 && !zombi.isDead)
        {
            recorrido += Time.deltaTime / 1.5f;
            zombi.transform.position = Vector3.Lerp(inicio, entrada.position, recorrido);
            zombi.transform.rotation = entrada.rotation;
            yield return null;
        }
        if (zombi.enemyAnimator) zombi.enemyAnimator.SetBool("IsMoving", false);
        while (TablasRestantes > 0 && !zombi.isDead)
        {
            if (zombi.enemyAnimator) zombi.enemyAnimator.SetTrigger("Golpear");
            yield return new WaitForSeconds(0.65f);
            if (zombi.isDead) break;
            RomperTabla();
            yield return new WaitForSeconds(0.65f);
        }
        if (!zombi.isDead)
        {
            if (zombi.enemyAnimator) zombi.enemyAnimator.SetBool("SaltarVentana", true);
            recorrido = 0;
            while (recorrido < 1 && !zombi.isDead)
            {
                recorrido += Time.deltaTime / 1.1f;
                zombi.transform.position = Vector3.Lerp(entrada.position, salida.position, Mathf.SmoothStep(0, 1, recorrido)) + Vector3.up * Mathf.Sin(Mathf.Clamp01(recorrido) * Mathf.PI) * 0.45f;
                yield return null;
            }
            if (zombi.enemyAnimator) zombi.enemyAnimator.SetBool("SaltarVentana", false);
            if (!zombi.isDead && zombi.agent && NavMesh.SamplePosition(salida.position, out var punto, 2, zombi.agent.areaMask))
            {
                zombi.transform.position = punto.position;
                zombi.agent.enabled = true;
                zombi.agent.Warp(punto.position);
            }
        }
        zombi.Entrando = false; ocupante = null;
    }
    public void RomperTabla()
    {
        for (int i = tablas.Length - 1; i >= 0; i--)
        {
            if (!tablas[i] || !tablas[i].activeSelf) continue;
            tablas[i].SetActive(false);
            if (sonido && romper) sonido.PlayOneShot(romper);
            MatchGame.Instance?.Impacto(raiz.position + Vector3.up, raiz.forward);
            return;
        }
    }
    public bool RepararTabla()
    {
        if (Time.time < proximaReparacion || (ocupante && ocupante.Entrando && TablasRestantes == 0)) return false;
        foreach (var tabla in tablas)
        {
            if (!tabla || tabla.activeSelf) continue;
            tabla.SetActive(true); proximaReparacion = Time.time + 0.8f;
            if (reparacionesPremiadas < 30) { MatchGame.Instance?.SumarPuntos(10); reparacionesPremiadas++; }
            if (sonido && reparar) sonido.PlayOneShot(reparar);
            return true;
        }
        return false;
    }
}
