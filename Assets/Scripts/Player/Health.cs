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

    private bool isDead;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

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
