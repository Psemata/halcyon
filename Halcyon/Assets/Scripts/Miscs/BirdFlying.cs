using UnityEngine;

public class BirdFlying : MonoBehaviour
{
    [SerializeField] private Transform centerPoint;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private Vector3 axis = Vector3.up;
    [SerializeField] private float yAmplitude = 1f;      // Amplitude du mouvement vertical
    [SerializeField] private float yFrequency = 1f;      // Fréquence du mouvement vertical

    private float angle;
    private float initialY;

    void Start()
    {
        initialY = transform.position.y;
    }

    void Update()
    {
        if (centerPoint == null) return;

        angle += speed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        // Mouvement vertical avec un sinus
        float y = initialY + Mathf.Sin(Time.time * yFrequency) * yAmplitude;

        Vector3 offset = new Vector3(x, 0, z);
        Vector3 circlePos = centerPoint.position + offset;
        circlePos.y = y;
        transform.position = circlePos;

        // Calcul de la tangente (dérivée du cercle)
        float dx = -Mathf.Sin(angle) * radius;
        float dz = Mathf.Cos(angle) * radius;
        Vector3 tangent = new Vector3(dx, 0, dz).normalized;

        // Oriente l'objet dans la direction de la tangente
        if (tangent != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(tangent, Vector3.up);
    }
}
