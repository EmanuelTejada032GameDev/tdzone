using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Tower Defense/Wave Data")]
public class WaveData : ScriptableObject
{
    [Header("Wave Info")]
    public string waveName = "Wave 1";
    public int waveNumber = 1;

    [Header("Enemy Spawning")]
    public EnemySpawnGroup[] enemyGroups;

    [Header("Timing")]
    [Tooltip("Delay before this wave starts")]
    public float waveStartDelay = 3f;

    [Header("Rewards")]
    public int waveCompleteGoldBonus = 50;
    public int waveCompleteScoreBonus = 500;

    [System.Serializable]
    public class EnemySpawnGroup
    {
        [Tooltip("The enemy prefab to spawn")]
        public GameObject enemyPrefab;

        [Tooltip("How many of this enemy to spawn")]
        public int count = 5;

        [Tooltip("Time between each spawn in this group")]
        public float spawnInterval = 1f;

        [Tooltip("Delay before starting this group")]
        public float groupDelay = 0f;
    }

    public int GetTotalEnemyCount()
    {
        int total = 0;
        foreach (var group in enemyGroups)
        {
            total += group.count;
        }
        return total;
    }
}