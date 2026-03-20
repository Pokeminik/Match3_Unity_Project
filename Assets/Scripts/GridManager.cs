using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    [Header("UI елементи")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private GameObject gameOverPanel; 
    [Header("Налаштування гри")]
    [SerializeField] private int movesLimit = 20;
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

    private int _score = 0;
    private int _comboCount = 0;
    private int _movesLeft;
    private NodeController _firstSelected;
    private NodeController _secondSelected;
    private bool _isProcessing = false;
    private NodeController[,] _nodes;

    public enum BoosterMode { None, Hammer, Bomb, Lightning, Arrow }
    private BoosterMode _currentMode = BoosterMode.None;
    private HashSet<string> _bonusesEarnedThisMove = new HashSet<string>();
    public void SetBoosterMode(BoosterMode mode)
    {
        _currentMode = mode;

        if (_currentMode != BoosterMode.None)
        {
            BoosterManager.Instance.HighlightBooster(_currentMode.ToString());
        }
        else
        {
            BoosterManager.Instance.ResetAllBoosters();
        }
    }
    void Start()
    {
        if (comboText != null) comboText.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        _movesLeft = movesLimit;
        UpdateMovesUI();

        GenerateLevel();
        CheckForMatches();
    }
    private void UpdateMovesUI()
    {
        if (movesText != null) movesText.text = "Moves: " + _movesLeft;

        if (_movesLeft <= 0)
        {
            Debug.Log("Ходи закінчилися! Чекаємо завершення анімацій...");
            StartCoroutine(WaitAndShowGameOver());
        }
    }
    private System.Collections.IEnumerator WaitAndShowGameOver()
    {
        while (_isProcessing)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        if (gameOverPanel != null)
        {
            Debug.Log("Показуємо GameOverPanel!");
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameOverPanel НЕ ПРИВ'ЯЗАНА в інспекторі!");
        }
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
    private void ShowGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }
    public void RestartLevel()
    {
        if (UIManager.Instance != null) UIManager.Instance.PlayClickSound();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

        if (_currentMode != BoosterMode.None)
        {
            _comboCount = 0; 
            ExecuteBooster(node);
            return;
        }


        if (_movesLeft <= 0) return;

        if (_firstSelected == null)
        {
            _bonusesEarnedThisMove.Clear(); 
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
    private void DestroyRow(int r)
    {
        for (int c = 0; c < columns; c++) DestroyNode(GetNodeAt(r, c));
    }

    private void DestroyColumn(int c)
    {
        for (int r = 0; r < rows; r++) DestroyNode(GetNodeAt(r, c));
    }

    private void InstantiateExplosion(Vector3 pos, Sprite sprite)
    {
        GameObject exp = Instantiate(explosionPrefab, pos, Quaternion.identity);
        exp.GetComponent<Explosion>().Init(GetColorFromSprite(sprite));
    }
    private void DestroyNode(NodeController node)
    {
        if (node == null) return;

        _score += 10;
        UpdateScoreUI();

        InstantiateExplosion(node.transform.position, node.GetSprite());
        _nodes[node.Row, node.Col] = null;
        Destroy(node.gameObject);
    }

    private void DestroyArea(int centerX, int centerY, int range)
    {
        for (int r = centerX - range; r <= centerX + range; r++)
        {
            for (int c = centerY - range; c <= centerY + range; c++)
            {
                NodeController target = GetNodeAt(r, c);
                if (target != null) DestroyNode(target);
            }
        }
    }
    public void ExecuteShuffle()
    {
        if (_isProcessing) return;

        if (BoosterManager.Instance.UseBooster("shuffle"))
        {
            StartCoroutine(ShuffleRoutine());
        }
    }
    private System.Collections.IEnumerator ShuffleRoutine()
    {
        _isProcessing = true;

        List<Sprite> activeSprites = new List<Sprite>();
        foreach (NodeController node in _nodes)
        {
            if (node != null) activeSprites.Add(node.GetSprite());
        }

        for (int i = 0; i < activeSprites.Count; i++)
        {
            Sprite temp = activeSprites[i];
            int randomIndex = Random.Range(i, activeSprites.Count);
            activeSprites[i] = activeSprites[randomIndex];
            activeSprites[randomIndex] = temp;
        }

        int index = 0;
        foreach (NodeController node in _nodes)
        {
            if (node != null)
            {
                node.SetSprite(activeSprites[index]);
                index++;
            }
        }

        yield return new WaitForSeconds(0.5f);

        _isProcessing = false;
        CheckForMatches();
    }
    private void ExecuteBooster(NodeController node)
    {
        bool used = false;

        switch (_currentMode)
        {
            case BoosterMode.Hammer:
                if (BoosterManager.Instance.UseBooster("hammer")) { DestroyNode(node); used = true; }
                break;
            case BoosterMode.Bomb:
                if (BoosterManager.Instance.UseBooster("bomb")) { DestroyArea(node.Row, node.Col, 1); used = true; }
                break;
            case BoosterMode.Lightning:
                if (BoosterManager.Instance.UseBooster("lightning")) { DestroyColumn(node.Col); used = true; }
                break;
            case BoosterMode.Arrow:
                if (BoosterManager.Instance.UseBooster("arrow")) { DestroyRow(node.Row); used = true; }
                break;
        }

        if (used)
        {
            _currentMode = BoosterMode.None;
            BoosterManager.Instance.ResetAllBoosters(); 
            StartCoroutine(WaitAndFill());
        }
    }
    private void CheckForMatches()
    {
        HashSet<NodeController> matches = new HashSet<NodeController>();

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

                    int lineLength = 3;
                    if (c + 3 < columns && _nodes[r, c + 3] && _nodes[r, c + 3].GetSprite() == n1.GetSprite())
                    {
                        matches.Add(_nodes[r, c + 3]); lineLength = 4;
                        if (c + 4 < columns && _nodes[r, c + 4] && _nodes[r, c + 4].GetSprite() == n1.GetSprite())
                        {
                            matches.Add(_nodes[r, c + 4]); lineLength = 5;
                        }
                    }

                    if (lineLength == 4 && !_bonusesEarnedThisMove.Contains("arrow"))
                    {
                        BoosterManager.Instance.AddBooster("arrow");
                        _bonusesEarnedThisMove.Add("arrow");
                    }
                    else if (lineLength >= 5 && !_bonusesEarnedThisMove.Contains("shuffle"))
                    {
                        BoosterManager.Instance.AddBooster("shuffle");
                        _bonusesEarnedThisMove.Add("shuffle");
                    }
                }
            }
        }

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

                    int lineLength = 3;
                    if (r + 3 < rows && _nodes[r + 3, c] && _nodes[r + 3, c].GetSprite() == n1.GetSprite())
                    {
                        matches.Add(_nodes[r + 3, c]); lineLength = 4;
                        if (r + 4 < rows && _nodes[r + 4, c] && _nodes[r + 4, c].GetSprite() == n1.GetSprite())
                        {
                            matches.Add(_nodes[r + 4, c]); lineLength = 5;
                        }
                    }

                    if (lineLength == 4 && !_bonusesEarnedThisMove.Contains("lightning"))
                    {
                        BoosterManager.Instance.AddBooster("lightning");
                        _bonusesEarnedThisMove.Add("lightning");
                    }
                    else if (lineLength >= 5 && !_bonusesEarnedThisMove.Contains("shuffle"))
                    {
                        BoosterManager.Instance.AddBooster("shuffle");
                        _bonusesEarnedThisMove.Add("shuffle");
                    }
                }
            }
        }

        for (int r = 0; r < rows - 1; r++)
        {
            for (int c = 0; c < columns - 1; c++)
            {
                NodeController n1 = _nodes[r, c];
                NodeController n2 = _nodes[r + 1, c];
                NodeController n3 = _nodes[r, c + 1];
                NodeController n4 = _nodes[r + 1, c + 1];

                if (n1 && n2 && n3 && n4)
                {
                    Sprite s = n1.GetSprite();
                    if (n2.GetSprite() == s && n3.GetSprite() == s && n4.GetSprite() == s)
                    {
                        matches.Add(n1); matches.Add(n2); matches.Add(n3); matches.Add(n4);
                        if (!_bonusesEarnedThisMove.Contains("bomb"))
                        {
                            BoosterManager.Instance.AddBooster("bomb");
                            _bonusesEarnedThisMove.Add("bomb");
                        }
                    }
                }
            }
        }

        if (_comboCount >= 3 && !_bonusesEarnedThisMove.Contains("hammer"))
        {
            BoosterManager.Instance.AddBooster("hammer");
            _bonusesEarnedThisMove.Add("hammer");
        }

        if (matches.Count > 0)
        {
            _isProcessing = true;
            AudioManager.Instance.PlayMatch(_comboCount);
            _comboCount++;
            ShowComboUI();

            int basePoints = matches.Count * 10;
            _score += basePoints * _comboCount;
            UpdateScoreUI();

            foreach (var match in matches)
            {
                if (match != null)
                {
                    if (explosionPrefab != null)
                    {
                        GameObject exp = Instantiate(explosionPrefab, match.transform.position, Quaternion.identity);
                        Color effectColor = GetColorFromSprite(match.GetSprite());
                        Explosion explosionScript = exp.GetComponent<Explosion>();
                        if (explosionScript != null) explosionScript.Init(effectColor);
                    }

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
    public void OnHoverNode(NodeController node, bool isEntering)
    {
        if (_currentMode == BoosterMode.None) return;

        List<NodeController> targets = new List<NodeController>();

        switch (_currentMode)
        {
            case BoosterMode.Hammer:
                targets.Add(node);
                break;
            case BoosterMode.Bomb:
                targets = GetAreaNodes(node.Row, node.Col, 1);
                break;
            case BoosterMode.Arrow:
                targets = GetRowNodes(node.Row);
                break;
            case BoosterMode.Lightning:
                targets = GetColumnNodes(node.Col);
                break;
        }

        foreach (var target in targets)
        {
            if (target != null) target.Highlight(isEntering);
        }
    }
    private List<NodeController> GetAreaNodes(int r, int c, int range)
    {
        List<NodeController> nodes = new List<NodeController>();
        for (int i = r - range; i <= r + range; i++)
            for (int j = c - range; j <= c + range; j++)
                nodes.Add(GetNodeAt(i, j));
        return nodes;
    }
    private List<NodeController> GetRowNodes(int r)
    {
        List<NodeController> nodes = new List<NodeController>();
        for (int c = 0; c < columns; c++) nodes.Add(GetNodeAt(r, c));
        return nodes;
    }
    private List<NodeController> GetColumnNodes(int c)
    {
        List<NodeController> nodes = new List<NodeController>();
        for (int r = 0; r < rows; r++) nodes.Add(GetNodeAt(r, c));
        return nodes;
    }
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
    public BoosterMode GetCurrentMode()
    {
        return _currentMode;
    }
    private void ShowComboUI()
    {
        if (_comboCount > 1 && comboText != null)
        {
            comboText.text = "COMBO x" + _comboCount;

            comboText.gameObject.SetActive(true);
            StopCoroutine("FadeOutComboText");
            StartCoroutine(FadeOutComboText());
        }
    }
    private System.Collections.IEnumerator FadeOutComboText()
    {
        comboText.transform.localScale = Vector3.one * 1.5f;
        float elapsed = 0;
        float duration = 0.8f;

        while (elapsed < duration)
        {
            comboText.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, elapsed / duration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); 
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
            _movesLeft--;
            UpdateMovesUI();
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
        if (_nodes != null && Application.isPlaying)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    if (_nodes[r, c] != null)
                    {
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
        float xPos = ((c - (columns / 2f) + 0.5f) * spacing) + gridOffset.x;
        float yPos = ((r - (rows / 2f) + 0.5f) * spacing) + gridOffset.y;
        return new Vector3(xPos, yPos, 0);
    }

    public void GoToMainMenu()
    {
        if (UIManager.Instance != null) UIManager.Instance.PlayClickSound();
        Invoke("DelayedLoadMenu", 0.15f);
    }
    private void DelayedLoadMenu() => SceneManager.LoadScene("MainMenu");
}