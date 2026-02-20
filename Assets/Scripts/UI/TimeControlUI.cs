using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Two-button time control: one pause/play toggle, one speed cycle (x1 → x2 → x4).
/// Auto-unpauses when speed button is clicked while paused.
/// Disables itself when the game ends (defeat/victory).
/// </summary>
public class TimeControlUI : MonoBehaviour
{
    public static TimeControlUI Instance { get; private set; }

    [Header("Buttons")]
    [SerializeField] private Button pausePlayButton;
    [SerializeField] private Button speedButton;

    [Header("Icons - Pause/Play")]
    [SerializeField] private Image pausePlayIcon;
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite playSprite;

    [Header("Speed Labels")]
    [SerializeField] private TextMeshProUGUI speedLabel;

    private readonly float[] speedSteps = { 1f, 2f, 4f };
    private readonly string[] speedLabels = { "x1", "x2", "x4" };

    private int currentSpeedIndex;
    private bool isPaused;

    public float CurrentTimeScale => isPaused ? 0f : speedSteps[currentSpeedIndex];

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentSpeedIndex = 0;
        isPaused = false;

        if (pausePlayButton != null)
            pausePlayButton.onClick.AddListener(OnPausePlayClicked);

        if (speedButton != null)
            speedButton.onClick.AddListener(OnSpeedClicked);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver += OnGameEnd;
            GameManager.Instance.OnVictory += OnGameEnd;
        }

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver -= OnGameEnd;
            GameManager.Instance.OnVictory -= OnGameEnd;
        }

        if (Instance == this)
            Instance = null;
    }

    private void OnPausePlayClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (isPaused)
            Unpause();
        else
            Pause();
    }

    private void OnSpeedClicked()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        currentSpeedIndex = (currentSpeedIndex + 1) % speedSteps.Length;

        // Auto-unpause when changing speed
        if (isPaused)
            isPaused = false;

        ApplyTimeScale();
        UpdateUI();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        UpdateUI();
    }

    public void Unpause()
    {
        isPaused = false;
        ApplyTimeScale();
        UpdateUI();
    }

    private void ApplyTimeScale()
    {
        Time.timeScale = speedSteps[currentSpeedIndex];
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private void UpdateUI()
    {
        // Pause/Play icon: show pause icon when running, play icon when paused
        if (pausePlayIcon != null)
        {
            pausePlayIcon.sprite = isPaused ? playSprite : pauseSprite;
        }

        // Speed label
        if (speedLabel != null)
        {
            speedLabel.SetText(speedLabels[currentSpeedIndex]);
        }
    }

    private void OnGameEnd(object sender, System.EventArgs e)
    {
        // Game ended — disable buttons, don't override timeScale (GameManager handles it)
        isPaused = false;
        currentSpeedIndex = 0;

        if (pausePlayButton != null) pausePlayButton.interactable = false;
        if (speedButton != null) speedButton.interactable = false;

        UpdateUI();
    }
}
