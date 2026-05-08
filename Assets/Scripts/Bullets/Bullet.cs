using UnityEngine;

/// <summary>
/// Attach this to your bullet prefab.
/// The bullet uses its own Rigidbody so Unity gravity gives you bullet drop
/// automatically — just tune BulletDropGravityScale in the inspector.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 25f;

    /// <summary>
    /// Multiplier on Physics.gravity applied to this bullet.
    /// 1 = full gravity (strong drop), 0.2 = subtle drop, 0 = no drop (laser).
    /// </summary>
    public float BulletDropGravityScale = 0.4f;

    /// <summary>How many seconds before the bullet self-destructs if it hits nothing.</summary>
    public float lifetime = 4f;

    [Header("FX")]
    public GameObject impactFX;       // Optional particle/decal prefab spawned on hit
    public GameObject trailFX;        // Optional trail renderer child (already on prefab)

    private Rigidbody rb;

    // -----------------------------------------------------------------------
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // We control gravity manually so we can scale it independently.
        rb.useGravity = false;

        // Make the collider a trigger so we get OnTriggerEnter callbacks
        // without blocking the bullet physically mid-flight.
        foreach (var col in GetComponents<Collider>())
            col.isTrigger = true;

        Destroy(gameObject, lifetime);
    }

    // -----------------------------------------------------------------------
    void FixedUpdate()
    {
        // Apply scaled gravity manually every physics tick.
        rb.AddForce(Physics.gravity * BulletDropGravityScale, ForceMode.Acceleration);

        // Rotate bullet to always face its velocity vector — looks natural in flight.
        if (rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
    }

    // -----------------------------------------------------------------------
    /// <summary>
    /// Called by PistolShoot to fire the bullet from a spawn point.
    /// </summary>
    public void Launch(Vector3 direction, float speed)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    // -----------------------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        // Ignore the shooter and detection colliders
        if (!(other.CompareTag("Ground") || other.CompareTag("Enemy"))) {
            return;
        }
        // Try to deal damage to whatever was hit.
        var health = other.GetComponent<Health>();
        if (health != null)
            health.TakeDamage(damage);

        // Spawn impact FX at hit point.
        if (impactFX != null)
        {
            Vector3 hitNormal = (transform.position - other.ClosestPoint(transform.position)).normalized;
            Instantiate(impactFX, transform.position, Quaternion.LookRotation(hitNormal));
        }

        Destroy(gameObject);
    }
}
