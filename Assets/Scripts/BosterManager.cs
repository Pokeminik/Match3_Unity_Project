using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterManager : Singleton<BoosterManager>
{
    [Header("Кількість бонусів")]
    public int hammerCount = 0;
    public int bombCount = 0;
    public int lightningCount = 0;
    public int arrowCount = 0;
    public int shuffleCount = 0;

    [SerializeField] private GameObject shuffleConfirmPanel;

    [Header("Кнопки")]
    [SerializeField] private Button hammerBtn;
    [SerializeField] private Button bombBtn;
    [SerializeField] private Button lightningBtn;
    [SerializeField] private Button arrowBtn;
    [SerializeField] private Button shuffleBtn;

    [Header("Іконки (Image компоненти)")]
    [SerializeField] private Image hammerIconImg;
    [SerializeField] private Image bombIconImg;
    [SerializeField] private Image lightningIconImg;
    [SerializeField] private Image arrowIconImg;
    [SerializeField] private Image shuffleIconImg;

    [Header("Тексти")]
    [SerializeField] private TextMeshProUGUI hammerText;
    [SerializeField] private TextMeshProUGUI bombText;
    [SerializeField] private TextMeshProUGUI lightningText;
    [SerializeField] private TextMeshProUGUI arrowText;
    [SerializeField] private TextMeshProUGUI shuffleText;

    [Header("Пульсація")]
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private float pulseAmount = 0.12f;

    private RectTransform _activeIconTransform;
    private float _pulseTimer = 0f;

    void Start() => UpdateBoosterUI();

    void Update()
    {
        if (_activeIconTransform != null)
        {
            _pulseTimer += Time.deltaTime * pulseSpeed;
            float scale = 1.1f + Mathf.Sin(_pulseTimer) * pulseAmount;
            _activeIconTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    public bool IsShufflePanelActive() => shuffleConfirmPanel != null && shuffleConfirmPanel.activeSelf;

    public void AddBooster(string type)
    {
        switch (type.ToLower())
        {
            case "hammer": if (hammerCount < 3) hammerCount++; break;
            case "bomb": if (bombCount < 3) bombCount++; break;
            case "lightning": if (lightningCount < 3) lightningCount++; break;
            case "arrow": if (arrowCount < 3) arrowCount++; break;
            case "shuffle": if (shuffleCount < 3) shuffleCount++; break;
        }
        UpdateBoosterUI();
    }

    public bool UseBooster(string type)
    {
        bool result = false;
        switch (type.ToLower())
        {
            case "hammer": if (hammerCount > 0) { hammerCount--; result = true; } break;
            case "bomb": if (bombCount > 0) { bombCount--; result = true; } break;
            case "lightning": if (lightningCount > 0) { lightningCount--; result = true; } break;
            case "arrow": if (arrowCount > 0) { arrowCount--; result = true; } break;
            case "shuffle": if (shuffleCount > 0) { shuffleCount--; result = true; } break;
        }
        if (result) UpdateBoosterUI();
        return result;
    }

    public void UpdateBoosterUI()
    {
        UpdateState(hammerBtn, hammerText, hammerIconImg, hammerCount);
        UpdateState(bombBtn, bombText, bombIconImg, bombCount);
        UpdateState(lightningBtn, lightningText, lightningIconImg, lightningCount);
        UpdateState(arrowBtn, arrowText, arrowIconImg, arrowCount);
        UpdateState(shuffleBtn, shuffleText, shuffleIconImg, shuffleCount);
    }

    private void UpdateState(Button b, TextMeshProUGUI t, Image i, int c)
    {
        if (!b || !t || !i) return;
        t.text = c.ToString();
        i.color = (c <= 0) ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : Color.white;
        b.interactable = c > 0;
        t.alpha = (c <= 0) ? 0.5f : 1f;
    }

    public void HighlightBooster(string type)
    {
        ResetAllBoosters();
        Image target = null;
        switch (type.ToLower())
        {
            case "hammer": target = hammerIconImg; break;
            case "bomb": target = bombIconImg; break;
            case "lightning": target = lightningIconImg; break;
            case "arrow": target = arrowIconImg; break;
        }
        if (target) { _activeIconTransform = target.rectTransform; _pulseTimer = 0f; }
    }

    public void ResetAllBoosters()
    {
        _activeIconTransform = null;
        hammerIconImg.rectTransform.localScale = Vector3.one;
        bombIconImg.rectTransform.localScale = Vector3.one;
        lightningIconImg.rectTransform.localScale = Vector3.one;
        arrowIconImg.rectTransform.localScale = Vector3.one;
    }

    public void OnBoosterClick(string type)
    {
        GridManager gm = Object.FindFirstObjectByType<GridManager>();
        if (!gm || IsShufflePanelActive()) return;

        if (gm.GetCurrentMode().ToString().ToLower() == type.ToLower())
        {
            gm.SetBoosterMode(GridManager.BoosterMode.None);
            ResetAllBoosters();
            return;
        }

        switch (type.ToLower())
        {
            case "hammer": if (hammerCount > 0) gm.SetBoosterMode(GridManager.BoosterMode.Hammer); break;
            case "bomb": if (bombCount > 0) gm.SetBoosterMode(GridManager.BoosterMode.Bomb); break;
            case "lightning": if (lightningCount > 0) gm.SetBoosterMode(GridManager.BoosterMode.Lightning); break;
            case "arrow": if (arrowCount > 0) gm.SetBoosterMode(GridManager.BoosterMode.Arrow); break;
            case "shuffle": if (shuffleCount > 0) ShowShuffleConfirm(); break;
        }
    }

    public void ShowShuffleConfirm()
    {
        if (shuffleCount > 0)
        {
            // 1. Знаходимо GridManager
            GridManager gm = Object.FindFirstObjectByType<GridManager>();

            if (gm != null)
            {
                // 2. Примусово вимикаємо режим будь-якого іншого бустера
                gm.SetBoosterMode(GridManager.BoosterMode.None);

                // 3. Очищаємо підсвічування фруктів на полі (метод додамо нижче)
                gm.ClearAllHighlights();
            }

            // 4. Тільки після цього показуємо панель
            shuffleConfirmPanel.SetActive(true);
        }
    }
    public void ConfirmShuffle()
    {
        GridManager gm = Object.FindFirstObjectByType<GridManager>();
        if (gm) gm.ExecuteShuffle();
        shuffleConfirmPanel.SetActive(false);
    }
    public void CancelShuffle() => shuffleConfirmPanel.SetActive(false);
}