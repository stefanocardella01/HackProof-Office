using UnityEngine;

public class FloatingMarker : MonoBehaviour
{
    [Header("Floating")]
    public float amplitude = 0.25f;
    public float speed = 2f;

    [Header("Rotation")]
    public float rotationSpeed = 90f; // gradi al secondo

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Movimento su e giù
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * speed) * amplitude;

        // Rotazione continua sull'asse Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }
}

