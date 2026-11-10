using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReciveDamage : MonoBehaviour
{
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

    void OnCollisionEnter(Collision collision)
    {
        var projectile = collision.rigidbody ? collision.rigidbody.gameObject : collision.gameObject;
        if (!projectile.CompareTag("Projectile")) return;
        // Disable immediately so one projectile cannot damage two enemies in one physics step.
        if (!projectile.activeSelf) return;
        projectile.SetActive(false);
        Destroy(projectile);
        TakeDamage(25);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || Health <= 0 || (enemyNavigation && enemyNavigation.isDead) || (MatchGame.Instance && MatchGame.Instance.Finished)) return;
        Health = Mathf.Max(0, Health - damage);
        RefreshUI();
        flashUntil = Time.time + 0.12f;
        MatchGame.Instance?.ShowHit();
        if (Health == 0)
        {
            if (enemyNavigation) enemyNavigation.Die();
            else Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        bool flashing = Time.time < flashUntil;
        for (int i = 0; i < visuals.Length; i++)
        {
            var visual = visuals[i];
            if (!visual || !visual.enabled) continue;
            visual.GetPropertyBlock(block);
            block.SetColor("_BaseColor", flashing ? Color.Lerp(originalColors[i], Color.red, 0.7f) : originalColors[i]);
            visual.SetPropertyBlock(block);
        }
        // A short procedural recoil works with the existing Mixamo clips.
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


