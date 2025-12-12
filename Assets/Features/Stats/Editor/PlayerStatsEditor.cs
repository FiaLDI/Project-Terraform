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
            EditorGUILayout.HelpBox("Enter Play Mode to see live values.", MessageType.Info);
            return;
        }

        if (p.Facade == null)
        {
            EditorGUILayout.HelpBox("Stats not initialized yet.", MessageType.Info);
            return;
        }

        //
        // HEALTH
        //
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Current HP", p.Debug_HP);
        EditorGUILayout.FloatField("Max HP", p.Debug_MaxHP);

        //
        // ENERGY
        //
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Energy", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Current Energy", p.Debug_Energy);
        EditorGUILayout.FloatField("Max Energy", p.Debug_MaxEnergy);
        EditorGUILayout.FloatField("Regen", p.FinalEnergyRegen);

        //
        // COMBAT
        //
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Combat", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Damage Multiplier", p.Debug_DamageMultiplier);
        EditorGUILayout.FloatField("Final Damage", p.FinalDamage);

        //
        // MOVEMENT
        //
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Base Speed", p.BaseSpeed);
        EditorGUILayout.FloatField("Walk Speed", p.WalkSpeed);
        EditorGUILayout.FloatField("Sprint Speed", p.SprintSpeed);
        EditorGUILayout.FloatField("Crouch Speed", p.CrouchSpeed);

        //
        // MINING
        //
        GUILayout.Space(5);
        EditorGUILayout.LabelField("Mining", EditorStyles.boldLabel);
        EditorGUILayout.FloatField("Mining Power", p.MiningSpeed);
    }
}
#endif
