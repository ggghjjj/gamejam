using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlacementSystem : MonoBehaviour
{
    public static PlacementSystem Instance { get; private set; }

    [Header("Settings")]
    public int maxTowers = 5;
    public float minTowerDistance = 1.5f;

    [Header("Runtime")]
    public int towersPlaced = 0;
    public bool isPlacing = false;
    public int selectedTowerIndex = -1;

    private List<TowerData> _towerTemplates = new List<TowerData>();
    private List<GameObject> _placedTowers = new List<GameObject>();

    // UI
    private GameObject _placementPanel;
    private Text _slotText;
    private GameObject _previewGO;

    public System.Action OnTowerPlaced;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildTowerTemplates();
    }

    private void Start()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveComplete += OnWaveComplete;
            WaveManager.Instance.OnWaveStart += OnWaveStart;
        }
    }

    private void OnDestroy()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveComplete -= OnWaveComplete;
            WaveManager.Instance.OnWaveStart -= OnWaveStart;
        }
    }

    private void OnWaveComplete(int wave)
    {
        if (towersPlaced < maxTowers)
        {
            ShowPlacementUI();
        }
    }

    private void OnWaveStart(int wave)
    {
        HidePlacementUI();
        CancelPlacing();
    }

    private void Update()
    {
        if (!isPlacing) return;

        // Preview at mouse
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        if (_previewGO != null)
        {
            _previewGO.transform.position = mouseWorld;
        }

        // Place on click
        if (Input.GetMouseButtonDown(0))
        {
            if (IsValidPosition(mouseWorld))
            {
                PlaceTower(mouseWorld);
            }
        }

        // Cancel on right click
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacing();
        }
    }

    private bool IsValidPosition(Vector3 pos)
    {
        // Check distance from other towers
        foreach (var tower in _placedTowers)
        {
            if (tower != null && Vector2.Distance(tower.transform.position, pos) < minTowerDistance)
                return false;
        }

        // Check within screen bounds
        Camera cam = Camera.main;
        if (cam != null)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            if (Mathf.Abs(pos.x) > halfW - 0.5f || Mathf.Abs(pos.y) > halfH - 0.5f)
                return false;
        }

        return true;
    }

    private void PlaceTower(Vector3 pos)
    {
        if (selectedTowerIndex < 0 || selectedTowerIndex >= _towerTemplates.Count) return;

        TowerData data = _towerTemplates[selectedTowerIndex];

        GameObject towerGO = new GameObject($"Tower_{data.towerName}");
        towerGO.transform.position = pos;
        towerGO.AddComponent<SpriteRenderer>();

        TowerBase tower = towerGO.AddComponent<TowerBase>();
        tower.Init(data);

        _placedTowers.Add(towerGO);
        towersPlaced++;

        CancelPlacing();
        OnTowerPlaced?.Invoke();
        UpdateSlotText();

        if (towersPlaced >= maxTowers)
        {
            HidePlacementUI();
        }
    }

    private void CancelPlacing()
    {
        isPlacing = false;
        if (_previewGO != null) { Destroy(_previewGO); _previewGO = null; }
    }

    public void StartPlacing(int index)
    {
        CancelPlacing();
        if (towersPlaced >= maxTowers) return;

        selectedTowerIndex = index;
        isPlacing = true;

        // Create preview ghost
        TowerData data = _towerTemplates[index];
        _previewGO = new GameObject("TowerPreview");
        var sr = _previewGO.AddComponent<SpriteRenderer>();
        Color ghost = data.color;
        ghost.a = 0.4f;

        if (data.type == TowerType.Mage)
            sr.sprite = SpriteFactory.CreateCircle(ghost);
        else
            sr.sprite = SpriteFactory.CreateSquare(ghost);

        sr.sortingOrder = 20;
        _previewGO.transform.localScale = Vector3.one * data.spriteScale;
    }

    // ========== UI ==========
    public void BuildPlacementUI(Transform canvasTransform)
    {
        _placementPanel = new GameObject("PlacementPanel");
        _placementPanel.transform.SetParent(canvasTransform, false);

        Image bg = _placementPanel.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
        RectTransform panelRT = _placementPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.05f, 0.04f);
        panelRT.anchorMax = new Vector2(0.95f, 0.18f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // Slot text
        GameObject slotGO = new GameObject("SlotText");
        slotGO.transform.SetParent(_placementPanel.transform, false);
        _slotText = slotGO.AddComponent<Text>();
        _slotText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _slotText.fontSize = 18;
        _slotText.color = Color.white;
        _slotText.alignment = TextAnchor.MiddleCenter;
        RectTransform slotRT = slotGO.GetComponent<RectTransform>();
        slotRT.anchorMin = new Vector2(0f, 0.7f);
        slotRT.anchorMax = new Vector2(1f, 1f);
        slotRT.offsetMin = Vector2.zero;
        slotRT.offsetMax = Vector2.zero;

        // Tower buttons
        float btnWidth = 0.28f;
        float gap = 0.04f;
        float startX = 0.5f - (_towerTemplates.Count * btnWidth + (_towerTemplates.Count - 1) * gap) / 2f;

        for (int i = 0; i < _towerTemplates.Count; i++)
        {
            float x = startX + i * (btnWidth + gap);
            CreateTowerButton(_placementPanel.transform, _towerTemplates[i], i, x, btnWidth);
        }

        UpdateSlotText();
        _placementPanel.SetActive(false);
    }

    private void CreateTowerButton(Transform parent, TowerData data, int index, float x, float width)
    {
        GameObject btnGO = new GameObject($"Btn_{data.towerName}");
        btnGO.transform.SetParent(parent, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = data.color * 0.5f;
        btnImg.color = new Color(btnImg.color.r, btnImg.color.g, btnImg.color.b, 0.9f);
        Button btn = btnGO.AddComponent<Button>();
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(x, 0.05f);
        btnRT.anchorMax = new Vector2(x + width, 0.65f);
        btnRT.offsetMin = Vector2.zero;
        btnRT.offsetMax = Vector2.zero;

        // Label
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text = $"{data.towerName}";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        int capturedIndex = index;
        btn.onClick.AddListener(() => StartPlacing(capturedIndex));
    }

    private void UpdateSlotText()
    {
        if (_slotText != null)
            _slotText.text = $"\u653e\u7f6e\u9632\u5fa1\u5854 (\u5269\u4f59 {maxTowers - towersPlaced} \u4e2a\u69fd\u4f4d) - \u53f3\u952e\u53d6\u6d88";
    }

    private void ShowPlacementUI()
    {
        if (_placementPanel != null && towersPlaced < maxTowers)
        {
            _placementPanel.SetActive(true);
            UpdateSlotText();
        }
    }

    private void HidePlacementUI()
    {
        if (_placementPanel != null) _placementPanel.SetActive(false);
    }

    // ========== Tower templates ==========
    private void BuildTowerTemplates()
    {
        _towerTemplates.Clear();

        var archer = ScriptableObject.CreateInstance<TowerData>();
        archer.towerName = "\u5c04\u624b";
        archer.type = TowerType.Archer;
        archer.color = new Color(0.2f, 0.8f, 0.2f);
        archer.baseDamage = 8f;
        archer.baseAttackSpeed = 2f;
        archer.baseRange = 4f;
        archer.spriteScale = 0.45f;
        _towerTemplates.Add(archer);

        var warrior = ScriptableObject.CreateInstance<TowerData>();
        warrior.towerName = "\u6218\u58eb";
        warrior.type = TowerType.Warrior;
        warrior.color = new Color(0.9f, 0.4f, 0.1f);
        warrior.baseDamage = 15f;
        warrior.baseAttackSpeed = 0.8f;
        warrior.baseRange = 2f;
        warrior.spriteScale = 0.55f;
        _towerTemplates.Add(warrior);

        var mage = ScriptableObject.CreateInstance<TowerData>();
        mage.towerName = "\u6cd5\u5e08";
        mage.type = TowerType.Mage;
        mage.color = new Color(0.6f, 0.3f, 0.9f);
        mage.baseDamage = 20f;
        mage.baseAttackSpeed = 0.5f;
        mage.baseRange = 5f;
        mage.spriteScale = 0.5f;
        _towerTemplates.Add(mage);
    }
}
