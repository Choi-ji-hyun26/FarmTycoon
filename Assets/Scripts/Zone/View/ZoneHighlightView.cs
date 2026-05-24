using System.Collections.Generic;
using UnityEngine;

public class ZoneHighlightView : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;

    private Material runtimeMaterial;
    private readonly HashSet<ZoneHighlightTarget> inside = new();

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
            runtimeMaterial = targetRenderer.material;

        ApplyNormal();
    }

    private void OnTriggerEnter(Collider other)
    {
        var t = other.GetComponentInParent<ZoneHighlightTarget>();
        if (t == null) return;

        inside.Add(t);
        Refresh();
    }

    private void OnTriggerExit(Collider other)
    {
        var t = other.GetComponentInParent<ZoneHighlightTarget>();
        if (t == null) return;

        inside.Remove(t);
        Refresh();
    }

    private void Refresh()
    {
        if (runtimeMaterial == null) return;
        runtimeMaterial.color = inside.Count > 0 ? activeColor : normalColor;
    }

    private void ApplyNormal()
    {
        if (runtimeMaterial == null) return;
        runtimeMaterial.color = normalColor;
    }
}