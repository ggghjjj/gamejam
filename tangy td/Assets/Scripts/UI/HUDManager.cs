using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    private Text _livesText;
    private Text _waveText;
    private Text _levelText;
    private Text _goldText;
    private Image _expBarFill;
    private Text _waveAnnounce;
    private float _announceTimer;

    private void Start()
    {
        BuildHUD();

        // Subscribe to events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnExpChanged += UpdateExp;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnGoldChanged += UpdateGold;
        }
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart += OnWaveStart;
            WaveManager.Instance.OnCooldownTick += OnCooldownTick;
        }

        // Initial values
        UpdateLives(GameManager.Instance != null ? GameManager.Instance.playerLives : 0);
        UpdateWaveText();
        UpdateLevelText();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnExpChanged -= UpdateExp;
            GameManager.Instance.OnGameOver -= ShowGameOver;
            GameManager.Instance.OnGoldChanged -= UpdateGold;
        }
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnWaveStart -= OnWaveStart;
            WaveManager.Instance.OnCooldownTick -= OnCooldownTick;
        }
    }

    private void Update()
    {
        UpdateWaveText();
        UpdateLevelText();

        // Fade out wave announcement
        if (_announceTimer > 0f)
        {
            _announceTimer -= Time.deltaTime;
            if (_announceTimer <= 0f && _waveAnnounce != null)
            {
                _waveAnnounce.gameObject.SetActive(false);
            }
        }
    }

    private void BuildHUD()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        Transform root = canvas.transform;

        // --- Top left: Lives ---
        _livesText = CreateText(root, "LivesText", "\u751f\u547d: 30", 28,
            new Vector2(0f, 0.93f), new Vector2(0.2f, 1f), TextAnchor.MiddleLeft);
        _livesText.color = new Color(1f, 0.4f, 0.4f);

        // --- Top: Gold ---
        _goldText = CreateText(root, "GoldText", "\u91d1\u5e01: 0", 28,
            new Vector2(0.2f, 0.93f), new Vector2(0.4f, 1f), TextAnchor.MiddleLeft);
        _goldText.color = new Color(1f, 0.85f, 0.2f);

        // --- Top center: Wave ---
        _waveText = CreateText(root, "WaveText", "\u6ce2\u6b21: 0", 28,
            new Vector2(0.4f, 0.93f), new Vector2(0.65f, 1f), TextAnchor.MiddleCenter);
        _waveText.color = Color.white;

        // --- Top right: Level ---
        _levelText = CreateText(root, "LevelText", "\u7b49\u7ea7: 1", 28,
            new Vector2(0.7f, 0.93f), new Vector2(1f, 1f), TextAnchor.MiddleRight);
        _levelText.color = new Color(0.6f, 0.8f, 1f);

        // --- Exp bar (bottom of screen) ---
        GameObject expBarBG = new GameObject("ExpBarBG");
        expBarBG.transform.SetParent(root, false);
        Image bgImg = expBarBG.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
        RectTransform bgRT = expBarBG.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0.1f, 0f);
        bgRT.anchorMax = new Vector2(0.9f, 0.03f);
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        GameObject expBarFill = new GameObject("ExpBarFill");
        expBarFill.transform.SetParent(expBarBG.transform, false);
        _expBarFill = expBarFill.AddComponent<Image>();
        _expBarFill.color = new Color(0.3f, 0.7f, 1f);
        RectTransform fillRT = expBarFill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = new Vector2(0f, 1f); // starts empty
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // --- Wave announcement (center) ---
        _waveAnnounce = CreateText(root, "WaveAnnounce", "", 48,
            new Vector2(0.2f, 0.45f), new Vector2(0.8f, 0.55f), TextAnchor.MiddleCenter);
        _waveAnnounce.color = Color.yellow;
        _waveAnnounce.fontStyle = FontStyle.Bold;
        _waveAnnounce.gameObject.SetActive(false);
    }

    private void UpdateLives(int lives)
    {
        if (_livesText != null) _livesText.text = $"\u751f\u547d: {lives}";
    }

    private void UpdateGold(int gold)
    {
        if (_goldText != null) _goldText.text = $"\u91d1\u5e01: {gold}";
    }

    private void UpdateExp(int current, int required)
    {
        if (_expBarFill != null)
        {
            float ratio = required > 0 ? (float)current / required : 0f;
            RectTransform rt = _expBarFill.GetComponent<RectTransform>();
            rt.anchorMax = new Vector2(ratio, 1f);
        }
    }

    private void UpdateWaveText()
    {
        if (_waveText != null && GameManager.Instance != null)
        {
            _waveText.text = $"\u6ce2\u6b21: {GameManager.Instance.currentWave}";
        }
    }

    private void UpdateLevelText()
    {
        if (_levelText != null && GameManager.Instance != null)
        {
            _levelText.text = $"\u7b49\u7ea7: {GameManager.Instance.level}";
        }
    }

    private void OnWaveStart(int wave)
    {
        if (_waveAnnounce != null)
        {
            _waveAnnounce.text = $"-- \u7b2c {wave} \u6ce2 --";
            _waveAnnounce.gameObject.SetActive(true);
            _announceTimer = 2f;
        }
    }

    private void OnCooldownTick(float remaining)
    {
        if (_waveAnnounce != null && remaining > 0f)
        {
            _waveAnnounce.text = $"\u4e0b\u4e00\u6ce2: {remaining:F0} \u79d2";
            _waveAnnounce.gameObject.SetActive(true);
            _announceTimer = 1f;
        }
    }

    private void ShowGameOver()
    {
        if (_waveAnnounce != null)
        {
            _waveAnnounce.text = "\u6e38\u620f\u7ed3\u675f";
            _waveAnnounce.color = Color.red;
            _waveAnnounce.fontSize = 60;
            _waveAnnounce.gameObject.SetActive(true);
            _announceTimer = 999f;
        }
    }

    private Text CreateText(Transform parent, string name, string content, int fontSize,
        Vector2 anchorMin, Vector2 anchorMax, TextAnchor alignment)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text text = go.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(10, 0);
        rt.offsetMax = new Vector2(-10, 0);

        return text;
    }
}
