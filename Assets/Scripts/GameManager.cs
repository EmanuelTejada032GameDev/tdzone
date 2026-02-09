using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private bool isGameOver;
    [SerializeField] private bool isVictory;

    [Header("Stats")]
    [SerializeField] private int playerGold = 100;
    [SerializeField] private int playerScore = 0;
    [SerializeField] private int enemiesKilled = 0;

    [Header("References")]
    [SerializeField] private Tower tower;

    public event EventHandler OnGameOver;
    public event EventHandler OnVictory;
    public event EventHandler<int> OnGoldChanged;
    public event EventHandler<int> OnScoreChanged;

    public bool IsGameOver => isGameOver;
    public bool IsVictory => isVictory;
    public int PlayerGold => playerGold;
    public int PlayerScore => playerScore;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Find tower if not assigned
        if (tower == null)
        {
            tower = FindObjectOfType<Tower>();
        }

        if (tower != null)
        {
            HealthSystem towerHealth = tower.GetComponent<HealthSystem>();
            if (towerHealth != null)
            {
                towerHealth.OnDied += TowerHealth_OnDied;
            }
        }

        // Subscribe to enemy events
        Enemy.OnEnemyDestroyed += Enemy_OnEnemyDestroyed;
        Enemy.OnEnemyReachedTower += Enemy_OnEnemyReachedTower;

        WaveSpawner.OnWaveStarted += WaveSpawner_OnWaveStarted;
        WaveSpawner.OnWaveCompleted += WaveSpawner_OnWaveCompleted;
        WaveSpawner.OnAllWavesCompleted += WaveSpawner_OnAllWavesCompleted;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (tower != null)
        {
            HealthSystem towerHealth = tower.GetComponent<HealthSystem>();
            if (towerHealth != null)
            {
                towerHealth.OnDied -= TowerHealth_OnDied;
            }
        }

        Enemy.OnEnemyDestroyed -= Enemy_OnEnemyDestroyed;
        Enemy.OnEnemyReachedTower -= Enemy_OnEnemyReachedTower;

        WaveSpawner.OnWaveStarted -= WaveSpawner_OnWaveStarted;
        WaveSpawner.OnWaveCompleted -= WaveSpawner_OnWaveCompleted;
        WaveSpawner.OnAllWavesCompleted -= WaveSpawner_OnAllWavesCompleted;
    }

    private void WaveSpawner_OnWaveStarted(object sender, WaveEventArgs e)
    {
    }

    private void WaveSpawner_OnWaveCompleted(object sender, WaveEventArgs e)
    {
    }

    private void WaveSpawner_OnAllWavesCompleted(object sender, System.EventArgs e)
    {
        Victory();
    }

    private void TowerHealth_OnDied(object sender, EventArgs e)
    {
        GameOver();
    }

    private void Enemy_OnEnemyDestroyed(object sender, EnemyDestroyedEventArgs e)
    {
        enemiesKilled++;
        AddGold(e.GoldReward);
        AddScore(e.ScoreReward);
    }

    private void Enemy_OnEnemyReachedTower(object sender, EventArgs e)
    {
    }

    private void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Time.timeScale = 0f;

        OnGameOver?.Invoke(this, EventArgs.Empty);
    }

    private void Victory()
    {
        if (isGameOver) return;

        isGameOver = true;
        isVictory = true;
        Time.timeScale = 0f;

        OnVictory?.Invoke(this, EventArgs.Empty);
    }

    public void AddGold(int amount)
    {
        playerGold += amount;
        OnGoldChanged?.Invoke(this, playerGold);
    }

    public void AddScore(int amount)
    {
        playerScore += amount;
        OnScoreChanged?.Invoke(this, playerScore);
    }

    public void RestartGame()
    {
        SceneLoader.LoadGameScene();
    }
}