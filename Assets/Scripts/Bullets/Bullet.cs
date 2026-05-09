using UnityEngine;
using Entropy.Perks;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 25f;
    public float bulletDropGravityScale = 0.4f;
    public float lifetime = 4f;

    [Header("Explosion")]
    public bool explosive = true;
    public float explosionRadius = 3f;
    public float explosionForce = 6f;
    public float upwardForce = 1f;
    public LayerMask explosionLayers = ~0;

    [Header("FX")]
    public GameObject impactFX;

    private Rigidbody rb;
    private Vector3 lastPosition;

    public bool CanRicochet;
    private bool _hasRicocheted;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;

        foreach (Collider col in GetComponents<Collider>())
        {
            col.isTrigger = true;
        }

        lastPosition = transform.position;

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;
        Vector3 movement = currentPosition - lastPosition;

        // Raycast between last frame and this frame so the bullet does not go through the ground.
        if (movement.sqrMagnitude > 0.0001f)
        {
            if (Physics.Raycast(lastPosition, movement.normalized, out RaycastHit hit, movement.magnitude))
            {
                if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Enemy"))
                {
                    HitSomething(hit.collider, hit.point, hit.normal);
                    return;
                }
            }
        }

        rb.AddForce(Physics.gravity * bulletDropGravityScale, ForceMode.Acceleration);

        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }

        lastPosition = transform.position;
    }

    public void Launch(Vector3 direction, float speed)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!(other.CompareTag("Ground") || other.CompareTag("Enemy")))
        {
            return;
        }

        Vector3 hitPoint = other.ClosestPointOnBounds(transform.position);
        Vector3 hitNormal = (transform.position - hitPoint).normalized;

        if (hitNormal == Vector3.zero)
        {
            hitNormal = -transform.forward;
        }

        HitSomething(other, hitPoint, hitNormal);
    }

    private void HitSomething(Collider other, Vector3 hitPoint, Vector3 hitNormal)
    {
        GameEvents.BulletHit(this, other, hitPoint, hitNormal);

        Health health = other.GetComponent<Health>();

        if (CanRicochet && !_hasRicocheted)
        {
            _hasRicocheted = true;
            rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, hitNormal) * 0.8f;
            return;
        }

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        if (impactFX != null)
        {
            Instantiate(impactFX, hitPoint, Quaternion.LookRotation(hitNormal));
        }

        Explode(hitPoint);

        Destroy(gameObject);
    }

    private void Explode(Vector3 explosionPoint)
    {
        if (!explosive)
        {
            return;
        }

        Collider[] nearbyColliders = Physics.OverlapSphere(
            explosionPoint,
            explosionRadius,
            explosionLayers
        );

        foreach (Collider nearby in nearbyColliders)
        {
            if (!(nearby.CompareTag("Player") || nearby.CompareTag("Enemy")))
            {
                continue;
            }

            Rigidbody targetRb = nearby.GetComponentInParent<Rigidbody>();

            if (targetRb == null)
            {
                continue;
            }

            Vector3 closestPoint = nearby.ClosestPoint(explosionPoint);
            Vector3 direction = (closestPoint - explosionPoint).normalized;

            if (direction == Vector3.zero)
            {
                direction = (nearby.transform.position - explosionPoint).normalized;
            }

            direction += Vector3.up * upwardForce;
            direction.Normalize();

            float distance = Vector3.Distance(explosionPoint, closestPoint);
            float distanceMultiplier = 1f - Mathf.Clamp01(distance / explosionRadius);

            targetRb.AddForce(
                direction * explosionForce * distanceMultiplier,
                ForceMode.Impulse
            );
        }
    }
}