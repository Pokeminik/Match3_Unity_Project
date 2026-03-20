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
        _renderer.color = active ? new Color(1f, 1f, 0.5f, 0.7f) : Color.white;
    }
    public void Select()
    {
        transform.localScale = _originalScale * 1.2f;
        _renderer.sortingOrder = 10;
        _renderer.color = new Color(1f, 1f, 1f, 0.8f);
    }
    private void OnMouseDown()
    {
        Debug.Log("Клік по фрукту спрацював!");
        GridManager gm = Object.FindFirstObjectByType<GridManager>();

        if (gm != null)
        {
            gm.OnNodeClick(this);
        }
    }
    public void Deselect()
    {
        transform.localScale = _originalScale;
        _renderer.sortingOrder = 0;
        _renderer.color = Color.white;
    }
}