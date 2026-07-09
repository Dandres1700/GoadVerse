using UnityEngine;

[DisallowMultipleComponent]
public class MapBoundaryWalls : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private Vector3 center = Vector3.zero;
    [SerializeField] private Vector2 playAreaSize = new Vector2(50f, 50f);
    [SerializeField] private float floorY = 0f;
    [SerializeField] private bool fitToRendererOnAwake = true;
    [SerializeField] private string boundsRendererObjectName = "st081_pitch_0";
    [SerializeField] private Vector2 boundsPadding = Vector2.zero;

    [Header("Paredes invisibles")]
    [SerializeField] private float wallHeight = 8f;
    [SerializeField] private float wallThickness = 1f;
    [SerializeField] private PhysicsMaterial wallMaterial = null;
    [SerializeField] private float wallBounciness = 0.5f;

    [Header("Arcos")]
    [SerializeField] private bool openGoalMouthsOnEastWest = true;
    [SerializeField] private float goalCenterZ = -14.4f;
    [SerializeField] private float goalOpeningWidth = 14f;

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.7f, 1f, 0.35f);

    private const string NorthWallName = "InvisibleWall_North";
    private const string SouthWallName = "InvisibleWall_South";
    private const string EastWallName = "InvisibleWall_East";
    private const string WestWallName = "InvisibleWall_West";
    private const string EastWallLowerName = "InvisibleWall_East_Lower";
    private const string EastWallUpperName = "InvisibleWall_East_Upper";
    private const string WestWallLowerName = "InvisibleWall_West_Lower";
    private const string WestWallUpperName = "InvisibleWall_West_Upper";

    private PhysicsMaterial runtimeWallMaterial;

    private void Awake()
    {
        ClampValues();
        FitToRendererBounds();
        BuildWalls();
    }

    private void FitToRendererBounds()
    {
        if (!fitToRendererOnAwake || string.IsNullOrEmpty(boundsRendererObjectName))
        {
            return;
        }

        GameObject boundsObject = GameObject.Find(boundsRendererObjectName);

        if (boundsObject == null)
        {
            return;
        }

        Renderer boundsRenderer = boundsObject.GetComponent<Renderer>();

        if (boundsRenderer == null)
        {
            boundsRenderer = boundsObject.GetComponentInChildren<Renderer>();
        }

        if (boundsRenderer == null)
        {
            return;
        }

        Bounds bounds = boundsRenderer.bounds;
        Vector3 localCenter = transform.InverseTransformPoint(new Vector3(bounds.center.x, floorY, bounds.center.z));

        center = localCenter;
        playAreaSize = new Vector2(
            Mathf.Max(1f, bounds.size.x + boundsPadding.x),
            Mathf.Max(1f, bounds.size.z + boundsPadding.y)
        );
    }

    public void BuildWalls()
    {
        float halfWidth = playAreaSize.x * 0.5f;
        float halfDepth = playAreaSize.y * 0.5f;
        float wallY = floorY + wallHeight * 0.5f;

        CreateOrUpdateWall(
            NorthWallName,
            new Vector3(center.x, wallY, center.z + halfDepth + wallThickness * 0.5f),
            new Vector3(playAreaSize.x + wallThickness * 2f, wallHeight, wallThickness)
        );

        CreateOrUpdateWall(
            SouthWallName,
            new Vector3(center.x, wallY, center.z - halfDepth - wallThickness * 0.5f),
            new Vector3(playAreaSize.x + wallThickness * 2f, wallHeight, wallThickness)
        );

        if (openGoalMouthsOnEastWest)
        {
            RemoveWall(EastWallName);
            RemoveWall(WestWallName);

            CreateGoalSideWalls(
                EastWallLowerName,
                EastWallUpperName,
                center.x + halfWidth + wallThickness * 0.5f,
                wallY,
                halfDepth
            );

            CreateGoalSideWalls(
                WestWallLowerName,
                WestWallUpperName,
                center.x - halfWidth - wallThickness * 0.5f,
                wallY,
                halfDepth
            );
        }
        else
        {
            RemoveWall(EastWallLowerName);
            RemoveWall(EastWallUpperName);
            RemoveWall(WestWallLowerName);
            RemoveWall(WestWallUpperName);

            CreateOrUpdateWall(
                EastWallName,
                new Vector3(center.x + halfWidth + wallThickness * 0.5f, wallY, center.z),
                new Vector3(wallThickness, wallHeight, playAreaSize.y + wallThickness * 2f)
            );

            CreateOrUpdateWall(
                WestWallName,
                new Vector3(center.x - halfWidth - wallThickness * 0.5f, wallY, center.z),
                new Vector3(wallThickness, wallHeight, playAreaSize.y + wallThickness * 2f)
            );
        }
    }

    private void CreateGoalSideWalls(string lowerWallName, string upperWallName, float wallX, float wallY, float halfDepth)
    {
        float minZ = center.z - halfDepth;
        float maxZ = center.z + halfDepth;
        float halfGoal = goalOpeningWidth * 0.5f;
        float gapMinZ = Mathf.Clamp(goalCenterZ - halfGoal, minZ, maxZ);
        float gapMaxZ = Mathf.Clamp(goalCenterZ + halfGoal, minZ, maxZ);

        CreateWallSegment(lowerWallName, wallX, wallY, minZ, gapMinZ);
        CreateWallSegment(upperWallName, wallX, wallY, gapMaxZ, maxZ);
    }

    private void CreateWallSegment(string wallName, float wallX, float wallY, float startZ, float endZ)
    {
        float length = Mathf.Abs(endZ - startZ);

        if (length <= 0.1f)
        {
            RemoveWall(wallName);
            return;
        }

        CreateOrUpdateWall(
            wallName,
            new Vector3(wallX, wallY, (startZ + endZ) * 0.5f),
            new Vector3(wallThickness, wallHeight, length)
        );
    }

    private void RemoveWall(string wallName)
    {
        Transform wallTransform = transform.Find(wallName);

        if (wallTransform == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(wallTransform.gameObject);
        }
        else
        {
            DestroyImmediate(wallTransform.gameObject);
        }
    }

    private void CreateOrUpdateWall(string wallName, Vector3 localPosition, Vector3 colliderSize)
    {
        Transform wallTransform = transform.Find(wallName);

        if (wallTransform == null)
        {
            GameObject wallObject = new GameObject(wallName);
            wallTransform = wallObject.transform;
            wallTransform.SetParent(transform, false);
        }

        wallTransform.localPosition = localPosition;
        wallTransform.localRotation = Quaternion.identity;
        wallTransform.localScale = Vector3.one;

        BoxCollider wallCollider = wallTransform.GetComponent<BoxCollider>();

        if (wallCollider == null)
        {
            wallCollider = wallTransform.gameObject.AddComponent<BoxCollider>();
        }

        wallCollider.isTrigger = false;
        wallCollider.center = Vector3.zero;
        wallCollider.size = colliderSize;
        wallCollider.material = GetWallMaterial();
    }

    private PhysicsMaterial GetWallMaterial()
    {
        if (wallMaterial != null)
        {
            return wallMaterial;
        }

        if (runtimeWallMaterial == null)
        {
            runtimeWallMaterial = new PhysicsMaterial("Runtime Boundary Bounce")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = wallBounciness,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Maximum
            };
        }

        runtimeWallMaterial.bounciness = wallBounciness;
        return runtimeWallMaterial;
    }

    private void OnValidate()
    {
        ClampValues();
    }

    private void ClampValues()
    {
        playAreaSize.x = Mathf.Max(1f, Mathf.Abs(playAreaSize.x));
        playAreaSize.y = Mathf.Max(1f, Mathf.Abs(playAreaSize.y));
        wallHeight = Mathf.Max(1f, Mathf.Abs(wallHeight));
        wallThickness = Mathf.Max(0.1f, Mathf.Abs(wallThickness));
        wallBounciness = Mathf.Clamp01(wallBounciness);
        goalOpeningWidth = Mathf.Max(0.1f, Mathf.Abs(goalOpeningWidth));
        boundsPadding.x = Mathf.Max(-playAreaSize.x + 1f, boundsPadding.x);
        boundsPadding.y = Mathf.Max(-playAreaSize.y + 1f, boundsPadding.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;

        Vector3 wallCenter = transform.TransformPoint(new Vector3(center.x, floorY + wallHeight * 0.5f, center.z));
        Vector3 wallSize = new Vector3(playAreaSize.x, wallHeight, playAreaSize.y);

        Gizmos.DrawWireCube(wallCenter, wallSize);
    }
}
