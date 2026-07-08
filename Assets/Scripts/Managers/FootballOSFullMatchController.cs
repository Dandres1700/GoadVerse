using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FootballOSFullMatchController : MonoBehaviour
{
    private enum Team
    {
        Player,
        Rival
    }

    private enum CommandGrade
    {
        Perfect,
        Good,
        Bad,
        Miss
    }

    [Header("Escena")]
    [SerializeField] private string canchaSceneName = "minijuego1";

    [Header("Balón")]
    [SerializeField] private string ballName = "Ball";
    [SerializeField] private float ballHeight = 0.43f;
    [SerializeField] private float ballOffset = 1.25f;
    [SerializeField] private float passArcHeight = 0.45f;

    [Header("Campo")]
    [SerializeField] private float centerX = -4f;
    [SerializeField] private float playerGoalZ = -14f;
    [SerializeField] private float rivalGoalZ = 15f;

    [Header("Movimiento")]
    [SerializeField] private float supportMoveSpeed = 2.8f;
    [SerializeField] private float carrySpeed = 0.7f;
    [SerializeField] private float playerGroundY = 0.4f;

    [Header("Control equipo jugador")]
    [SerializeField] private bool enableManualPlayerTeamControl = true;
    [SerializeField] private string playerTeamRootName = "Team_Player";
    [SerializeField] private int manualPlayerIndex = 6;
    [SerializeField] private KeyCode switchManualPlayerKey = KeyCode.E;
    [SerializeField] private float manualWalkSpeed = 5f;
    [SerializeField] private float manualRunSpeed = 8f;
    [SerializeField] private float manualTurnSpeed = 12f;
    [SerializeField] private bool manualPlayerKeepsBall = true;
    [SerializeField] private bool disablePlayerTeamAutoSupportWhileManual = true;

    [Header("CPU / IA")]
    [SerializeField] private float defensivePressureDistance = 2.2f;
    [SerializeField] private float defensiveMarkingBlend = 0.45f;
    [SerializeField] private float passLaneRiskPenalty = 5f;
    [SerializeField] private float receiverMarkedPenalty = 3f;
    [SerializeField] private float safePassBonusDistance = 2.4f;

    [Header("Partido")]
    [SerializeField] private bool loopMatch = true;
    [SerializeField] private int actionsPerPossession = 8;
[SerializeField] private int commandEveryActions = 3;
[SerializeField] private float minPassDistance = 3f;
[SerializeField] private float maxPassDistance = 13f;

    private string[] playerNames =
    {
        "P_GK", "P_LB", "P_CB1", "P_CB2", "P_RB",
        "P_LM", "P_CM", "P_RM",
        "P_LW", "P_ST", "P_RW"
    };

    private string[] rivalNames =
    {
        "R_GK", "R_LB", "R_CB1", "R_CB2", "R_RB",
        "R_LM", "R_CM", "R_RM",
        "R_LW", "R_ST", "R_RW"
    };

    private Transform[] playerTeam = new Transform[11];
    private Transform[] rivalTeam = new Transform[11];

    private Animator[] playerAnimators = new Animator[11];
    private Animator[] rivalAnimators = new Animator[11];

    private bool[] playerBusy = new bool[11];
    private bool[] rivalBusy = new bool[11];

    private Vector3[] playerBase = new Vector3[11];
    private Vector3[] rivalBase = new Vector3[11];

    private Transform ball;
    private Rigidbody ballRb;

    private FootballOSUIController uiController;

    private FootballOSCommandOverlay commandOverlay;

    private bool ready;
    private bool playerPossession = true;
    private int ownerIndex = 6;

    private int ecuadorScore;
    private int cpuScore;
    private int minute;

    private CommandGrade lastCommandResult = CommandGrade.Miss;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int PassHash = Animator.StringToHash("Pass");
    private static readonly int ReceiveHash = Animator.StringToHash("Receive");
    private static readonly int TackleHash = Animator.StringToHash("Tackle");
    private static readonly int ShootHash = Animator.StringToHash("Shoot");
    private static readonly int CelebrateHash = Animator.StringToHash("Celebrate");

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != canchaSceneName)
            return;

        StartCoroutine(StartMatchWhenReady());
    }

private void Update()
{
    DisableManualPlayer();

    if (!ready) return;

    if (enableManualPlayerTeamControl)
    {
        HandleManualPlayerTeamControl();
    }

    if (!enableManualPlayerTeamControl || !disablePlayerTeamAutoSupportWhileManual)
    {
        UpdateSupportMovement(playerTeam, playerAnimators, playerBusy, playerBase, true);
    }

    UpdateSupportMovement(rivalTeam, rivalAnimators, rivalBusy, rivalBase, false);
}

    private IEnumerator StartMatchWhenReady()
    {
        DisableManualPlayer();

        while (!FindEverything())
        {
            UpdateUI(
                "- Buscando jugadores en la cancha\n- Revisa nombres P_ y R_",
                "FOOTBALL OS",
                "Initializing"
            );

            yield return null;
        }

        uiController = FindFirstObjectByType<FootballOSUIController>();
        commandOverlay = FindFirstObjectByType<FootballOSCommandOverlay>();

        PrepareFormations();
        PrepareBall();

        ready = true;

        SetupKickOff();

        if (!enableManualPlayerTeamControl)
        {
            StartCoroutine(MatchLoop());
        }
        else
        {
            SelectManualPlayer(manualPlayerIndex);
        }
    }

    private bool FindEverything()
    {
        for (int i = 0; i < playerNames.Length; i++)
        {
            FindPlayer(playerNames[i], ref playerTeam[i], ref playerAnimators[i]);
        }

        for (int i = 0; i < rivalNames.Length; i++)
        {
            FindPlayer(rivalNames[i], ref rivalTeam[i], ref rivalAnimators[i]);
        }

        GameObject ballObj = GameObject.Find(ballName);

        if (ballObj != null)
        {
            ball = ballObj.transform;
            ballRb = ballObj.GetComponent<Rigidbody>();
        }

        for (int i = 0; i < 11; i++)
        {
            if (playerTeam[i] == null) return false;
            if (rivalTeam[i] == null) return false;
        }

        return ball != null;
    }

    private void FindPlayer(string objectName, ref Transform player, ref Animator animator)
    {
        if (player != null) return;

        GameObject obj = FindPlayerTeamObject(objectName);

        if (obj == null) return;

        player = obj.transform;
        animator = obj.GetComponentInChildren<Animator>();
        ConfigureAnimator(animator);

        if (objectName.StartsWith("P_"))
        {
            DisablePlayerMovementBlockers(obj);
        }
    }

    private GameObject FindPlayerTeamObject(string objectName)
    {
        GameObject root = GameObject.Find(playerTeamRootName);

        if (root != null)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.name == objectName)
                {
                    return child.gameObject;
                }
            }
        }

        return GameObject.Find(objectName);
    }

    private void DisablePlayerMovementBlockers(GameObject playerObject)
    {
        if (playerObject == null) return;

        PlayerMovement3D playerMovement = playerObject.GetComponent<PlayerMovement3D>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        GoalkeeperController goalkeeperController = playerObject.GetComponent<GoalkeeperController>();

        if (goalkeeperController != null)
        {
            goalkeeperController.enabled = false;
        }
    }

    private void DisableManualPlayer()
    {
        GameObject manualPlayer = GameObject.Find("HumanM_Model");

        if (manualPlayer != null)
        {
            Behaviour movement = manualPlayer.GetComponent("PlayerMovement3D") as Behaviour;

            if (movement != null)
                movement.enabled = false;

            CharacterController controller = manualPlayer.GetComponent<CharacterController>();

            if (controller != null)
                controller.enabled = false;

            Renderer[] renderers = manualPlayer.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }

            manualPlayer.transform.position = new Vector3(0f, -100f, 0f);
        }

        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            Behaviour thirdPersonCamera = mainCamera.GetComponent("ThirdPersonCamera") as Behaviour;

            if (thirdPersonCamera != null)
                thirdPersonCamera.enabled = false;
        }
    }

    private void PrepareFormations()
{
    // ECUADOR - 4-3-3
    playerBase[0] = new Vector3(centerX, 0f, -20f);       // P_GK

    playerBase[1] = new Vector3(centerX - 9f, 0f, -13f);  // P_LB
    playerBase[2] = new Vector3(centerX - 3f, 0f, -14f);  // P_CB1
    playerBase[3] = new Vector3(centerX + 3f, 0f, -14f);  // P_CB2
    playerBase[4] = new Vector3(centerX + 9f, 0f, -13f);  // P_RB

    playerBase[5] = new Vector3(centerX - 7f, 0f, -5f);   // P_LM
    playerBase[6] = new Vector3(centerX, 0f, -4f);        // P_CM
    playerBase[7] = new Vector3(centerX + 7f, 0f, -5f);   // P_RM

    playerBase[8] = new Vector3(centerX - 9f, 0f, 6f);    // P_LW
    playerBase[9] = new Vector3(centerX, 0f, 9f);         // P_ST
    playerBase[10] = new Vector3(centerX + 9f, 0f, 6f);   // P_RW

    // CPU - 4-3-3
    rivalBase[0] = new Vector3(centerX, 0f, 22f);         // R_GK

    rivalBase[1] = new Vector3(centerX + 9f, 0f, 14f);    // R_LB
    rivalBase[2] = new Vector3(centerX + 3f, 0f, 15f);    // R_CB1
    rivalBase[3] = new Vector3(centerX - 3f, 0f, 15f);    // R_CB2
    rivalBase[4] = new Vector3(centerX - 9f, 0f, 14f);    // R_RB

    rivalBase[5] = new Vector3(centerX + 7f, 0f, 5f);     // R_LM
    rivalBase[6] = new Vector3(centerX, 0f, 4f);          // R_CM
    rivalBase[7] = new Vector3(centerX - 7f, 0f, 5f);     // R_RM

    rivalBase[8] = new Vector3(centerX + 9f, 0f, -6f);    // R_LW
    rivalBase[9] = new Vector3(centerX, 0f, -9f);         // R_ST
    rivalBase[10] = new Vector3(centerX - 9f, 0f, -6f);   // R_RW
}

    private void PrepareBall()
{
    if (ballRb == null) return;

    ballRb.isKinematic = false;
    ballRb.useGravity = false;
    ballRb.linearVelocity = Vector3.zero;
    ballRb.angularVelocity = Vector3.zero;

    ballRb.isKinematic = true;
}

    private IEnumerator MatchLoop()
{
    SetupKickOff();

    while (loopMatch)
    {
        yield return PlayDynamicPossession(Team.Player, actionsPerPossession);

        yield return new WaitForSeconds(0.4f);

        yield return PlayDynamicPossession(Team.Rival, actionsPerPossession);

        minute += 4;

        if (minute >= 90)
        {
            UpdateUI(
                "- Partido finalizado\n- Simulación Football OS completada",
                "FOOTBALL OS",
                "Full Time"
            );

            yield return new WaitForSeconds(2f);

            minute = 0;
            ecuadorScore = 0;
            cpuScore = 0;

            SetupKickOff();
        }
    }
}

    private void SetupKickOff()
{
    // ECUADOR - 4-3-3, ocupando toda la cancha
    playerBase[0] = new Vector3(centerX, 0.4f, -20f);       // P_GK

    playerBase[1] = new Vector3(centerX - 9f, 0.4f, -13f);  // P_LB
    playerBase[2] = new Vector3(centerX - 3f, 0.4f, -14f);  // P_CB1
    playerBase[3] = new Vector3(centerX + 3f, 0.4f, -14f);  // P_CB2
    playerBase[4] = new Vector3(centerX + 9f, 0.4f, -13f);  // P_RB

    playerBase[5] = new Vector3(centerX - 8f, 0.4f, -4f);   // P_LM
    playerBase[6] = new Vector3(centerX, 0.4f, -3f);        // P_CM
    playerBase[7] = new Vector3(centerX + 8f, 0.4f, -4f);   // P_RM

    playerBase[8] = new Vector3(centerX - 9f, 0.4f, 7f);    // P_LW
    playerBase[9] = new Vector3(centerX, 0.4f, 10f);        // P_ST
    playerBase[10] = new Vector3(centerX + 9f, 0.4f, 7f);   // P_RW

    // CPU - 4-3-3, ocupando el lado contrario
    rivalBase[0] = new Vector3(centerX, 0.4f, 22f);         // R_GK

    rivalBase[1] = new Vector3(centerX + 9f, 0.4f, 15f);    // R_LB
    rivalBase[2] = new Vector3(centerX + 3f, 0.4f, 16f);    // R_CB1
    rivalBase[3] = new Vector3(centerX - 3f, 0.4f, 16f);    // R_CB2
    rivalBase[4] = new Vector3(centerX - 9f, 0.4f, 15f);    // R_RB

    rivalBase[5] = new Vector3(centerX + 8f, 0.4f, 6f);     // R_LM
    rivalBase[6] = new Vector3(centerX, 0.4f, 5f);          // R_CM
    rivalBase[7] = new Vector3(centerX - 8f, 0.4f, 6f);     // R_RM

    rivalBase[8] = new Vector3(centerX + 9f, 0.4f, -6f);    // R_LW
    rivalBase[9] = new Vector3(centerX, 0.4f, -9f);         // R_ST
    rivalBase[10] = new Vector3(centerX - 9f, 0.4f, -6f);   // R_RW

    for (int i = 0; i < 11; i++)
    {
        playerTeam[i].position = playerBase[i];
        rivalTeam[i].position = rivalBase[i];

        SetSpeed(playerAnimators[i], 0f);
        SetSpeed(rivalAnimators[i], 0f);

        playerBusy[i] = false;
        rivalBusy[i] = false;
    }

    playerPossession = true;
    ownerIndex = 6;

    LookAtFlat(playerTeam[6], rivalTeam[0].position);
    AttachBall(playerTeam[6]);

    UpdateUI(
        "- Kick off desde media cancha\n- Ecuador inicia posesión",
        "P_CM",
        "Build Up"
    );
}

    private IEnumerator EcuadorAttack()
    {
        playerPossession = true;
        ownerIndex = 6;

        yield return CarryWithBall(Team.Player, 6, new Vector3(centerX, 0f, 1.8f), 0.9f);

        yield return CommandPass(
            "PASS LEFT CHANNEL",
            new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D },
            Team.Player,
            6,
            Team.Player,
            5,
            new Vector3(centerX - 5.8f, 0f, 3.8f),
            Team.Rival,
            6
        );

        if (!playerPossession) yield break;

        yield return CarryWithBall(Team.Player, 5, new Vector3(centerX - 5.8f, 0f, 6.2f), 0.7f);

        yield return CommandPass(
            "WIDE PROGRESSION",
            new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.Space },
            Team.Player,
            5,
            Team.Player,
            8,
            new Vector3(centerX - 5.8f, 0f, 8.5f),
            Team.Rival,
            2
        );

        if (!playerPossession) yield break;

        yield return CommandPass(
            "CROSS TO STRIKER",
            new KeyCode[] { KeyCode.W, KeyCode.D, KeyCode.Space },
            Team.Player,
            8,
            Team.Player,
            9,
            new Vector3(centerX, 0f, 11.8f),
            Team.Rival,
            3
        );

        if (!playerPossession) yield break;

        yield return CommandShot();
    }

    private IEnumerator CpuAttack()
    {
        playerPossession = false;
        ownerIndex = 0;
        AttachBall(rivalTeam[0]);

        UpdateUI(
            "- CPU reinicia desde defensa\n- Ecuador activa presión",
            "R_GK",
            "CPU Build Up"
        );

        yield return PassBall(Team.Rival, 0, Team.Rival, 2, new Vector3(centerX + 2f, 0f, 8.5f), 0.8f);
        yield return PassBall(Team.Rival, 2, Team.Rival, 6, new Vector3(centerX, 0f, 4.5f), 0.8f);

        yield return RunCommand(
            "DEFENSIVE PRESS",
            new KeyCode[] { KeyCode.Q, KeyCode.E, KeyCode.R },
            "P_CM",
            "Recover Ball"
        );

        if (lastCommandResult == CommandGrade.Perfect || lastCommandResult == CommandGrade.Good)
        {
            yield return Interception(Team.Player, 6);

            playerPossession = true;
            yield break;
        }

        yield return PassBall(Team.Rival, 6, Team.Rival, 9, new Vector3(centerX, 0f, -7.5f), 0.9f);

        yield return CpuShot();
    }

    private IEnumerator CommandPass(
        string commandName,
        KeyCode[] sequence,
        Team fromTeam,
        int fromIndex,
        Team toTeam,
        int toIndex,
        Vector3 receiverTarget,
        Team interceptorTeam,
        int interceptorIndex
    )
    {
        yield return RunCommand(
            commandName,
            sequence,
            GetName(fromTeam, fromIndex),
            "Command Pass"
        );

        if (lastCommandResult == CommandGrade.Miss)
        {
            yield return Interception(interceptorTeam, interceptorIndex);
            yield break;
        }

        float passTime = 0.8f;

        if (lastCommandResult == CommandGrade.Perfect) passTime = 0.5f;
        if (lastCommandResult == CommandGrade.Good) passTime = 0.7f;
        if (lastCommandResult == CommandGrade.Bad) passTime = 1.05f;

        yield return PassBall(fromTeam, fromIndex, toTeam, toIndex, receiverTarget, passTime);
    }

    private IEnumerator CommandShot()
    {
        yield return RunCommand(
            "SHOT EXECUTION",
            new KeyCode[] { KeyCode.W, KeyCode.D, KeyCode.Space },
            "P_ST",
            "Shoot"
        );

        Animator strikerAnim = playerAnimators[9];

        Trigger(strikerAnim, ShootHash);

        yield return new WaitForSeconds(0.3f);

        Vector3 start = GetBallPositionNear(playerTeam[9]);

        if (lastCommandResult == CommandGrade.Perfect || lastCommandResult == CommandGrade.Good)
        {
            Vector3 goalTarget = new Vector3(centerX, ballHeight, rivalGoalZ + 0.8f);

            yield return MoveBallArc(start, goalTarget, 0.9f, 0.25f);

            ecuadorScore++;
            minute += 5;

            UpdateUI(
                "- GOL DE ECUADOR\n- Football OS ejecutó la jugada",
                "P_ST",
                "Goal"
            );

            Trigger(strikerAnim, CelebrateHash);

            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            Vector3 saveTarget = GetBallPositionNear(rivalTeam[0]);

            yield return MoveBallArc(start, saveTarget, 0.8f, 0.25f);

            AttachBall(rivalTeam[0]);
            Trigger(rivalAnimators[0], ReceiveHash);

            UpdateUI(
                "- Disparo impreciso\n- Portero CPU controla",
                "R_GK",
                "Save"
            );

            yield return new WaitForSeconds(0.8f);
        }
    }

    private IEnumerator CpuShot()
    {
        Trigger(rivalAnimators[9], ShootHash);

        Vector3 start = GetBallPositionNear(rivalTeam[9]);
        Vector3 end = new Vector3(centerX, ballHeight, playerGoalZ - 0.8f);

        yield return MoveBallArc(start, end, 0.8f, 0.2f);

        cpuScore++;
        minute += 5;

        UpdateUI(
            "- CPU marca gol\n- Reiniciando sistema táctico",
            "R_ST",
            "CPU Goal"
        );

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator CarryWithBall(Team team, int index, Vector3 target, float duration)
    {
        Transform player = GetPlayer(team, index);
        Animator animator = GetAnimator(team, index);
        bool[] busy = GetBusyArray(team);

        target = WithGroundY(target);
        busy[index] = true;

        LookAtFlat(player, target);
        SetSpeed(animator, carrySpeed);

        Vector3 start = player.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Smooth(timer / duration);

            player.position = Vector3.Lerp(start, target, t);
            AttachBall(player);

            yield return null;
        }

        player.position = target;
        AttachBall(player);
        SetSpeed(animator, 0f);

        busy[index] = false;
        ownerIndex = index;
        playerPossession = team == Team.Player;
    }

    private IEnumerator PassBall(Team fromTeam, int fromIndex, Team toTeam, int toIndex, Vector3 receiverTarget, float duration)
    {
        Transform passer = GetPlayer(fromTeam, fromIndex);
        Transform receiver = GetPlayer(toTeam, toIndex);

        Animator passerAnimator = GetAnimator(fromTeam, fromIndex);
        Animator receiverAnimator = GetAnimator(toTeam, toIndex);

        bool[] fromBusy = GetBusyArray(fromTeam);
        bool[] toBusy = GetBusyArray(toTeam);

        receiverTarget = WithGroundY(receiverTarget);
        fromBusy[fromIndex] = true;
        toBusy[toIndex] = true;

        LookAtFlat(passer, receiverTarget);
        LookAtFlat(receiver, passer.position);

        Trigger(passerAnimator, PassHash);

        yield return new WaitForSeconds(0.2f);

        Vector3 start = GetBallPositionNear(passer);
        Vector3 receiverStart = receiver.position;

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float smooth = Smooth(t);

            receiver.position = Vector3.Lerp(receiverStart, receiverTarget, smooth);
            SetSpeed(receiverAnimator, 0.65f);

            Vector3 end = GetBallPositionNear(receiver);
            Vector3 ballPos = Vector3.Lerp(start, end, smooth);
            ballPos.y += Mathf.Sin(t * Mathf.PI) * passArcHeight;

            ball.position = ballPos;

            yield return null;
        }

        receiver.position = receiverTarget;

        Trigger(receiverAnimator, ReceiveHash);
        SetSpeed(receiverAnimator, 0f);

        fromBusy[fromIndex] = false;
        toBusy[toIndex] = false;

        playerPossession = toTeam == Team.Player;
        ownerIndex = toIndex;

        AttachBall(receiver);
    }

    private IEnumerator RunCommand(string commandName, KeyCode[] sequence, string player, string action)
{
    lastCommandResult = CommandGrade.Miss;

    float previousTimeScale = Time.timeScale;
    Time.timeScale = 0.85f;

    int perfectCount = 0;
    int goodCount = 0;
    int badCount = 0;
    int missCount = 0;

    if (commandOverlay != null)
    {
        commandOverlay.ShowCommand(commandName);
    }

    UpdateUI(
        "- COMMAND MODE ACTIVADO\n- Ejecuta la secuencia del software",
        player,
        action
    );

    for (int i = 0; i < sequence.Length; i++)
    {
        KeyCode expected = sequence[i];

        float timer = 0f;
        float duration = 0.85f;
        bool pressed = false;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / duration);
            float sync = progress * 100f;

            if (commandOverlay != null)
            {
                commandOverlay.UpdateCommand(sequence, i, sync);
            }

            if (Input.GetKeyDown(expected))
            {
                pressed = true;

                float target = 0.70f;
                float diff = Mathf.Abs(progress - target);

                if (diff <= 0.08f)
                {
                    perfectCount++;
                }
                else if (diff <= 0.18f)
                {
                    goodCount++;
                }
                else if (diff <= 0.32f)
                {
                    badCount++;
                }
                else
                {
                    missCount++;
                }

                break;
            }

            yield return null;
        }

        if (!pressed)
        {
            missCount++;
        }

        yield return new WaitForSecondsRealtime(0.05f);
    }

    if (missCount > 0)
    {
        lastCommandResult = CommandGrade.Miss;
    }
    else if (perfectCount == sequence.Length)
    {
        lastCommandResult = CommandGrade.Perfect;
    }
    else if (badCount > 0)
    {
        lastCommandResult = CommandGrade.Bad;
    }
    else
    {
        lastCommandResult = CommandGrade.Good;
    }

    if (commandOverlay != null)
    {
        commandOverlay.ShowResult(lastCommandResult.ToString().ToUpper());
    }

    UpdateUI(
        "- COMMAND RESULT: " + lastCommandResult.ToString().ToUpper(),
        player,
        action
    );

    yield return new WaitForSecondsRealtime(0.45f);

    if (commandOverlay != null)
    {
        commandOverlay.HideCommand();
    }

    Time.timeScale = previousTimeScale;
}

    private IEnumerator Interception(Team team, int index)
    {
        Transform interceptor = GetPlayer(team, index);
        Animator animator = GetAnimator(team, index);
        bool[] busy = GetBusyArray(team);

        busy[index] = true;

        UpdateUI(
            "- Secuencia fallida\n- Intercepción detectada",
            GetName(team, index),
            "Interception"
        );

        Vector3 target = WithGroundY(new Vector3(ball.position.x, playerGroundY, ball.position.z));

        yield return MovePlayerTo(team, index, target, 0.65f, 0.9f);

        Trigger(animator, ReceiveHash);

        playerPossession = team == Team.Player;
        ownerIndex = index;

        AttachBall(interceptor);

        busy[index] = false;

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator MovePlayerTo(Team team, int index, Vector3 target, float duration, float speed)
    {
        Transform player = GetPlayer(team, index);
        Animator animator = GetAnimator(team, index);

        target = WithGroundY(target);
        LookAtFlat(player, target);
        SetSpeed(animator, speed);

        Vector3 start = player.position;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Smooth(timer / duration);

            player.position = Vector3.Lerp(start, target, t);

            yield return null;
        }

        player.position = target;
        SetSpeed(animator, 0f);
    }

    private IEnumerator MoveBallArc(Vector3 start, Vector3 end, float duration, float arcHeight)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float smooth = Smooth(t);

            Vector3 pos = Vector3.Lerp(start, end, smooth);
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            ball.position = pos;

            yield return null;
        }

        ball.position = end;
    }

    private void HandleManualPlayerTeamControl()
    {
        HandleManualPlayerSelectionInput();

        manualPlayerIndex = Mathf.Clamp(manualPlayerIndex, 0, playerTeam.Length - 1);
        Transform player = playerTeam[manualPlayerIndex];

        if (player == null) return;

        playerPossession = true;
        ownerIndex = manualPlayerIndex;
        playerBusy[manualPlayerIndex] = true;

        Vector2 input = GetManualMoveInput();
        bool isMoving = input.sqrMagnitude > 0.01f;
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (isMoving)
        {
            Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            player.rotation = Quaternion.Slerp(player.rotation, targetRotation, manualTurnSpeed * Time.deltaTime);

            float speed = isRunning ? manualRunSpeed : manualWalkSpeed;
            player.position = ClampToField(player.position + direction * speed * Time.deltaTime);
            SetSpeed(playerAnimators[manualPlayerIndex], isRunning ? 1f : 0.55f);
        }
        else
        {
            SetSpeed(playerAnimators[manualPlayerIndex], 0f);
        }

        if (manualPlayerKeepsBall)
        {
            AttachBall(player);
        }
    }

    private void HandleManualPlayerSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectManualPlayer(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectManualPlayer(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectManualPlayer(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectManualPlayer(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectManualPlayer(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectManualPlayer(5);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SelectManualPlayer(6);
        if (Input.GetKeyDown(KeyCode.Alpha8)) SelectManualPlayer(7);
        if (Input.GetKeyDown(KeyCode.Alpha9)) SelectManualPlayer(8);
        if (Input.GetKeyDown(KeyCode.Alpha0)) SelectManualPlayer(9);

        if (Input.GetKeyDown(switchManualPlayerKey) || Input.GetKeyDown(KeyCode.Tab))
        {
            SelectManualPlayer((manualPlayerIndex + 1) % playerTeam.Length);
        }
    }

    private Vector2 GetManualMoveInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;

        return Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
    }

    private void SelectManualPlayer(int index)
    {
        manualPlayerIndex = Mathf.Clamp(index, 0, playerTeam.Length - 1);
        ownerIndex = manualPlayerIndex;
        playerPossession = true;

        for (int i = 0; i < playerBusy.Length; i++)
        {
            playerBusy[i] = i == manualPlayerIndex;
        }

        if (manualPlayerKeepsBall && playerTeam[manualPlayerIndex] != null)
        {
            AttachBall(playerTeam[manualPlayerIndex]);
        }

        UpdateUI(
            "- Control manual activo\n- WASD/Flechas mueven, E cambia jugador",
            GetName(Team.Player, manualPlayerIndex),
            "Manual Control"
        );
    }

    private void UpdateSupportMovement(Transform[] team, Animator[] animators, bool[] busy, Vector3[] basePositions, bool isPlayerTeam)
    {
        if (ball == null) return;

        for (int i = 0; i < team.Length; i++)
        {
            if (team[i] == null) continue;
            if (busy[i]) continue;
            if (enableManualPlayerTeamControl && isPlayerTeam && i == manualPlayerIndex) continue;

            bool isOwner = isPlayerTeam == playerPossession && i == ownerIndex;

            if (isOwner)
                continue;

            Vector3 target = GetDynamicSupportPosition(basePositions[i], isPlayerTeam, i);

            float distance = Vector3.Distance(team[i].position, target);

            if (distance > 0.05f)
            {
                LookAtFlat(team[i], target);
                team[i].position = Vector3.MoveTowards(team[i].position, target, supportMoveSpeed * Time.deltaTime);
                SetSpeed(animators[i], 0.35f);
            }
            else
            {
                SetSpeed(animators[i], 0f);
                LookAtFlat(team[i], ball.position);
            }
        }
    }

    private Vector3 GetDynamicSupportPosition(Vector3 basePosition, bool isPlayerTeam, int index)
{
    if (ball == null) return basePosition;

    bool teamHasBall = isPlayerTeam == playerPossession;

    if (index == 0)
    {
        float goalkeeperX = Mathf.Clamp(ball.position.x, centerX - 3.5f, centerX + 3.5f);
        return new Vector3(goalkeeperX, playerGroundY, basePosition.z);
    }

    if (!teamHasBall)
    {
        if (IsOneOfClosestToBall(isPlayerTeam, index, 2))
        {
            Vector3 pressureTarget = Vector3.MoveTowards(
                WithGroundY(ball.position),
                basePosition,
                defensivePressureDistance
            );

            return ClampToField(pressureTarget);
        }

        Transform mark = FindClosestOpponentForMarking(basePosition, isPlayerTeam);

        if (mark != null)
        {
            Vector3 markingTarget = Vector3.Lerp(basePosition, WithGroundY(mark.position), defensiveMarkingBlend);
            return ClampToField(markingTarget);
        }
    }

    float attackDirection = isPlayerTeam ? 1f : -1f;

    float xInfluence = Mathf.Clamp((ball.position.x - centerX) * 0.12f, -1.5f, 1.5f);
    float zInfluence = Mathf.Clamp((ball.position.z - basePosition.z) * 0.05f, -1.8f, 1.8f);

    Vector3 target;

    if (teamHasBall)
    {
        target = new Vector3(
            basePosition.x + xInfluence,
            playerGroundY,
            basePosition.z + zInfluence + attackDirection * 0.8f
        );
    }
    else
    {
        target = new Vector3(
            basePosition.x + xInfluence,
            playerGroundY,
            basePosition.z + zInfluence
        );
    }

    return ClampToField(target);
}

    private Transform FindClosestOpponentForMarking(Vector3 basePosition, bool isPlayerTeam)
{
    Transform[] opponents = isPlayerTeam ? rivalTeam : playerTeam;

    Transform closest = null;
    float bestDistance = 9999f;

    for (int i = 1; i < opponents.Length; i++)
    {
        if (opponents[i] == null) continue;

        float distance = Vector3.Distance(basePosition, opponents[i].position);

        if (distance < bestDistance)
        {
            bestDistance = distance;
            closest = opponents[i];
        }
    }

    return closest;
}

    private bool IsOneOfClosestToBall(bool isPlayerTeam, int index, int count)
{
    if (ball == null) return false;

    Transform[] team = isPlayerTeam ? playerTeam : rivalTeam;
    float candidateDistance = GetFlatDistance(team[index].position, ball.position);
    int closerPlayers = 0;

    for (int i = 1; i < team.Length; i++)
    {
        if (i == index || team[i] == null) continue;

        if (GetFlatDistance(team[i].position, ball.position) < candidateDistance)
        {
            closerPlayers++;
        }
    }

    return closerPlayers < count;
}

private Transform GetNearestOpponent(Team team, Vector3 position)
{
    Transform[] opponents = team == Team.Player ? rivalTeam : playerTeam;
    Transform closest = null;
    float bestDistance = 9999f;

    for (int i = 1; i < opponents.Length; i++)
    {
        if (opponents[i] == null) continue;

        float distance = GetFlatDistance(position, opponents[i].position);

        if (distance < bestDistance)
        {
            bestDistance = distance;
            closest = opponents[i];
        }
    }

    return closest;
}

private float GetNearestOpponentDistance(Team team, Vector3 position)
{
    Transform nearest = GetNearestOpponent(team, position);

    if (nearest == null) return 9999f;

    return GetFlatDistance(position, nearest.position);
}

private float GetPassLaneRisk(Team team, Vector3 from, Vector3 to)
{
    Transform[] opponents = team == Team.Player ? rivalTeam : playerTeam;
    float risk = 0f;

    for (int i = 1; i < opponents.Length; i++)
    {
        if (opponents[i] == null) continue;

        float distanceToLane = DistancePointToSegmentFlat(opponents[i].position, from, to);

        if (distanceToLane < 1.6f)
        {
            risk += 1f - distanceToLane / 1.6f;
        }
    }

    return Mathf.Clamp01(risk);
}

private float DistancePointToSegmentFlat(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd)
{
    Vector2 p = new Vector2(point.x, point.z);
    Vector2 a = new Vector2(segmentStart.x, segmentStart.z);
    Vector2 b = new Vector2(segmentEnd.x, segmentEnd.z);
    Vector2 ab = b - a;

    if (ab.sqrMagnitude < 0.0001f)
    {
        return Vector2.Distance(p, a);
    }

    float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
    return Vector2.Distance(p, a + ab * t);
}

private float GetFlatDistance(Vector3 a, Vector3 b)
{
    a.y = 0f;
    b.y = 0f;
    return Vector3.Distance(a, b);
}

private Vector3 ClampToField(Vector3 target)
{
    target.x = Mathf.Clamp(target.x, centerX - 10f, centerX + 10f);
    target.z = Mathf.Clamp(target.z, playerGoalZ + 2f, rivalGoalZ - 2f);
    return WithGroundY(target);
}

    private void AttachBall(Transform owner)
    {
        if (owner == null || ball == null) return;

        ball.position = GetBallPositionNear(owner);
        ball.rotation = Quaternion.identity;
    }

    private Vector3 GetBallPositionNear(Transform player)
    {
        return player.position + player.forward * ballOffset + Vector3.up * ballHeight;
    }

    private Transform GetPlayer(Team team, int index)
    {
        return team == Team.Player ? playerTeam[index] : rivalTeam[index];
    }

    private Animator GetAnimator(Team team, int index)
    {
        return team == Team.Player ? playerAnimators[index] : rivalAnimators[index];
    }

    private bool[] GetBusyArray(Team team)
    {
        return team == Team.Player ? playerBusy : rivalBusy;
    }

    private string GetName(Team team, int index)
    {
        return team == Team.Player ? playerNames[index] : rivalNames[index];
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

    private void Trigger(Animator animator, int hash)
    {
        if (animator == null) return;
        if (!HasAnimatorParameter(animator, hash, AnimatorControllerParameterType.Trigger)) return;

        animator.SetTrigger(hash);
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

    private void UpdateUI(string eventLog, string playerInControl, string action)
    {
        if (uiController == null) return;

        uiController.SetEventLog(
            "TIME: " + minute.ToString("00") + "'  SCORE: ECU " + ecuadorScore + " - " + cpuScore + " CPU\n" +
            eventLog
        );

        uiController.SetControlData(playerInControl, action);
    }
    private IEnumerator PlayDynamicPossession(Team team, int maxActions)
{
    playerPossession = team == Team.Player;

    if (team == Team.Player)
    {
        UpdateUI(
            "- Ecuador construye jugada\n- Football OS analiza opciones",
            GetName(team, ownerIndex),
            "Build Up"
        );
    }
    else
    {
        UpdateUI(
            "- CPU recupera posesión\n- Ecuador activa presión",
            GetName(team, ownerIndex),
            "CPU Build Up"
        );
    }

    for (int action = 0; action < maxActions; action++)
    {
        Team opponentTeam = GetOpponent(team);

        if (CanShoot(team, ownerIndex))
        {
            if (team == Team.Player)
            {
                yield return CommandShot();
            }
            else
            {
                yield return CpuShot();
            }

            yield break;
        }

        int receiverIndex = ChooseBestReceiver(team, ownerIndex);

        if (receiverIndex == ownerIndex)
        {
            receiverIndex = GetRandomOutfieldPlayer();
        }

        Vector3 receiverTarget = GetDynamicReceiverTarget(team, receiverIndex);

        bool importantAction = team == Team.Player && action % commandEveryActions == 0;

        if (importantAction)
        {
            CommandGrade commandResult;

            yield return RunCommandForDynamicPass(
                "TACTICAL PASS",
                new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D },
                GetName(team, ownerIndex),
                "Pass"
            );

            commandResult = lastCommandResult;

            if (commandResult == CommandGrade.Miss)
            {
                int interceptor = FindNearestPlayerToBall(opponentTeam);

                yield return Interception(opponentTeam, interceptor);
                yield break;
            }

            float passTime = GetPassTime(commandResult);

            yield return PassBall(
                team,
                ownerIndex,
                team,
                receiverIndex,
                receiverTarget,
                passTime
            );
        }
        else
        {
            yield return PassBall(
                team,
                ownerIndex,
                team,
                receiverIndex,
                receiverTarget,
                Random.Range(0.55f, 0.95f)
            );
        }

        ownerIndex = receiverIndex;
        playerPossession = team == Team.Player;

        Vector3 carryTarget = GetCarryTarget(team, ownerIndex);

        yield return CarryWithBall(
            team,
            ownerIndex,
            carryTarget,
            Random.Range(0.45f, 0.75f)
        );

        minute += 1;

        yield return new WaitForSeconds(0.08f);
    }
}
private Team GetOpponent(Team team)
{
    return team == Team.Player ? Team.Rival : Team.Player;
}

private int ChooseBestReceiver(Team team, int currentOwner)
{
    Transform owner = GetPlayer(team, currentOwner);

    if (owner == null) return currentOwner;

    float attackDirection = team == Team.Player ? 1f : -1f;

    int bestIndex = currentOwner;
    float bestScore = -9999f;

    for (int i = 1; i < 11; i++)
    {
        if (i == currentOwner) continue;

        Transform candidate = GetPlayer(team, i);

        if (candidate == null) continue;

        float distance = Vector3.Distance(owner.position, candidate.position);

        if (distance < minPassDistance || distance > maxPassDistance)
            continue;

        float forwardProgress = (candidate.position.z - owner.position.z) * attackDirection;

        float widthBonus = Mathf.Abs(candidate.position.x - centerX) * 0.25f;
        float nearestOpponentDistance = GetNearestOpponentDistance(team, candidate.position);
        float safeDistance = Mathf.Max(0.01f, safePassBonusDistance);
        float markingRisk = Mathf.Clamp01((safeDistance - nearestOpponentDistance) / safeDistance);
        float laneRisk = GetPassLaneRisk(team, owner.position, candidate.position);

        float roleBonus = 0f;

        if (i == 8 || i == 9 || i == 10)
            roleBonus += 3f;

        if (i == 5 || i == 6 || i == 7)
            roleBonus += 1.5f;

        float randomness = Random.Range(-2f, 3f);

        float score =
            forwardProgress * 2.2f +
            widthBonus +
            roleBonus +
            randomness +
            nearestOpponentDistance * 0.25f -
            distance * 0.15f -
            markingRisk * receiverMarkedPenalty -
            laneRisk * passLaneRiskPenalty;

        if (forwardProgress < -2f)
        {
            score -= 6f;
        }

        if (score > bestScore)
        {
            bestScore = score;
            bestIndex = i;
        }
    }

    if (bestIndex == currentOwner)
    {
        bestIndex = Random.Range(5, 11);
    }

    return bestIndex;
}

private Vector3 GetDynamicReceiverTarget(Team team, int index)
{
    Vector3 basePosition = team == Team.Player ? playerBase[index] : rivalBase[index];

    float attackDirection = team == Team.Player ? 1f : -1f;

    float randomX = Random.Range(-2.0f, 2.0f);
    float randomZ = Random.Range(1.5f, 4.0f) * attackDirection;

    Vector3 target = new Vector3(
        basePosition.x + randomX,
        playerGroundY,
        basePosition.z + randomZ
    );

    target.x = Mathf.Clamp(target.x, centerX - 10f, centerX + 10f);
    target.z = Mathf.Clamp(target.z, playerGoalZ + 2f, rivalGoalZ - 2f);

    return WithGroundY(target);
}

private Vector3 GetCarryTarget(Team team, int index)
{
    Transform player = GetPlayer(team, index);

    if (player == null) return Vector3.zero;

    float attackDirection = team == Team.Player ? 1f : -1f;

    Vector3 target = player.position + new Vector3(
        Random.Range(-1.2f, 1.2f),
        0f,
        Random.Range(1.0f, 2.8f) * attackDirection
    );

    Transform nearestOpponent = GetNearestOpponent(team, player.position);

    if (nearestOpponent != null)
    {
        Vector3 away = player.position - nearestOpponent.position;
        away.y = 0f;

        if (away.sqrMagnitude > 0.001f)
        {
            float pressureDistance = Vector3.Distance(player.position, nearestOpponent.position);
            float evadeWeight = Mathf.Clamp01((4.5f - pressureDistance) / 4.5f);
            target += away.normalized * evadeWeight * 2f;
        }
    }

    return ClampToField(target);
}

private bool CanShoot(Team team, int index)
{
    Transform player = GetPlayer(team, index);

    if (player == null) return false;

    if (team == Team.Player)
    {
        return player.position.z > rivalGoalZ - 7f && (index == 8 || index == 9 || index == 10);
    }

    return player.position.z < playerGoalZ + 7f && (index == 8 || index == 9 || index == 10);
}

private int GetRandomOutfieldPlayer()
{
    return Random.Range(1, 11);
}

private float GetPassTime(CommandGrade grade)
{
    if (grade == CommandGrade.Perfect) return 0.5f;
    if (grade == CommandGrade.Good) return 0.7f;
    if (grade == CommandGrade.Bad) return 1.0f;

    return 1.2f;
}

private int FindNearestPlayerToBall(Team team)
{
    int bestIndex = 1;
    float bestDistance = 9999f;

    for (int i = 1; i < 11; i++)
    {
        Transform player = GetPlayer(team, i);

        if (player == null) continue;

        float distance = Vector3.Distance(player.position, ball.position);

        if (distance < bestDistance)
        {
            bestDistance = distance;
            bestIndex = i;
        }
    }

    return bestIndex;
}

private IEnumerator RunCommandForDynamicPass(string commandName, KeyCode[] sequence, string player, string action)
{
    yield return RunCommand(commandName, sequence, player, action);
}
}
