using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GridManager;

public class BoosterManager : Singleton<BoosterManager>
{
    [Header("Кількість бонусів")]
    public int hammerCount = 0;
    public int bombCount = 0;
    public int lightningCount = 0;
    public int arrowCount = 0;
    public int shuffleCount = 0;
    [SerializeField] private GameObject shuffleConfirmPanel;
    [Header("UI Елементи (Кнопки)")]
    [SerializeField] private Button hammerBtn;
    [SerializeField] private Button bombBtn;
    [SerializeField] private Button lightningBtn;
    [SerializeField] private Button arrowBtn;
    [SerializeField] private Button shuffleBtn;

    [Header("Тексти лічильників")]
    [SerializeField] private TextMeshProUGUI hammerText;
    [SerializeField] private TextMeshProUGUI bombText;
    [SerializeField] private TextMeshProUGUI lightningText;
    [SerializeField] private TextMeshProUGUI arrowText;
    [SerializeField] private TextMeshProUGUI shuffleText;

    void Start()
    {
        UpdateBoosterUI();
    }

    // Метод для додавання заряду (викликаємо з GridManager)
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

    // Метод для використання (викликатимемо, коли активуємо бонус)
    public bool UseBooster(string type)
    {
        switch (type.ToLower())
        {
            case "hammer": if (hammerCount > 0) { hammerCount--; UpdateBoosterUI(); return true; } break;
            case "bomb": if (bombCount > 0) { bombCount--; UpdateBoosterUI(); return true; } break;
            case "lightning": if (lightningCount > 0) { lightningCount--; UpdateBoosterUI(); return true; } break;
            case "arrow": if (arrowCount > 0) { arrowCount--; UpdateBoosterUI(); return true; } break;
            case "shuffle": if (shuffleCount > 0) { shuffleCount--; UpdateBoosterUI(); return true; } break;
        }
        return false;
    }

    public void UpdateBoosterUI()
    {
        UpdateButtonState(hammerBtn, hammerText, hammerCount);
        UpdateButtonState(bombBtn, bombText, bombCount);
        UpdateButtonState(lightningBtn, lightningText, lightningCount);
        UpdateButtonState(arrowBtn, arrowText, arrowCount);
        UpdateButtonState(shuffleBtn, shuffleText, shuffleCount);
    }
    public void OnBoosterClick(string type)
    {
        GridManager gm = Object.FindFirstObjectByType<GridManager>();
        if (gm == null) return;

        // Перевіряємо, чи цей бустер ВЖЕ активний
        // Якщо так — скасовуємо режим
        if (gm.GetCurrentMode().ToString().ToLower() == type.ToLower())
        {
            gm.SetBoosterMode(GridManager.BoosterMode.None);
            ResetAllBoosters();
            return;
        }

        // Якщо ні — активуємо новий
        switch (type.ToLower())
        {
            case "hammer": if (hammerCount > 0) gm.SetBoosterMode(GridManager.BoosterMode.Hammer); break;
            case "bomb": if (bombCount > 0) gm.SetBoosterMode(GridManager.BoosterMode.Bomb); break;
            case "lightning": if (lightningCount > 0) gm.SetBoosterMode(GridManager.BoosterMode.Lightning); break;
            case "arrow": if (arrowCount > 0) gm.SetBoosterMode(GridManager.BoosterMode.Arrow); break;
            case "shuffle": if (shuffleCount > 0) ShowShuffleConfirm(); break;
        }
    }
    private void UpdateButtonState(Button btn, TextMeshProUGUI txt, int count)
    {
        if (btn == null || txt == null) return;

        txt.text = count.ToString();
        Image img = btn.GetComponent<Image>();

        if (count <= 0)
        {
            if (img != null) img.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Темно-сірий
            btn.interactable = false;
        }
        else
        {
            if (img != null) img.color = Color.white;
            btn.interactable = true;
        }
    }
    public void HighlightBooster(string type)
    {
        // Спочатку скидаємо всі кнопки до нормального розміру
        ResetAllBoosters();

        Button targetBtn = null;
        switch (type.ToLower())
        {
            case "hammer": targetBtn = hammerBtn; break;
            case "bomb": targetBtn = bombBtn; break;
            case "lightning": targetBtn = lightningBtn; break;
            case "arrow": targetBtn = arrowBtn; break;
        }

        if (targetBtn != null)
        {
            targetBtn.transform.localScale = Vector3.one * 1.2f; // Збільшуємо
                                                                 // Можна додати компонент Outline на кнопку в Unity для підсвічування
        }
    }
    public void ShowShuffleConfirm()
    {
        if (shuffleCount > 0) shuffleConfirmPanel.SetActive(true);
    }
    public void ConfirmShuffle()
    {
        GridManager gm = Object.FindFirstObjectByType<GridManager>();
        gm.ExecuteShuffle(); // Викликаємо реальний шафл
        shuffleConfirmPanel.SetActive(false);
    }
    public void CancelShuffle() => shuffleConfirmPanel.SetActive(false);
    public void ResetAllBoosters()
    {
        hammerBtn.transform.localScale = Vector3.one;
        bombBtn.transform.localScale = Vector3.one;
        lightningBtn.transform.localScale = Vector3.one;
        arrowBtn.transform.localScale = Vector3.one;
        // Shuffle не скидаємо тут, бо він миттєвий
    }
}
