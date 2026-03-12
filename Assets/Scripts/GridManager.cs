using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Для роботи з кнопками
using TMPro;           // Для роботи з текстом TextMeshPro
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    [Header("UI елементи")]
    [SerializeField] private TextMeshProUGUI scoreText;
    private int _score = 0;
    private NodeController _firstSelected;
    private NodeController _secondSelected;
    private bool _isProcessing = false;

    [Header("Посилання на префаб")]
    [SerializeField] private GameObject nodePrefab;

    [Header("Налаштування сітки")]
    [SerializeField] private int rows = 8;
    [SerializeField] private int columns = 8;
    [SerializeField] private float spacing = 1.1f;
    [Header("Audio")]
    [SerializeField] private AudioClip matchSound;
    private NodeController[,] _nodes;
    private int _comboCount = 0;
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    void Start()
    {
        GenerateLevel();
        CheckForMatches();
    }

    void GenerateLevel()
    {
        _nodes = new NodeController[rows, columns];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Color randomColor;
                int attempts = 0;
                do
                {
                    randomColor = GetRandomColor();
                    attempts++;
                } while (WouldCreateMatch(r, c, randomColor) && attempts < 100);

                Vector3 pos = GetWorldPosition(r, c);
                GameObject newNode = Instantiate(nodePrefab, pos, Quaternion.identity);
                newNode.transform.parent = transform;

                NodeController controller = newNode.GetComponent<NodeController>();
                controller.Setup(r, c, randomColor);
                _nodes[r, c] = controller;
            }
        }
    }
    private void PlayMatchSound()
    {
        if (UIManager.Instance != null && matchSound != null)
        {
            var source = UIManager.Instance.GetComponent<AudioSource>();
            float vol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

            // Кожен наступний вибух підвищує тон на 0.1 (10%)
            // Початковий тон 1.0, далі 1.1, 1.2 і так далі
            source.pitch = 1f + (_comboCount * 0.1f);

            // Обмежуємо максимальний пітч, щоб звук не став занадто "писклявим"
            if (source.pitch > 2f) source.pitch = 2f;

            source.PlayOneShot(matchSound, vol);

            _comboCount++; // Збільшуємо комбо після кожного вибуху
        }
    }
    private Vector3 GetWorldPosition(int r, int c)
    {
        float xPos = (c - (columns / 2f) + 0.5f) * spacing;
        float yPos = (r - (rows / 2f) + 0.5f) * spacing;
        return new Vector3(xPos, yPos, 0);
    }

    bool WouldCreateMatch(int r, int c, Color color)
    {
        if (c >= 2)
        {
            NodeController n1 = GetNodeAt(r, c - 1);
            NodeController n2 = GetNodeAt(r, c - 2);
            if (n1 != null && n2 != null && n1.GetColor() == color && n2.GetColor() == color) return true;
        }
        if (r >= 2)
        {
            NodeController n1 = GetNodeAt(r - 1, c);
            NodeController n2 = GetNodeAt(r - 2, c);
            if (n1 != null && n2 != null && n1.GetColor() == color && n2.GetColor() == color) return true;
        }
        return false;
    }

    public void OnNodeClick(NodeController node)
    {
        if (_isProcessing) return;

        if (_firstSelected == null)
        {
            _comboCount = 0;
            _firstSelected = node;
            _firstSelected.Select();
        }
        else if (_firstSelected == node)
        {
            _firstSelected.Deselect();
            _firstSelected = null;
        }
        else
        {
            if (IsAdjacent(_firstSelected, node))
            {
                _secondSelected = node;
                _firstSelected.Deselect();
                _secondSelected.Deselect();
                StartCoroutine(SwapNodes(_firstSelected, _secondSelected));
                _firstSelected = null;
                _secondSelected = null;
            }
            else
            {
                _firstSelected.Deselect();
                _firstSelected = node;
                _firstSelected.Select();
            }
        }
    }

    private void CheckForMatches()
    {
        HashSet<NodeController> matches = new HashSet<NodeController>();

        // Перевірка горизонталей
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns - 2; c++)
            {
                NodeController n1 = _nodes[r, c];
                NodeController n2 = _nodes[r, c + 1];
                NodeController n3 = _nodes[r, c + 2];
                if (n1 && n2 && n3 && n1.GetColor() == n2.GetColor() && n2.GetColor() == n3.GetColor())
                {
                    matches.Add(n1); matches.Add(n2); matches.Add(n3);
                }
            }
        }

        // Перевірка вертикалей
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows - 2; r++)
            {
                NodeController n1 = _nodes[r, c];
                NodeController n2 = _nodes[r + 1, c];
                NodeController n3 = _nodes[r + 2, c];
                if (n1 && n2 && n3 && n1.GetColor() == n2.GetColor() && n2.GetColor() == n3.GetColor())
                {
                    matches.Add(n1); matches.Add(n2); matches.Add(n3);
                }
            }
        }

        if (matches.Count > 0)
        {
            PlayMatchSound();
            _score += matches.Count * 10; // Наприклад, 10 очок за фішку
            UpdateScoreUI();
            _isProcessing = true;
            foreach (var match in matches)
            {
                if (match != null)
                {
                    _nodes[match.Row, match.Col] = null;
                    Destroy(match.gameObject);
                }
            }
            StartCoroutine(WaitAndFill());
        }
        else
        {
            _isProcessing = false; // Вихід з циклу обробки
        }
    }
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + _score;
    }
    private System.Collections.IEnumerator WaitAndFill()
    {
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(FillHoles());
    }

    private System.Collections.IEnumerator FillHoles()
    {
        // 1. Падіння існуючих
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (_nodes[r, c] == null)
                {
                    for (int nextR = r + 1; nextR < rows; nextR++)
                    {
                        if (_nodes[nextR, c] != null)
                        {
                            NodeController upperNode = _nodes[nextR, c];
                            _nodes[r, c] = upperNode;
                            _nodes[nextR, c] = null;
                            upperNode.UpdateCoordinates(r, c);
                            StartCoroutine(MoveNode(upperNode, GetWorldPosition(r, c)));
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(0.25f);

        // 2. Спавн нових
        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                if (_nodes[r, c] == null)
                {
                    Vector3 spawnPos = GetWorldPosition(rows, c);
                    GameObject newNode = Instantiate(nodePrefab, spawnPos, Quaternion.identity);
                    newNode.transform.parent = transform;
                    NodeController controller = newNode.GetComponent<NodeController>();
                    controller.Setup(r, c, GetRandomColor());
                    _nodes[r, c] = controller;
                    StartCoroutine(MoveNode(controller, GetWorldPosition(r, c)));
                }
            }
        }
        yield return new WaitForSeconds(0.3f);
        CheckForMatches();
    }

    private System.Collections.IEnumerator SwapNodes(NodeController a, NodeController b)
    {
        _isProcessing = true;

        // Візуальний обмін
        yield return StartCoroutine(AnimateSwap(a, b));

        // Логічний обмін
        int rA = a.Row; int cA = a.Col;
        int rB = b.Row; int cB = b.Col;
        a.UpdateCoordinates(rB, cB);
        b.UpdateCoordinates(rA, cA);
        _nodes[rA, cA] = b;
        _nodes[rB, cB] = a;

        yield return new WaitForSeconds(0.1f);

        if (!HasMatches())
        {
            yield return StartCoroutine(AnimateSwap(a, b));
            // Повертаємо логіку назад
            a.UpdateCoordinates(rA, cA);
            b.UpdateCoordinates(rB, cB);
            _nodes[rA, cA] = a;
            _nodes[rB, cB] = b;
            _isProcessing = false;
        }
        else
        {
            CheckForMatches();
        }
    }

    private System.Collections.IEnumerator AnimateSwap(NodeController a, NodeController b)
    {
        float elapsed = 0;
        float duration = 0.2f;
        Vector3 posA = a.transform.position;
        Vector3 posB = b.transform.position;

        while (elapsed < duration)
        {
            if (a == null || b == null) yield break;
            a.transform.position = Vector3.Lerp(posA, posB, elapsed / duration);
            b.transform.position = Vector3.Lerp(posB, posA, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        a.transform.position = posB;
        b.transform.position = posA;
    }

    private System.Collections.IEnumerator MoveNode(NodeController node, Vector3 target)
    {
        float elapsed = 0;
        float duration = 0.2f;
        if (node == null) yield break;
        Vector3 startPos = node.transform.position;
        while (elapsed < duration)
        {
            if (node == null) yield break;
            node.transform.position = Vector3.Lerp(startPos, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (node != null) node.transform.position = target;
    }

    private bool HasMatches()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (_nodes[r, c] == null) continue;
                Color cur = _nodes[r, c].GetColor();
                if (c < columns - 2 && _nodes[r, c + 1] && _nodes[r, c + 2] &&
                    _nodes[r, c + 1].GetColor() == cur && _nodes[r, c + 2].GetColor() == cur) return true;
                if (r < rows - 2 && _nodes[r + 1, c] && _nodes[r + 2, c] &&
                    _nodes[r + 1, c].GetColor() == cur && _nodes[r + 2, c].GetColor() == cur) return true;
            }
        }
        return false;
    }

    private NodeController GetNodeAt(int r, int c)
    {
        if (r < 0 || r >= rows || c < 0 || c >= columns) return null;
        return _nodes[r, c];
    }

    private bool IsAdjacent(NodeController a, NodeController b)
    {
        return Mathf.Abs(a.Row - b.Row) + Mathf.Abs(a.Col - b.Col) == 1;
    }

    Color GetRandomColor()
    {
        Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta };
        return colors[Random.Range(0, colors.Length)];
    }
}