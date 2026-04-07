using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuNavigation : MonoBehaviour
{
    [Header("Textos de las opciones (en orden)")]
    public RectTransform[] menuOptions;

    [Header("Indicador Book")]
    public RectTransform bookIndicator;
    public float bookOffsetX = -80f;
    public float bookOffsetY = 0f;
    public float moveSpeed = 12f;

    [Header("Colores")]
    public Color selectedColor = Color.white;
    public Color deselectedColor = new Color(1f, 1f, 1f, 0.25f);
    public float colorLerpSpeed = 10f;        // Velocidad de transición del color

    [Header("Escala de opciones")]
    public float selectedScale = 1.15f;       // Tamaño al estar seleccionado
    public float deselectedScale = 1f;
    public float scaleLerpSpeed = 12f;        // Suavidad del escalado

    [Header("Animación del Book")]
    public float bobHeight = 8f;              // Cuánto sube y baja flotando
    public float bobSpeed = 2f;               // Qué tan rápido flota
    public float bookRotationAmount = 12f;    // Rotación máxima al moverse
    public float rotationReturnSpeed = 8f;    // Qué tan rápido vuelve a 0°

    [Header("Nombres de escenas")]
    public string playSceneName = "Museo";
    public string loadSceneName = "CargarPartida";
    public string creditsSceneName = "MenuCreditos";

    private int currentIndex = 0;
    private Vector2 bookTargetPosition;
    private float currentRotation = 0f;
    private float rotationVelocity = 0f;
    private int lastIndex = 0;

    // Guardamos el color y escala actual de cada opción para lerp suave
    private Color[] currentColors;
    private float[] currentScales;

    void Start()
    {
        currentColors = new Color[menuOptions.Length];
        currentScales = new float[menuOptions.Length];

        for (int i = 0; i < menuOptions.Length; i++)
        {
            currentColors[i] = deselectedColor;
            currentScales[i] = deselectedScale;
        }

        UpdateSelection(instant: true);
    }

    void Update()
    {
        HandleInput();
        AnimateBook();
        AnimateOptions();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            lastIndex = currentIndex;
            currentIndex = (currentIndex + 1) % menuOptions.Length;
            rotationVelocity = -bookRotationAmount; // Inclina hacia abajo
            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            lastIndex = currentIndex;
            currentIndex = (currentIndex - 1 + menuOptions.Length) % menuOptions.Length;
            rotationVelocity = bookRotationAmount;  // Inclina hacia arriba
            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Return))
        {
            // Pequeño squish al confirmar
            bookIndicator.localScale = Vector3.one * 0.8f;
            ConfirmSelection();
        }
    }

    void AnimateBook()
    {
        // Deslizamiento suave hacia la opción seleccionada
        bookIndicator.anchoredPosition = Vector2.Lerp(
            bookIndicator.anchoredPosition,
            bookTargetPosition + new Vector2(0f, Mathf.Sin(Time.time * bobSpeed) * bobHeight),
            Time.deltaTime * moveSpeed
        );

        // Rotación: se inclina al moverse y vuelve suavemente a 0
        currentRotation = Mathf.Lerp(currentRotation, 0f, Time.deltaTime * rotationReturnSpeed);
        currentRotation += rotationVelocity * Time.deltaTime;
        rotationVelocity = Mathf.Lerp(rotationVelocity, 0f, Time.deltaTime * rotationReturnSpeed);
        bookIndicator.localRotation = Quaternion.Euler(0f, 0f, currentRotation);

        // Escala vuelve a 1 si se hizo squish al confirmar
        bookIndicator.localScale = Vector3.Lerp(
            bookIndicator.localScale,
            Vector3.one,
            Time.deltaTime * 15f
        );
    }

    void AnimateOptions()
    {
        for (int i = 0; i < menuOptions.Length; i++)
        {
            bool isSelected = (i == currentIndex);

            // Color con lerp suave
            Color targetColor = isSelected ? selectedColor : deselectedColor;
            currentColors[i] = Color.Lerp(currentColors[i], targetColor, Time.deltaTime * colorLerpSpeed);

            TextMeshProUGUI tmp = menuOptions[i].GetComponent<TextMeshProUGUI>();
            if (tmp != null)
                tmp.color = currentColors[i];

            // Escala con lerp suave
            float targetScale = isSelected ? selectedScale : deselectedScale;
            currentScales[i] = Mathf.Lerp(currentScales[i], targetScale, Time.deltaTime * scaleLerpSpeed);
            menuOptions[i].localScale = Vector3.one * currentScales[i];
        }
    }

    void UpdateSelection(bool instant = false)
    {
        RectTransform selected = menuOptions[currentIndex];

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bookIndicator.parent as RectTransform,
            RectTransformUtility.WorldToScreenPoint(null, selected.position),
            null,
            out localPoint
        );

        float leftEdge = localPoint.x - (selected.rect.width * 0.5f * selectedScale);

        bookTargetPosition = new Vector2(
            leftEdge + bookOffsetX,
            localPoint.y + bookOffsetY
        );

        if (instant)
        {
            bookIndicator.anchoredPosition = bookTargetPosition;

            // Aplicar estado inicial instantáneo
            for (int i = 0; i < menuOptions.Length; i++)
            {
                bool isSelected = (i == currentIndex);
                currentColors[i] = isSelected ? selectedColor : deselectedColor;
                currentScales[i] = isSelected ? selectedScale : deselectedScale;

                TextMeshProUGUI tmp = menuOptions[i].GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.color = currentColors[i];

                menuOptions[i].localScale = Vector3.one * currentScales[i];
            }
        }
    }

    void ConfirmSelection()
    {
        switch (currentIndex)
        {
            case 0: SceneManager.LoadScene(playSceneName); break;
            case 1: SceneManager.LoadScene(loadSceneName); break;
            case 2: SceneManager.LoadScene(creditsSceneName); break;
            case 3: QuitGame(); break;
        }
    }

    void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}