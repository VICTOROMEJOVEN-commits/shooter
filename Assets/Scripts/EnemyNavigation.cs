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
    float nextAttack;
    float nextPath;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponentInChildren<Animator>();
        if (enemyAnimator) enemyAnimator.applyRootMotion = false;
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) target = player.transform;
        if (!GetComponent<ReciveDamage>()) gameObject.AddComponent<ReciveDamage>();
        if (agent) agent.stoppingDistance = attackRange * 0.75f;
        MatchGame.Instance?.Register(this);
    }

    void Update()
    {
        if (isDead || !target || (MatchGame.Instance && MatchGame.Instance.Finished)) return;
        if (agent && agent.enabled && agent.isOnNavMesh)
        {
            if (Time.time >= nextPath)
            {
                agent.SetDestination(target.position);
                nextPath = Time.time + 0.2f;
            }
            if (enemyAnimator) enemyAnimator.SetBool("IsMoving", agent.velocity.sqrMagnitude > 0.01f);
        }
        if (Time.time >= nextAttack && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            // Walls block melee attacks as well as projectiles.
            Vector3 origin = transform.position + Vector3.up * 0.4f;
            Vector3 destination = target.position + Vector3.up * 0.8f;
            if (!Physics.Linecast(origin, destination, out var hit) || hit.transform == target || hit.transform.IsChildOf(target))
            {
                target.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
                nextAttack = Time.time + attackInterval;
            }
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
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
