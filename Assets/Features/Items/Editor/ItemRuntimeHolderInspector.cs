using Features.Items.UnityIntegration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemRuntimeHolder))]
public class ItemRuntimeHolderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Item Setup", EditorStyles.boldLabel);

        if (GUILayout.Button("Add World Item Components"))
        {
            ApplyWorldItem(target as ItemRuntimeHolder);
        }

        if (GUILayout.Button("Add Equip Item Components"))
        {
            ApplyEquipItem(target as ItemRuntimeHolder);
        }
    }

    private void ApplyWorldItem(ItemRuntimeHolder holder)
    {
        var go = holder.gameObject;

        Undo.RegisterFullObjectHierarchyUndo(go, "Apply World Item");

        AddOrGet<BoxCollider>(go, c =>
        {
            c.center = new Vector3(0, -0.2f, 0);
            c.size   = new Vector3(0.27f, 0.98f, 0.7f);
            c.isTrigger = false;
        });

        AddOrGet<SphereCollider>(go, c =>
        {
            c.center = Vector3.zero;
            c.radius = 1.12f;
            c.isTrigger = true;
        });

        AddOrGet<Rigidbody>(go, rb =>
        {
            rb.mass = 1f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.angularDamping = 0.05f;
        });

        AddOrGet<NearbyItemPresenter>(go);

        Debug.Log($"[ItemSetup] World Item applied to {go.name}");
    }

    private void ApplyEquipItem(ItemRuntimeHolder holder)
    {
        var go = holder.gameObject;

        Undo.RegisterFullObjectHierarchyUndo(go, "Apply Equip Item");

        // НИЧЕГО про IUsable
        // Только то, что реально нужно

        if (go.GetComponent<DrillToolPresenter>() == null &&
            go.GetComponent<ScannerTool>() == null &&
            go.GetComponent<ThrowablePresenter>() == null)
        {
            Debug.LogWarning(
                $"[ItemSetup] No specific Equip Presenter found for {go.name}"
            );
        }

        Debug.Log($"[ItemSetup] Equip Item setup checked for {go.name}");
    }

    private T AddOrGet<T>(GameObject go, System.Action<T> setup = null)
        where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null)
            comp = Undo.AddComponent<T>(go);

        setup?.Invoke(comp);
        EditorUtility.SetDirty(comp);
        return comp;
    }
}
