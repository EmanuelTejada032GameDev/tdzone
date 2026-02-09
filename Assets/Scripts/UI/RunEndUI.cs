using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Combined defeat/victory end-of-run screen.
/// Subscribes to PlayerProgressionManager.OnRunEnd to show results.
/// Lives in the GameScene.
/// </summary>
public class RunEndUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject runEndPanel;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI goldEarnedText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    [Header("Title Strings")]
    [SerializeField] private string defeatTitle = "Defeat";
    [SerializeField] private string victoryTitle = "Victory!";

    private void Start()
    {
        if (runEndPanel != null)
        {
            runEndPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuClicked);
        }

        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.OnRunEnd += OnRunEnd;
        }
    }

    private void OnDestroy()
    {
        if (PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.OnRunEnd -= OnRunEnd;
        }
    }

    private void OnRunEnd(object sender, RunEndEventArgs e)
    {
        if (titleText != null)
        {
            titleText.SetText(e.IsVictory ? victoryTitle : defeatTitle);
        }

        if (goldEarnedText != null)
        {
            goldEarnedText.SetText($"+{e.GoldEarned}");
        }

        if (runEndPanel != null)
        {
            runEndPanel.SetActive(true);
        }
    }

    private void OnRestartClicked()
    {
        SceneLoader.LoadGameScene();
    }

    private void OnMenuClicked()
    {
        SceneLoader.LoadMainMenu();
    }
}
