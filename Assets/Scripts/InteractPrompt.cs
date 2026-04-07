using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InteractPrompt : MonoBehaviour
{
    [Header("Collider trigger")]
    public string playerTag = "Player";

    [Header("Prompt UI")]
    public GameObject promptObject;
    public Image promptImage;
    public Sprite jNormal;
    public Sprite jSolicit;

    [Header("Panel que se abre al presionar J")]
    public GameObject panel;

    [Header("Animación del prompt")]
    public float switchInterval = 0.6f;
    public float fadeInDuration = 0.3f;
    public float scaleOnAppear = 1.2f;

    [Header("Animación de cierre del panel con K")]
    public float panelExitDuration = 0.4f;      // Duración de la animación de salida
    public float panelExitSlideY = -80f;         // Cuánto baja el panel al cerrarse

    [Header("Modelo a eliminar al abrir el panel")]
    public GameObject modelToRemove;  // Arrastra el modelo del mapa aquí

    [Header("Notificación al cerrar")]
    public NotificationManager notificationManager;
    public int notificationIndex = 2;

    private bool playerInRange = false;
    private bool panelOpen = false;
    private bool isAnimating = false;
    private bool hasBeenUsed = false;            // Una sola vez
    private CanvasGroup promptCanvasGroup;
    private Coroutine blinkCoroutine;

    void Start()
    {
        promptCanvasGroup = promptObject.GetComponent<CanvasGroup>();
        if (promptCanvasGroup == null)
            promptCanvasGroup = promptObject.AddComponent<CanvasGroup>();

        // Siempre visible pero estático al inicio
        promptObject.SetActive(true);
        promptImage.sprite = jNormal;
        promptCanvasGroup.alpha = 1f;

        if (panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange || isAnimating) return;

        // Abrir panel con J
        if (Input.GetKeyDown(KeyCode.J) && !panelOpen && !hasBeenUsed)
            StartCoroutine(OpenPanel());

        // Cerrar panel con K
        if (Input.GetKeyDown(KeyCode.K) && panelOpen)
            StartCoroutine(ClosePanelWithAnimation());
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (hasBeenUsed) return;   // Si ya se usó no hacemos nada

        playerInRange = true;
        blinkCoroutine = StartCoroutine(BlinkPrompt());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        playerInRange = false;

        if (!panelOpen)
        {
            // Detenemos el parpadeo y dejamos el sprite normal fijo
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);

            promptImage.sprite = jNormal;
            promptObject.transform.localScale = Vector3.one;
            // NO ocultamos el prompt
        }
    }

    IEnumerator ShowPrompt()
    {
        promptObject.SetActive(true);
        promptImage.sprite = jNormal;
        promptCanvasGroup.alpha = 0f;
        promptObject.transform.localScale = Vector3.one * scaleOnAppear;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float eased = 1f - Mathf.Pow(1f - elapsed / fadeInDuration, 3f);
            promptCanvasGroup.alpha = eased;
            promptObject.transform.localScale = Vector3.Lerp(
                Vector3.one * scaleOnAppear, Vector3.one, eased);
            yield return null;
        }

        promptCanvasGroup.alpha = 1f;
        promptObject.transform.localScale = Vector3.one;
        blinkCoroutine = StartCoroutine(BlinkPrompt());
    }

    IEnumerator HidePrompt()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        float elapsed = 0f;
        float startAlpha = promptCanvasGroup.alpha;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            promptCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t * t);
            yield return null;
        }

        promptCanvasGroup.alpha = 0f;
        promptObject.SetActive(false);
        promptImage.sprite = jNormal;
    }

    IEnumerator BlinkPrompt()
    {
        while (true)
        {
            promptImage.sprite = jNormal;
            yield return new WaitForSeconds(switchInterval);

            StartCoroutine(PunchScale(promptObject.transform, 1.15f, 0.1f));
            promptImage.sprite = jSolicit;
            yield return new WaitForSeconds(switchInterval);
        }
    }

    IEnumerator PunchScale(Transform target, float peakScale, float duration)
    {
        Vector3 original = Vector3.one;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(original, Vector3.one * peakScale,
                                Mathf.Sin(elapsed / duration * Mathf.PI));
            yield return null;
        }
        target.localScale = original;
    }

    IEnumerator OpenPanel()
    {
        isAnimating = true;
        panelOpen = true;

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        yield return StartCoroutine(HidePrompt());

        panel.SetActive(true);

        // Eliminamos el modelo del mapa
    if (modelToRemove != null)
        Destroy(modelToRemove);

        CanvasGroup panelCG = GetOrAddCanvasGroup(panel);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        Vector2 originalPos = panelRect.anchoredPosition;

        // Entrada: sube desde abajo + fade in
        Vector2 startPos = originalPos + new Vector2(0f, -60f);
        panelRect.anchoredPosition = startPos;
        panelCG.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float eased = 1f - Mathf.Pow(1f - elapsed / fadeInDuration, 3f);
            panelCG.alpha = eased;
            panelRect.anchoredPosition = Vector2.Lerp(startPos, originalPos, eased);
            yield return null;
        }

        panelCG.alpha = 1f;
        panelRect.anchoredPosition = originalPos;
        isAnimating = false;
    }

    IEnumerator ClosePanelWithAnimation()
    {
        isAnimating = true;
        panelOpen = false;
        hasBeenUsed = true;

        CanvasGroup panelCG = GetOrAddCanvasGroup(panel);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        Vector2 startPos = panelRect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, panelExitSlideY);

        float elapsed = 0f;
        while (elapsed < panelExitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / panelExitDuration;
            float eased = t * t;

            panelCG.alpha = Mathf.Lerp(1f, 0f, eased);
            panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            yield return null;
        }

        panelCG.alpha = 0f;
        panel.SetActive(false);
        panelRect.anchoredPosition = startPos;

        // Disparamos la notificación
        if (notificationManager != null)
            notificationManager.ShowNotification(notificationIndex);

        // Restauramos el prompt con jNormal fijo, sin parpadeo
        promptImage.sprite = jNormal;
        promptObject.SetActive(true);
        promptCanvasGroup.alpha = 1f;

        isAnimating = false;
    }

    CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}