using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main menu controller. Lives in the MainMenu scene (between runs).
///
/// Game Flow:
///   1. MainMenu: player sees 3 center buttons (Play, Upgrades, Unlocks) + currency top-right
///   2. Click Upgrades → opens Skill Tree panel (permanent stat bonuses)
///   3. Click Unlocks → opens Unlock Shop panel (unlock/upgrade tower types)
///   4. Click Play → loads GameScene to start a run
///   5. During run: earn currency by killing enemies
///   6. Run ends (win/lose) → earned currency added to persistent balance
///   7. Player returns to MainMenu → spend currency, start new run
/// </summary>
public class ProgressionMenuUI : MonoBehaviour
{
    [Header("Main Buttons")]
    [SerializeField] private GameObject buttonsPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button upgradesButton;
    [SerializeField] private Button unlocksButton;

    [Header("Panels")]
    [SerializeField] private SkillTreeUI skillTreePanel;
    [SerializeField] private UnlockShopUI unlockShopPanel;

    [Header("Back Buttons")]
    [SerializeField] private Button skillTreeBackButton;
    [SerializeField] private Button unlockShopBackButton;

    [Header("Currency")]
    [SerializeField] private TextMeshProUGUI currencyText;

    private void Start()
    {
        // Wire main buttons
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (upgradesButton != null)
        {
            upgradesButton.onClick.AddListener(OnUpgradesClicked);
        }

        if (unlocksButton != null)
        {
            unlocksButton.onClick.AddListener(OnUnlocksClicked);
        }

        // Wire back buttons
        if (skillTreeBackButton != null)
        {
            skillTreeBackButton.onClick.AddListener(CloseCurrentPanel);
        }

        if (unlockShopBackButton != null)
        {
            unlockShopBackButton.onClick.AddListener(CloseCurrentPanel);
        }

        // Subscribe to currency changes
        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
            UpdateCurrencyDisplay(PlayerProgressionManager.Instance.Currency);
        }

        // Start with buttons visible, panels hidden
        ShowMainButtons();
    }

    private void OnDisable()
    {
        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
        }
    }

    private void OnPlayClicked()
    {
        SceneLoader.LoadGameScene();
    }

    private void OnUpgradesClicked()
    {
        if (buttonsPanel != null) buttonsPanel.SetActive(false);
        if (unlockShopPanel != null) unlockShopPanel.Hide();
        if (skillTreePanel != null) skillTreePanel.Show();
    }

    private void OnUnlocksClicked()
    {
        if (buttonsPanel != null) buttonsPanel.SetActive(false);
        if (skillTreePanel != null) skillTreePanel.Hide();
        if (unlockShopPanel != null) unlockShopPanel.Show();
    }

    private void CloseCurrentPanel()
    {
        if (skillTreePanel != null) skillTreePanel.Hide();
        if (unlockShopPanel != null) unlockShopPanel.Hide();
        ShowMainButtons();
    }

    private void ShowMainButtons()
    {
        if (buttonsPanel != null) buttonsPanel.SetActive(true);
        if (skillTreePanel != null) skillTreePanel.Hide();
        if (unlockShopPanel != null) unlockShopPanel.Hide();
    }

    private void UpdateCurrencyDisplay(int amount)
    {
        if (currencyText != null)
        {
            currencyText.SetText(amount.ToString());
        }
    }

    private void OnCurrencyChanged(object sender, int newCurrency)
    {
        UpdateCurrencyDisplay(newCurrency);
    }
}
