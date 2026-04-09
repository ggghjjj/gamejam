using UnityEngine;
using UnityEngine.UI;

public class GameFlowUI : MonoBehaviour
{
    private GameObject _startPanel;
    private GameObject _gameOverPanel;
    private Text _gameOverStats;

    private void Start()
    {
        BuildStartScreen();
        BuildGameOverScreen();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += ShowGameOver;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= ShowGameOver;
        }
    }

    // ==================== Start Screen ====================
    private void BuildStartScreen()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        _startPanel = CreateFullPanel(canvas.transform, "StartPanel", new Color(0.05f, 0.08f, 0.15f, 0.95f));

        // Title
        CreateLabel(_startPanel.transform, "Title", "Tangy TD",
            new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.8f),
            60, Color.white, FontStyle.Bold);

        // Subtitle
        CreateLabel(_startPanel.transform, "Subtitle", "\u5854\u9632 + \u8089\u9e3d",
            new Vector2(0.2f, 0.48f), new Vector2(0.8f, 0.56f),
            28, new Color(0.7f, 0.7f, 0.7f), FontStyle.Normal);

        // Instructions
        CreateLabel(_startPanel.transform, "Instructions",
            "WASD \u79fb\u52a8\u82f1\u96c4\n\u81ea\u52a8\u653b\u51fb\u8303\u56f4\u5185\u654c\u4eba\n\u6d88\u706d\u654c\u4eba\u83b7\u53d6\u7ecf\u9a8c\uff0c\u5347\u7ea7\u9009\u62e9\u5f3a\u5316",
            new Vector2(0.15f, 0.3f), new Vector2(0.85f, 0.47f),
            22, new Color(0.6f, 0.8f, 1f), FontStyle.Normal);

        // Start button
        CreateButton(_startPanel.transform, "StartBtn",
            "\u5f00\u59cb\u6e38\u620f",
            new Vector2(0.3f, 0.12f), new Vector2(0.7f, 0.25f),
            new Color(0.2f, 0.7f, 0.3f), 32,
            () =>
            {
                _startPanel.SetActive(false);
                GameManager.Instance?.StartGame();
            });

        _startPanel.SetActive(true);
    }

    // ==================== Game Over Screen ====================
    private void BuildGameOverScreen()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        _gameOverPanel = CreateFullPanel(canvas.transform, "GameOverPanel", new Color(0.1f, 0.02f, 0.02f, 0.92f));

        // Title
        CreateLabel(_gameOverPanel.transform, "GOTitle", "\u6e38\u620f\u7ed3\u675f",
            new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.85f),
            56, new Color(1f, 0.3f, 0.3f), FontStyle.Bold);

        // Stats
        _gameOverStats = CreateLabel(_gameOverPanel.transform, "GOStats", "",
            new Vector2(0.15f, 0.35f), new Vector2(0.85f, 0.62f),
            26, Color.white, FontStyle.Normal);

        // Restart button
        CreateButton(_gameOverPanel.transform, "RestartBtn",
            "\u518d\u6765\u4e00\u5c40",
            new Vector2(0.3f, 0.12f), new Vector2(0.7f, 0.25f),
            new Color(0.8f, 0.3f, 0.2f), 32,
            () => GameManager.Instance?.RestartGame());

        _gameOverPanel.SetActive(false);
    }

    private void ShowGameOver()
    {
        if (_gameOverPanel == null) return;

        var gm = GameManager.Instance;
        string stats = "";
        if (gm != null)
        {
            stats = $"\u5b58\u6d3b\u6ce2\u6b21:  {gm.currentWave}\n" +
                    $"\u51fb\u6740\u6570:     {gm.totalKills}\n" +
                    $"\u8fbe\u5230\u7b49\u7ea7:  {gm.level}";
        }
        _gameOverStats.text = stats;

        _gameOverPanel.SetActive(true);
    }

    // ==================== UI Helpers ====================
    private GameObject CreateFullPanel(Transform parent, string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image img = panel.AddComponent<Image>();
        img.color = bgColor;
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    private Text CreateLabel(Transform parent, string name, string content,
        Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color, FontStyle style)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text text = go.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return text;
    }

    private void CreateButton(Transform parent, string name, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color btnColor, int fontSize,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = btnColor;
        Button btn = btnGO.AddComponent<Button>();
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = anchorMin;
        btnRT.anchorMax = anchorMax;
        btnRT.offsetMin = Vector2.zero;
        btnRT.offsetMax = Vector2.zero;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        btn.onClick.AddListener(onClick);
    }
}
