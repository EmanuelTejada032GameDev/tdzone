using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "TD Zone/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Identity")]
    public string enemyId;
    public string enemyName;
    public Sprite icon;

    [Header("Prefab")]
    [Tooltip("The enemy prefab to instantiate (mesh + Enemy.cs + HealthSystem + collider)")]
    public GameObject enemyPrefab;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float stoppingDistance = 2f;

    [Header("Combat")]
    public int maxHealth = 10;
    public int damage = 2;
    public float attackCooldown = 1f;

    [Header("Rewards")]
    public int goldReward = 10;
    public int scoreReward = 100;
}
