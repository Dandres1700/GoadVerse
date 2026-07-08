using UnityEngine;

[DisallowMultipleComponent]
public class MapBoundaryWalls : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private Vector3 center = Vector3.zero;
    [SerializeField] private Vector2 playAreaSize = new Vector2(50f, 50f);
    [SerializeField] private float floorY = 0f;

    [Header("Paredes invisibles")]
    [SerializeField] private float wallHeight = 8f;
    [SerializeField] private float wallThickness = 1f;
    [SerializeField] private PhysicsMaterial wallMaterial = null;
    [SerializeField] private float wallBounciness = 0.5f;

    [Header("Gizmos")]
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.7f, 1f, 0.35f);

    private const string NorthWallName = "InvisibleWall_North";
    private const string SouthWallName = "InvisibleWall_South";
    private const string EastWallName = "InvisibleWall_East";
    private const string WestWallName = "InvisibleWall_West";

    private PhysicsMaterial runtimeWallMaterial;

    private void Awake()
    {
        BuildWalls();
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
        playAreaSize.x = Mathf.Max(1f, playAreaSize.x);
        playAreaSize.y = Mathf.Max(1f, playAreaSize.y);
        wallHeight = Mathf.Max(1f, wallHeight);
        wallThickness = Mathf.Max(0.1f, wallThickness);
        wallBounciness = Mathf.Clamp01(wallBounciness);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;

        Vector3 wallCenter = transform.TransformPoint(new Vector3(center.x, floorY + wallHeight * 0.5f, center.z));
        Vector3 wallSize = new Vector3(playAreaSize.x, wallHeight, playAreaSize.y);

        Gizmos.DrawWireCube(wallCenter, wallSize);
    }
}
