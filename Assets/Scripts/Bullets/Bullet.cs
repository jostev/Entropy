using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 25f;
    public float bulletDropGravityScale = 0.4f;
    public float lifetime = 4f;

    [Header("FX")]
    public GameObject impactFX;

    private Rigidbody rb;
    private Vector3 lastPosition;

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

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitNormal = (transform.position - hitPoint).normalized;

        if (hitNormal == Vector3.zero)
        {
            hitNormal = -transform.forward;
        }

        HitSomething(other, hitPoint, hitNormal);
    }

    private void HitSomething(Collider other, Vector3 hitPoint, Vector3 hitNormal)
    {
        Health health = other.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        if (impactFX != null)
        {
            Instantiate(impactFX, hitPoint, Quaternion.LookRotation(hitNormal));
        }

        Destroy(gameObject);
    }
}