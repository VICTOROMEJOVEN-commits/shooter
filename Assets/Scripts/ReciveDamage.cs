using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReciveDamage : MonoBehaviour
{
    [Header("Proyectil")]
    public bool esProyectil;
    public int danoProyectil = 25;
    bool consumido;
    [Header("Vida del enemigo")]
    public TextMeshProUGUI HealthText;
    public Image HealthBar;
    public int Health = 100;
    public EnemyNavigation enemyNavigation;
    int maximumHealth;
    float flashUntil;
    Renderer[] visuals;
    MaterialPropertyBlock block;
    Color[] originalColors;
    Transform animatedModel;
    Quaternion originalRotation;

    void Awake()
    {
        if (esProyectil) return;
        maximumHealth = Mathf.Max(1, Health);
        enemyNavigation = GetComponent<EnemyNavigation>() ?? GetComponentInChildren<EnemyNavigation>();
        visuals = GetComponentsInChildren<Renderer>();
        block = new MaterialPropertyBlock();
        originalColors = new Color[visuals.Length];
        for (int i = 0; i < visuals.Length; i++)
        {
            var material = visuals[i].sharedMaterial;
            originalColors[i] = material && material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : Color.white;
        }
        var animator = GetComponentInChildren<Animator>();
        if (animator && animator.transform != transform)
        {
            animatedModel = animator.transform;
            originalRotation = animatedModel.localRotation;
        }
        RefreshUI();
    }

    public void ConfigurarVida(int cantidad) { Health = Mathf.Max(1, cantidad); maximumHealth = Health; RefreshUI(); }
    void OnCollisionEnter(Collision collision)
    {
        // El mismo script original gestiona la recepción y la aplicación del daño.
        if (esProyectil)
        {
            if (consumido) return;
            consumido = true;
            var enemigo = collision.collider.GetComponentInParent<ReciveDamage>();
            if (enemigo && !enemigo.esProyectil) enemigo.TakeDamage(danoProyectil);
            else
            {
                MatchGame.Instance?.ImpactarTabla(collision.transform);
                if (collision.contactCount > 0) MatchGame.Instance?.Impacto(collision.GetContact(0).point, collision.GetContact(0).normal);
            }
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        var projectile = collision.rigidbody ? collision.rigidbody.gameObject : collision.gameObject;
        var datos = projectile.GetComponent<ReciveDamage>();
        if (datos && datos.esProyectil) return; // El proyectil ya procesa el impacto.
        if (!projectile.CompareTag("Projectile") || !projectile.activeSelf) return;
        projectile.SetActive(false);
        Destroy(projectile);
        TakeDamage(25); // Compatibilidad con el proyectil del ejercicio original.
    }

    public void TakeDamage(int damage)
    {
        if (esProyectil || damage <= 0 || Health <= 0 || (enemyNavigation && enemyNavigation.isDead) || (MatchGame.Instance && MatchGame.Instance.Finished)) return;
        Health = Mathf.Max(0, Health - damage);
        MatchGame.Instance?.SumarPuntos(10);
        MatchGame.Instance?.Dano(transform.position + Vector3.up);
        RefreshUI();
        flashUntil = Time.time + 0.12f;
        MatchGame.Instance?.ShowHit();
        if (Health > 0) { CombatSfx.EnemyHit(transform.position + Vector3.up * 0.5f); if (enemyNavigation && enemyNavigation.enemyAnimator) enemyNavigation.enemyAnimator.SetTrigger("Impacto"); }
        if (Health == 0)
        {
            if (enemyNavigation) enemyNavigation.Die();
            else Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        if (esProyectil) return;
        bool flashing = Time.time < flashUntil;
        for (int i = 0; i < visuals.Length; i++)
        {
            var visual = visuals[i];
            if (!visual || !visual.enabled) continue;
            visual.GetPropertyBlock(block);
            block.SetColor("_BaseColor", flashing ? Color.Lerp(originalColors[i], Color.red, 0.7f) : originalColors[i]);
            visual.SetPropertyBlock(block);
        }
        // Reacción breve al impacto sin cambiar el modelo ni sus animaciones.
        if (animatedModel)
        {
            float angle = flashing && Health > 0 ? Mathf.Sin((flashUntil - Time.time) / 0.12f * Mathf.PI) * 7f : 0;
            animatedModel.localRotation = originalRotation * Quaternion.Euler(0, 0, angle);
        }
    }
    void RefreshUI()
    {
        if (HealthText) HealthText.text = Health.ToString();
        if (HealthBar) HealthBar.fillAmount = (float)Health / maximumHealth;
    }
}
