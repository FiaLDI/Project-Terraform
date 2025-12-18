using UnityEngine;
using System.Collections.Generic;

public class LeaksQuickCheck : MonoBehaviour
{
    float timer;

    void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer < 5f) return;
        timer = 0f;

        var gos = Resources.FindObjectsOfTypeAll<GameObject>();

        Dictionary<string, int> counts = new();

        foreach (var go in gos)
        {
            string n = go.name;

            if (!counts.ContainsKey(n))
                counts[n] = 0;

            counts[n]++;
        }

        Debug.Log($"[LEAK CHECK] Total GameObjects = {gos.Length}");

        // выводим топ 15
        int printed = 0;
        foreach (var kv in counts)
        {
            if (printed >= 15) break;
            printed++;

            Debug.Log($"[LEAK] {kv.Key}: {kv.Value}");
        }

        Debug.Log("===============================\n");
    }
}
