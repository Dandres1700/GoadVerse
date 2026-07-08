using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FootballOSCinematicCamera : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] private string canchaSceneName = "minijuego1";

    [Header("Objetivos")]
    [SerializeField] private string midfielderName = "Player_Midfielder";
    [SerializeField] private string forwardName = "Player_Forward";
    [SerializeField] private string ballName = "Ball";

    [Header("Ajustes")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotateSpeed = 5f;

    private Transform midfielder;
    private Transform forward;
    private Transform ball;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != canchaSceneName)
        {
            return;
        }

        StartCoroutine(FindTargets());
    }

    private IEnumerator FindTargets()
    {
        while (midfielder == null || forward == null || ball == null)
        {
            GameObject m = GameObject.Find(midfielderName);
            GameObject f = GameObject.Find(forwardName);
            GameObject b = GameObject.Find(ballName);

            if (m != null) midfielder = m.transform;
            if (f != null) forward = f.transform;
            if (b != null) ball = b.transform;

            yield return null;
        }

        StartCoroutine(CameraSequence());
    }

    private IEnumerator CameraSequence()
    {
        // Toma general
        yield return MoveCameraTo(
            new Vector3(-4f, 7f, -8f),
            GetCenterPoint(),
            2.5f
        );

        // Cámara del mediocampista
        yield return MoveCameraTo(
            midfielder.position + new Vector3(2f, 3f, -5f),
            midfielder.position + Vector3.up * 1.2f,
            2.5f
        );

        // Cámara del pase
        yield return MoveCameraTo(
            new Vector3(-6f, 5f, 4f),
            ball.position + Vector3.up * 0.5f,
            2.5f
        );

        // Cámara del delantero
        yield return MoveCameraTo(
            forward.position + new Vector3(2f, 3f, -5f),
            forward.position + Vector3.up * 1.2f,
            2.5f
        );

        // Cámara del disparo
        yield return MoveCameraTo(
            forward.position + new Vector3(0f, 4f, -7f),
            forward.position + forward.forward * 5f + Vector3.up * 1f,
            3f
        );
    }

    private IEnumerator MoveCameraTo(Vector3 targetPosition, Vector3 lookTarget, float duration)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - targetPosition);

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    private Vector3 GetCenterPoint()
    {
        if (midfielder == null || forward == null) return Vector3.zero;

        return (midfielder.position + forward.position) / 2f + Vector3.up * 1.2f;
    }
}