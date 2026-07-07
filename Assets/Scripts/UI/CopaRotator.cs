using UnityEngine;

public class CopaRotator : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetName = "Copa mundial (1)";

    [Header("Rotation")]
    [SerializeField] private float degreesPerSecond = 12f;
    [SerializeField] private Vector3 fixedLocalPosition = new Vector3(2.04f, -0.7f, 3.57f);
    [SerializeField] private Vector3 fixedLocalScale = new Vector3(400f, 400f, 450f);
    [SerializeField] private bool forceExactLocalPosition = true;
    [SerializeField] private bool forceExactLocalScale = true;
    [SerializeField] private bool rotateOnlyOnY = true;
    [SerializeField] private Vector3 fixedRootEulerAngles = Vector3.zero;
    [SerializeField] private bool forceRootRotation = true;
    [SerializeField] private bool rotateVisualModel = true;
    [SerializeField] private bool centerVisualChildren = true;

    [Header("Menu Camera")]
    [SerializeField] private bool frameMainCamera = true;
    [SerializeField] private Vector3 menuCameraPosition = new Vector3(0f, 500f, -900f);
    [SerializeField] private Vector3 menuCameraEulerAngles = Vector3.zero;
    [SerializeField] private float menuCameraFarClip = 3000f;

    private float currentYRotation;
    private bool hasCurrentRotation;
    private bool cameraConfigured;
    private Transform visualPivot;
    private Transform rotationTarget;
    private bool rotationTargetReady;

    private void Start()
    {
        ResolveTarget();
        ApplyFixedRootTransform();
        SetupRotationTarget();
        CacheCurrentRotation();
        ConfigureMenuCamera();
    }

    private void LateUpdate()
    {
        if (!cameraConfigured)
        {
            ConfigureMenuCamera();
        }

        if (target == null)
        {
            ResolveTarget();
            CacheCurrentRotation();
        }

        if (target == null) return;

        ApplyFixedRootTransform();
        SetupRotationTarget();

        if (!hasCurrentRotation)
        {
            CacheCurrentRotation();
        }

        Transform rotatingTransform = GetRotationTarget();

        if (rotatingTransform == null) return;

        currentYRotation += degreesPerSecond * Time.deltaTime;
        currentYRotation = Mathf.Repeat(currentYRotation, 360f);

        if (rotateOnlyOnY)
        {
            rotatingTransform.localEulerAngles = new Vector3(0f, currentYRotation, 0f);
        }
        else
        {
            rotatingTransform.localEulerAngles = new Vector3(
                rotatingTransform.localEulerAngles.x,
                currentYRotation,
                rotatingTransform.localEulerAngles.z
            );
        }
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

    private void ApplyFixedRootTransform()
    {
        if (target == null) return;

        if (forceExactLocalPosition)
        {
            target.localPosition = fixedLocalPosition;
        }

        if (forceExactLocalScale)
        {
            target.localScale = fixedLocalScale;
        }

        if (forceRootRotation)
        {
            target.localEulerAngles = fixedRootEulerAngles;
        }
    }

    private void SetupRotationTarget()
    {
        if (target == null || rotationTargetReady) return;

        if (!rotateVisualModel || target.childCount == 0)
        {
            rotationTarget = target;
            rotationTargetReady = true;
            return;
        }

        if (centerVisualChildren)
        {
            CenterDirectVisualChildren();
        }

        Transform[] children = new Transform[target.childCount];

        for (int i = 0; i < target.childCount; i++)
        {
            children[i] = target.GetChild(i);
        }

        Vector3 visualCenter = GetRendererCenter();
        GameObject pivotObject = new GameObject($"{target.name} Visual Pivot");
        visualPivot = pivotObject.transform;
        visualPivot.SetParent(target, false);
        visualPivot.SetPositionAndRotation(visualCenter, target.rotation);

        foreach (Transform child in children)
        {
            if (child != null && child != visualPivot)
            {
                child.SetParent(visualPivot, true);
            }
        }

        rotationTarget = visualPivot;
        rotationTargetReady = true;
    }

    private void CenterDirectVisualChildren()
    {
        for (int i = 0; i < target.childCount; i++)
        {
            Transform child = target.GetChild(i);

            if (child == visualPivot) continue;

            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }
    }

    private Vector3 GetRendererCenter()
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            return target.position;
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.center;
    }

    private Transform GetRotationTarget()
    {
        return rotationTarget != null ? rotationTarget : target;
    }

    private void CacheCurrentRotation()
    {
        hasCurrentRotation = false;

        Transform rotatingTransform = GetRotationTarget();

        if (rotatingTransform == null) return;

        currentYRotation = rotatingTransform.localEulerAngles.y;
        hasCurrentRotation = true;
    }

    private void ConfigureMenuCamera()
    {
        if (!frameMainCamera)
        {
            cameraConfigured = true;
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null) return;

        mainCamera.transform.SetPositionAndRotation(
            menuCameraPosition,
            Quaternion.Euler(menuCameraEulerAngles)
        );
        mainCamera.farClipPlane = menuCameraFarClip;
        cameraConfigured = true;
    }

    private void OnValidate()
    {
        degreesPerSecond = Mathf.Max(0f, degreesPerSecond);
        menuCameraFarClip = Mathf.Max(0.3f, menuCameraFarClip);
    }
}
