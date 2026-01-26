using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WaveSpawner : MonoBehaviour
{
    [Header("Waves")]
    [SerializeField] private WaveData[] waves;
    [SerializeField] private bool autoStartFirstWave = true;
    [SerializeField] private float timeBetweenWaves = 5f;

    [Header("Spawn Points")]
    [SerializeField] private SpawnPoint[] spawnPoints;
    [SerializeField] private SpawnPointMode spawnMode = SpawnPointMode.Random;

    [Header("Current Wave State Runtime Data")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private int enemiesAlive = 0;
    [SerializeField] private int enemiesSpawnedThisWave = 0;
    [SerializeField] private bool isSpawning = false;
    [SerializeField] private bool allWavesComplete = false;

    private int nextSpawnPointIndex = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();

    public static event EventHandler<WaveEventArgs> OnWaveStarted;
    public static event EventHandler<WaveEventArgs> OnWaveCompleted;
    public static event EventHandler OnAllWavesCompleted;

    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int TotalWaves => waves.Length;
    public bool IsSpawning => isSpawning;
    public bool AllWavesComplete => allWavesComplete;

    private void Start()
    {
        // Find spawn points if not assigned
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("WaveSpawner: No spawn points found! Please add SpawnPoint components to the scene.");
            return;
        }

        // Subscribe to enemy events
        Enemy.OnEnemyDestroyed += Enemy_OnEnemyDestroyed;

        if (autoStartFirstWave)
        {
            StartCoroutine(StartNextWaveWithDelay(2f));
        }
    }

    private void OnDestroy()
    {
        Enemy.OnEnemyDestroyed -= Enemy_OnEnemyDestroyed;
    }

    private void Update()
    {
        // Manual wave start for testing (Press N key)
        if (Input.GetKeyDown(KeyCode.N) && !isSpawning && !allWavesComplete)
        {
            StartNextWave();
        }
    }

    public void StartNextWave()
    {
        if (isSpawning || allWavesComplete) return;

        if (currentWaveIndex >= waves.Length)
        {
            CompleteAllWaves();
            return;
        }

        StartCoroutine(SpawnWave(waves[currentWaveIndex]));
    }

    private IEnumerator StartNextWaveWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        isSpawning = true;
        enemiesSpawnedThisWave = 0;


        // Invoke wave started event
        OnWaveStarted?.Invoke(this, new WaveEventArgs
        {
            WaveNumber = currentWaveIndex + 1,
            WaveName = wave.waveName,
            TotalEnemies = wave.GetTotalEnemyCount()
        });

        // Wait for wave start delay
        yield return new WaitForSeconds(wave.waveStartDelay);

        // Spawn each enemy group
        foreach (var group in wave.enemyGroups)
        {
            // Wait for group delay
            if (group.groupDelay > 0f)
            {
                yield return new WaitForSeconds(group.groupDelay);
            }

            // Spawn enemies in this group
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                enemiesSpawnedThisWave++;

                // Wait before spawning next enemy
                if (i < group.count - 1)
                {
                    yield return new WaitForSeconds(group.spawnInterval);
                }
            }
        }

        isSpawning = false;

        // Wait for all enemies to be defeated
        StartCoroutine(WaitForWaveCompletion(wave));
    }

    private IEnumerator WaitForWaveCompletion(WaveData wave)
    {
        // Wait until all enemies are dead
        while (enemiesAlive > 0)
        {
            yield return new WaitForSeconds(0.5f);
        }


        // Give rewards
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(wave.waveCompleteGoldBonus);
            GameManager.Instance.AddScore(wave.waveCompleteScoreBonus);
        }

        // Invoke wave completed event
        OnWaveCompleted?.Invoke(this, new WaveEventArgs
        {
            WaveNumber = currentWaveIndex + 1,
            WaveName = wave.waveName,
            TotalEnemies = wave.GetTotalEnemyCount()
        });

        currentWaveIndex++;

        // Check if more waves exist
        if (currentWaveIndex >= waves.Length)
        {
            CompleteAllWaves();
        }
        else
        {
            // Start next wave after delay
            StartCoroutine(StartNextWaveWithDelay(timeBetweenWaves));
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("WaveSpawner: Enemy prefab is null!");
            return;
        }

        SpawnPoint spawnPoint = GetNextSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogWarning("WaveSpawner: No valid spawn point!");
            return;
        }

        // Spawn enemy at spawn point
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.Position, spawnPoint.Rotation);
        activeEnemies.Add(enemy);
        enemiesAlive++;

    }

    private SpawnPoint GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return null;

        SpawnPoint selectedPoint = null;

        switch (spawnMode)
        {
            case SpawnPointMode.Random:
                selectedPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
                break;

            case SpawnPointMode.Sequential:
                selectedPoint = spawnPoints[nextSpawnPointIndex];
                nextSpawnPointIndex = (nextSpawnPointIndex + 1) % spawnPoints.Length;
                break;

            case SpawnPointMode.FirstOnly:
                selectedPoint = spawnPoints[0];
                break;
        }

        return selectedPoint;
    }

    private void Enemy_OnEnemyDestroyed(object sender, EnemyDestroyedEventArgs e)
    {
        enemiesAlive--;
        enemiesAlive = Mathf.Max(0, enemiesAlive); // Prevent negative
    }

    private void CompleteAllWaves()
    {
        if (allWavesComplete) return;

        allWavesComplete = true;

        OnAllWavesCompleted?.Invoke(this, EventArgs.Empty);
    }

    public enum SpawnPointMode
    {
        Random,      // Spawn at random spawn points
        Sequential,  // Cycle through spawn points in order
        FirstOnly    // Always use first spawn point
    }

    private void OnDrawGizmosSelected()
    {
        // Draw lines from spawner to spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawLine(transform.position, point.Position);
                }
            }
        }
    }
}

public class WaveEventArgs : EventArgs
{
    public int WaveNumber;
    public string WaveName;
    public int TotalEnemies;
}