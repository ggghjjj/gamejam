using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlacementSystem : MonoBehaviour
{
    public static PlacementSystem Instance { get; private set; }

    [Header("Settings")]
    public int maxTowers = 8;
    public float minTowerDistance = 1f;

    [Header("Tower Prices")]
    public int archerPrice = 30;
    public int warriorPrice = 50;
    public int magePrice = 80;

    [Header("Runtime")]
    public int towersPlaced = 0;
    public bool isPlacing = false;
    public int selectedTowerIndex = -1;

    private List<TowerData> _towerTemplates = new List<TowerData>();
    private List<GameObject> _placedTowers = new List<GameObject>();

    // UI
    private GameObject _shopPanel;
    private List<Button> _shopButtons = new List<Button>();
    private List<Text> _shopTexts = new List<Text>();
    private GameObject _previewGO;
    private SpriteRenderer _previewSR;
    private bool _previewValid;

    public System.Action OnTowerPlaced;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildTowerTemplates();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += OnGoldChanged;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= OnGoldChanged;
        }
    }

    private void OnGoldChanged(int gold)
    {
        UpdateShopButtons();
    }

    private void Update()
    {
        if (!isPlacing || _previewGO == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        _previewGO.transform.position = mouseWorld;

        // Check validity and color preview
        _previewValid = IsValidPosition(mouseWorld);
        if (_previewSR != null)
        {
            Color c = _towerTemplates[selectedTowerIndex].color;
            if (_previewValid)
            {
                c.a = 0.5f;
            }
            else
            {
                c = Color.red;
                c.a = 0.5f;
            }
            _previewSR.color = c;
        }

        if (Input.GetMouseButtonDown(0) && _previewValid)
        {
            PlaceTower(mouseWorld);
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacing();
        }
    }

    private bool IsValidPosition(Vector3 pos)
    {
        // Check overlap with obstacles (trees) and existing towers
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, 0.3f);
        foreach (var hit in hits)
        {
            if (hit.gameObject.name.StartsWith("Tree") || hit.GetComponent<TowerBase>() != null)
                return false;
        }

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
        int price = GetPrice(selectedTowerIndex);

        if (!GameManager.Instance.SpendGold(price)) return;

        GameObject towerGO = new GameObject($"Tower_{data.towerName}");
        towerGO.transform.position = pos;
        towerGO.AddComponent<SpriteRenderer>();
        var col = towerGO.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;

        TowerBase tower = towerGO.AddComponent<TowerBase>();
        tower.Init(data);

        _placedTowers.Add(towerGO);
        towersPlaced++;
        SFXManager.PlayBuy();

        CancelPlacing();
        OnTowerPlaced?.Invoke();
        UpdateShopButtons();
    }

    private void CancelPlacing()
    {
        isPlacing = false;
        if (_previewGO != null) { Destroy(_previewGO); _previewGO = null; _previewSR = null; }
    }

    public void StartPlacing(int index)
    {
        CancelPlacing();
        if (towersPlaced >= maxTowers) return;

        int price = GetPrice(index);
        if (GameManager.Instance == null || GameManager.Instance.gold < price) return;

        selectedTowerIndex = index;
        isPlacing = true;

        TowerData data = _towerTemplates[index];
        _previewGO = new GameObject("TowerPreview");
        _previewSR = _previewGO.AddComponent<SpriteRenderer>();
        Color ghost = data.color;
        ghost.a = 0.5f;

        if (data.type == TowerType.Mage)
            _previewSR.sprite = SpriteFactory.CreateCircle(ghost);
        else
            _previewSR.sprite = SpriteFactory.CreateSquare(ghost);

        _previewSR.sortingOrder = 20;
        _previewGO.transform.localScale = Vector3.one * data.spriteScale;
    }

    private int GetPrice(int index)
    {
        switch (index)
        {
            case 0: return archerPrice;
            case 1: return warriorPrice;
            case 2: return magePrice;
            default: return 999;
        }
    }

    // ========== UI: Always-visible shop bar ==========
    public void BuildPlacementUI(Transform canvasTransform)
    {
        _shopPanel = new GameObject("ShopPanel");
        _shopPanel.transform.SetParent(canvasTransform, false);

        Image bg = _shopPanel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);
        RectTransform panelRT = _shopPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.15f, 0f);
        panelRT.anchorMax = new Vector2(0.85f, 0.1f);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        float btnWidth = 0.3f;
        float gap = 0.03f;
        float startX = 0.5f - (_towerTemplates.Count * btnWidth + (_towerTemplates.Count - 1) * gap) / 2f;

        for (int i = 0; i < _towerTemplates.Count; i++)
        {
            float x = startX + i * (btnWidth + gap);
            CreateShopButton(_shopPanel.transform, _towerTemplates[i], i, x, btnWidth);
        }

        _shopPanel.SetActive(true);
    }

    private void CreateShopButton(Transform parent, TowerData data, int index, float x, float width)
    {
        GameObject btnGO = new GameObject($"Shop_{data.towerName}");
        btnGO.transform.SetParent(parent, false);
        Image btnImg = btnGO.AddComponent<Image>();
        Color c = data.color * 0.4f;
        c.a = 0.9f;
        btnImg.color = c;
        Button btn = btnGO.AddComponent<Button>();
        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(x, 0.1f);
        btnRT.anchorMax = new Vector2(x + width, 0.9f);
        btnRT.offsetMin = Vector2.zero;
        btnRT.offsetMax = Vector2.zero;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        int price = GetPrice(index);
        text.text = $"{data.towerName} ({price}G)";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 18;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        int capturedIndex = index;
        btn.onClick.AddListener(() => StartPlacing(capturedIndex));

        _shopButtons.Add(btn);
        _shopTexts.Add(text);
    }

    private void UpdateShopButtons()
    {
        int gold = GameManager.Instance != null ? GameManager.Instance.gold : 0;
        for (int i = 0; i < _shopButtons.Count; i++)
        {
            int price = GetPrice(i);
            bool canAfford = gold >= price && towersPlaced < maxTowers;
            _shopButtons[i].interactable = canAfford;
            if (_shopTexts[i] != null)
            {
                _shopTexts[i].color = canAfford ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }
        }
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
        archer.baseRange = 3f;
        archer.spriteScale = 0.3f;
        _towerTemplates.Add(archer);

        var warrior = ScriptableObject.CreateInstance<TowerData>();
        warrior.towerName = "\u6218\u58eb";
        warrior.type = TowerType.Warrior;
        warrior.color = new Color(0.9f, 0.4f, 0.1f);
        warrior.baseDamage = 15f;
        warrior.baseAttackSpeed = 0.8f;
        warrior.baseRange = 1.5f;
        warrior.spriteScale = 0.35f;
        _towerTemplates.Add(warrior);

        var mage = ScriptableObject.CreateInstance<TowerData>();
        mage.towerName = "\u6cd5\u5e08";
        mage.type = TowerType.Mage;
        mage.color = new Color(0.6f, 0.3f, 0.9f);
        mage.baseDamage = 20f;
        mage.baseAttackSpeed = 0.5f;
        mage.baseRange = 4f;
        mage.spriteScale = 0.3f;
        _towerTemplates.Add(mage);
    }

    public List<GameObject> GetPlacedTowers() => _placedTowers;
}
