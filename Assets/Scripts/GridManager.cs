using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    [Header("UI елементи")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText; 
    private int _score = 0;
    private int _comboCount = 0;

    private NodeController _firstSelected;
    private NodeController _secondSelected;
    private bool _isProcessing = false;

    [Header("Налаштування сітки")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private int rows = 7;
    [SerializeField] private int columns = 8;
    [SerializeField] private float spacing = 1.1f;
    [Header("Зміщення сітки")]
    [SerializeField] private Vector2 gridOffset = Vector2.zero; 
    [Header("Текстури фішок")]
    [SerializeField] private Sprite[] nodeSprites;
    [Header("Ефекти сітки")]
    [SerializeField] private GameObject tileBGPrefab; 
    private GameObject[,] _tileBGs;
    [SerializeField] private GameObject explosionPrefab;
    [Header("Audio")]
    [SerializeField] private AudioClip matchSound;

    private NodeController[,] _nodes;

    void Start()
    {
        if (comboText != null) comboText.gameObject.SetActive(false);
        GenerateLevel();
        CheckForMatches();
    }

    void GenerateLevel()
    {
        _nodes = new NodeController[rows, columns];
        _tileBGs = new GameObject[rows, columns]; 

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector3 pos = GetWorldPosition(r, c);


                GameObject newTileBG = Instantiate(tileBGPrefab, pos, Quaternion.identity);
                newTileBG.transform.parent = transform;
                _tileBGs[r, c] = newTileBG; 


                Sprite randomSprite;
                int attempts = 0;
                do
                {
                    randomSprite = GetRandomSprite();
                    attempts++;
                } while (WouldCreateMatch(r, c, randomSprite) && attempts < 100);

                GameObject newNode = Instantiate(nodePrefab, pos, Quaternion.identity);
                newNode.transform.parent = transform;

                NodeController controller = newNode.GetComponent<NodeController>();
                controller.Setup(r, c, randomSprite);
                _nodes[r, c] = controller;
            }
        }
    }

    private Sprite GetRandomSprite()
    {
        return nodeSprites[Random.Range(0, nodeSprites.Length)];
    }

    bool WouldCreateMatch(int r, int c, Sprite sprite)
    {
        if (c >= 2)
        {
            NodeController n1 = GetNodeAt(r, c - 1);
            NodeController n2 = GetNodeAt(r, c - 2);
            if (n1 && n2 && n1.GetSprite() == sprite && n2.GetSprite() == sprite) return true;
        }
        if (r >= 2)
        {
            NodeController n1 = GetNodeAt(r - 1, c);
            NodeController n2 = GetNodeAt(r - 2, c);
            if (n1 && n2 && n1.GetSprite() == sprite && n2.GetSprite() == sprite) return true;
        }
        return false;
    }

    public void OnNodeClick(NodeController node)
    {
        if (_isProcessing) return;

        if (_firstSelected == null)
        {
            _comboCount = 0; // Скидаємо комбо при новому ході
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
                if (n1 && n2 && n3 && n1.GetSprite() == n2.GetSprite() && n2.GetSprite() == n3.GetSprite())
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
                if (n1 && n2 && n3 && n1.GetSprite() == n2.GetSprite() && n2.GetSprite() == n3.GetSprite())
                {
                    matches.Add(n1); matches.Add(n2); matches.Add(n3);
                }
            }
        }

        if (matches.Count > 0)
        {
            _isProcessing = true;
            PlayMatchSound();
            ShowComboUI();
            int basePoints = matches.Count * 10;
            _score += basePoints * _comboCount;
            UpdateScoreUI();

            foreach (var match in matches)
            {
                if (match != null)
                {
                    // --- СТВОРЕННЯ ВИБУХУ ЧАСТИНОК ---
                    if (explosionPrefab != null)
                    {
                        GameObject exp = Instantiate(explosionPrefab, match.transform.position, Quaternion.identity);

                        // Отримуємо колір на основі спрайту
                        Color effectColor = GetColorFromSprite(match.GetSprite());

                        // Ініціалізуємо скрипт вибуху
                        Explosion explosionScript = exp.GetComponent<Explosion>();
                        if (explosionScript != null)
                        {
                            explosionScript.Init(effectColor);
                        }
                    }
                    // --------------------------------

                    _nodes[match.Row, match.Col] = null;
                    StartCoroutine(ShrinkAndDestroy(match.gameObject));
                }
            }
            StartCoroutine(WaitAndFill());
        }
        else
        {
            _isProcessing = false;
        }
    }

    // Допоміжний метод для визначення кольору вибуху
    private Color GetColorFromSprite(Sprite sprite)
    {
        if (sprite.name.Contains("apple")) return Color.red;
        if (sprite.name.Contains("banana")) return Color.yellow;
        if (sprite.name.Contains("blueberry")) return Color.blue;
        if (sprite.name.Contains("grape")) return new Color(0.5f, 0, 0.5f);
        if (sprite.name.Contains("strawberry")) return Color.red;
        return Color.white;
    }
    private System.Collections.IEnumerator ShrinkAndDestroy(GameObject node)
    {
        float elapsed = 0;
        float duration = 0.2f;
        Vector3 startScale = node.transform.localScale;

        while (elapsed < duration)
        {
            if (node == null) yield break;
            node.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(node);
    }

    private void ShowComboUI()
    {
        // Показуємо комбо, тільки якщо це вже друга або більше серія вибухів
        if (_comboCount > 1 && comboText != null)
        {
            // Прибираємо (+ 1), тепер буде писати реальне значення комбо
            comboText.text = "COMBO x" + _comboCount;

            comboText.gameObject.SetActive(true);
            StopCoroutine("FadeOutComboText");
            StartCoroutine(FadeOutComboText());
        }
    }
    private System.Collections.IEnumerator FadeOutComboText()
    {
        // Ефект "удару" — текст з'являється великим і зменшується
        comboText.transform.localScale = Vector3.one * 1.5f;
        float elapsed = 0;
        float duration = 0.8f;

        while (elapsed < duration)
        {
            // Плавне зменшення масштабу до нормального
            comboText.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, elapsed / duration);

            // Можна додати зникнення (альфа-канал), якщо захочеш пізніше
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // Даємо гравцеві помилуватися написом
        comboText.gameObject.SetActive(false);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + _score;
            StopCoroutine("PunchScoreText");
            StartCoroutine(PunchScoreText());
        }
    }

    private System.Collections.IEnumerator PunchScoreText()
    {
        float duration = 0.1f;
        Vector3 punchScale = new Vector3(1.2f, 1.2f, 1.2f);

        float elapsed = 0;
        while (elapsed < duration)
        {
            scoreText.transform.localScale = Vector3.Lerp(Vector3.one, punchScale, elapsed / duration);
            elapsed += Time.deltaTime; yield return null;
        }
        elapsed = 0;
        while (elapsed < duration)
        {
            scoreText.transform.localScale = Vector3.Lerp(punchScale, Vector3.one, elapsed / duration);
            elapsed += Time.deltaTime; yield return null;
        }
    }

    private void PlayMatchSound()
    {
        if (UIManager.Instance != null && matchSound != null)
        {
            var source = UIManager.Instance.GetComponent<AudioSource>();
            float vol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
            source.pitch = 1f + (_comboCount * 0.1f);
            if (source.pitch > 2f) source.pitch = 2f;
            source.PlayOneShot(matchSound, vol);
            _comboCount++;
        }
    }
    private System.Collections.IEnumerator SwapNodes(NodeController a, NodeController b)
    {
        _isProcessing = true;
        yield return StartCoroutine(AnimateSwap(a, b));

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
        float elapsed = 0; float duration = 0.2f;
        Vector3 posA = a.transform.position; Vector3 posB = b.transform.position;
        while (elapsed < duration)
        {
            if (a == null || b == null) yield break;
            a.transform.position = Vector3.Lerp(posA, posB, elapsed / duration);
            b.transform.position = Vector3.Lerp(posB, posA, elapsed / duration);
            elapsed += Time.deltaTime; yield return null;
        }
        if (a) a.transform.position = posB; if (b) b.transform.position = posA;
    }

    private System.Collections.IEnumerator WaitAndFill()
    {
        yield return new WaitForSeconds(0.25f);
        yield return StartCoroutine(FillHoles());
    }

    private System.Collections.IEnumerator FillHoles()
    {
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
        yield return new WaitForSeconds(0.2f);

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
                    controller.Setup(r, c, GetRandomSprite());
                    _nodes[r, c] = controller;
                    StartCoroutine(MoveNode(controller, GetWorldPosition(r, c)));
                }
            }
        }
        yield return new WaitForSeconds(0.3f);
        CheckForMatches();
    }
    private void OnValidate()
    {
        // Перевіряємо, чи гра запущена і чи сітка вже створена
        if (_nodes != null && Application.isPlaying)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (_nodes[r, c] != null)
                    {
                        // Миттєво переміщуємо фрукт на нову позицію з урахуванням офсету
                        _nodes[r, c].transform.position = GetWorldPosition(r, c);
                    }
                }
            }
        }
    }
    private System.Collections.IEnumerator MoveNode(NodeController node, Vector3 target)
    {
        float elapsed = 0; float duration = 0.2f;
        Vector3 startPos = node.transform.position;
        while (elapsed < duration)
        {
            if (node == null) yield break;
            node.transform.position = Vector3.Lerp(startPos, target, elapsed / duration);
            elapsed += Time.deltaTime; yield return null;
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
                Sprite cur = _nodes[r, c].GetSprite();
                if (c < columns - 2 && _nodes[r, c + 1] && _nodes[r, c + 2] &&
                    _nodes[r, c + 1].GetSprite() == cur && _nodes[r, c + 2].GetSprite() == cur) return true;
                if (r < rows - 2 && _nodes[r + 1, c] && _nodes[r + 2, c] &&
                    _nodes[r + 1, c].GetSprite() == cur && _nodes[r + 2, c].GetSprite() == cur) return true;
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

    private Vector3 GetWorldPosition(int r, int c)
    {
        // Додаємо gridOffset.x до X та gridOffset.y до Y
        float xPos = ((c - (columns / 2f) + 0.5f) * spacing) + gridOffset.x;
        float yPos = ((r - (rows / 2f) + 0.5f) * spacing) + gridOffset.y;
        return new Vector3(xPos, yPos, 0);
    }

    public void GoToMainMenu() => SceneManager.LoadScene("MainMenu");
}