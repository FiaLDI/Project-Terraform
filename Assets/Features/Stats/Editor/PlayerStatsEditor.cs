#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Features.Stats.UnityIntegration;

[CustomEditor(typeof(PlayerStats))]
public class PlayerStatsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var p = (PlayerStats)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("===== DEBUG STATS =====", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode to see live values.",
                MessageType.Info);
            return;
        }

        if (p == null || p.Facade == null || p.Adapter == null || !p.Adapter.IsReady)
        {
            EditorGUILayout.HelpBox(
                "Stats not initialized yet.",
                MessageType.Info);
            return;
        }

        var stats = p.Facade;

        // =====================================================
        // HEALTH
        // =====================================================

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Current HP", stats.Health.CurrentHp);
        EditorGUILayout.FloatField("Max HP", stats.Health.MaxHp);

        // =====================================================
        // ENERGY
        // =====================================================

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Energy", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Current Energy", stats.Energy.CurrentEnergy);
        EditorGUILayout.FloatField("Max Energy", stats.Energy.MaxEnergy);
        EditorGUILayout.FloatField("Regen", stats.Energy.Regen);
        EditorGUILayout.FloatField("Cost Multiplier", stats.Energy.CostMultiplier);

        // =====================================================
        // COMBAT
        // =====================================================

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);
        EditorGUILayout.FloatField(
            "Damage Multiplier",
            stats.Combat.DamageMultiplier
        );

        // =====================================================
        // MOVEMENT
        // =====================================================

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Base Speed", stats.Movement.BaseSpeed);
        EditorGUILayout.FloatField("Walk Speed", stats.Movement.WalkSpeed);
        EditorGUILayout.FloatField("Sprint Speed", stats.Movement.SprintSpeed);
        EditorGUILayout.FloatField("Crouch Speed", stats.Movement.CrouchSpeed);
        EditorGUILayout.FloatField("Rotation Speed", stats.Movement.RotationSpeed);

        // =====================================================
        // MINING
        // =====================================================

        GUILayout.Space(5);
        EditorGUILayout.LabelField("Mining", EditorStyles.boldLabel);
        EditorGUILayout.FloatField(
            "Mining Power",
            stats.Mining.MiningPower
        );
    }
}
#endif
