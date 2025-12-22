using Features.Items.UnityIntegration;
using UnityEditor;
using UnityEngine;

public static class ItemSetupMenu
{
    // ===============================
    // WORLD ITEM
    // ===============================

    [MenuItem("Items/World Item/Apply to Selected")]
    private static void ApplyWorldItem()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogWarning("[Items] No GameObject selected");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(go, "Apply World Item");

        AddOrGet<BoxCollider>(go, c =>
        {
            c.center = new Vector3(0, -0.2f, 0);
            c.size = new Vector3(0.27f, 0.98f, 0.7f);
            c.isTrigger = false;
        });

        AddOrGet<SphereCollider>(go, c =>
        {
            c.isTrigger = true;
            c.radius = 1.12f;
            c.center = Vector3.zero;
        });

        AddOrGet<Rigidbody>(go, rb =>
        {
            rb.mass = 1f;
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.angularDamping = 0.05f;
        });

        AddOrGet<ItemRuntimeHolder>(go);
        AddOrGet<WorldItemNetwork>(go);

        Debug.Log($"[Items] World Item applied to {go.name}");
    }

    // ===============================
    // EQUIP ITEM
    // ===============================

    [MenuItem("Items/Equip Item/Apply to Selected")]
    private static void ApplyEquipItem()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogWarning("[Items] No GameObject selected");
            return;
        }

        Undo.RegisterFullObjectHierarchyUndo(go, "Apply Equip Item");

        AddOrGet<ItemRuntimeHolder>(go);
        AddOrGet<DrillToolPresenter>(go);

        Debug.Log($"[Items] Equip Item applied to {go.name}");
    }

    // ===============================
    // HELPERS
    // ===============================

    private static T AddOrGet<T>(GameObject go, System.Action<T> setup = null)
        where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = Undo.AddComponent<T>(go);
        }

        setup?.Invoke(comp);
        EditorUtility.SetDirty(comp);
        return comp;
    }
}
