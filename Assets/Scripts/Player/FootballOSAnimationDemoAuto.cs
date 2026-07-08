using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FootballOSAnimationDemoAuto : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] private string canchaSceneName = "minijuego1";

    [Header("Equipo jugador")]
    [SerializeField] private string midfielderName = "Player_Midfielder";
    [SerializeField] private string leftName = "Player_Left";
    [SerializeField] private string forwardName = "Player_Forward";
    [SerializeField] private string rightName = "Player_Right";

    [Header("Equipo rival")]
    [SerializeField] private string defenderName = "Player_Defender";
    [SerializeField] private string rivalDefender2Name = "Rival_Defender_2";
    [SerializeField] private string rivalDefender3Name = "Rival_Defender_3";
    [SerializeField] private string goalkeeperName = "Rival_Goalkeeper";

    [Header("Balon")]
    [SerializeField] private string ballName = "Ball";
    [SerializeField] private float ballHeight = 0.18f;
    [SerializeField] private float ballOffset = 0.9f;
    [SerializeField] private float passArcHeight = 0.55f;

    [Header("Demo")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopDemo = false;
    [SerializeField] private float playerGroundY = 0.4f;

    private Transform midfielder;
    private Transform leftPlayer;
    private Transform forward;
    private Transform rightPlayer;

    private Transform defender1;
    private Transform defender2;
    private Transform defender3;
    private Transform goalkeeper;

    private Animator midfielderAnim;
    private Animator leftAnim;
    private Animator forwardAnim;
    private Animator rightAnim;

    private Animator defender1Anim;
    private Animator defender2Anim;
    private Animator defender3Anim;
    private Animator goalkeeperAnim;

    private Transform ball;
    private Rigidbody ballRb;

    private FootballOSUIController uiController;

    private FootballOSCommandSystem commandSystem;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int PassHash = Animator.StringToHash("Pass");
    private static readonly int ReceiveHash = Animator.StringToHash("Receive");
    private static readonly int TackleHash = Animator.StringToHash("Tackle");

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != canchaSceneName)
            return;

        StartCoroutine(StartWhenReady());
    }

    private IEnumerator StartWhenReady()
    {
        while (!FindEverything())
        {
            yield return null;
        }

        uiController = FindAnyObjectByType<FootballOSUIController>();
        commandSystem = FindAnyObjectByType<FootballOSCommandSystem>();

        PrepareBall();

        if (playOnStart)
        {
            StartCoroutine(PlayRoutine());
        }
    }

    private bool FindEverything()
    {
        FindPlayer(midfielderName, ref midfielder, ref midfielderAnim);
        FindPlayer(leftName, ref leftPlayer, ref leftAnim);
        FindPlayer(forwardName, ref forward, ref forwardAnim);
        FindPlayer(rightName, ref rightPlayer, ref rightAnim);

        FindPlayer(defenderName, ref defender1, ref defender1Anim);
        FindPlayer(rivalDefender2Name, ref defender2, ref defender2Anim);
        FindPlayer(rivalDefender3Name, ref defender3, ref defender3Anim);
        FindPlayer(goalkeeperName, ref goalkeeper, ref goalkeeperAnim);

        GameObject ballObj = GameObject.Find(ballName);

        if (ballObj != null)
        {
            ball = ballObj.transform;
            ballRb = ballObj.GetComponent<Rigidbody>();
        }

        return midfielder != null &&
               leftPlayer != null &&
               forward != null &&
               rightPlayer != null &&
               defender1 != null &&
               ball != null;
    }

    private void FindPlayer(string objectName, ref Transform playerTransform, ref Animator playerAnimator)
    {
        if (playerTransform != null) return;

        GameObject obj = GameObject.Find(objectName);

        if (obj == null) return;

        playerTransform = obj.transform;
        playerAnimator = obj.GetComponentInChildren<Animator>();
        ConfigureAnimator(playerAnimator);
    }

    private void PrepareBall()
    {
        if (ballRb == null) return;

        ballRb.isKinematic = true;
        ballRb.useGravity = false;
    }

    private IEnumerator PlayRoutine()
    {
        do
        {
            SetupInitialPositions();

            UpdateOSUI(
                "- Analizando espacios...\n- Detectando líneas de pase...",
                "Player_Midfielder",
                "Carry Ball"
            );

            yield return new WaitForSeconds(0.4f);

            // Apertura: todos empiezan a moverse
            StartCoroutine(MovePlayer(leftPlayer, leftAnim, new Vector3(-8.5f, 0f, 6.2f), 1.3f, 0.55f));
            StartCoroutine(MovePlayer(rightPlayer, rightAnim, new Vector3(0.5f, 0f, 6.5f), 1.3f, 0.55f));
            StartCoroutine(MovePlayer(defender1, defender1Anim, new Vector3(-3.3f, 0f, 3.3f), 1.1f, 0.75f));
            StartCoroutine(MovePlayer(defender2, defender2Anim, new Vector3(-6.8f, 0f, 6.2f), 1.3f, 0.65f));
            StartCoroutine(MovePlayer(defender3, defender3Anim, new Vector3(-1.2f, 0f, 7.2f), 1.3f, 0.65f));

            // Midfielder conduce
            yield return MoveWithBall(midfielder, midfielderAnim, new Vector3(-4f, 0f, 2.2f), 1.1f, 0.65f);

            UpdateOSUI(
                "- Pase a banda izquierda\n- Receptor detectado: Player_Left",
                "Player_Midfielder",
                "Pass to Left"
            );

            // Midfielder -> Left
            CommandResult passLeftResult = CommandResult.Miss;

yield return RunCommandMode(
    "PASS LEFT CHANNEL",
    new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D },
    "Player_Midfielder",
    "Pass Left",
    r => passLeftResult = r
);

if (passLeftResult == CommandResult.Miss)
{
    yield return InterceptionRoutine(defender2, defender2Anim, "Secuencia fallida");
    continue;
}

yield return PassToMovingPlayer(
    midfielder,
    midfielderAnim,
    leftPlayer,
    leftAnim,
    new Vector3(-7.4f, 0f, 7.3f),
    GetPassTimeByResult(passLeftResult),
    0.2f
);
            // Left conduce
            UpdateOSUI(
                "- Control exitoso\n- Avance por banda izquierda",
                "Player_Left",
                "Carry Ball"
            );

            yield return MoveWithBall(leftPlayer, leftAnim, new Vector3(-6.5f, 0f, 8.2f), 0.7f, 0.65f);

            // Left -> Forward
            UpdateOSUI(
                "- Presión rival detectada\n- Pase interior a Player_Forward",
                "Player_Left",
                "Pass to Forward"
            );

            StartCoroutine(MovePlayer(defender1, defender1Anim, new Vector3(-4.1f, 0f, 6.5f), 1.0f, 0.85f));
            TriggerTackle(defender1Anim);

            CommandResult passForwardResult = CommandResult.Miss;

yield return RunCommandMode(
    "INTERIOR PASS",
    new KeyCode[] { KeyCode.W, KeyCode.D, KeyCode.Space },
    "Player_Left",
    "Pass Forward",
    r => passForwardResult = r
);

if (passForwardResult == CommandResult.Miss)
{
    yield return InterceptionRoutine(defender1, defender1Anim, "Pase interior perdido");
    continue;
}

yield return PassToMovingPlayer(
    leftPlayer,
    leftAnim,
    forward,
    forwardAnim,
    new Vector3(-4.2f, 0f, 9.1f),
    GetPassTimeByResult(passForwardResult),
    0.15f
);

            // Forward controla y avanza
            UpdateOSUI(
                "- Recepción limpia\n- Avance hacia zona ofensiva",
                "Player_Forward",
                "Carry Ball"
            );

            yield return MoveWithBall(forward, forwardAnim, new Vector3(-3.7f, 0f, 10.2f), 0.75f, 0.8f);

            UpdateOSUI(
                "- Cambio de orientación\n- Pase hacia Player_Right",
                "Player_Forward",
                "Pass to Right"
            );

            // Forward -> Right
            CommandResult passRightResult = CommandResult.Miss;

yield return RunCommandMode(
    "SWITCH TO RIGHT",
    new KeyCode[] { KeyCode.D, KeyCode.A, KeyCode.Space },
    "Player_Forward",
    "Pass Right",
    r => passRightResult = r
);

if (passRightResult == CommandResult.Miss)
{
    yield return InterceptionRoutine(defender3, defender3Anim, "Cambio de orientación fallido");
    continue;
}

yield return PassToMovingPlayer(
    forward,
    forwardAnim,
    rightPlayer,
    rightAnim,
    new Vector3(-0.6f, 0f, 9.4f),
    GetPassTimeByResult(passRightResult),
    0.15f
);

            // Right conduce
            UpdateOSUI(
                "- Player_Right controla el balón\n- Buscando apoyo hacia atrás",
                "Player_Right",
                "Carry Ball"
            );

            yield return MoveWithBall(rightPlayer, rightAnim, new Vector3(-1.2f, 0f, 10.8f), 0.75f, 0.75f);

            // Midfielder se ofrece para recibir
            UpdateOSUI(
                "- Presión rival cercana\n- Player_Midfielder ofrece apoyo",
                "Player_Right",
                "Back Pass Option"
            );

            StartCoroutine(MovePlayer(midfielder, midfielderAnim, new Vector3(-3.8f, 0f, 7.5f), 1.0f, 0.7f));

            // Right -> Midfielder
            UpdateOSUI(
    "- Pase atrás ejecutado\n- Reorganizando posesión",
    "Player_Right",
    "Pass to Midfielder"
);
            yield return PassToMovingPlayer(
                rightPlayer,
                rightAnim,
                midfielder,
                midfielderAnim,
                new Vector3(-3.8f, 0f, 7.5f),
                0.85f,
                0.15f
            );

            // Midfielder -> Forward
            UpdateOSUI(
    "- Nuevo espacio detectado\n- Pase vertical hacia Player_Forward",
    "Player_Midfielder",
    "Pass to Forward"
);
            StartCoroutine(MovePlayer(forward, forwardAnim, new Vector3(-4.8f, 0f, 11.3f), 0.85f, 0.8f));

            yield return PassToMovingPlayer(
                midfielder,
                midfielderAnim,
                forward,
                forwardAnim,
                new Vector3(-4.8f, 0f, 11.3f),
                0.75f,
                0.15f
            );

            // Forward -> Right, pase final controlado
            UpdateOSUI(
    "- Cambio de orientación final\n- Player_Right queda libre",
    "Player_Forward",
    "Pass to Right"
);
            yield return PassToMovingPlayer(
                forward,
                forwardAnim,
                rightPlayer,
                rightAnim,
                new Vector3(-2.2f, 0f, 12.1f),
                0.8f,
                0.2f
            );

            // Right controla, pero no termina todavía
            StopBallAtPlayer(rightPlayer, rightAnim);

            yield return new WaitForSeconds(0.4f);

            // Los rivales presionan al jugador que tiene balón
            UpdateOSUI(
    "- Rivales activan presión alta\n- Buscando salida segura",
    "Player_Right",
    "Protect Ball"
);
            StartCoroutine(MovePlayer(defender3, defender3Anim, new Vector3(-2.5f, 0f, 11.5f), 0.8f, 0.85f));
            StartCoroutine(MovePlayer(defender1, defender1Anim, new Vector3(-3.3f, 0f, 9.8f), 0.9f, 0.75f));

            yield return new WaitForSeconds(0.3f);

            // Midfielder se ofrece como apoyo atrás
            StartCoroutine(MovePlayer(midfielder, midfielderAnim, new Vector3(-4.2f, 0f, 9.5f), 0.9f, 0.7f));

            // Right pasa atrás a Midfielder
            yield return PassToMovingPlayer(
                rightPlayer,
                rightAnim,
                midfielder,
                midfielderAnim,
                new Vector3(-4.2f, 0f, 9.5f),
                0.75f,
                0.15f
            );

            // Midfielder controla y cambia hacia Left
            StartCoroutine(MovePlayer(leftPlayer, leftAnim, new Vector3(-7.2f, 0f, 10.8f), 0.9f, 0.7f));

            yield return PassToMovingPlayer(
                midfielder,
                midfielderAnim,
                leftPlayer,
                leftAnim,
                new Vector3(-7.2f, 0f, 10.8f),
                0.75f,
                0.15f
            );

            // Left controla y ahí sí termina la jugada con balón parado
            UpdateOSUI(
                "- Control final completado\n- Simulación finalizada correctamente",
                "Player_Left",
                "Hold Ball"
            );

            StopBallAtPlayer(leftPlayer, leftAnim);

            yield return new WaitForSeconds(2f);

        } while (loopDemo);
    }

    private void SetupInitialPositions()
    {
        SetPosition(midfielder, new Vector3(-4f, 0f, 0f));
        SetPosition(leftPlayer, new Vector3(-8f, 0f, 4.8f));
        SetPosition(forward, new Vector3(-4f, 0f, 8f));
        SetPosition(rightPlayer, new Vector3(0f, 0f, 4.8f));

        SetPosition(defender1, new Vector3(-3f, 0f, 4.5f));
        SetPosition(defender2, new Vector3(-6.2f, 0f, 5.8f));
        SetPosition(defender3, new Vector3(-1.2f, 0f, 6.8f));
        SetPosition(goalkeeper, new Vector3(-4f, 0f, 15f));

        LookAtFlat(midfielder, forward.position);
        LookAtFlat(leftPlayer, forward.position);
        LookAtFlat(forward, rightPlayer.position);
        LookAtFlat(rightPlayer, forward.position);

        LookAtFlat(defender1, midfielder.position);
        LookAtFlat(defender2, leftPlayer.position);
        LookAtFlat(defender3, forward.position);
        LookAtFlat(goalkeeper, forward.position);

        SetSpeed(midfielderAnim, 0f);
        SetSpeed(leftAnim, 0f);
        SetSpeed(forwardAnim, 0f);
        SetSpeed(rightAnim, 0f);

        SetSpeed(defender1Anim, 0f);
        SetSpeed(defender2Anim, 0f);
        SetSpeed(defender3Anim, 0f);
        SetSpeed(goalkeeperAnim, 0f);

        AttachBall(midfielder);
    }

    private IEnumerator MoveWithBall(Transform player, Animator animator, Vector3 targetPosition, float duration, float speed)
    {
        if (player == null) yield break;

        targetPosition = WithGroundY(targetPosition);
        LookAtFlat(player, targetPosition);
        SetSpeed(animator, speed);

        Vector3 start = player.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Smooth(timer / duration);

            player.position = Vector3.Lerp(start, targetPosition, t);
            AttachBall(player);

            yield return null;
        }

        player.position = targetPosition;
        AttachBall(player);
        SetSpeed(animator, 0f);
    }

    private IEnumerator PassToMovingPlayer(
        Transform passer,
        Animator passerAnimator,
        Transform receiver,
        Animator receiverAnimator,
        Vector3 receiverTargetPosition,
        float travelTime,
        float controlDelay
    )
    {
        if (passer == null || receiver == null || ball == null) yield break;

        receiverTargetPosition = WithGroundY(receiverTargetPosition);
        LookAtFlat(passer, receiverTargetPosition);
        LookAtFlat(receiver, passer.position);

        TriggerPass(passerAnimator);

        yield return new WaitForSeconds(0.25f);

        Vector3 passStart = GetBallPositionNear(passer);
        Vector3 receiverStart = receiver.position;

        StartCoroutine(ReceiveAnimationLater(receiverAnimator, travelTime * 0.65f));

        float timer = 0f;

        while (timer < travelTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / travelTime);
            float smoothT = Smooth(t);

            receiver.position = Vector3.Lerp(receiverStart, receiverTargetPosition, smoothT);
            SetSpeed(receiverAnimator, 0.65f);

            Vector3 passEnd = GetBallPositionNear(receiver);
            Vector3 ballPosition = Vector3.Lerp(passStart, passEnd, smoothT);
            ballPosition.y += Mathf.Sin(t * Mathf.PI) * passArcHeight;

            ball.position = ballPosition;

            yield return null;
        }

        receiver.position = receiverTargetPosition;
        SetSpeed(receiverAnimator, 0f);
        AttachBall(receiver);

        yield return new WaitForSeconds(controlDelay);
    }

    private IEnumerator MovePlayer(Transform player, Animator animator, Vector3 targetPosition, float duration, float speed)
    {
        if (player == null) yield break;

        targetPosition = WithGroundY(targetPosition);
        LookAtFlat(player, targetPosition);
        SetSpeed(animator, speed);

        Vector3 start = player.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Smooth(timer / duration);

            player.position = Vector3.Lerp(start, targetPosition, t);

            yield return null;
        }

        player.position = targetPosition;
        SetSpeed(animator, 0f);
    }

    private IEnumerator ReceiveAnimationLater(Animator animator, float delay)
    {
        yield return new WaitForSeconds(delay);

        TriggerReceive(animator);
    }

    private void AttachBall(Transform owner)
    {
        if (owner == null || ball == null) return;

        ball.position = GetBallPositionNear(owner);
        ball.rotation = Quaternion.identity;

        MakeEveryoneLookAtBall();
    }

    private void MakeEveryoneLookAtBall()
    {
        if (ball == null) return;

        LookAtFlat(defender1, ball.position);
        LookAtFlat(defender2, ball.position);
        LookAtFlat(defender3, ball.position);
        LookAtFlat(goalkeeper, ball.position);
    }

    private void StopBallAtPlayer(Transform player, Animator animator)
    {
        AttachBall(player);
        SetSpeed(animator, 0f);
        TriggerReceive(animator);
    }

    private Vector3 GetBallPositionNear(Transform player)
    {
        return player.position + player.forward * ballOffset * GetPlayerScale(player) + Vector3.up * ballHeight;
    }

    private float GetPlayerScale(Transform player)
    {
        if (player == null) return 1f;

        Vector3 scale = player.lossyScale;
        return Mathf.Max(0.01f, (Mathf.Abs(scale.x) + Mathf.Abs(scale.z)) * 0.5f);
    }

    private void SetPosition(Transform target, Vector3 position)
{
    if (target == null) return;

    position = WithGroundY(position);
    target.position = position;
}

    private void LookAtFlat(Transform who, Vector3 target)
    {
        if (who == null) return;

        Vector3 direction = target - who.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        who.rotation = Quaternion.LookRotation(direction);
    }

    private void SetSpeed(Animator animator, float value)
    {
        if (animator == null) return;
        if (!HasAnimatorParameter(animator, SpeedHash, AnimatorControllerParameterType.Float)) return;

        animator.SetFloat(SpeedHash, value);
    }

    private void TriggerPass(Animator animator)
    {
        if (animator == null) return;
        if (!HasAnimatorParameter(animator, PassHash, AnimatorControllerParameterType.Trigger)) return;

        animator.SetTrigger(PassHash);
    }

    private void TriggerReceive(Animator animator)
    {
        if (animator == null) return;
        if (!HasAnimatorParameter(animator, ReceiveHash, AnimatorControllerParameterType.Trigger)) return;

        animator.SetTrigger(ReceiveHash);
    }

    private void TriggerTackle(Animator animator)
    {
        if (animator == null) return;
        if (!HasAnimatorParameter(animator, TackleHash, AnimatorControllerParameterType.Trigger)) return;

        animator.SetTrigger(TackleHash);
    }

    private void ConfigureAnimator(Animator animator)
    {
        if (animator == null) return;

        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    private bool HasAnimatorParameter(Animator animator, int hash, AnimatorControllerParameterType type)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == hash && parameter.type == type)
            {
                return true;
            }
        }

        return false;
    }

    private Vector3 WithGroundY(Vector3 position)
    {
        position.y = playerGroundY;
        return position;
    }

    private float Smooth(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }

    private void UpdateOSUI(string eventLog, string playerInControl, string nextAction)
    {
        if (uiController == null) return;

        uiController.SetEventLog(eventLog);
        uiController.SetControlData(playerInControl, nextAction);
    }
    private IEnumerator RunCommandMode(string commandName, KeyCode[] sequence, string player, string action, Action<CommandResult> onResult)
{
    UpdateOSUI(
        "- COMMAND MODE ACTIVADO\n- Ejecuta la secuencia del software",
        player,
        action
    );

    CommandResult result = CommandResult.Miss;

    if (commandSystem != null)
    {
        yield return commandSystem.RunCommand(commandName, sequence, r => result = r);
    }

    onResult?.Invoke(result);
}

private float GetPassTimeByResult(CommandResult result)
{
    if (result == CommandResult.Perfect) return 0.55f;
    if (result == CommandResult.Good) return 0.75f;
    if (result == CommandResult.Bad) return 1.05f;

    return 1.2f;
}

private IEnumerator InterceptionRoutine(Transform interceptor, Animator interceptorAnimator, string reason)
{
    if (interceptor == null) yield break;

    UpdateOSUI(
        "- " + reason + "\n- CPU intercepta el balón",
        interceptor.name,
        "Interception"
    );

    Vector3 target = WithGroundY(new Vector3(ball.position.x, playerGroundY, ball.position.z));

    yield return MovePlayer(interceptor, interceptorAnimator, target, 0.7f, 0.85f);

    TriggerReceive(interceptorAnimator);
    AttachBall(interceptor);

    yield return new WaitForSeconds(1.2f);
}
}
