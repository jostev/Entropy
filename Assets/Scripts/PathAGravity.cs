using UnityEngine;

// PATH A — Heavy gravity until player finds the hidden cube.
// 
// SETUP:
//   1. Add this script to your room's trigger collider (Is Trigger = true).
//   2. Assign the hidden cube's collider to Hidden Cube.
//   3. Tune Heavy Gravity and Normal Gravity in the inspector.
//   4. Repeat for all 4 rooms.

[RequireComponent(typeof(Collider))]
public class PathA_PressurePlate : MonoBehaviour
{
    [Header("Gravity")]
    public float heavyGravity  = 25f;
    public float normalGravity = 9.81f;

    [Header("Hidden Cube")]
    [Tooltip("Drag the hidden cube's collider here")]
    public Collider hiddenCube;

    private Rigidbody playerRb;
    private bool activated = false;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        if (hiddenCube == null)
        {
            Debug.LogWarning($"[PathA] {name}: No hidden cube assigned.");
            return;
        }

        hiddenCube.isTrigger = true;

        // Listen for player touching the cube
        var listener = hiddenCube.gameObject.AddComponent<PathA_CubeListener>();
        listener.onPlayerTouch = OnCubeTouched;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Rigidbody>(out var rb)) return;
        playerRb = rb;
        playerRb.useGravity = false;
        ApplyGravity(activated ? normalGravity : heavyGravity);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() == playerRb)
            playerRb = null;
    }

    void OnCubeTouched()
    {
        if (activated) return;
        activated = true;

        hiddenCube.gameObject.SetActive(false);

        if (playerRb != null) ApplyGravity(normalGravity);

        Debug.Log($"[PathA] {name} — cube found, gravity normalised.");
    }

    void ApplyGravity(float strength)
    {
        Physics.gravity = Vector3.down * strength;
        
        // Kill vertical velocity so gravity change doesn't launch the player
        if (playerRb != null)
            playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
    }
}
// Listens on the hidden cube — auto added at runtime, no setup needed
public class PathA_CubeListener : MonoBehaviour
{
    public System.Action onPlayerTouch;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
            onPlayerTouch?.Invoke();
    }
}