using UnityEngine;

public class NodeController : MonoBehaviour
{
    private GridManager _gridManager;
    private SpriteRenderer _renderer;
    private Color _baseColor;
    public int Row { get; private set; }
    public int Col { get; private set; }
    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        // Шукаємо менеджер на сцені
        _gridManager = Object.FindFirstObjectByType<GridManager>();
    }

    // Метод для встановлення початкового кольору (викликається з GridManager)
    public void Setup(int r, int c, Color color)
    {
        Row = r;
        Col = c;
        _baseColor = color;
        _renderer.color = _baseColor;
    }

    private void OnMouseDown()
    {
        if (_gridManager != null)
            _gridManager.OnNodeClick(this);
    }

    public void Select()
    {
        transform.localScale = Vector3.one * 1.1f; // Трохи збільшуємо
        _renderer.color = _baseColor * 0.8f;       // Робимо колір на 20% темнішим
    }
    public Color GetColor()
    {
        return _baseColor;
    }
    public void Deselect()
    {
        transform.localScale = Vector3.one;        // Скидаємо масштаб
        _renderer.color = _baseColor;              // Повертаємо оригінальний колір
    }
    public void UpdateCoordinates(int r, int c)
    {
        Row = r;
        Col = c;
    }
}