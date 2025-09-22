using UnityEngine;

public class ShowcaseGridSpawner : MonoBehaviour
{
    [Header("What to show (drag your controllers here)")]
    public RuntimeAnimatorController[] controllers;

    [Header("Layout")]
    [Min(1)] public int columns = 3;               // how many per row
    public Vector2 cellSpacing = new Vector2(1.4f, -1.2f);
    public Vector2 startOffset = Vector2.zero;     // top-left cell local offset
    public Vector3 cellScale = Vector3.one * 1.0f; // size of each preview
    public int sortingOrder = 20;                  // render above hearts/board

    [ContextMenu("Build Grid")]
    public void BuildGrid()
    {
        // clear any old children
        for (int i = transform.childCount - 1; i >= 0; --i)
            DestroyImmediate(transform.GetChild(i).gameObject);

        if (controllers == null) return;

        for (int i = 0; i < controllers.Length; i++)
        {
            var ctrl = controllers[i];
            if (ctrl == null) continue;

            int r = i / columns;
            int c = i % columns;

            var go = new GameObject(ctrl.name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition =
                new Vector3(startOffset.x + c * cellSpacing.x,
                            startOffset.y + r * cellSpacing.y, 0f);
            go.transform.localScale = cellScale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sortingOrder;      // ensure on top

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = ctrl; // plays on loop automatically
        }
    }
}
