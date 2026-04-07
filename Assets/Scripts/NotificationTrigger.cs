using UnityEngine;

public class NotificationTrigger : MonoBehaviour
{
    [Header("Configuración")]
    public NotificationManager notificationManager;
    public int notificationIndex = 1;
    public string playerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        notificationManager.ShowNotification(notificationIndex);

        // Se desactiva a sí mismo al ser tocado
        gameObject.SetActive(false);
    }
}