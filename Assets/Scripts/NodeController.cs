using UnityEngine;

public class NodeController : MonoBehaviour
{
    public int Row { get; private set; }
    public int Col { get; private set; }

    private SpriteRenderer _renderer;
    private Sprite _mySprite;
    private Vector3 _originalScale;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;
    }

    public void Setup(int r, int c, Sprite sprite)
    {
        Row = r;
        Col = c;
        _mySprite = sprite;

        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = _mySprite;
    }

    public Sprite GetSprite() => _mySprite;
    public void SetSprite(Sprite newSprite)
    {
        _mySprite = newSprite;
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        _renderer.sprite = _mySprite;
    }
    public void UpdateCoordinates(int r, int c)
    {
        Row = r;
        Col = c;
    }
    private void OnMouseEnter()
    {
        GridManager gm = Object.FindFirstObjectByType<GridManager>();
        if (gm != null) gm.OnHoverNode(this, true);
    }
    private void OnMouseExit()
    {
        GridManager gm = Object.FindFirstObjectByType<GridManager>();
        if (gm != null) gm.OnHoverNode(this, false);
    }
    public void Highlight(bool active)
    {
        if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
        // Робимо фрукт напівпрозорим або жовтуватим
        _renderer.color = active ? new Color(1f, 1f, 0.5f, 0.7f) : Color.white;
    }
    // Метод при натисканні (Вибір)
    public void Select()
    {
        // Збільшуємо фрукт на 20%
        transform.localScale = _originalScale * 1.2f;
        // Виносимо його на передній план (Sorting Order)
        _renderer.sortingOrder = 10;
        // Можна додати легке підсвічування
        _renderer.color = new Color(1f, 1f, 1f, 0.8f);
    }
    // Цей метод автоматично викликається Unity, коли ти клацаєш на Collider об'єкта
    private void OnMouseDown()
    {
        Debug.Log("Клік по фрукту спрацював!");
        // Знаходимо наш GridManager на сцені
        GridManager gm = Object.FindFirstObjectByType<GridManager>();

        if (gm != null)
        {
            // Передаємо команду кліку менеджеру
            gm.OnNodeClick(this);
        }
    }
    // Метод при знятті вибору
    public void Deselect()
    {
        // Повертаємо розмір назад
        transform.localScale = _originalScale;
        // Повертаємо шар назад
        _renderer.sortingOrder = 0;
        _renderer.color = Color.white;
    }
}