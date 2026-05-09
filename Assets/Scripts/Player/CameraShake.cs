using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake")]
    public float returnSpeed = 18f;

    private Vector3 originalLocalPosition;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeStrength;

    void Awake()
    {
        originalLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            float progress = shakeTimer / shakeDuration;
            float currentStrength = shakeStrength * progress;

            Vector3 randomOffset = Random.insideUnitSphere * currentStrength;
            randomOffset.z = 0f;

            transform.localPosition = originalLocalPosition + randomOffset;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(
                transform.localPosition,
                originalLocalPosition,
                returnSpeed * Time.deltaTime
            );
        }
    }

    public void Shake(float strength, float duration)
    {
        shakeStrength = strength;
        shakeDuration = duration;
        shakeTimer = duration;
    }
}
