using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SelectionRingLine : MonoBehaviour
{
    [Header("Aro")]
    [SerializeField] private float radius = 0.75f;
    [SerializeField] private float heightOffset = 0.03f;
    [SerializeField] private int segments = 80;
    [SerializeField] private float lineWidth = 0.06f;
    [SerializeField] private Color ringColor = Color.cyan;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        Material material = new Material(Shader.Find("Sprites/Default"));
        line.material = material;

        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = segments;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.startColor = ringColor;
        line.endColor = ringColor;

        DrawRing();
    }

    private void OnValidate()
    {
        if (line == null)
        {
            line = GetComponent<LineRenderer>();
        }

        if (line == null) return;

        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = segments;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.startColor = ringColor;
        line.endColor = ringColor;

        DrawRing();
    }

    private void DrawRing()
    {
        if (line == null) return;

        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * Mathf.PI * 2f;

            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            line.SetPosition(i, new Vector3(x, heightOffset, z));
        }
    }

    public void SetRingColor(Color color)
    {
        ringColor = color;

        if (line == null)
        {
            line = GetComponent<LineRenderer>();
        }

        if (line == null) return;

        line.startColor = ringColor;
        line.endColor = ringColor;
    }
}
