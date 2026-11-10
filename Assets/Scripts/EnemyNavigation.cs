using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
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
        float duration = 0.1f;
        if (enemyAnimator && enemyAnimator.runtimeAnimatorController)
        {
            enemyAnimator.SetBool("IsMoving", false);
            enemyAnimator.SetTrigger("Die");
            foreach (var clip in enemyAnimator.runtimeAnimatorController.animationClips)
                if (clip.name.ToLowerInvariant().Contains("dying")) duration = Mathf.Max(duration, clip.length + 0.3f);
        }
        // Victory waits for the last death animation to finish.
        // Once the transition starts, retain ground contact even when the
        // imported humanoid root has a different height from the idle clip.
        yield return new WaitForSeconds(0.3f);
        var baked = new Mesh();
        float elapsed = 0.3f;
        while (elapsed < duration)
        {
            GroundCorpse(baked);
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        GroundCorpse(baked);
        Destroy(baked);
        yield return new WaitForSeconds(0.7f);
        MatchGame.Instance?.EnemyDefeated(this);
        Destroy(gameObject);
    }
    void GroundCorpse(Mesh baked)
    {
        Vector3 lowest = Vector3.up * float.PositiveInfinity;
        foreach (var skin in GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            skin.BakeMesh(baked);
            foreach (var vertex in baked.vertices)
            {
                var world = skin.transform.TransformPoint(vertex);
                if (world.y < lowest.y) lowest = world;
            }
        }
        if (float.IsInfinity(lowest.y)) return;
        var hits = Physics.RaycastAll(lowest + Vector3.up * 1.5f, Vector3.down, 3f, ~0, QueryTriggerInteraction.Ignore);
        float floor = float.NegativeInfinity;
        foreach (var hit in hits)
        {
            if (hit.normal.y < 0.7f || hit.transform.IsChildOf(transform) || hit.collider.attachedRigidbody) continue;
            if (hit.point.y > floor) floor = hit.point.y;
        }
        if (!float.IsInfinity(floor)) transform.position += Vector3.up * (floor + 0.01f - lowest.y);
    }
}
