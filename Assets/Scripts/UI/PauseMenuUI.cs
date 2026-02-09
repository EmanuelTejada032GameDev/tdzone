using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game pause menu. ESC toggles pause on/off.
/// Ignores input when the game is already over (defeat/victory screens handle that).
/// Lives in the GameScene.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitToMenuButton;

    private bool isPaused;

    private void Start()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (quitToMenuButton != null)
        {
            quitToMenuButton.onClick.AddListener(QuitToMenu);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // Don't allow pause when game is already over
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void QuitToMenu()
    {
        isPaused = false;
        SceneLoader.LoadMainMenu();
    }
}
