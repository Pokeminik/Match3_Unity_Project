using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private Vector3 _originalScale;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickScale = 0.95f;

    void Awake() => _originalScale = transform.localScale;

    // Наведення мишки
    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(_originalScale * hoverScale));
    }

    // Мишка пішла геть
    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(_originalScale));
    }

    // Натискання
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = _originalScale * clickScale;

        // Використовуємо Твій існуючий UIManager!
        if (UIManager.Instance != null)
            UIManager.Instance.PlayClickSound();
    }

    private System.Collections.IEnumerator ScaleTo(Vector3 targetScale)
    {
        float elapsed = 0;
        float duration = 0.1f;
        Vector3 currentScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(currentScale, targetScale, elapsed / duration);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}