using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TalentTreeUI : MonoBehaviour
{
    private GameObject _panel;
    private Text _diamondText;
    private List<TalentNode> _nodes = new List<TalentNode>();

    private struct TalentDef
    {
        public string key, name, desc;
        public Color color;
        public TalentDef(string k, string n, string d, Color c) { key = k; name = n; desc = d; color = c; }
    }

    private static readonly TalentDef[] Talents = new TalentDef[]
    {
        new TalentDef("atk",    "\u653b\u51fb\u529b", "\u82f1\u96c4\u653b\u51fb\u529b +2/\u7ea7", new Color(1f, 0.3f, 0.3f)),
        new TalentDef("atkspd", "\u653b\u901f",   "\u82f1\u96c4\u653b\u901f +5%/\u7ea7",   new Color(1f, 0.8f, 0.2f)),
        new TalentDef("hp",     "\u751f\u547d",   "\u82f1\u96c4\u751f\u547d +10/\u7ea7",  new Color(0.2f, 0.9f, 0.2f)),
        new TalentDef("range",  "\u5c04\u7a0b",   "\u82f1\u96c4\u5c04\u7a0b +0.2/\u7ea7",new Color(0.5f, 1f, 0.5f)),
        new TalentDef("movspd", "\u79fb\u901f",   "\u82f1\u96c4\u79fb\u901f +3%/\u7ea7",  new Color(0.3f, 0.9f, 1f)),
        new TalentDef("crit",   "\u66b4\u51fb",   "\u66b4\u51fb\u7387 +2%/\u7ea7",       new Color(1f, 1f, 0.3f)),
        new TalentDef("gold",   "\u91d1\u5e01",   "\u91d1\u5e01\u6389\u843d +1/\u7ea7",  new Color(1f, 0.85f, 0.2f)),
        new TalentDef("exp",    "\u7ecf\u9a8c",   "\u7ecf\u9a8c\u83b7\u53d6 +5%/\u7ea7", new Color(0.6f, 0.6f, 1f)),
    };

    public void Show(Transform canvasTransform)
    {
        if (_panel != null) { _panel.SetActive(true); UpdateAll(); return; }
        Build(canvasTransform);
    }

    public void Hide()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    private void Build(Transform canvasTransform)
    {
        _panel = new GameObject("TalentPanel");
        _panel.transform.SetParent(canvasTransform, false);
        var img = _panel.AddComponent<Image>();
        img.color = new Color(0.04f, 0.04f, 0.08f, 0.96f);
        var rt = _panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        // Title
        MakeLabel(_panel.transform, "\u5929\u8d4b\u6811", new Vector2(0.3f, 0.88f), new Vector2(0.7f, 0.97f), 36, new Color(1f, 0.85f, 0.3f));

        // Diamond display
        _diamondText = MakeLabel(_panel.transform, "", new Vector2(0.05f, 0.88f), new Vector2(0.3f, 0.97f), 24, new Color(0.5f, 0.8f, 1f));

        // Talent grid: 2 columns x 4 rows
        float startY = 0.78f;
        float rowH = 0.13f;
        for (int i = 0; i < Talents.Length; i++)
        {
            int col = i % 2;
            int row = i / 2;
            float x = col == 0 ? 0.08f : 0.52f;
            float y = startY - row * rowH;
            CreateTalentNode(Talents[i], x, y);
        }

        // Connection lines between rows
        for (int i = 0; i < 3; i++)
        {
            float y = startY - i * rowH - rowH * 0.5f;
            for (int c = 0; c < 2; c++)
            {
                float x = c == 0 ? 0.25f : 0.69f;
                var line = new GameObject("Line");
                line.transform.SetParent(_panel.transform, false);
                var lineImg = line.AddComponent<Image>();
                lineImg.color = new Color(0.2f, 0.5f, 0.3f, 0.4f);
                var lineRT = line.GetComponent<RectTransform>();
                lineRT.anchorMin = new Vector2(x - 0.003f, y);
                lineRT.anchorMax = new Vector2(x + 0.003f, y + rowH * 0.3f);
                lineRT.offsetMin = Vector2.zero; lineRT.offsetMax = Vector2.zero;
            }
        }

        // Buttons
        MakeButton(_panel.transform, "RESET", "\u91cd\u7f6e\u5929\u8d4b",
            new Vector2(0.65f, 0.05f), new Vector2(0.85f, 0.12f),
            new Color(0.7f, 0.2f, 0.2f), 18, () => { PlayerSave.ResetTalents(); UpdateAll(); });

        var panelRef = _panel; // capture for lambda
        MakeButton(_panel.transform, "DONE", "\u5b8c\u6210",
            new Vector2(0.35f, 0.05f), new Vector2(0.55f, 0.12f),
            new Color(0.2f, 0.6f, 0.3f), 18, () => { if (panelRef != null) panelRef.SetActive(false); });

        // Debug: add test diamonds
        MakeButton(_panel.transform, "DEBUG", "+100\u94bb\u77f3(\u6d4b\u8bd5)",
            new Vector2(0.05f, 0.05f), new Vector2(0.3f, 0.12f),
            new Color(0.4f, 0.4f, 0.5f), 14, () => { PlayerSave.AddTestDiamonds(100); UpdateAll(); });

        UpdateAll();
    }

    private void CreateTalentNode(TalentDef def, float x, float y)
    {
        var node = new GameObject($"Talent_{def.key}");
        node.transform.SetParent(_panel.transform, false);
        var nodeImg = node.AddComponent<Image>();
        nodeImg.color = def.color * 0.3f;
        nodeImg.color = new Color(nodeImg.color.r, nodeImg.color.g, nodeImg.color.b, 0.9f);
        var nodeRT = node.GetComponent<RectTransform>();
        nodeRT.anchorMin = new Vector2(x, y);
        nodeRT.anchorMax = new Vector2(x + 0.4f, y + 0.1f);
        nodeRT.offsetMin = Vector2.zero; nodeRT.offsetMax = Vector2.zero;

        // Name + level
        var nameText = MakeLabel(node.transform, $"{def.name} Lv.{PlayerSave.GetTalent(def.key)}",
            new Vector2(0.05f, 0.5f), new Vector2(0.65f, 0.95f), 18, Color.white);

        // Description
        MakeLabel(node.transform, def.desc, new Vector2(0.05f, 0.05f), new Vector2(0.65f, 0.5f), 13, new Color(0.7f, 0.7f, 0.7f));

        // Upgrade button
        MakeButton(node.transform, "Up", "+", new Vector2(0.7f, 0.15f), new Vector2(0.95f, 0.85f),
            def.color * 0.6f, 22, () =>
            {
                if (PlayerSave.Diamonds > 0)
                {
                    PlayerSave.Diamonds--;
                    PlayerSave.SetTalent(def.key, PlayerSave.GetTalent(def.key) + 1);
                    PlayerSave.TotalTalentSpent++;
                    UpdateAll();
                }
            });

        _nodes.Add(new TalentNode { key = def.key, nameText = nameText, def = def });
    }

    private void UpdateAll()
    {
        if (_diamondText != null)
            _diamondText.text = $"\u94bb\u77f3: {PlayerSave.Diamonds}";

        foreach (var n in _nodes)
        {
            if (n.nameText != null)
                n.nameText.text = $"{n.def.name} Lv.{PlayerSave.GetTalent(n.key)}";
        }
    }

    /// <summary>
    /// Apply talent bonuses to hero at game start
    /// </summary>
    public static void ApplyTalentsToHero(HeroController hero)
    {
        if (hero == null) return;
        hero.damage.AddFlat(PlayerSave.GetTalent("atk") * 2f);
        hero.attackSpeed.AddMultiplier(PlayerSave.GetTalent("atkspd") * 5f);
        hero.maxHPStat.AddFlat(PlayerSave.GetTalent("hp") * 10f);
        hero.attackRange.AddFlat(PlayerSave.GetTalent("range") * 0.2f);
        hero.moveSpeedStat.AddMultiplier(PlayerSave.GetTalent("movspd") * 3f);
        hero.currentHP = hero.maxHP;
    }

    // ===== Helpers =====
    private Text MakeLabel(Transform parent, string content, Vector2 amin, Vector2 amax, int size, Color color)
    {
        var go = new GameObject("Label");
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.text = content; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.color = color; t.alignment = TextAnchor.MiddleCenter;
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.offsetMin = r.offsetMax = Vector2.zero;
        return t;
    }

    private void MakeButton(Transform parent, string name, string label, Vector2 amin, Vector2 amax, Color c, int size, UnityEngine.Events.UnityAction act)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = c;
        go.AddComponent<Button>().onClick.AddListener(act);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.offsetMin = r.offsetMax = Vector2.zero;
        var tgo = new GameObject("T");
        tgo.transform.SetParent(go.transform, false);
        var t = tgo.AddComponent<Text>();
        t.text = label; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.color = Color.white; t.fontStyle = FontStyle.Bold; t.alignment = TextAnchor.MiddleCenter;
        var tr = tgo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
    }

    private struct TalentNode
    {
        public string key;
        public Text nameText;
        public TalentDef def;
    }
}
