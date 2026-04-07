using UnityEngine;
using System.Collections;

public class MovementModeTrigger : MonoBehaviour
{
    [Header("Modo al entrar a esta zona")]
    public bool enable3DMovement = true;
    public string playerTag = "Player";

    [Header("Notificación")]
    public NotificationManager notificationManager;
    public int notificationIndex = 1;

    [Header("Delay de activación")]
    public float activationDelay = 8f;   // Segundos antes de activar el collider

    private Collider triggerCollider;

    void Start()
    {
        triggerCollider = GetComponent<Collider>();

        // Desactivamos el collider al inicio y lo activamos después del delay
        triggerCollider.enabled = false;
        StartCoroutine(EnableAfterDelay());
    }

    IEnumerator EnableAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        triggerCollider.enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
            player.SetMovementMode(enable3DMovement);

        // Disparamos la notificación si está asignada
        if (notificationManager != null)
            notificationManager.ShowNotification(notificationIndex);
    }
}