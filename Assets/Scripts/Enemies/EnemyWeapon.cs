using UnityEngine;

namespace Entropy.Enemies
{
    public class EnemyWeapon : MonoBehaviour
    {
        [Header("References")]
        public GameObject bulletPrefab;
        public Transform firePoint;

        [Header("Combat Stats")]
        public float bulletSpeed = 40f;
        public float damage = 15f;
        public float fireRate = 60f;
        public float bulletSpread = 3f;
        public int bulletCount = 1;

        [Header("Aim Settings")]
        public float aimLeadFactor = 0f;
        public float optimalRange = 15f;
        public float maxRange = 40f;

        private float nextFireTime;
        private Transform target;

        public void SetTarget(Transform targetTransform)
        {
            target = targetTransform;
        }

        public bool CanFire()
        {
            return Time.time >= nextFireTime && target != null;
        }

        public bool IsTargetInRange()
        {
            if (target == null) return false;
            float distance = Vector3.Distance(transform.position, target.position);
            return distance <= maxRange;
        }

        public bool IsTargetInOptimalRange()
        {
            if (target == null) return false;
            float distance = Vector3.Distance(transform.position, target.position);
            return distance <= optimalRange * 1.2f;
        }

        public void Fire()
        {
            if (!CanFire() || bulletPrefab == null || firePoint == null)
                return;

            nextFireTime = Time.time + 60f / fireRate;

            Vector3 baseDirection = GetAimDirection();

            for (int i = 0; i < bulletCount; i++)
            {
                Vector3 spreadDirection = ApplySpread(baseDirection);

                GameObject bulletGO = Instantiate(
                    bulletPrefab,
                    firePoint.position,
                    Quaternion.LookRotation(spreadDirection)
                );

                Bullet bullet = bulletGO.GetComponent<Bullet>();
                if (bullet != null)
                {
                    bullet.Launch(spreadDirection, bulletSpeed);
                    bullet.damage = damage;
                    bullet.Shooter = gameObject;
                }
            }
        }

        private Vector3 GetAimDirection()
        {
            if (target == null)
                return firePoint.forward;

            Vector3 targetPos = target.position;

            if (aimLeadFactor > 0f)
            {
                Rigidbody targetRb = target.GetComponent<Rigidbody>();
                if (targetRb != null)
                {
                    float timeToTarget = Vector3.Distance(firePoint.position, targetPos) / bulletSpeed;
                    targetPos += targetRb.linearVelocity * timeToTarget * aimLeadFactor;
                }
            }

            return (targetPos - firePoint.position).normalized;
        }

        private Vector3 ApplySpread(Vector3 direction)
        {
            float yaw = Random.Range(-bulletSpread, bulletSpread);
            float pitch = Random.Range(-bulletSpread, bulletSpread);
            Quaternion spreadRotation = Quaternion.Euler(pitch, yaw, 0f);
            return spreadRotation * direction;
        }
    }
}
