using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelProgressUI : Singleton<LevelProgressUI>
{
    [Header("Посилання")]
    [SerializeField] private Slider progressSlider;
    [SerializeField] private RectTransform scoreTextRect;
    [SerializeField] private TextMeshProUGUI scoreTMP;
    [SerializeField] private RectTransform fillAreaRect;

    [Header("Зірочки")]
    [SerializeField] private Image[] starImages;      // Сюди перетягни свої 3 зірки
    [SerializeField] private Sprite starActiveSprite; // Яскрава зірка
    [SerializeField] private Sprite starNormalSprite; // Тьмяна зірка

    // Поріг очок для кожної зірки (наприклад: 300, 600, 1000)
    [SerializeField] private int[] starScoreThresholds = { 300, 600, 1000 };
    void Start()
    {
        AlignStars();
        UpdateProgress(0);
    }
    private void AlignStars()
    {
        if (fillAreaRect == null || starImages == null || starScoreThresholds.Length == 0) return;

        float totalHeight = fillAreaRect.rect.height;
        int maxScore = starScoreThresholds[starScoreThresholds.Length - 1];

        for (int i = 0; i < starImages.Length; i++)
        {
            float ratio = (float)starScoreThresholds[i] / maxScore;
            float newY = ratio * totalHeight;

            RectTransform starRect = starImages[i].rectTransform;

            // Автоматично вираховуємо половину висоти зірки
            // Якщо зірка 100 пікселів, то зміщення буде 50.
            float offset = starRect.rect.height / 2f;

            Vector2 pos = starRect.anchoredPosition;
            pos.y = newY - offset; // Віднімаємо половину висоти, щоб відцентрувати

            starRect.anchoredPosition = pos;
        }
    }
    public void UpdateProgress(int currentScore)
    {
        scoreTMP.text = "" + currentScore;

        // Беремо останній поріг як максимальний бал рівня
        int maxScore = starScoreThresholds[starScoreThresholds.Length - 1];
        float progress = Mathf.Clamp01((float)currentScore / maxScore);

        progressSlider.value = progress;

        // Рухаємо текст
        float height = fillAreaRect.rect.height;
        Vector2 newPos = scoreTextRect.anchoredPosition;
        newPos.y = progress * height;
        scoreTextRect.anchoredPosition = newPos;

        // ПЕРЕВІРКА ЗІРОЧОК
        CheckStars(currentScore);
    }

    private void CheckStars(int currentScore)
    {
        for (int i = 0; i < starImages.Length; i++)
        {
            // Якщо ми набрали достатньо очок для цієї зірки
            if (currentScore >= starScoreThresholds[i])
            {
                // Якщо зірка ще не активна — активуємо її
                if (starImages[i].sprite != starActiveSprite)
                {
                    starImages[i].sprite = starActiveSprite;
                    // Додамо маленький ефект "вистрибування"
                    starImages[i].transform.localScale = Vector3.one * 1.5f;
                    StartCoroutine(SmoothScaleBack(starImages[i].transform));
                }
            }
            else
            {
                starImages[i].sprite = starNormalSprite;
            }
        }
    }

    // Корутина для плавного повернення розміру зірки
    private System.Collections.IEnumerator SmoothScaleBack(Transform starTransform)
    {
        float elapsed = 0;
        float duration = 0.3f;
        while (elapsed < duration)
        {
            starTransform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        starTransform.localScale = Vector3.one;
    }
}