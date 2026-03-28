using UnityEngine;
using System.Collections;

public class FlyingBooster : MonoBehaviour
{
    public void StartFlight(Vector3 startWorldPos, Vector3 targetScreenPos, System.Action onComplete)
    {
        StartCoroutine(FlightRoutine(startWorldPos, targetScreenPos, onComplete));
    }

    private IEnumerator FlightRoutine(Vector3 startPos, Vector3 targetPos, System.Action onComplete)
    {
        float duration = 1.0f; 
        float elapsed = 0;

        RectTransform rect = GetComponent<RectTransform>();
        Canvas canvas = GetComponentInParent<Canvas>();

        // 1. Конвертуємо світову позицію фрукта в координати всередині Canvas
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, startPos);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPoint, canvas.worldCamera, out localPoint);

        // 2. Те саме робимо для цілі (кнопки)
        Vector2 targetScreenPoint = RectTransformUtility.WorldToScreenPoint(null, targetPos); // Кнопки вже в екрані
        Vector2 targetLocalPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, targetScreenPoint, canvas.worldCamera, out targetLocalPoint);

        rect.anchoredPosition = localPoint;

        // Виносимо на самий передній план
        rect.SetAsLastSibling();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curve = 1f - Mathf.Pow(1f - t, 3);

            // Рухаємо через anchoredPosition
            rect.anchoredPosition = Vector2.Lerp(localPoint, targetLocalPoint, curve);

            // Робимо його великим, щоб точно побачити
            rect.localScale = Vector3.one * (1.5f + Mathf.Sin(t * Mathf.PI));

            yield return null;
        }

        onComplete?.Invoke();
        Destroy(gameObject);
    }
}