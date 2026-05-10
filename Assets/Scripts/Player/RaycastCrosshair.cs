using UnityEngine;

public class RaycastCrosshair : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
		public Transform firePoint;

    [Header("Raycast")]
    public float range = 100f;
    public LayerMask hitLayers = ~0;

    public bool HasHit { get; private set; }
    public RaycastHit CurrentHit { get; private set; }

    void Update()
    {
        CheckCrosshairRaycast();
    }

    void CheckCrosshairRaycast()
    {
        HasHit = Physics.Raycast(
            // playerCamera.transform.position,
						firePoint.position,
            playerCamera.transform.forward,
            out RaycastHit hit,
            range,
            hitLayers
        );
				Debug.Log(HasHit);

        if (HasHit)
        {
            CurrentHit = hit;

            Debug.DrawLine(
                playerCamera.transform.position,
                hit.point,
                Color.green
            );
        }
        else
        {
            Debug.DrawRay(
                playerCamera.transform.position,
                playerCamera.transform.forward * range,
                Color.red
            );
        }
    }
}
