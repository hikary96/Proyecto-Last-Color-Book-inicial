using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    [System.Serializable]
    public class Notification
    {
        public GameObject panel;            // El GameObject de la notificación (Notificacion1, etc.)
        public float duration = 6.5f;       // Cuánto tiempo se muestra
        public AnimationType exitAnimation; // Qué animación usa al salir

        public enum AnimationType
        {
            SlideLeft,   // Se va deslizando a la izquierda
            SlideRight,  // Se va deslizando a la derecha
            FadeOut,     // Se desvanece
            SlideUp      // Se va hacia arriba
        }
    }

    [Header("Notificaciones en orden")]
    public Notification[] notifications;

    [Header("Animación")]
    public float animationDuration = 0.5f;   // Duración de la animación de salida
    public float slideDistance = 600f;        // Distancia del deslizamiento

    [Header("Animación de entrada")]
    public float entryDuration = 0.4f;        // Duración del slide de entrada

    // Guardamos posiciones originales de cada panel
    private RectTransform[] rects;
    private Vector2[] originalPositions;
    private CanvasGroup[] canvasGroups;

    void Start()
    {
        // Inicializamos referencias y ocultamos todas
        rects = new RectTransform[notifications.Length];
        originalPositions = new Vector2[notifications.Length];
        canvasGroups = new CanvasGroup[notifications.Length];

        for (int i = 0; i < notifications.Length; i++)
        {
            if (notifications[i].panel == null) continue;

            rects[i] = notifications[i].panel.GetComponent<RectTransform>();
            originalPositions[i] = rects[i].anchoredPosition;

            // Añadimos CanvasGroup si no tiene para manejar el alpha
            canvasGroups[i] = notifications[i].panel.GetComponent<CanvasGroup>();
            if (canvasGroups[i] == null)
                canvasGroups[i] = notifications[i].panel.AddComponent<CanvasGroup>();

            notifications[i].panel.SetActive(false);
        }

        // La primera notificación se muestra automáticamente al inicio
        ShowNotification(0);
    }

    // Llamar con el índice de la notificación (0 = primera, 1 = segunda, etc.)
    public void ShowNotification(int index)
    {
        if (index < 0 || index >= notifications.Length) return;
        if (notifications[index].panel == null) return;

        StartCoroutine(NotificationRoutine(index));
    }

    IEnumerator NotificationRoutine(int index)
    {
        GameObject panel = notifications[index].panel;
        RectTransform rect = rects[index];
        CanvasGroup cg = canvasGroups[index];

        // Activamos y animamos entrada
        panel.SetActive(true);
        yield return StartCoroutine(AnimateEntry(rect, cg, index));

        // Esperamos la duración
        float waitTime = Random.Range(
            notifications[index].duration - 0.5f,
            notifications[index].duration + 0.5f  // Pequeña variación aleatoria
        );
        yield return new WaitForSeconds(waitTime);

        // Animamos salida
        yield return StartCoroutine(AnimateExit(rect, cg, index));

        panel.SetActive(false);

        // Restauramos posición para si se vuelve a mostrar
        rect.anchoredPosition = originalPositions[index];
        cg.alpha = 1f;
    }

    IEnumerator AnimateEntry(RectTransform rect, CanvasGroup cg, int index)
    {
        
        Vector2 startPos = originalPositions[index] + new Vector2(0f, slideDistance);
        rect.anchoredPosition = startPos;
        cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < entryDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / entryDuration;
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            rect.anchoredPosition = Vector2.Lerp(startPos, originalPositions[index], eased);
            cg.alpha = Mathf.Lerp(0f, 1f, eased);

            yield return null;
        }

        rect.anchoredPosition = originalPositions[index];
        cg.alpha = 1f;
    }

    IEnumerator AnimateExit(RectTransform rect, CanvasGroup cg, int index)
    {
        Vector2 startPos = originalPositions[index];
        Vector2 endPos;

        switch (notifications[index].exitAnimation)
        {
            case Notification.AnimationType.SlideLeft:
                endPos = startPos + new Vector2(-slideDistance, 0f);
                break;
            case Notification.AnimationType.SlideRight:
                endPos = startPos + new Vector2(slideDistance, 0f);
                break;
            case Notification.AnimationType.SlideUp:
                endPos = startPos + new Vector2(0f, slideDistance);
                break;
            default: // FadeOut — no mueve, solo desvanece
                endPos = startPos;
                break;
        }

        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            float eased = t * t; // Ease in: empieza lento, acelera al final

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            cg.alpha = Mathf.Lerp(1f, 0f, eased);

            yield return null;
        }
    }
}