using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Distancia")]
    public float defaultDistance = 5f;
    public float minDistance = 0.5f;

    [Header("Offset sobre el jugador")]
    public Vector3 pivotOffset = new Vector3(0f, 2f, 0f);

    [Header("Rotación fija de la cámara")]
    public float pitchAngle = 15f;   // Ángulo vertical (mira hacia abajo)
    public float yawAngle = 0f;      // Ángulo horizontal (0 = detrás del jugador)

    [Header("Suavizado")]
    public float followSpeed = 10f;
    public float recoverSpeed = 3f;

    [Header("Colisión")]
    public float sphereRadius = 0.3f;
    public float wallOffset = 0.15f;       // Margen para no pegarse a la pared
    public LayerMask collisionLayers;      // ← ASIGNA TUS LAYERS DE PAREDES AQUÍ

    private float currentDistance;

    void Start()
    {
        currentDistance = defaultDistance;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Punto de pivote (sobre el jugador)
        Vector3 pivot = target.position + pivotOffset;

        // Rotación deseada: combinamos yaw del jugador + offsets configurados
        Quaternion rotation = Quaternion.Euler(pitchAngle, target.eulerAngles.y + yawAngle, 0f);

        // Dirección detrás del jugador según esa rotación
        Vector3 desiredDirection = rotation * Vector3.back;

        // Calcular distancia segura con SphereCast
        float safeDistance = GetSafeDistance(pivot, desiredDirection);

        // Suavizar: acercarse rápido si hay pared, alejarse lento cuando se despeja
        float targetSpeed = (safeDistance < currentDistance) ? followSpeed : recoverSpeed;
        currentDistance = Mathf.Lerp(currentDistance, safeDistance, Time.deltaTime * targetSpeed);

        // Posicionar y orientar la cámara
        transform.position = pivot + desiredDirection * currentDistance;
        transform.LookAt(pivot);
    }

    float GetSafeDistance(Vector3 pivot, Vector3 direction)
    {
        if (Physics.SphereCast(
                pivot,
                sphereRadius,
                direction,
                out RaycastHit hit,
                defaultDistance,
                collisionLayers,
                QueryTriggerInteraction.Ignore)) // Ignora triggers para no interferir con tu zona 3D
        {
            return Mathf.Clamp(hit.distance - wallOffset, minDistance, defaultDistance);
        }

        return defaultDistance;
    }

    // Dibuja gizmos en el editor para depurar el raycast
    void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Vector3 pivot = target.position + pivotOffset;
        Quaternion rotation = Quaternion.Euler(pitchAngle, target.eulerAngles.y + yawAngle, 0f);
        Vector3 direction = rotation * Vector3.back;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pivot, pivot + direction * defaultDistance);
        Gizmos.DrawWireSphere(pivot + direction * defaultDistance, sphereRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}