using UnityEngine;

public class IslandOrbit : MonoBehaviour
{
    [Header("Orbit Centre")]
    public Transform centre;

    [Header("Ellipse Shape")]
    public float semiMajorAxis = 10f;
    public float semiMinorAxis = 6f;

    [Header("Orbit Speed")]
    public float orbitSpeed = 0.3f;
    public float phaseOffset = 0f;

    [Header("Vertical Bob")]
    public float bobAmplitude = 0f;
    public float bobFrequency = 0.8f;
    public float bobOffset = 0f;

    private float _angle;

    void Start()
    {
        _angle = phaseOffset;
    }

    void Update()
    {
        if (centre == null) return;

        _angle += orbitSpeed * Time.deltaTime;

        float x = Mathf.Cos(_angle) * semiMajorAxis;
        float z = Mathf.Sin(_angle) * semiMinorAxis;

        float bob = Mathf.Sin(Time.time * bobFrequency + bobOffset) * bobAmplitude;
        float finalY = Mathf.Max(0f, bob);

        transform.position = new Vector3(
            centre.position.x + x,
            centre.position.y + finalY,
            centre.position.z + z
        );
    }
}