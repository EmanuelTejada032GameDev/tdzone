using TMPro;
using UnityEngine;

public class GameScoreUI : MonoBehaviour
{

    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI scoreText;

    void Start()
    {
        gameManager.OnScoreChanged += GameManager_OnScoreChanged;
        UpdateScoreTextVisual(gameManager.PlayerScore.ToString());

    }

    private void GameManager_OnScoreChanged(object sender, int e)
    {
        scoreText.SetText(e.ToString());
    }


    private void UpdateScoreTextVisual(string newValue)
    {
        scoreText.SetText(newValue);
    }
}
