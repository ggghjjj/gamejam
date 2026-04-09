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

        // --- Top center: Enemy count ---
        _waveText = CreateText(root, "EnemyCount", "\u602a\u7269: 0/0", 28,
            new Vector2(0.38f, 0.93f), new Vector2(0.65f, 1f), TextAnchor.MiddleCenter);
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

        // --- Pause button (top right corner) ---
        BuildPauseUI(root);
    }

    private GameObject _pausePanel;

    private void BuildPauseUI(Transform root)
    {
        // Small pause button
        GameObject pauseBtnGO = new GameObject("PauseBtn");
        pauseBtnGO.transform.SetParent(root, false);
        Image pauseImg = pauseBtnGO.AddComponent<Image>();
        pauseImg.color = new Color(0.3f, 0.3f, 0.4f, 0.8f);
        Button pauseBtn = pauseBtnGO.AddComponent<Button>();
        RectTransform pauseRT = pauseBtnGO.GetComponent<RectTransform>();
        pauseRT.anchorMin = new Vector2(0.92f, 0.85f);
        pauseRT.anchorMax = new Vector2(1f, 0.92f);
        pauseRT.offsetMin = Vector2.zero;
        pauseRT.offsetMax = Vector2.zero;

        var pText = new GameObject("T"); pText.transform.SetParent(pauseBtnGO.transform, false);
        Text pt = pText.AddComponent<Text>();
        pt.text = "||"; pt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pt.fontSize = 22; pt.color = Color.white; pt.alignment = TextAnchor.MiddleCenter;
        pt.fontStyle = FontStyle.Bold;
        var ptr = pText.GetComponent<RectTransform>();
        ptr.anchorMin = Vector2.zero; ptr.anchorMax = Vector2.one;
        ptr.offsetMin = ptr.offsetMax = Vector2.zero;

        // Pause overlay panel (hidden)
        _pausePanel = new GameObject("PauseOverlay");
        _pausePanel.transform.SetParent(root, false);
        Image overlayImg = _pausePanel.AddComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.7f);
        var ort = _pausePanel.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero; ort.anchorMax = Vector2.one;
        ort.offsetMin = ort.offsetMax = Vector2.zero;

        // Paused title
        var ptitle = new GameObject("Title"); ptitle.transform.SetParent(_pausePanel.transform, false);
        Text tt = ptitle.AddComponent<Text>();
        tt.text = "\u6682\u505c"; tt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tt.fontSize = 48; tt.color = Color.white; tt.alignment = TextAnchor.MiddleCenter;
        tt.fontStyle = FontStyle.Bold;
        var trt = ptitle.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.2f, 0.55f); trt.anchorMax = new Vector2(0.8f, 0.7f);
        trt.offsetMin = trt.offsetMax = Vector2.zero;

        // Resume button
        CreatePauseButton(_pausePanel.transform, "\u7ee7\u7eed\u6e38\u620f",
            new Vector2(0.25f, 0.35f), new Vector2(0.75f, 0.48f),
            new Color(0.2f, 0.6f, 0.3f), () => {
                _pausePanel.SetActive(false);
                GameManager.Instance?.ResumeGame();
            });

        // Back to menu button
        CreatePauseButton(_pausePanel.transform, "\u8fd4\u56de\u4e3b\u83dc\u5355",
            new Vector2(0.25f, 0.2f), new Vector2(0.75f, 0.33f),
            new Color(0.6f, 0.3f, 0.2f), () => {
                Time.timeScale = 1f;
                GameManager.SkipMenuOnLoad = false;
                GameManager.Instance?.RestartGame();
            });

        _pausePanel.SetActive(false);

        pauseBtn.onClick.AddListener(() => {
            GameManager.Instance?.PauseGame();
            _pausePanel.SetActive(true);
        });
    }

    private void CreatePauseButton(Transform parent, string label, Vector2 amin, Vector2 amax, Color c, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject("Btn"); go.transform.SetParent(parent, false);
        go.AddComponent<Image>().color = c;
        go.AddComponent<Button>().onClick.AddListener(action);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = amin; r.anchorMax = amax; r.offsetMin = r.offsetMax = Vector2.zero;
        var tgo = new GameObject("T"); tgo.transform.SetParent(go.transform, false);
        var t = tgo.AddComponent<Text>();
        t.text = label; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 26; t.color = Color.white; t.fontStyle = FontStyle.Bold; t.alignment = TextAnchor.MiddleCenter;
        var tr = tgo.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one; tr.offsetMin = tr.offsetMax = Vector2.zero;
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
        if (_waveText != null && WaveManager.Instance != null)
        {
            int killed = WaveManager.Instance.enemiesKilledThisLevel;
            int total = WaveManager.Instance.totalEnemiesForLevel;
            _waveText.text = $"\u602a\u7269: {killed}/{total}";
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
