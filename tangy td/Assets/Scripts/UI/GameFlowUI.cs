using UnityEngine;
using UnityEngine.UI;

public class GameFlowUI : MonoBehaviour
{
    private GameObject _mainMenuPanel;
    private GameObject _levelSelectPanel;
    private GameObject _gameOverPanel;
    private Text _gameOverStats;

    // Level definitions
    private static readonly string[] LevelNames = new string[]
    {
        "\u7eff\u91ce\u68ee\u6797", "\u6c99\u6f20\u53e4\u9053", "\u6708\u5149\u6cb3\u8c37", "\u6bd2\u7259\u6cbc\u6cfd",
        "\u7075\u9b42\u5c71\u8109", "\u7194\u5ca9\u5de8\u53e3", "\u6676\u77f3\u77ff\u6d1e", "\u66b4\u98ce\u5c71\u5d16",
        "\u5e7d\u6697\u5893\u5730", "\u5929\u7a7a\u4e4b\u57ce", "\u9f99\u5ca9\u5821\u5792", "\u6df1\u6e0a\u88c2\u7f1d",
        "\u6c38\u6052\u738b\u5ea7", "\u6df7\u6c8c\u6838\u5fc3", "\u8d85\u8d8a\u65f6\u7a7a"
    };

    private void Start()
    {
        BuildMainMenu();
        BuildLevelSelect();
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

    // ==================== Main Menu ====================
    private void BuildMainMenu()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        _mainMenuPanel = CreateFullPanel(canvas.transform, "MainMenu", new Color(0.04f, 0.06f, 0.12f, 0.97f));

        CreateLabel(_mainMenuPanel.transform, "Title", "Tangy TD",
            new Vector2(0.1f, 0.6f), new Vector2(0.9f, 0.85f),
            64, Color.white, FontStyle.Bold);

        CreateLabel(_mainMenuPanel.transform, "Sub", "\u5854\u9632 + \u8089\u9e3d",
            new Vector2(0.2f, 0.53f), new Vector2(0.8f, 0.6f),
            22, new Color(0.6f, 0.6f, 0.6f), FontStyle.Normal);

        // Level Select button
        CreateButton(_mainMenuPanel.transform, "BtnLevels", "\u5173\u5361\u6311\u6218",
            new Vector2(0.25f, 0.32f), new Vector2(0.75f, 0.45f),
            new Color(0.2f, 0.6f, 0.3f), 30,
            () => { _mainMenuPanel.SetActive(false); _levelSelectPanel.SetActive(true); });

        // Talent Tree button (placeholder)
        CreateButton(_mainMenuPanel.transform, "BtnTalent", "\u5929\u8d4b\u6811",
            new Vector2(0.25f, 0.18f), new Vector2(0.75f, 0.28f),
            new Color(0.5f, 0.3f, 0.7f), 26,
            () => { Debug.Log("\u5929\u8d4b\u6811\u5f85\u5b9e\u73b0"); });

        _mainMenuPanel.SetActive(true);
    }

    // ==================== Level Select (Map-style drag) ====================
    private void BuildLevelSelect()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        _levelSelectPanel = CreateFullPanel(canvas.transform, "LevelSelect", new Color(0.04f, 0.06f, 0.1f, 0.98f));

        // Title bar
        CreateLabel(_levelSelectPanel.transform, "LSTitle", "\u9009\u62e9\u5173\u5361",
            new Vector2(0.25f, 0.91f), new Vector2(0.75f, 0.99f),
            32, new Color(1f, 0.85f, 0.3f), FontStyle.Bold);

        CreateButton(_levelSelectPanel.transform, "BtnBack", "\u8fd4\u56de",
            new Vector2(0.02f, 0.91f), new Vector2(0.15f, 0.99f),
            new Color(0.35f, 0.35f, 0.4f), 18,
            () => { _levelSelectPanel.SetActive(false); _mainMenuPanel.SetActive(true); });

        // Scrollable map area
        GameObject scrollGO = new GameObject("MapScroll");
        scrollGO.transform.SetParent(_levelSelectPanel.transform, false);
        RectTransform scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f);
        scrollRT.anchorMax = new Vector2(1f, 0.9f);
        scrollRT.offsetMin = Vector2.zero;
        scrollRT.offsetMax = Vector2.zero;

        ScrollRect scroll = scrollGO.AddComponent<ScrollRect>();
        Image scrollBG = scrollGO.AddComponent<Image>();
        scrollBG.color = new Color(0.03f, 0.05f, 0.08f, 1f);
        scrollGO.AddComponent<Mask>().showMaskGraphic = true;

        // Content (tall, draggable)
        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(scrollGO.transform, false);
        RectTransform contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);

        float nodeSpacing = 100f;
        float totalHeight = LevelNames.Length * nodeSpacing + 80f;
        contentRT.sizeDelta = new Vector2(0, totalHeight);

        scroll.content = contentRT;
        scroll.vertical = true;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;

        // Generate nodes with zigzag path
        float[] xPositions = new float[LevelNames.Length];
        for (int i = 0; i < LevelNames.Length; i++)
        {
            // Sine wave zigzag
            xPositions[i] = Mathf.Sin(i * 0.8f) * 120f;
        }

        for (int i = 0; i < LevelNames.Length; i++)
        {
            float yPos = -50f - i * nodeSpacing;
            float xPos = xPositions[i];
            int levelIdx = i;

            // Chain line to next
            if (i < LevelNames.Length - 1)
            {
                float nextX = xPositions[i + 1];
                float nextY = -50f - (i + 1) * nodeSpacing;

                GameObject line = new GameObject("Chain");
                line.transform.SetParent(contentGO.transform, false);
                Image lineImg = line.AddComponent<Image>();
                lineImg.color = new Color(0.15f, 0.6f, 0.3f, 0.5f);
                RectTransform lineRT = line.GetComponent<RectTransform>();
                lineRT.anchorMin = new Vector2(0.5f, 0);
                lineRT.anchorMax = new Vector2(0.5f, 0);
                lineRT.pivot = new Vector2(0.5f, 0.5f);

                // Position at midpoint, rotate toward next node
                float midX = (xPos + nextX) / 2f;
                float midY = (yPos + nextY) / 2f;
                lineRT.anchoredPosition = new Vector2(midX, midY);
                float dist = Vector2.Distance(new Vector2(xPos, yPos), new Vector2(nextX, nextY));
                lineRT.sizeDelta = new Vector2(4f, dist);
                float angle = Mathf.Atan2(nextX - xPos, yPos - nextY) * Mathf.Rad2Deg;
                lineRT.localRotation = Quaternion.Euler(0, 0, angle);
            }

            // Circular node
            GameObject nodeGO = new GameObject($"Level_{i}");
            nodeGO.transform.SetParent(contentGO.transform, false);
            Image nodeImg = nodeGO.AddComponent<Image>();

            Color nodeColor;
            if (i < 5) nodeColor = new Color(0.15f, 0.55f, 0.2f, 0.95f);
            else if (i < 10) nodeColor = new Color(0.65f, 0.45f, 0.1f, 0.95f);
            else nodeColor = new Color(0.65f, 0.15f, 0.15f, 0.95f);
            nodeImg.color = nodeColor;

            // Make it circular using a circle sprite as source
            // (Unity UI Image is square by default, but we can use rounded approach)

            Button nodeBtn = nodeGO.AddComponent<Button>();
            RectTransform nodeRT = nodeGO.GetComponent<RectTransform>();
            nodeRT.anchorMin = new Vector2(0.5f, 0);
            nodeRT.anchorMax = new Vector2(0.5f, 0);
            nodeRT.pivot = new Vector2(0.5f, 0.5f);
            nodeRT.anchoredPosition = new Vector2(xPos, yPos);
            nodeRT.sizeDelta = new Vector2(70f, 70f);

            // Level number inside circle
            GameObject numGO = new GameObject("Num");
            numGO.transform.SetParent(nodeGO.transform, false);
            Text numText = numGO.AddComponent<Text>();
            numText.text = $"{i + 1}";
            numText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            numText.fontSize = 24;
            numText.color = Color.white;
            numText.fontStyle = FontStyle.Bold;
            numText.alignment = TextAnchor.MiddleCenter;
            RectTransform numRT = numGO.GetComponent<RectTransform>();
            numRT.anchorMin = Vector2.zero;
            numRT.anchorMax = Vector2.one;
            numRT.offsetMin = Vector2.zero;
            numRT.offsetMax = Vector2.zero;

            // Level name below circle
            GameObject nameGO = new GameObject("Name");
            nameGO.transform.SetParent(nodeGO.transform, false);
            Text nameText = nameGO.AddComponent<Text>();
            nameText.text = LevelNames[i];
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 14;
            nameText.color = new Color(0.8f, 0.8f, 0.8f);
            nameText.alignment = TextAnchor.UpperCenter;
            RectTransform nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0);
            nameRT.anchorMax = new Vector2(1, 0);
            nameRT.pivot = new Vector2(0.5f, 1);
            nameRT.anchoredPosition = new Vector2(0, -5f);
            nameRT.sizeDelta = new Vector2(120f, 20f);

            // Jelly hover animation
            var jelly = nodeGO.AddComponent<JellyHover>();
            jelly.baseSize = 70f;

            nodeBtn.onClick.AddListener(() =>
            {
                GameSetup.CurrentLevel = levelIdx;
                if (GameManager.Instance != null)
                    GameManager.Instance.wavesPerLevel = 5 + levelIdx * 2;
                _levelSelectPanel.SetActive(false);
                _mainMenuPanel.SetActive(false);
                GameManager.Instance?.StartGame();
            });
        }

        _levelSelectPanel.SetActive(false);
    }

    // ==================== Game Over ====================
    private void BuildGameOverScreen()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        _gameOverPanel = CreateFullPanel(canvas.transform, "GameOverPanel", new Color(0.1f, 0.02f, 0.02f, 0.92f));

        CreateLabel(_gameOverPanel.transform, "GOTitle", "\u6e38\u620f\u7ed3\u675f",
            new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.85f),
            56, new Color(1f, 0.3f, 0.3f), FontStyle.Bold);

        _gameOverStats = CreateLabel(_gameOverPanel.transform, "GOStats", "",
            new Vector2(0.15f, 0.35f), new Vector2(0.85f, 0.62f),
            26, Color.white, FontStyle.Normal);

        CreateButton(_gameOverPanel.transform, "RestartBtn", "\u518d\u6765\u4e00\u5c40",
            new Vector2(0.15f, 0.12f), new Vector2(0.48f, 0.25f),
            new Color(0.8f, 0.3f, 0.2f), 28,
            () => GameManager.Instance?.RestartGame());

        CreateButton(_gameOverPanel.transform, "MenuBtn", "\u8fd4\u56de\u4e3b\u83dc\u5355",
            new Vector2(0.52f, 0.12f), new Vector2(0.85f, 0.25f),
            new Color(0.4f, 0.4f, 0.5f), 28,
            () => GameManager.Instance?.RestartGame());

        _gameOverPanel.SetActive(false);
    }

    private void ShowGameOver()
    {
        if (_gameOverPanel == null) return;
        var gm = GameManager.Instance;
        if (gm != null)
        {
            _gameOverStats.text = $"\u5b58\u6d3b\u6ce2\u6b21:  {gm.currentWave}\n\u51fb\u6740\u6570:     {gm.totalKills}\n\u7b49\u7ea7:       {gm.level}";
        }
        _gameOverPanel.SetActive(true);
    }

    // ==================== Helpers ====================
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
