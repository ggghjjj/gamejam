using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class VictoryUI : MonoBehaviour
{
    private GameObject _panel;

    private void Start()
    {
        BuildUI();
        _panel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnVictory += ShowVictory;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnVictory -= ShowVictory;
        }
    }

    private void BuildUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        _panel = new GameObject("VictoryPanel");
        _panel.transform.SetParent(canvas.transform, false);
        Image bg = _panel.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.92f);
        RectTransform rt = _panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void ShowVictory()
    {
        // Clear old content (except panel bg)
        for (int i = _panel.transform.childCount - 1; i >= 0; i--)
            Destroy(_panel.transform.GetChild(i).gameObject);

        // Title
        CreateLabel(_panel.transform, "\u901a\u5173\uff01",
            new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.95f),
            48, new Color(1f, 0.85f, 0.2f), FontStyle.Bold);

        // Stats
        var gm = GameManager.Instance;
        string stats = $"\u6ce2\u6b21: {gm.currentWave}  |  \u51fb\u6740: {gm.totalKills}  |  \u7b49\u7ea7: {gm.level}";
        CreateLabel(_panel.transform, stats,
            new Vector2(0.1f, 0.74f), new Vector2(0.9f, 0.82f),
            22, Color.white, FontStyle.Normal);

        // Damage ranking title
        CreateLabel(_panel.transform, "\u4f24\u5bb3\u6392\u540d",
            new Vector2(0.1f, 0.65f), new Vector2(0.9f, 0.73f),
            26, new Color(0.8f, 0.6f, 1f), FontStyle.Bold);

        // Get tower damage rankings
        List<TowerBase> towers = new List<TowerBase>();
        if (PlacementSystem.Instance != null)
        {
            foreach (var go in PlacementSystem.Instance.GetPlacedTowers())
            {
                if (go != null)
                {
                    var tower = go.GetComponent<TowerBase>();
                    if (tower != null) towers.Add(tower);
                }
            }
        }

        // Add hero damage
        var hero = Object.FindAnyObjectByType<HeroController>();

        // Sort by damage
        towers = towers.OrderByDescending(t => t.totalDamageDealt).ToList();

        float yStart = 0.58f;
        float lineHeight = 0.06f;
        int rank = 1;

        // Hero entry
        if (hero != null)
        {
            string heroLine = $"#0 \u82f1\u96c4 (HP: {hero.currentHP:F0}/{hero.maxHP:F0})";
            CreateLabel(_panel.transform, heroLine,
                new Vector2(0.15f, yStart), new Vector2(0.85f, yStart + lineHeight),
                20, new Color(0f, 0.9f, 0.9f), FontStyle.Normal);
            yStart -= lineHeight;
        }

        foreach (var tower in towers)
        {
            Color c = Color.white;
            if (rank == 1) c = new Color(1f, 0.85f, 0.2f); // gold
            else if (rank == 2) c = new Color(0.8f, 0.8f, 0.8f); // silver

            string line = $"#{rank} {tower.towerName} - \u4f24\u5bb3: {tower.totalDamageDealt:F0}";
            CreateLabel(_panel.transform, line,
                new Vector2(0.15f, yStart), new Vector2(0.85f, yStart + lineHeight),
                20, c, FontStyle.Normal);
            yStart -= lineHeight;
            rank++;
        }

        if (towers.Count == 0)
        {
            CreateLabel(_panel.transform, "\u672a\u653e\u7f6e\u82f1\u96c4",
                new Vector2(0.2f, yStart), new Vector2(0.8f, yStart + lineHeight),
                18, new Color(0.6f, 0.6f, 0.6f), FontStyle.Normal);
        }

        // Buttons
        CreateButton(_panel.transform, "\u518d\u6765\u4e00\u5c40",
            new Vector2(0.15f, 0.05f), new Vector2(0.45f, 0.15f),
            new Color(0.7f, 0.3f, 0.2f), () => GameManager.Instance?.RestartGame());

        CreateButton(_panel.transform, "\u4e0b\u4e00\u5173",
            new Vector2(0.55f, 0.05f), new Vector2(0.85f, 0.15f),
            new Color(0.2f, 0.7f, 0.3f), () => {
                GameSetup.CurrentLevel++;
                GameManager.Instance?.RestartGame();
            });

        _panel.SetActive(true);
    }

    private void CreateLabel(Transform parent, string content,
        Vector2 anchorMin, Vector2 anchorMax, int fontSize, Color color, FontStyle style)
    {
        GameObject go = new GameObject("Label");
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
    }

    private void CreateButton(Transform parent, string label,
        Vector2 anchorMin, Vector2 anchorMax, Color btnColor,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGO = new GameObject("Btn");
        btnGO.transform.SetParent(parent, false);
        Image img = btnGO.AddComponent<Image>();
        img.color = btnColor;
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
        text.fontSize = 24;
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
