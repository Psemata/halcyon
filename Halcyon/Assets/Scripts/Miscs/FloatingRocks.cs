using UnityEngine;

public class FloatingRocks : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.5f; // Hauteur du flottement
    [SerializeField] private float frequency = 1f;   // Vitesse du flottement
    [SerializeField] private bool rotate = false;
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 30f, 0); // degrés/seconde

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        if (rotate)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}
