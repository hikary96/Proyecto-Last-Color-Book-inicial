using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SequenceNotificationTrigger : MonoBehaviour
{
    [Header("Configuración")]
    public string playerTag = "Player";
    public NotificationManager notificationManager;

    [Header("Índices de las 3 notificaciones en orden")]
    public int firstNotificationIndex = 1;
    public int secondNotificationIndex = 2;
    public int thirdNotificationIndex = 3;

    [Header("Tiempo entre segunda y tercera notificación")]
    public float delayBetweenSecond = 7f;

    [Header("J - Prompt")]
    public GameObject jObject;             // El GameObject del prompt J en el HUD
    public Image jImage;                   // La Image del prompt J
    public Sprite jNormal;                 // Sprite J normal
    public Sprite jSolicit;               // Sprite J con manito
    public float jSwitchInterval = 0.6f;  // Velocidad de parpadeo
    public float jFadeInDuration = 0.4f;

    [Header("K - Mochila")]
    public GameObject kObject;
    public Image kImage;
    public Sprite kOff;
    public Sprite kOn;
    public float kFadeInDuration = 0.4f;

    [Header("Panel que abre la K")]
    public GameObject kPanel;

    [Header("Animación del panel")]
    public float panelFadeInDuration = 0.35f;

    private bool kEnabled = false;
    private bool panelOpen = false;
    private bool isAnimating = false;
    private bool waitingForJ = false;      // Esperando que el jugador presione J
    private CanvasGroup jCanvasGroup;
    private Coroutine jBlinkCoroutine;

    void Start()
    {
        if (jObject != null)
        {
            jCanvasGroup = jObject.GetComponent<CanvasGroup>();
            if (jCanvasGroup == null)
                jCanvasGroup = jObject.AddComponent<CanvasGroup>();
            jObject.SetActive(false);
        }

        // La K siempre visible pero en off desde el inicio
        if (kObject != null)
        {
            kObject.SetActive(true);
            kImage.sprite = kOff;
        }

        if (kPanel != null)
            kPanel.SetActive(false);
    }

    void Update()
    {
        // J para pasar a la segunda notificación
        if (waitingForJ && Input.GetKeyDown(KeyCode.J))
        {
            waitingForJ = false;
            StartCoroutine(ContinueSequence());
        }

        // K para abrir/cerrar el panel
        if (kEnabled && !isAnimating)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                if (!panelOpen) StartCoroutine(OpenPanel());
                else StartCoroutine(ClosePanel());
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        GetComponent<Collider>().enabled = false;
        StartCoroutine(NotificationSequence());
    }

    IEnumerator NotificationSequence()
    {
        // Primera notificación + aparece el prompt de la J
        notificationManager.ShowNotification(firstNotificationIndex);
        yield return StartCoroutine(ShowJPrompt());

        // Esperamos a que el jugador presione J
        waitingForJ = true;
    }

    IEnumerator ContinueSequence()
    {
        // Ocultamos el prompt de la J
        yield return StartCoroutine(HideJPrompt());

        // Segunda notificación
        notificationManager.ShowNotification(secondNotificationIndex);

        // Esperamos antes de la tercera
        yield return new WaitForSeconds(delayBetweenSecond);

        // Tercera notificación + habilitamos la K
        notificationManager.ShowNotification(thirdNotificationIndex);
        yield return StartCoroutine(EnableKSprite());
    }

    IEnumerator ShowJPrompt()
    {
        jObject.SetActive(true);
        jImage.sprite = jNormal;
        jCanvasGroup.alpha = 0f;
        jObject.transform.localScale = Vector3.one * 1.2f;

        float elapsed = 0f;
        while (elapsed < jFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float eased = 1f - Mathf.Pow(1f - elapsed / jFadeInDuration, 3f);
            jCanvasGroup.alpha = eased;
            jObject.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, eased);
            yield return null;
        }

        jCanvasGroup.alpha = 1f;
        jObject.transform.localScale = Vector3.one;

        jBlinkCoroutine = StartCoroutine(BlinkJPrompt());
    }

    IEnumerator BlinkJPrompt()
    {
        while (true)
        {
            jImage.sprite = jNormal;
            yield return new WaitForSeconds(jSwitchInterval);

            StartCoroutine(PunchScale(jObject.transform, 1.15f, 0.1f));
            jImage.sprite = jSolicit;
            yield return new WaitForSeconds(jSwitchInterval);
        }
    }

    IEnumerator HideJPrompt()
    {
        if (jBlinkCoroutine != null)
            StopCoroutine(jBlinkCoroutine);

        float elapsed = 0f;
        float startAlpha = jCanvasGroup.alpha;

        while (elapsed < jFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jFadeInDuration;
            jCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t * t);
            yield return null;
        }

        jCanvasGroup.alpha = 0f;
        jObject.SetActive(false);
    }

    IEnumerator EnableKSprite()
    {
        // Ya está activo, solo cambiamos el sprite con punch de escala
        kImage.sprite = kOn;
        kObject.transform.localScale = Vector3.one * 1.3f;

        CanvasGroup cg = GetOrAddCanvasGroup(kObject);
        cg.alpha = 1f; // Ya era visible, no hacemos fade

        float elapsed = 0f;
        while (elapsed < kFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float eased = 1f - Mathf.Pow(1f - elapsed / kFadeInDuration, 3f);
            kObject.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, eased);
            yield return null;
        }

        kObject.transform.localScale = Vector3.one;
        kEnabled = true;
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
        kPanel.SetActive(true);

        CanvasGroup panelCG = GetOrAddCanvasGroup(kPanel);
        RectTransform panelRect = kPanel.GetComponent<RectTransform>();
        Vector2 originalPos = panelRect.anchoredPosition;
        Vector2 startPos = originalPos + new Vector2(0f, -60f);

        panelRect.anchoredPosition = startPos;
        panelCG.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < panelFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float eased = 1f - Mathf.Pow(1f - elapsed / panelFadeInDuration, 3f);
            panelCG.alpha = eased;
            panelRect.anchoredPosition = Vector2.Lerp(startPos, originalPos, eased);
            yield return null;
        }

        panelCG.alpha = 1f;
        panelRect.anchoredPosition = originalPos;
        isAnimating = false;
    }

    IEnumerator ClosePanel()
    {
        isAnimating = true;
        panelOpen = false;

        CanvasGroup panelCG = GetOrAddCanvasGroup(kPanel);
        RectTransform panelRect = kPanel.GetComponent<RectTransform>();
        Vector2 startPos = panelRect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, -80f);

        float elapsed = 0f;
        while (elapsed < panelFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / panelFadeInDuration;
            panelCG.alpha = Mathf.Lerp(1f, 0f, t * t);
            panelRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t * t);
            yield return null;
        }

        panelCG.alpha = 0f;
        kPanel.SetActive(false);
        panelRect.anchoredPosition = startPos;
        isAnimating = false;
    }

    CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}