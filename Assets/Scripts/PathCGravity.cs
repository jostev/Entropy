using UnityEngine;

// PATH C — Rolls random gravity on entry. 33% low, 33% high, 33% normal.
//
// SETUP:
//   1. Add this script to each room's trigger collider (Is Trigger = true).
//   2. Tune the three gravity values in the inspector.
//   3. That's it — no cube, no key, just walk in and get chaos.

[RequireComponent(typeof(Collider))]
public class PathC_RandomGrav : MonoBehaviour
{
    [Header("Gravity Values")]
    public float lowGravity    = 2.5f;
    public float normalGravity = 9.81f;
    public float highGravity   = 24f;


    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() == null) return;

        int roll = Random.Range(0, 4);

        float strength = roll switch
        {
            0 => lowGravity,
            1 => highGravity,
            _ => normalGravity
        };

        Physics.gravity = Vector3.down * strength;

        string label = roll switch { 0 => "LOW", 1 => "HIGH", _ => "NORMAL" };
        Debug.Log($"[PathC] {name} — rolled {label} ({strength:F1})");
    }
}