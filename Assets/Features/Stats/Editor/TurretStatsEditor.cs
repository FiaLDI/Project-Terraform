#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Features.Stats.UnityIntegration.TurretStats))]
public class TurretStatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = (Features.Stats.UnityIntegration.TurretStats)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("===== DEBUG STATS =====", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Run Play mode to see live values", MessageType.Info);
            return;
        }

        EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("HP", t.Debug_HP);
        EditorGUILayout.FloatField("Max HP", t.Debug_MaxHP);

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Damage Multiplier", t.Debug_DamageMultiplier);
        EditorGUILayout.FloatField("Final Damage", t.FinalDamage);

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Movement / Rotation", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Base Rotation Speed", t.Debug_RotationSpeed);
        EditorGUILayout.FloatField("Final Rotation Speed", t.FinalRotationSpeed);

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Fire Rate", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Fire Rate Multiplier", t.FireRateMultiplier);
        EditorGUILayout.FloatField("Final Fire Rate", t.FinalFireRate);
    }
}
#endif
