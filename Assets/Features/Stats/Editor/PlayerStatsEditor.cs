#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using FishNet.Object;
using Features.Stats.UnityIntegration;
using Features.Stats.Adapter;

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

        if (p == null)
        {
            EditorGUILayout.HelpBox("PlayerStats is null.", MessageType.Warning);
            return;
        }

        // =====================================================
        // SERVER (AUTHORITATIVE DOMAIN)
        // =====================================================
        if (p.IsServerInitialized && p.Facade != null)
        {
            DrawServerStats(p);
            return;
        }

        // =====================================================
        // CLIENT (VIEWMODEL)
        // =====================================================
        var adapter = p.GetComponent<StatsFacadeAdapter>();
        if (adapter == null)
        {
            EditorGUILayout.HelpBox(
                "StatsFacadeAdapter not found (client view not ready).",
                MessageType.Info);
            return;
        }

        DrawClientStats(adapter);
    }

    // =====================================================
    // SERVER VIEW
    // =====================================================

    private void DrawServerStats(PlayerStats p)
    {
        var stats = p.Facade;

        EditorGUILayout.LabelField("SERVER (Authoritative)", EditorStyles.boldLabel);

        // -------- HEALTH --------
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Current HP", stats.Health.CurrentHp);
        EditorGUILayout.FloatField("Max HP", stats.Health.MaxHp);
        EditorGUILayout.FloatField("Regen", stats.Health.FinalRegen);

        // -------- ENERGY --------
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Energy", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Current Energy", stats.Energy.CurrentEnergy);
        EditorGUILayout.FloatField("Max Energy", stats.Energy.MaxEnergy);
        EditorGUILayout.FloatField("Regen", stats.Energy.Regen);
        EditorGUILayout.FloatField("Cost Multiplier", stats.Energy.CostMultiplier);

        // -------- COMBAT --------
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);
        EditorGUILayout.FloatField(
            "Damage Multiplier",
            stats.Combat.DamageMultiplier
        );

        // -------- MOVEMENT --------
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Base Speed", stats.Movement.BaseSpeed);
        EditorGUILayout.FloatField("Walk Speed", stats.Movement.WalkSpeed);
        EditorGUILayout.FloatField("Sprint Speed", stats.Movement.SprintSpeed);
        EditorGUILayout.FloatField("Crouch Speed", stats.Movement.CrouchSpeed);
        EditorGUILayout.FloatField("Rotation Speed", stats.Movement.RotationSpeed);

        // -------- MINING --------
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Mining", EditorStyles.boldLabel);
        EditorGUILayout.FloatField(
            "Mining Power",
            stats.Mining.MiningPower
        );
    }

    // =====================================================
    // CLIENT VIEW
    // =====================================================

    private void DrawClientStats(StatsFacadeAdapter adapter)
    {
        EditorGUILayout.LabelField("CLIENT (ViewModel)", EditorStyles.boldLabel);

        // -------- HEALTH --------
        if (adapter.HealthStats != null && adapter.HealthStats.IsReady)
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
            EditorGUILayout.FloatField("Current HP", adapter.HealthStats.CurrentHp);
            EditorGUILayout.FloatField("Max HP", adapter.HealthStats.MaxHp);
        }

        // -------- ENERGY --------
        if (adapter.EnergyStats != null && adapter.EnergyStats.IsReady)
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Energy", EditorStyles.boldLabel);
            EditorGUILayout.FloatField("Current Energy", adapter.EnergyStats.Current);
            EditorGUILayout.FloatField("Max Energy", adapter.EnergyStats.Max);
            EditorGUILayout.FloatField("Regen (view)", adapter.EnergyStats.Regen);
            EditorGUILayout.FloatField("Cost Mult (view)", adapter.EnergyStats.CostMultiplier);
        }

        // -------- INFO --------
        GUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "Client shows interpolated ViewModel values.\n" +
            "Authoritative values are available only on Server.",
            MessageType.Info);
    }
}
#endif
