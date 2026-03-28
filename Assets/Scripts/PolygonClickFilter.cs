using UnityEngine;
using UnityEngine.UI;

public class PolygonClickFilter : MonoBehaviour, ICanvasRaycastFilter
{
    private PolygonCollider2D _collider;
    private RectTransform _rectTransform;

    void Awake()
    {
        _collider = GetComponent<PolygonCollider2D>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        // ѕеретворюЇмо координату екрана (де мишка) у локальну координату об'Їкта
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenPoint, eventCamera, out localPoint);

        // ѕерев≥р€Їмо, чи ц€ точка знаходитьс€ всередин≥ нашого зеленого Polygon Collider
        return _collider.OverlapPoint(transform.TransformPoint(localPoint));
    }
}