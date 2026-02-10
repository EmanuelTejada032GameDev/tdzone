using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Persists player progression to a JSON file.
/// Auto-saves on unlock, upgrade, skill purchase, and run end.
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SAVE_FILE_NAME = "save.json";

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (PlayerProgressionManager.Instance == null) return;

        PlayerProgressionManager.Instance.OnTowerUnlocked += HandleAutoSave;
        PlayerProgressionManager.Instance.OnTowerLeveledUp += HandleAutoSave;
        PlayerProgressionManager.Instance.OnSkillPurchased += HandleAutoSave;
        PlayerProgressionManager.Instance.OnRunEnd += HandleAutoSave;
    }

    private void UnsubscribeFromEvents()
    {
        if (PlayerProgressionManager.Instance == null) return;

        PlayerProgressionManager.Instance.OnTowerUnlocked -= HandleAutoSave;
        PlayerProgressionManager.Instance.OnTowerLeveledUp -= HandleAutoSave;
        PlayerProgressionManager.Instance.OnSkillPurchased -= HandleAutoSave;
        PlayerProgressionManager.Instance.OnRunEnd -= HandleAutoSave;
    }

    private void HandleAutoSave(object sender, EventArgs e) => Save();
    private void HandleAutoSave(object sender, string _) => Save();
    private void HandleAutoSave(object sender, TowerLevelUpEventArgs _) => Save();
    private void HandleAutoSave(object sender, SkillPurchasedEventArgs _) => Save();
    private void HandleAutoSave(object sender, RunEndEventArgs _) => Save();

    #region Save / Load / Delete

    public void Save()
    {
        if (PlayerProgressionManager.Instance == null) return;

        var saveData = new SaveData
        {
            progression = PlayerProgressionManager.Instance.GetDataCopy()
        };

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SaveFilePath, json);

        if (debugMode)
        {
            Debug.Log($"[SaveManager] Saved to {SaveFilePath}");
        }
    }

    public SaveData Load()
    {
        if (!File.Exists(SaveFilePath))
        {
            if (debugMode)
            {
                Debug.Log("[SaveManager] No save file found");
            }
            return null;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (debugMode)
            {
                Debug.Log($"[SaveManager] Loaded from {SaveFilePath}");
            }
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to load save: {e.Message}");
            return null;
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(SaveFilePath);
    }

    public void DeleteSave()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);

            if (debugMode)
            {
                Debug.Log("[SaveManager] Save file deleted");
            }
        }
    }

    #endregion

    #region Debug

    [ContextMenu("Debug: Force Save")]
    private void DebugForceSave()
    {
        Save();
        Debug.Log("[SaveManager] Force saved");
    }

    [ContextMenu("Debug: Force Load")]
    private void DebugForceLoad()
    {
        SaveData data = Load();
        if (data != null && PlayerProgressionManager.Instance != null)
        {
            PlayerProgressionManager.Instance.SetData(data.progression);
            Debug.Log($"[SaveManager] Force loaded — Currency: {data.progression.currency}");
        }
    }

    [ContextMenu("Debug: Delete Save")]
    private void DebugDeleteSave()
    {
        DeleteSave();
    }

    [ContextMenu("Debug: Log Save Path")]
    private void DebugLogSavePath()
    {
        Debug.Log($"[SaveManager] Save path: {SaveFilePath}");
        Debug.Log($"[SaveManager] File exists: {HasSaveFile()}");
    }

    #endregion
}
