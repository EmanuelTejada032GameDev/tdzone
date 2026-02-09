using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static utility for scene transitions.
/// Centralizes scene names and always resets Time.timeScale before loading
/// to prevent frozen game after pause or game-over.
/// </summary>
public static class SceneLoader
{
    public const string MAIN_MENU_SCENE = "MainMenu";
    public const string GAME_SCENE = "GameScene";

    public static void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MAIN_MENU_SCENE);
    }

    public static void LoadGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GAME_SCENE);
    }
}
