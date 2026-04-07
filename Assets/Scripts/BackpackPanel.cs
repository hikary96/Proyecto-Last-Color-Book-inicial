using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BackpackPanel : MonoBehaviour
{
    [System.Serializable]
    public class BackpackItem
    {
        public Image itemImage;              // La imagen del objeto
        public GameObject descriptionPanel; // Panel con la descripción
    }

    [Header("Items del panel (en orden)")]
    public BackpackItem[] items;

    [Header("Folleto (Item 0)")]
    public GameObject folletoPanel;         // Panel especial que abre el item 0 con J

    [Header("Navegación")]
    public float switchInterval = 0.15f;
    public string playerTag = "Player";

    [Header("Escala de selección")]
    public float selectedScale = 1.25f;
    public float deselectedScale = 1f;
    public float scaleLerpSpeed = 14f;

    [Header("Brillo de selección")]
    public Color selectedColor = Color.white;
    public Color deselectedColor = new Color(0.45f, 0.45f, 0.45f, 1f);
    public float colorLerpSpeed = 10f;

    [Header("Animación de descripción")]
    public float descFadeInDuration = 0.35f;
    public float descSlideDistance = 40f;

    [Header("Animación de selección con J")]
    public float jPunchScale = 1.45f;
    public float jPunchDuration = 0.18f;

    [Header("Indicador de navegación (opcional)")]
    public GameObject arrowLeft;
    public GameObject arrowRight;

    [Header("Salir con K")]
    public NotificationManager notificationManager; // NotificationManager a activar al salir
    public int notificationIndexOnExit = 0;         // Índice de la notificación a mostrar


    private int currentIndex = 0;
    private int activeDescIndex = -1;
    private float[] currentScales;
    private Color[] currentColors;
    private CanvasGroup[] descCanvasGroups;
    private RectTransform[] descRects;
    private Vector2[] descOriginalPositions;
    private bool isAnimating = false;
    private float inputTimer = 0f;

    void Start()
    {
        int count = items.Length;
        currentScales = new float[count];
        currentColors = new Color[count];
        descCanvasGroups = new CanvasGroup[count];
        descRects = new RectTransform[count];
        descOriginalPositions = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            currentScales[i] = deselectedScale;
            currentColors[i] = deselectedColor;

            if (items[i].descriptionPanel != null)
            {
                descCanvasGroups[i] = GetOrAddCanvasGroup(items[i].descriptionPanel);
                descRects[i] = items[i].descriptionPanel.GetComponent<RectTransform>();
                descOriginalPositions[i] = descRects[i].anchoredPosition;

                descCanvasGroups[i].alpha = 0f;
                items[i].descriptionPanel.SetActive(false);
            }
        }

        // Ocultamos el folleto al inicio
        if (folletoPanel != null)
            folletoPanel.SetActive(false);

        ApplySelectionInstant(0);
        // Abrimos la descripción del primer item automáticamente
        StartCoroutine(AutoOpenDescription(0));
        UpdateArrows();
    }

    void OnEnable()
    {
        if (currentScales != null)
        {
            ApplySelectionInstant(0);
            StartCoroutine(AutoOpenDescription(0));
        }
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        HandleNavigation();

        if (Input.GetKeyDown(KeyCode.J))
            StartCoroutine(HandleJPress());

        if (Input.GetKeyDown(KeyCode.K))
            CloseAndNotify();

        AnimateItems();
    }

    void CloseAndNotify()
    {
        // Cerrar la mochila
        gameObject.SetActive(false);

        // Lanzar la notificación
        if (notificationManager != null)
            notificationManager.ShowNotification(notificationIndexOnExit);
    }

    void HandleNavigation()
    {
        inputTimer -= Time.deltaTime;
        if (inputTimer > 0f) return;

        bool moved = false;
        int previousIndex = currentIndex;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (currentIndex < items.Length - 1)
            {
                currentIndex++;
                moved = true;
            }
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                moved = true;
            }
        }

        if (moved)
        {
            inputTimer = switchInterval;
            UpdateArrows();
            StartCoroutine(SwitchDescription(previousIndex, currentIndex));
        }
    }

    // Cierra la descripción anterior y abre la nueva al navegar
    IEnumerator SwitchDescription(int fromIndex, int toIndex)
    {
        if (isAnimating) yield break;
        isAnimating = true;

        // Cerramos la descripción anterior
        if (fromIndex != -1 && activeDescIndex == fromIndex)
            yield return StartCoroutine(HideDescription(fromIndex));

        // Abrimos la del nuevo item
        activeDescIndex = toIndex;
        yield return StartCoroutine(ShowDescription(toIndex));

        isAnimating = false;
    }

    // Para el primer item al abrir el panel (sin bloquear isAnimating)
    IEnumerator AutoOpenDescription(int index)
    {
        yield return null; // Un frame de margen para que Start() termine
        if (activeDescIndex != -1 && activeDescIndex != index)
            yield return StartCoroutine(HideDescription(activeDescIndex));

        activeDescIndex = index;
        yield return StartCoroutine(ShowDescription(index));
    }

    // J: en item 0 abre el folleto, en otros hace punch + toggle descripción
    IEnumerator HandleJPress()
    {
        if (isAnimating) yield break;
        isAnimating = true;

        int index = currentIndex;

        // Punch de escala siempre
        yield return StartCoroutine(PunchScale(items[index].itemImage.transform, jPunchScale, jPunchDuration));

        if (index == 0 && folletoPanel != null)
        {
            // Item 0: cierra la mochila y abre el folleto
            folletoPanel.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            // Otros items: toggle de la descripción
            if (activeDescIndex == index)
            {
                yield return StartCoroutine(HideDescription(index));
                activeDescIndex = -1;
            }
            else
            {
                if (activeDescIndex != -1)
                    yield return StartCoroutine(HideDescription(activeDescIndex));

                activeDescIndex = index;
                yield return StartCoroutine(ShowDescription(index));
            }
        }

        isAnimating = false;
    }

    void AnimateItems()
    {
        for (int i = 0; i < items.Length; i++)
        {
            bool isSelected = (i == currentIndex);

            float targetScale = isSelected ? selectedScale : deselectedScale;
            currentScales[i] = Mathf.Lerp(currentScales[i], targetScale, Time.deltaTime * scaleLerpSpeed);
            items[i].itemImage.transform.localScale = Vector3.one * currentScales[i];

            Color targetColor = isSelected ? selectedColor : deselectedColor;
            currentColors[i] = Color.Lerp(currentColors[i], targetColor, Time.deltaTime * colorLerpSpeed);
            items[i].itemImage.color = currentColors[i];
        }
    }

    IEnumerator ShowDescription(int index)
    {
        if (items[index].descriptionPanel == null) yield break;

        items[index].descriptionPanel.SetActive(true);
        CanvasGroup cg = descCanvasGroups[index];
        RectTransform rect = descRects[index];

        Vector2 startPos = descOriginalPositions[index] + new Vector2(0f, -descSlideDistance);
        rect.anchoredPosition = startPos;
        cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < descFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float eased = 1f - Mathf.Pow(1f - elapsed / descFadeInDuration, 3f);
            cg.alpha = eased;
            rect.anchoredPosition = Vector2.Lerp(startPos, descOriginalPositions[index], eased);
            yield return null;
        }

        cg.alpha = 1f;
        rect.anchoredPosition = descOriginalPositions[index];
    }

    IEnumerator HideDescription(int index)
    {
        if (items[index].descriptionPanel == null) yield break;

        CanvasGroup cg = descCanvasGroups[index];
        RectTransform rect = descRects[index];
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = descOriginalPositions[index] + new Vector2(0f, -descSlideDistance);

        float elapsed = 0f;
        while (elapsed < descFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / descFadeInDuration;
            cg.alpha = Mathf.Lerp(1f, 0f, t * t);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t * t);
            yield return null;
        }

        cg.alpha = 0f;
        rect.anchoredPosition = descOriginalPositions[index];
        items[index].descriptionPanel.SetActive(false);
    }

    IEnumerator PunchScale(Transform target, float peakScale, float duration)
    {
        Vector3 baseScale = Vector3.one * selectedScale;
        Vector3 peak = Vector3.one * peakScale;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(baseScale, peak,
                                Mathf.Sin(elapsed / duration * Mathf.PI));
            yield return null;
        }

        target.localScale = baseScale;
    }

    void ApplySelectionInstant(int index)
    {
        currentIndex = index;
        activeDescIndex = -1;

        for (int i = 0; i < items.Length; i++)
        {
            bool isSelected = (i == index);
            currentScales[i] = isSelected ? selectedScale : deselectedScale;
            currentColors[i] = isSelected ? selectedColor : deselectedColor;
            items[i].itemImage.transform.localScale = Vector3.one * currentScales[i];
            items[i].itemImage.color = currentColors[i];
        }
    }

    void UpdateArrows()
    {
        if (arrowLeft != null)
            arrowLeft.SetActive(currentIndex > 0);

        if (arrowRight != null)
            arrowRight.SetActive(currentIndex < items.Length - 1);
    }

    CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}