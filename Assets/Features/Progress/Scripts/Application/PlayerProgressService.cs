using System;
using System.IO;
using UnityEngine;

public class PlayerProgressService : MonoBehaviour
{
    public static PlayerProgressService Instance { get; private set; }

    private const string FileName = "player_progress.json";

    public PlayerProgressData Data { get; private set; }

    private string FilePath =>
        Path.Combine(Application.persistentDataPath, FileName);

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadOrCreate();
    }

    // --------------------------------------------------------------------
    // LOAD
    // --------------------------------------------------------------------

    public void LoadOrCreate(string defaultClassId = "engineer")
    {
        if (File.Exists(FilePath))
        {
            string json = File.ReadAllText(FilePath);
            Data = JsonUtility.FromJson<PlayerProgressData>(json);

            if (Data == null)
            {
                Debug.LogWarning("[Progress] Corrupted save. Recreating...");
                CreateNew(defaultClassId);
            }
        }
        else
        {
            CreateNew(defaultClassId);
        }
    }

    private void CreateNew(string defaultClassId)
    {
        Data = new PlayerProgressData();

        var newChar = new PlayerCharacterState
        {
            characterId = Guid.NewGuid().ToString(),
            nickname = "Rookie",
            classId = defaultClassId,
            level = 1,
            visualPresetId = defaultClassId + "_default"
        };

        Data.profile.characters.Add(newChar);
        Data.profile.activeCharacterIndex = 0;

        Save();
    }

    // --------------------------------------------------------------------
    // SAVE
    // --------------------------------------------------------------------

    public void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(Data, true);
            File.WriteAllText(FilePath, json);
            Debug.Log("[Progress] Saved");
        }
        catch (Exception e)
        {
            Debug.LogError("[Progress] Save failed: " + e);
        }
    }

    // --------------------------------------------------------------------
    // CHARACTER MANAGEMENT
    // --------------------------------------------------------------------

    public PlayerCharacterState GetActiveCharacter()
    {
        return Data.profile.GetActiveCharacter();
    }

    public void SelectCharacter(int index)
    {
        Data.profile.activeCharacterIndex = index;
        Save();
    }

    public PlayerCharacterState AddCharacter(string classId, string nickname)
    {
        var newChar = new PlayerCharacterState
        {
            characterId = Guid.NewGuid().ToString(),
            classId = classId,
            nickname = nickname,
            level = 1,
            visualPresetId = classId + "_default"
        };

        Data.profile.characters.Add(newChar);
        Data.profile.activeCharacterIndex = Data.profile.characters.Count - 1;

        Save();

        return newChar;
    }

    public void DeleteCharacter(int index)
    {
        var profile = Data.profile;

        if (index < 0 || index >= profile.characters.Count)
            return;

        profile.characters.RemoveAt(index);

        profile.activeCharacterIndex = profile.characters.Count > 0 ? 0 : -1;

        Save();
    }
}
