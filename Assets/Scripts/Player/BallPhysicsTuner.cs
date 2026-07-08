using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class BallPhysicsTuner : MonoBehaviour
{
    [Header("Escala")]
    [SerializeField] private bool forceHalfScale = true;
    [SerializeField] private Vector3 halfScale = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Rebote")]
    [SerializeField] private float bounceForce = 0.5f;
    [SerializeField] private float dynamicFriction;
    [SerializeField] private float staticFriction;

    [Header("Rigidbody")]
    [SerializeField] private float linearDamping;
    [SerializeField] private float angularDamping = 0.05f;

    private PhysicsMaterial runtimeBounceMaterial;

    private void Awake()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (forceHalfScale)
        {
            transform.localScale = halfScale;
        }

        Rigidbody ballRigidbody = GetComponent<Rigidbody>();
        ballRigidbody.linearDamping = linearDamping;
        ballRigidbody.angularDamping = angularDamping;

        PhysicsMaterial bounceMaterial = GetBounceMaterial();
        Collider[] colliders = GetComponents<Collider>();

        foreach (Collider ballCollider in colliders)
        {
            ballCollider.material = bounceMaterial;
        }
    }

    private PhysicsMaterial GetBounceMaterial()
    {
        if (runtimeBounceMaterial == null)
        {
            runtimeBounceMaterial = new PhysicsMaterial("Runtime Ball Bounce")
            {
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Maximum
            };
        }

        runtimeBounceMaterial.dynamicFriction = dynamicFriction;
        runtimeBounceMaterial.staticFriction = staticFriction;
        runtimeBounceMaterial.bounciness = bounceForce;
        return runtimeBounceMaterial;
    }

    private void OnValidate()
    {
        halfScale.x = Mathf.Max(0.01f, halfScale.x);
        halfScale.y = Mathf.Max(0.01f, halfScale.y);
        halfScale.z = Mathf.Max(0.01f, halfScale.z);
        bounceForce = Mathf.Clamp01(bounceForce);
        dynamicFriction = Mathf.Max(0f, dynamicFriction);
        staticFriction = Mathf.Max(0f, staticFriction);
        linearDamping = Mathf.Max(0f, linearDamping);
        angularDamping = Mathf.Max(0f, angularDamping);
    }
}
