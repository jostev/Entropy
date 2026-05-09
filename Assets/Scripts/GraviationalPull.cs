using UnityEngine;

public class BlackHoleGravity : MonoBehaviour
{
    public Transform blackHole;
    public float pullStrength = 5f;
    public float pullRadius = 12f;
    public float bounceForce = 8f;
    public float bounceRadius = 2f;

    private Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (blackHole == null) return;

        Vector3 direction = blackHole.position - transform.position;
        float distance = direction.magnitude;

        if (distance > pullRadius) return;

        if (distance < bounceRadius)
        {
            Vector3 bounceDirection = (-direction.normalized + Vector3.up * 2f).normalized;
            _rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
        }
        else
        {
            // Gentle pull, stronger closer
            float strength = pullStrength / (distance * distance);
            _rb.AddForce(direction.normalized * strength);
        }
    }
}