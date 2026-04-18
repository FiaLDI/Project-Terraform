using UnityEngine;

public static class PersistentIdProvider
{
    private const string Key = "PERSISTENT_ID";

    public static string GetOrCreate()
    {
    #if UNITY_EDITOR
        return System.Guid.NewGuid().ToString();
    #else
        if (PlayerPrefs.HasKey(Key))
            return PlayerPrefs.GetString(Key);

        string id = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString(Key, id);
        PlayerPrefs.Save();
        return id;
    #endif
    }

}
