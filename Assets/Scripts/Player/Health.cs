using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Attach to any GameObject that can take damage (enemies, destructibles, etc).
/// Bullet.cs calls TakeDamage() on hit.
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Events")]
    public UnityEvent OnDeath;          // Hook up in Inspector — e.g. play death animation
    public UnityEvent OnDamaged;        // Optional — e.g. flash red on hit

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

        // Default: destroy the object. 
        // Remove this line and use the OnDeath UnityEvent instead
        // if you want to play a death animation before destroying.
        Destroy(gameObject);
    }
}
