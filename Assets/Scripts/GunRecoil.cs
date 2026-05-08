using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    [Header("Recoil Amount")]
    public Vector3 recoilPosition = new Vector3(0f, 0.03f, -0.08f);
    public Vector3 recoilRotation = new Vector3(-8f, 0f, 0f);

    [Header("Recoil Speed")]
    public float kickSpeed = 25f;
    public float returnSpeed = 12f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Vector3 targetPositionOffset;
    private Vector3 currentPositionOffset;

    private Vector3 targetRotationOffset;
    private Vector3 currentRotationOffset;

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        currentPositionOffset = Vector3.Lerp(
            currentPositionOffset,
            targetPositionOffset,
            kickSpeed * Time.deltaTime
        );

        currentRotationOffset = Vector3.Lerp(
            currentRotationOffset,
            targetRotationOffset,
            kickSpeed * Time.deltaTime
        );

        targetPositionOffset = Vector3.Lerp(
            targetPositionOffset,
            Vector3.zero,
            returnSpeed * Time.deltaTime
        );

        targetRotationOffset = Vector3.Lerp(
            targetRotationOffset,
            Vector3.zero,
            returnSpeed * Time.deltaTime
        );

        transform.localPosition = originalPosition + currentPositionOffset;
        transform.localRotation = originalRotation * Quaternion.Euler(currentRotationOffset);
    }

    public void PlayRecoil()
    {
        targetPositionOffset += recoilPosition;
        targetRotationOffset += recoilRotation;
    }
}