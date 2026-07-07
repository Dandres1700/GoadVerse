using UnityEngine;

public class CopaRotator : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetName = "Copa mundial (1)";

    [Header("Rotation")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float degreesPerSecond = 12f;
    [SerializeField] private Space rotationSpace = Space.Self;
    [SerializeField] private bool useRendererCenterAsPivot = true;

    private Vector3 localPivot;
    private bool hasLocalPivot;

    private void Start()
    {
        ResolveTarget();
        CacheLocalPivot();
    }

    private void Update()
    {
        if (target == null)
        {
            ResolveTarget();
            CacheLocalPivot();
        }

        if (target == null) return;

        if (!hasLocalPivot)
        {
            CacheLocalPivot();
        }

        Vector3 axis = rotationAxis.sqrMagnitude > 0.0001f
            ? rotationAxis.normalized
            : Vector3.up;
        Vector3 worldAxis = rotationSpace == Space.Self
            ? target.TransformDirection(axis)
            : axis;
        Vector3 pivot = hasLocalPivot
            ? target.TransformPoint(localPivot)
            : target.position;

        target.RotateAround(pivot, worldAxis.normalized, degreesPerSecond * Time.deltaTime);
    }

    private void ResolveTarget()
    {
        if (target != null) return;
        if (string.IsNullOrWhiteSpace(targetName)) return;

        GameObject foundTarget = GameObject.Find(targetName);

        if (foundTarget != null)
        {
            target = foundTarget.transform;
        }
    }

    private void CacheLocalPivot()
    {
        hasLocalPivot = false;
        localPivot = Vector3.zero;

        if (target == null || !useRendererCenterAsPivot)
        {
            return;
        }

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        localPivot = target.InverseTransformPoint(bounds.center);
        hasLocalPivot = true;
    }

    private void OnValidate()
    {
        degreesPerSecond = Mathf.Max(0f, degreesPerSecond);
    }
}
