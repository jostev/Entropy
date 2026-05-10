using UnityEngine;

public class FallRespawn : MonoBehaviour
{
    public float fallLimit = -50f;

    void Update()
    {
        if (transform.position.y < fallLimit)
        {
            var health = GetComponent<Health>();
            if (health != null && !health.IsInvulnerable)
            {
                health.TakeDamage(health.maxHealth * 2f);
            }
        }
    }
}
