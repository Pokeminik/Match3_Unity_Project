using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Flight Settings")]
    [SerializeField] private GameObject flyingBoosterPrefab;
    [SerializeField] private RectTransform[] boosterButtonsTransforms;
    [SerializeField] private Sprite[] boosterSprites; // НОВЕ: сюди перетягнемо картинки

    private RectTransform _activeIconTransform;
    private float _pulseTimer = 0f;
    private string[] boosterTypes = { "hammer", "bomb", "arrow", "lightning", "shuffle" };

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
    public void SpawnFlyingBooster(int boosterType, Vector3 spawnWorldPos)
    {
        // НОВЕ: Перевірка ліміту. Якщо вже є 3 штуки — політ не починаємо.
        if (!CanAddMoreBoosters(boosterType))
        {
            Debug.Log($"Інвентар для бустера {boosterType} повний, політ скасовано.");
            return;
        }
        // 1. Шукаємо Canvas більш надійним способом
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) rootCanvas = Object.FindFirstObjectByType<Canvas>();

        // Створюємо об'єкт
        GameObject go = Instantiate(flyingBoosterPrefab, rootCanvas.transform);
        FlyingBooster flyer = go.GetComponent<FlyingBooster>();

        // Призначаємо спрайт
        Image img = go.GetComponent<Image>();
        if (img != null && boosterType < boosterSprites.Length && boosterSprites[boosterType] != null)
        {
            img.sprite = boosterSprites[boosterType];
        }

        // Запускаємо політ
        Vector3 targetPos = boosterButtonsTransforms[boosterType].position;
        flyer.StartFlight(spawnWorldPos, targetPos, () => {
            string[] types = { "hammer", "bomb", "arrow", "lightning", "shuffle" };
            AddBooster(types[boosterType]);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayBoosterCollect();
            RectTransform buttonRect = boosterButtonsTransforms[boosterType];
            Transform iconTransform = buttonRect.Find("Icon");
            if (iconTransform != null)
            {
                // Якщо знайшли — пульсує тільки іконка
                RectTransform iconRect = iconTransform.GetComponent<RectTransform>();
                StartCoroutine(PunchButton(iconRect));
            }
            else
            {
                // Якщо забув назвати об'єкт "Icon", пульсує вся кнопка і пише попередження
                Debug.LogWarning($"[BoosterManager] Не знайдено об'єкт 'Icon' у {buttonRect.name}. Перевір назву в Hierarchy.");
                StartCoroutine(PunchButton(buttonRect));
            }
        });
    }
    private System.Collections.IEnumerator ResetButtonScale(RectTransform rect)
    {
        yield return new WaitForSeconds(0.1f);
        rect.localScale = Vector3.one;
    }
    private bool CanAddMoreBoosters(int typeIndex)
    {
        // Перевіряємо за індексом (0-hammer, 1-bomb, 2-arrow, 3-lightning, 4-shuffle)
        switch (typeIndex)
        {
            case 0: return hammerCount < 3;
            case 1: return bombCount < 3;
            case 2: return arrowCount < 3;
            case 3: return lightningCount < 3;
            case 4: return shuffleCount < 3;
            default: return false;
        }
    }
    private IEnumerator PunchButton(RectTransform buttonRect)
    {
        float elapsed = 0;
        float duration = 0.2f;
        Vector3 startScale = Vector3.one;
        Vector3 punchScale = Vector3.one * 1.3f; // Збільшуємо на 30%

        // Швидко збільшуємо
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            buttonRect.localScale = Vector3.Lerp(startScale, punchScale, elapsed / (duration / 2));
            yield return null;
        }

        elapsed = 0;
        // Повертаємо назад
        while (elapsed < duration / 2)
        {
            elapsed += Time.deltaTime;
            buttonRect.localScale = Vector3.Lerp(punchScale, startScale, elapsed / (duration / 2));
            yield return null;
        }
        buttonRect.localScale = startScale;
    }
}