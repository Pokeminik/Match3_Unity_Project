using UnityEngine;

public class UITitleAnimate : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmount = 15f;
    private Vector3 _startPos;

    void Start() => _startPos = transform.localPosition;

    void Update()
    {
        // Плавне погойдування вгору-вниз
        float newY = _startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.localPosition = new Vector3(_startPos.x, newY, _startPos.z);

        // Легке обертання вліво-вправо
        float rotation = Mathf.Sin(Time.time * floatSpeed * 0.5f) * 3f;
        transform.localRotation = Quaternion.Euler(0, 0, rotation);
    }
}