using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleScreen : MonoBehaviour
{
    [Header("Referencias")]
    public Image pressJImage;
    public GameObject menuPanel;

    [Header("Parpadeo")]
    public float blinkSpeed = 1.2f;
    public float minAlpha = 0.05f;
    public float maxAlpha = 1f;

    [Header("Animación de entrada")]
    public float entryDelay = 0.5f;        // Tiempo antes de aparecer la imagen
    public float entryDuration = 1f;       // Duración del fade in inicial

    [Header("Animación de escala al pulsar")]
    public float punchScale = 1.25f;       // Escala máxima del punch
    public float punchDuration = 0.12f;    // Qué tan rápido llega al pico

    [Header("Fade al salir")]
    public float fadeDuration = 0.8f;

    [Header("Animación del menú al entrar")]
    public CanvasGroup menuCanvasGroup;    // CanvasGroup del menuPanel para fade in
    public float menuFadeInDuration = 0.6f;

    private bool isTransitioning = false;

    void Start()
    {
        // Empezamos invisible
        SetImageAlpha(pressJImage, 0f);
        pressJImage.transform.localScale = Vector3.one;

        menuPanel.SetActive(false);

        // Si no asignaron el CanvasGroup lo buscamos automáticamente
        if (menuCanvasGroup == null)
            menuCanvasGroup = menuPanel.GetComponent<CanvasGroup>();

        StartCoroutine(EntryAnimation());
    }

    void Update()
    {
        if (isTransitioning) return;

        // Parpadeo suave con seno
        float alpha = Mathf.Lerp(minAlpha, maxAlpha,
                      (Mathf.Sin(Time.time * blinkSpeed * Mathf.PI) + 1f) / 2f);

        SetImageAlpha(pressJImage, alpha);

        if (Input.GetKeyDown(KeyCode.J))
            StartCoroutine(TransitionToMenu());
    }

    // Fade in suave al inicio
    IEnumerator EntryAnimation()
    {
        yield return new WaitForSeconds(entryDelay);

        float elapsed = 0f;
        while (elapsed < entryDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / entryDuration;
            // Ease out: entra rápido y frena al final
            SetImageAlpha(pressJImage, Mathf.Lerp(0f, maxAlpha, 1f - Mathf.Pow(1f - t, 3f)));
            yield return null;
        }
    }

    IEnumerator TransitionToMenu()
    {
        isTransitioning = true;

        // Punch de escala antes del fade: crece y encoge
        yield return StartCoroutine(PunchScale());

        // Fade out de la imagen con ease in
        float elapsed = 0f;
        float startAlpha = pressJImage.color.a;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            // Ease in: empieza lento y acelera al final
            SetImageAlpha(pressJImage, Mathf.Lerp(startAlpha, 0f, t * t));
            yield return null;
        }

        SetImageAlpha(pressJImage, 0f);

        // Activamos el menú y hacemos fade in si tiene CanvasGroup
        menuPanel.SetActive(true);

        if (menuCanvasGroup != null)
            yield return StartCoroutine(FadeInMenu());
    }

    IEnumerator PunchScale()
    {
        Vector3 originalScale = pressJImage.transform.localScale;
        Vector3 punchedScale = originalScale * punchScale;

        // Crece al pico
        float elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / punchDuration;
            pressJImage.transform.localScale = Vector3.Lerp(originalScale, punchedScale, t);
            yield return null;
        }

        // Vuelve al tamaño original
        elapsed = 0f;
        while (elapsed < punchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / punchDuration;
            pressJImage.transform.localScale = Vector3.Lerp(punchedScale, originalScale, t);
            yield return null;
        }

        pressJImage.transform.localScale = originalScale;
    }

    IEnumerator FadeInMenu()
    {
        menuCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < menuFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / menuFadeInDuration;
            // Ease out: entra rápido y frena suave
            menuCanvasGroup.alpha = 1f - Mathf.Pow(1f - t, 3f);
            yield return null;
        }

        menuCanvasGroup.alpha = 1f;
    }

    void SetImageAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}