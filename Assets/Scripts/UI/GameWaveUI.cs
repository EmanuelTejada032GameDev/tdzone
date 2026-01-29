using TMPro;
using UnityEngine;

public class GameWaveUI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI waveTitleText;

    void Start()
    {
        WaveSpawner.OnWaveStarted += WaveSpawner_OnWaveStarted;
    }

    private void WaveSpawner_OnWaveStarted(object sender, WaveEventArgs e)
    {
       UpdateWaveTitleText(e.WaveNumber.ToString());
    }

    void Update()
    {
        
    }

    private void UpdateWaveTitleText(string waveNumber)
    {
        waveTitleText.SetText($"Wave {waveNumber}");
    }
}
