using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeUI : MonoBehaviour
{
    private GameObject _panel;
    private List<GameObject> _cards = new List<GameObject>();

    private void Start()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeChoicesReady += ShowChoices;
        }
        BuildUI();
        _panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnUpgradeChoicesReady -= ShowChoices;
        }
    }

    private void BuildUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        // Dark overlay panel
        _panel = CreatePanel(canvas.transform, "UpgradePanel");
        RectTransform panelRT = _panel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        Image panelImg = _panel.GetComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.7f);

        // Title
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(_panel.transform, false);
        Text titleText = titleGO.AddComponent<Text>();
        titleText.text = "\u5347\u7ea7\uff01\u8bf7\u9009\u62e9\u4e00\u4e2a\u5f3a\u5316:";
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 28;
        titleText.color = Color.yellow;
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.1f, 0.75f);
        titleRT.anchorMax = new Vector2(0.9f, 0.9f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
    }

    private void ShowChoices(List<UpgradeData> choices)
    {
        // Clear old cards
        foreach (var card in _cards) Destroy(card);
        _cards.Clear();

        // Create 3 cards side by side
        float cardWidth = 0.25f;
        float gap = 0.04f;
        float totalWidth = choices.Count * cardWidth + (choices.Count - 1) * gap;
        float startX = 0.5f - totalWidth / 2f;

        for (int i = 0; i < choices.Count; i++)
        {
            float x = startX + i * (cardWidth + gap);
            GameObject card = CreateCard(choices[i], x, cardWidth);
            _cards.Add(card);
        }

        _panel.SetActive(true);
    }

    private GameObject CreateCard(UpgradeData data, float xAnchor, float width)
    {
        GameObject card = CreatePanel(_panel.transform, $"Card_{data.upgradeName}");
        RectTransform rt = card.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xAnchor, 0.2f);
        rt.anchorMax = new Vector2(xAnchor + width, 0.72f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image cardImg = card.GetComponent<Image>();
        Color bgColor = data.cardColor * 0.3f;
        bgColor.a = 0.9f;
        cardImg.color = bgColor;

        // Color bar at top
        GameObject colorBar = CreatePanel(card.transform, "ColorBar");
        RectTransform barRT = colorBar.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0, 0.85f);
        barRT.anchorMax = Vector2.one;
        barRT.offsetMin = Vector2.zero;
        barRT.offsetMax = Vector2.zero;
        colorBar.GetComponent<Image>().color = data.cardColor;

        // Name
        GameObject nameGO = new GameObject("Name");
        nameGO.transform.SetParent(card.transform, false);
        Text nameText = nameGO.AddComponent<Text>();
        nameText.text = data.upgradeName;
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameText.fontSize = 22;
        nameText.color = Color.white;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.fontStyle = FontStyle.Bold;
        RectTransform nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0.05f, 0.6f);
        nameRT.anchorMax = new Vector2(0.95f, 0.82f);
        nameRT.offsetMin = Vector2.zero;
        nameRT.offsetMax = Vector2.zero;

        // Description
        GameObject descGO = new GameObject("Description");
        descGO.transform.SetParent(card.transform, false);
        Text descText = descGO.AddComponent<Text>();
        descText.text = data.description;
        descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        descText.fontSize = 16;
        descText.color = new Color(0.85f, 0.85f, 0.85f);
        descText.alignment = TextAnchor.UpperCenter;
        RectTransform descRT = descGO.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0.05f, 0.25f);
        descRT.anchorMax = new Vector2(0.95f, 0.58f);
        descRT.offsetMin = Vector2.zero;
        descRT.offsetMax = Vector2.zero;

        // Select button
        GameObject btnGO = new GameObject("SelectBtn");
        btnGO.transform.SetParent(card.transform, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = data.cardColor;
        Button btn = btnGO.AddComponent<Button>();
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.15f, 0.05f);
        btnRT.anchorMax = new Vector2(0.85f, 0.2f);
        btnRT.offsetMin = Vector2.zero;
        btnRT.offsetMax = Vector2.zero;

        GameObject btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        Text btnText = btnTextGO.AddComponent<Text>();
        btnText.text = "\u9009\u62e9";
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 18;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.fontStyle = FontStyle.Bold;
        RectTransform btnTextRT = btnTextGO.GetComponent<RectTransform>();
        btnTextRT.anchorMin = Vector2.zero;
        btnTextRT.anchorMax = Vector2.one;
        btnTextRT.offsetMin = Vector2.zero;
        btnTextRT.offsetMax = Vector2.zero;

        // Button click
        UpgradeData captured = data;
        btn.onClick.AddListener(() =>
        {
            UpgradeManager.Instance?.SelectUpgrade(captured);
            _panel.SetActive(false);
        });

        return card;
    }

    private GameObject CreatePanel(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<Image>();
        return go;
    }
}
