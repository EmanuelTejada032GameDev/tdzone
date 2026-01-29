using TMPro;
using UnityEngine;

public class GameCurrencyUI : MonoBehaviour
{

    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI goldText;

    void Start()
    {
        gameManager.OnGoldChanged += GameManager_OnGoldChanged;
        UpdateGoldTextVisual(gameManager.PlayerGold.ToString());
    }

    private void GameManager_OnGoldChanged(object sender, int e)
    {
        UpdateGoldTextVisual(e.ToString());
    }

    private void UpdateGoldTextVisual(string newValue)
    {
        goldText.SetText(newValue);
    }

}
