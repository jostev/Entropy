using UnityEngine;
using UnityEngine.Events;
using Entropy.Perks;

public class Health : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Events")]
    public UnityEvent OnDeath;
    public UnityEvent OnDamaged;

    [Header("Loot")]
    public GameObject ammoPickupPrefab;
    public float baseAmmoDropChance = 0.5f;

    [Header("Damage Source")]
    public GameObject lastDamageSource;

    private bool isDead;
    public bool IsInvulnerable { get; set; }

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        if (IsInvulnerable) return;

        if (CompareTag("Player") && PerksManager.Instance != null)
        {
            foreach (var perk in PerksManager.Instance.ActivePerks)
            {
                if (perk is ElasticSkinPerk elastic)
                {
                    amount *= elastic.DamageReductionMultiplier;
                    break;
                }
            }

            foreach (var perk in PerksManager.Instance.ActivePerks)
            {
                if (perk is TemporalAnchorPerk anchor && anchor.IsAnchored)
                {
                    return;
                }
            }
        }

        GameEvents.PlayerDamaged(amount, lastDamageSource);

        currentHealth -= amount;
        OnDamaged?.Invoke();

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        if (CompareTag("Player"))
            return;

        if (CompareTag("Enemy"))
            TryDropAmmo();

        var ec = GetComponent<EnemyController>();
        if (ec != null)
        {
            var rb = GetComponent<Rigidbody>();
            Vector3 vel = rb != null ? rb.linearVelocity : Vector3.zero;
            GameEvents.EnemyKilled(ec, transform.position, vel);
        }

        Destroy(gameObject);
    }

    public void Revive()
    {
        isDead = false;
        currentHealth = maxHealth;
    }

    private void TryDropAmmo()
    {
        if (ammoPickupPrefab == null) return;

        float chance = baseAmmoDropChance;

        if (PerksManager.Instance != null)
        {
            foreach (var perk in PerksManager.Instance.ActivePerks)
            {
                if (perk is ScavengerProtocolPerk scav)
                    chance += scav.DropChanceBonus;
            }
        }

        chance = Mathf.Clamp01(chance);

        if (Random.value < chance)
            Instantiate(ammoPickupPrefab, transform.position, Quaternion.identity);
    }
}
