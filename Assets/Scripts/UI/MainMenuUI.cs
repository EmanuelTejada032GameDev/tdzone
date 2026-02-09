using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu controller. Wires the Play button to start a run.
/// Lives in the MainMenu scene alongside ProgressionMenuUI.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
    }

    private void OnPlayClicked()
    {
        SceneLoader.LoadGameScene();
    }
}
