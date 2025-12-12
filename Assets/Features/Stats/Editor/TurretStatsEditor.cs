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
            EditorGUILayout.HelpBox("Enter Play Mode to view live runtime values.", MessageType.Info);
            return;
        }

        //
        // HEALTH
        //
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Current HP", t.Debug_HP);
        EditorGUILayout.FloatField("Max HP", t.Debug_MaxHP);

        //
        // COMBAT
        //
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Damage Multiplier", t.Debug_DamageMultiplier);
        EditorGUILayout.FloatField("Final Damage", t.FinalDamage);

        //
        // MOVEMENT / ROTATION
        //
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Movement / Rotation", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Final Rotation Speed", t.Debug_RotationSpeed);

        //
        // FIRE RATE
        //
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Fire Rate", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Final Fire Rate", t.FinalFireRate);
    }
}
#endif
