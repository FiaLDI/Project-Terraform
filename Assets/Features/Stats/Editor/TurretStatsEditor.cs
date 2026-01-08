#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Features.Stats.UnityIntegration;

[CustomEditor(typeof(TurretStats))]
public class TurretStatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = (TurretStats)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("===== DEBUG STATS =====", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode to view live runtime values.",
                MessageType.Info
            );
            return;
        }

        // =========================
        // HEALTH
        // =========================
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Current HP", t.CurrentHp);
        EditorGUILayout.FloatField("Max HP", t.MaxHp);

        // =========================
        // COMBAT
        // =========================
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Damage Multiplier", t.FinalDamage);

        // =========================
        // MOVEMENT / ROTATION
        // =========================
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Movement / Rotation", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Final Rotation Speed", t.FinalRotationSpeed);

        // =========================
        // FIRE RATE
        // =========================
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Fire Rate", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Final Fire Rate", t.FinalFireRate);
    }
}
#endif
