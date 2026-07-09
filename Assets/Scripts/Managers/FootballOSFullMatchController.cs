using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private float ballHeight = 0.75f;
    [SerializeField] private float ballOffset = 1.45f;
    [SerializeField] private float passArcHeight = 0.45f;
    [SerializeField] private bool useFixedInitialBallTransform = true;
    [SerializeField] private Vector3 initialBallPosition = new Vector3(-7.2f, 0.43f, -15.2f);
    [SerializeField] private Vector3 initialBallScale = new Vector3(1.8f, 1.8f, 1.8f);

    [Header("Campo")]
    [SerializeField] private float centerX = -4f;
    [SerializeField] private float playerGoalZ = -14f;
    [SerializeField] private float rivalGoalZ = 15f;
    [SerializeField] private Vector3 playAreaCenter = new Vector3(-6.88477f, 0f, -14.53401f);
    [SerializeField] private Vector2 playAreaSize = new Vector2(105f, 68f);
    [SerializeField] private float playAreaPadding = 1.5f;
    [SerializeField] private float playerGoalX = -60f;
    [SerializeField] private float rivalGoalX = 46f;
    [SerializeField] private float goalCenterZ = -14.4f;
    [SerializeField] private float goalWidth = 14f;
    [SerializeField] private float goalResetDelay = 1.25f;

    [Header("Movimiento")]
    [SerializeField] private float supportMoveSpeed = 2.8f;
    [SerializeField] private float carrySpeed = 0.7f;
    [SerializeField] private float playerGroundY = 0.4f;

    [Header("Control equipo jugador")]
    [SerializeField] private bool enableManualPlayerTeamControl = true;
    [SerializeField] private bool manualUseOnlyActiveScenePlayers = true;
    [SerializeField] private bool manualPreserveScenePositions = true;
    [SerializeField] private string playerTeamRootName = "Team_Player";
    [SerializeField] private string rivalTeamRootName = "Team_Rival";
    [SerializeField] private int manualPlayerIndex = 6;
    [SerializeField] private KeyCode switchManualPlayerKey = KeyCode.E;
    [SerializeField] private KeyCode manualKickKey = KeyCode.O;
    [SerializeField] private float manualWalkSpeed = 5f;
    [SerializeField] private float manualRunSpeed = 8f;
    [SerializeField] private float manualTurnSpeed = 12f;
    [SerializeField] private float manualAiTurnSpeed = 7.5f;
    [SerializeField] private float animationSpeedDampTime = 0.12f;
    [SerializeField] private float manualKickForce = 8f;
    [SerializeField] private float manualKickLift = 0.12f;
    [SerializeField] private float manualKickDelay = 0.15f;
    [SerializeField] private float manualKickReleaseSeconds = 0.8f;
    [SerializeField] private float manualPickupDistance = 2.5f;
    [SerializeField] private float manualPassDuration = 0.45f;
    [SerializeField] private float manualPassMaxDistance = 45f;
    [SerializeField] private float manualShootDistance = 14f;
    [SerializeField] private float manualSupportForwardSpacing = 8f;
    [SerializeField] private float manualSupportSideSpacing = 8f;
    [SerializeField] private float manualRivalPressureSpeed = 3.4f;
    [SerializeField] private float manualRivalStealDistance = 1.5f;
    [SerializeField] private float manualRecoverDistance = 1.8f;
    [SerializeField] private float manualStealCooldownSeconds = 1f;
    [SerializeField] private float manualRivalDecisionInterval = 1.4f;
    [SerializeField] private float manualRivalPassDuration = 0.55f;
    [SerializeField] private float manualRivalShootDistance = 12f;
    [SerializeField] private float manualRivalShotForceMultiplier = 0.92f;
    [SerializeField] private float manualRivalDribblePressureDistance = 6f;
    [SerializeField] private bool manualPlayerKeepsBall = true;
    [SerializeField] private bool attachBallToManualPlayerOnStart = false;
    [SerializeField] private Color manualControlledRingColor = Color.green;
    [SerializeField] private Color manualAvailableRingColor = new Color(1f, 0.92f, 0.02f, 1f);

    [Header("Panel de gol")]
    [SerializeField] private bool showGoalPanel = true;
    [SerializeField] private float goalPanelSeconds = 2f;
    [SerializeField] private Color playerGoalPanelColor = new Color(0.05f, 0.85f, 1f, 0.82f);
    [SerializeField] private Color rivalGoalPanelColor = new Color(1f, 0.12f, 0.08f, 0.82f);

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
    private Quaternion[] playerBaseRotation = new Quaternion[11];
    private Quaternion[] rivalBaseRotation = new Quaternion[11];

    private Transform ball;
    private Rigidbody ballRb;

    private FootballOSUIController uiController;

    private FootballOSCommandOverlay commandOverlay;

    private bool ready;
    private bool playerPossession = true;
    private int ownerIndex = 6;
    private bool manualBallAttached;
    private float manualBallReleaseTimer;
    private Coroutine manualKickRoutine;
    private bool manualRivalHasBall;
    private int manualRivalOwnerIndex = -1;
    private float manualStealCooldown;
    private float manualRivalDecisionTimer;
    private Coroutine manualGoalResetRoutine;
    private Coroutine manualRivalActionRoutine;
    private GameObject goalPanelObject;
    private Image goalPanelImage;
    private TMP_Text goalPanelText;
    private CanvasGroup goalPanelGroup;

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

    if (manualGoalResetRoutine != null)
    {
        return;
    }

    if (enableManualPlayerTeamControl)
    {
        HandleManualPlayerTeamControl();
        UpdateManualTeamSupport();
        UpdateManualRivalPressure();
        CheckManualGoal();
        return;
    }

    UpdateSupportMovement(playerTeam, playerAnimators, playerBusy, playerBase, true);
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

        uiController = FindAnyObjectByType<FootballOSUIController>();
        commandOverlay = FindAnyObjectByType<FootballOSCommandOverlay>();
        EnsureGoalPanel();

        yield return WaitForIntroCamera();

        PrepareBall();

        ready = true;

        if (UseSceneManualPlayers())
        {
            CaptureCurrentBasePositions();
            manualRivalHasBall = false;
            manualRivalOwnerIndex = -1;
            manualStealCooldown = manualStealCooldownSeconds;
            SelectManualPlayer(manualPlayerIndex, attachBallToManualPlayerOnStart);
        }
        else if (!enableManualPlayerTeamControl)
        {
            PrepareFormations();
            SetupKickOff();
            StartCoroutine(MatchLoop());
        }
        else
        {
            PrepareFormations();
            SetupKickOff();
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

        if (UseSceneManualPlayers())
        {
            return ball != null && GetAvailableManualPlayerCount() > 0 && GetAvailableRivalPlayerCount() > 0;
        }

        for (int i = 0; i < 11; i++)
        {
            if (playerTeam[i] == null) return false;
            if (rivalTeam[i] == null) return false;
        }

        return ball != null;
    }

    private bool UseSceneManualPlayers()
    {
        return enableManualPlayerTeamControl && manualUseOnlyActiveScenePlayers && manualPreserveScenePositions;
    }

    private IEnumerator WaitForIntroCamera()
    {
        MundialSystemCameraController cameraController = FindAnyObjectByType<MundialSystemCameraController>();

        while (cameraController != null && !cameraController.IsIntroComplete)
        {
            UpdateUI(
                "- Presentacion de cancha y equipos\n- El partido inicia al terminar la camara",
                "FOOTBALL OS",
                "Intro"
            );

            yield return null;
        }
    }

    private void FindPlayer(string objectName, ref Transform player, ref Animator animator)
    {
        if (player != null) return;

        bool includeInactive = !UseSceneManualPlayers();
        GameObject obj = FindPlayerTeamObject(objectName, includeInactive);

        if (obj == null) return;

        if (!includeInactive && !obj.activeInHierarchy)
        {
            return;
        }

        if (includeInactive && !obj.activeSelf)
        {
            obj.SetActive(true);
        }

        player = obj.transform;
        animator = obj.GetComponentInChildren<Animator>();
        ConfigureAnimator(animator);

        if (objectName.StartsWith("P_"))
        {
            DisablePlayerMovementBlockers(obj);
        }
    }

    private GameObject FindPlayerTeamObject(string objectName, bool includeInactive)
    {
        string rootName = objectName.StartsWith("R_") ? rivalTeamRootName : playerTeamRootName;
        GameObject root = GameObject.Find(rootName);

        if (root != null)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in children)
            {
                if (child.name == objectName && (includeInactive || child.gameObject.activeInHierarchy))
                {
                    return child.gameObject;
                }
            }
        }

        return GameObject.Find(objectName);
    }

    private void CaptureCurrentBasePositions()
    {
        for (int i = 0; i < playerTeam.Length; i++)
        {
            if (playerTeam[i] != null)
            {
                playerBase[i] = playerTeam[i].position;
                playerBaseRotation[i] = playerTeam[i].rotation;
                SetSpeed(playerAnimators[i], 0f);
            }

            playerBusy[i] = false;
        }

        for (int i = 0; i < rivalTeam.Length; i++)
        {
            if (rivalTeam[i] != null)
            {
                rivalBase[i] = rivalTeam[i].position;
                rivalBaseRotation[i] = rivalTeam[i].rotation;
                SetSpeed(rivalAnimators[i], 0f);
            }

            rivalBusy[i] = false;
        }
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

        PlayerFootballAnimationsTest animationTest = playerObject.GetComponent<PlayerFootballAnimationsTest>();

        if (animationTest != null)
        {
            animationTest.enabled = false;
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
    PlaceBallAtInitialTransform();

    if (ballRb == null) return;

    ballRb.isKinematic = false;
    ballRb.useGravity = false;
    ballRb.linearVelocity = Vector3.zero;
    ballRb.angularVelocity = Vector3.zero;

    ballRb.isKinematic = true;
}

    private void PlaceBallAtInitialTransform()
{
    if (!useFixedInitialBallTransform || ball == null) return;

    ball.position = initialBallPosition;
    ball.rotation = Quaternion.identity;
    ball.localScale = initialBallScale;
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

        if (manualBallReleaseTimer > 0f)
        {
            manualBallReleaseTimer -= Time.deltaTime;
        }

        if (manualStealCooldown > 0f)
        {
            manualStealCooldown -= Time.deltaTime;
        }

        manualPlayerIndex = Mathf.Clamp(manualPlayerIndex, 0, playerTeam.Length - 1);

        if (!IsManualPlayerAvailable(manualPlayerIndex))
        {
            SelectManualPlayer(manualPlayerIndex);
            return;
        }

        Transform player = playerTeam[manualPlayerIndex];

        if (player == null) return;

        playerPossession = !manualRivalHasBall;
        ownerIndex = manualRivalHasBall ? manualRivalOwnerIndex : manualPlayerIndex;
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

        if (Input.GetKeyDown(manualKickKey))
        {
            TryManualKick(player);
        }

        if (manualRivalHasBall && IsBallCloseTo(player, manualRecoverDistance))
        {
            if (manualRivalActionRoutine != null)
            {
                StopCoroutine(manualRivalActionRoutine);
                manualRivalActionRoutine = null;
            }

            manualRivalHasBall = false;
            manualRivalOwnerIndex = -1;
            manualBallAttached = true;
            manualStealCooldown = manualStealCooldownSeconds;
            Trigger(playerAnimators[manualPlayerIndex], ReceiveHash);
            UpdateUI(
                "- Recuperaste el balon\n- Busca pase o tiro con O",
                GetName(Team.Player, manualPlayerIndex),
                "Recover"
            );
        }

        if (!manualBallAttached && !manualRivalHasBall && manualBallReleaseTimer <= 0f)
        {
            TryPlayerTeamReceiveLooseBall();
        }

        if (manualPlayerKeepsBall && manualBallAttached && !manualRivalHasBall)
        {
            AttachBall(player);
        }
    }

    private void TryManualKick(Transform player)
    {
        if (player == null || ball == null) return;
        if (manualKickRoutine != null) return;
        if (manualRivalHasBall) return;
        if (!manualBallAttached && !IsBallCloseTo(player)) return;

        Trigger(playerAnimators[manualPlayerIndex], ShootHash);
        SetSpeed(playerAnimators[manualPlayerIndex], 0f);

        manualKickRoutine = StartCoroutine(ManualKickAfterDelay(player));

        UpdateUI(
            "- Disparo manual con O\n- El balón queda en juego",
            GetName(Team.Player, manualPlayerIndex),
            "Shoot"
        );
    }

    private IEnumerator ManualPassToReceiver(Transform passer, int receiverIndex)
    {
        if (manualKickDelay > 0f)
        {
            yield return new WaitForSeconds(manualKickDelay);
        }

        Transform receiver = IsManualPlayerAvailable(receiverIndex) ? playerTeam[receiverIndex] : null;

        if (passer != null && receiver != null && ball != null)
        {
            manualBallAttached = false;
            manualBallReleaseTimer = 0f;

            AttachBall(passer);

            Vector3 start = ball.position;
            Vector3 end = GetBallPositionNear(receiver);

            yield return MoveBallArc(start, end, manualPassDuration, passArcHeight);

            manualKickRoutine = null;
            Trigger(playerAnimators[receiverIndex], ReceiveHash);
            SelectManualPlayer(receiverIndex);
            yield break;
        }

        manualKickRoutine = null;
    }

    private IEnumerator ManualKickAfterDelay(Transform player)
    {
        if (manualKickDelay > 0f)
        {
            yield return new WaitForSeconds(manualKickDelay);
        }

        if (player != null && ball != null)
        {
            AttachBall(player);
            ReleaseBallFromManualPlayer(player);
        }

        manualKickRoutine = null;
    }

    private void ReleaseBallFromManualPlayer(Transform player)
    {
        manualBallAttached = false;
        manualBallReleaseTimer = manualKickReleaseSeconds;

        Vector3 direction = player.forward;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            Vector3 goalTarget = new Vector3(GetRivalGoalLineX() + Mathf.Sign(rivalGoalX - playerGoalX) * 2f, ball.position.y, goalCenterZ);
            direction = goalTarget - ball.position;
            direction.y = 0f;
        }

        ReleaseBallInDirection(direction, manualKickForce);
    }

    private void ReleaseBallToward(Vector3 target, float force)
    {
        if (ball == null) return;

        Vector3 direction = target - ball.position;
        direction.y = 0f;

        ReleaseBallInDirection(direction, force);
    }

    private void ReleaseBallInDirection(Vector3 direction, float force)
    {
        if (ball == null) return;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector3.right * Mathf.Sign(rivalGoalX - playerGoalX);
        }

        direction += Vector3.up * manualKickLift;
        direction.Normalize();

        if (ballRb != null)
        {
            ballRb.isKinematic = false;
            ballRb.useGravity = true;
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
            ballRb.AddForce(direction * force, ForceMode.Impulse);
        }
        else
        {
            ball.position += direction * force * Time.deltaTime;
        }
    }

    private bool IsBallCloseTo(Transform player)
    {
        return IsBallCloseTo(player, manualPickupDistance);
    }

    private bool IsBallCloseTo(Transform player, float distance)
    {
        if (player == null || ball == null) return false;

        return GetFlatDistance(player.position, ball.position) <= distance;
    }

    private bool ShouldManualShoot(Transform player)
    {
        if (player == null) return false;

        float attackDirection = Mathf.Sign(rivalGoalX - playerGoalX);
        float distanceToGoal = Mathf.Abs(GetRivalGoalLineX() - player.position.x);

        return distanceToGoal <= manualShootDistance
            && Mathf.Sign(GetRivalGoalLineX() - player.position.x) == attackDirection;
    }

    private int FindBestManualPassReceiver(Transform passer)
    {
        if (passer == null) return -1;

        int bestIndex = -1;
        float bestScore = -9999f;
        float attackDirection = Mathf.Sign(rivalGoalX - playerGoalX);

        for (int i = 0; i < playerTeam.Length; i++)
        {
            if (i == manualPlayerIndex || !IsManualPlayerAvailable(i)) continue;

            Transform candidate = playerTeam[i];
            float distance = GetFlatDistance(passer.position, candidate.position);

            if (distance > manualPassMaxDistance) continue;

            float progress = (candidate.position.x - passer.position.x) * attackDirection;
            float spacing = Mathf.Abs(candidate.position.z - passer.position.z);
            float safety = GetNearestOpponentDistance(Team.Player, candidate.position);
            float score = progress * 1.8f + spacing * 0.4f + safety * 0.35f - distance * 0.12f;

            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void TryPlayerTeamReceiveLooseBall()
    {
        for (int i = 0; i < playerTeam.Length; i++)
        {
            if (!IsManualPlayerAvailable(i)) continue;

            if (IsBallCloseTo(playerTeam[i]))
            {
                manualBallAttached = true;
                Trigger(playerAnimators[i], ReceiveHash);
                SelectManualPlayer(i);
                return;
            }
        }
    }

    private void HandleManualPlayerSelectionInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectManualPlayer(0, false);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectManualPlayer(1, false);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectManualPlayer(2, false);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectManualPlayer(3, false);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SelectManualPlayer(4, false);
        if (Input.GetKeyDown(KeyCode.Alpha6)) SelectManualPlayer(5, false);
        if (Input.GetKeyDown(KeyCode.Alpha7)) SelectManualPlayer(6, false);
        if (Input.GetKeyDown(KeyCode.Alpha8)) SelectManualPlayer(7, false);
        if (Input.GetKeyDown(KeyCode.Alpha9)) SelectManualPlayer(8, false);
        if (Input.GetKeyDown(KeyCode.Alpha0)) SelectManualPlayer(9, false);

        if (Input.GetKeyDown(switchManualPlayerKey) || Input.GetKeyDown(KeyCode.Tab))
        {
            SelectManualPlayer(GetNextManualPlayerIndex(manualPlayerIndex), false);
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

    private void SelectManualPlayer(int index, bool attachBallToSelected = true)
    {
        int selectableIndex = GetSelectableManualPlayerIndex(index);

        if (selectableIndex < 0)
        {
            UpdateUI(
                "- No hay jugadores activos para controlar\n- Deja activos los personajes P_ en la escena",
                "Team Player",
                "Manual Control"
            );

            return;
        }

        if (manualKickRoutine != null)
        {
            StopCoroutine(manualKickRoutine);
            manualKickRoutine = null;
        }

        bool wasBallAttached = manualBallAttached;

        manualPlayerIndex = selectableIndex;
        ownerIndex = manualRivalHasBall ? manualRivalOwnerIndex : manualPlayerIndex;
        playerPossession = !manualRivalHasBall;
        manualBallAttached = !manualRivalHasBall && manualPlayerKeepsBall && attachBallToSelected;

        if (manualBallAttached)
        {
            manualBallReleaseTimer = 0f;
        }
        else if (!manualRivalHasBall)
        {
            if (wasBallAttached)
            {
                DetachBallInPlace();
            }

            manualBallReleaseTimer = Mathf.Max(manualBallReleaseTimer, 0.25f);
        }

        for (int i = 0; i < playerBusy.Length; i++)
        {
            playerBusy[i] = i == manualPlayerIndex;
        }

        if (manualPlayerKeepsBall && manualBallAttached && playerTeam[manualPlayerIndex] != null)
        {
            AttachBall(playerTeam[manualPlayerIndex]);
        }

        UpdateManualSelectionRings();

        UpdateUI(
            "- Control manual activo\n- WASD mueve, E cambia jugador, O patea",
            GetName(Team.Player, manualPlayerIndex),
            "Manual Control"
        );
    }

    private void DetachBallInPlace()
    {
        manualBallAttached = false;

        if (ballRb == null) return;

        if (!ballRb.isKinematic)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }

        ballRb.useGravity = true;
        ballRb.isKinematic = false;
    }

    private void UpdateManualSelectionRings()
    {
        for (int i = 0; i < playerTeam.Length; i++)
        {
            if (playerTeam[i] == null) continue;

            Color color = i == manualPlayerIndex ? manualControlledRingColor : manualAvailableRingColor;
            SelectionRingLine[] rings = playerTeam[i].GetComponentsInChildren<SelectionRingLine>(true);

            foreach (SelectionRingLine ring in rings)
            {
                if (ring != null)
                {
                    ring.SetRingColor(color);
                }
            }
        }
    }

    private int GetAvailableManualPlayerCount()
    {
        int count = 0;

        for (int i = 0; i < playerTeam.Length; i++)
        {
            if (IsManualPlayerAvailable(i))
            {
                count++;
            }
        }

        return count;
    }

    private int GetSelectableManualPlayerIndex(int requestedIndex)
    {
        if (IsManualPlayerAvailable(requestedIndex))
        {
            return requestedIndex;
        }

        return GetNextManualPlayerIndex(requestedIndex);
    }

    private int GetNextManualPlayerIndex(int currentIndex)
    {
        int length = playerTeam.Length;
        int startIndex = ((currentIndex % length) + length) % length;

        for (int offset = 1; offset <= length; offset++)
        {
            int nextIndex = (startIndex + offset) % length;

            if (IsManualPlayerAvailable(nextIndex))
            {
                return nextIndex;
            }
        }

        return -1;
    }

    private bool IsManualPlayerAvailable(int index)
    {
        return index >= 0
            && index < playerTeam.Length
            && playerTeam[index] != null
            && playerTeam[index].gameObject.activeInHierarchy;
    }

    private int GetAvailableRivalPlayerCount()
    {
        int count = 0;

        for (int i = 0; i < rivalTeam.Length; i++)
        {
            if (IsRivalPlayerAvailable(i))
            {
                count++;
            }
        }

        return count;
    }

    private bool IsRivalPlayerAvailable(int index)
    {
        return index >= 0
            && index < rivalTeam.Length
            && rivalTeam[index] != null
            && rivalTeam[index].gameObject.activeInHierarchy;
    }

    private void UpdateManualTeamSupport()
    {
        Transform controlledPlayer = IsManualPlayerAvailable(manualPlayerIndex) ? playerTeam[manualPlayerIndex] : null;

        if (controlledPlayer == null || ball == null) return;

        int supportSlot = 0;

        for (int i = 0; i < playerTeam.Length; i++)
        {
            if (!IsManualPlayerAvailable(i)) continue;
            if (i == manualPlayerIndex) continue;

            Transform teammate = playerTeam[i];
            Vector3 target;

            if (manualRivalHasBall)
            {
                target = GetManualDefensiveSupportTarget(teammate, supportSlot);
            }
            else
            {
                target = GetManualAttackingSupportTarget(controlledPlayer, supportSlot);
            }

            MoveManualPlayerToward(teammate, playerAnimators[i], target, supportMoveSpeed);
            supportSlot++;
        }
    }

    private Vector3 GetManualAttackingSupportTarget(Transform controlledPlayer, int supportSlot)
    {
        float attackDirection = Mathf.Sign(rivalGoalX - playerGoalX);
        float side = supportSlot % 2 == 0 ? 1f : -1f;
        float depth = manualSupportForwardSpacing + supportSlot * 2f;

        Vector3 target = controlledPlayer.position
            + Vector3.right * attackDirection * depth
            + Vector3.forward * side * manualSupportSideSpacing;

        return ClampToField(target);
    }

    private Vector3 GetManualDefensiveSupportTarget(Transform teammate, int supportSlot)
    {
        int closestIndex = FindNearestPlayerToBall(Team.Player);

        if (playerTeam[closestIndex] == teammate)
        {
            return ClampToField(ball.position);
        }

        float ownGoalX = playerGoalX;
        float side = supportSlot % 2 == 0 ? 1f : -1f;

        Vector3 target = new Vector3(
            Mathf.Lerp(ball.position.x, ownGoalX, 0.35f),
            playerGroundY,
            ball.position.z + side * manualSupportSideSpacing
        );

        return ClampToField(target);
    }

    private void UpdateManualRivalPressure()
    {
        if (ball == null) return;

        if (manualRivalHasBall)
        {
            UpdateManualRivalPossession();
            return;
        }

        int primaryPresser = FindNearestAvailableRivalTo(ball.position);
        Transform controlledPlayer = IsManualPlayerAvailable(manualPlayerIndex) ? playerTeam[manualPlayerIndex] : null;

        for (int i = 0; i < rivalTeam.Length; i++)
        {
            if (!IsRivalPlayerAvailable(i)) continue;

            Transform rival = rivalTeam[i];
            bool shouldPress = i == primaryPresser;
            Vector3 target;
            float speed;

            if (shouldPress)
            {
                target = ClampToField(ball.position);
                speed = manualRivalPressureSpeed;
            }
            else if (controlledPlayer != null && GetFlatDistance(rival.position, controlledPlayer.position) <= manualRivalDribblePressureDistance * 1.4f)
            {
                target = GetManualRivalMarkingTarget(rival, controlledPlayer);
                speed = supportMoveSpeed;
            }
            else
            {
                target = GetManualRivalSupportTarget(rival, i);
                speed = supportMoveSpeed;
            }

            MoveManualPlayerToward(rival, rivalAnimators[i], target, speed);

            if (shouldPress && manualStealCooldown <= 0f && GetFlatDistance(rival.position, ball.position) <= manualRivalStealDistance)
            {
                ManualRivalSteal(i);
                return;
            }
        }
    }

    private void UpdateManualRivalPossession()
    {
        if (!IsRivalPlayerAvailable(manualRivalOwnerIndex))
        {
            manualRivalHasBall = false;
            manualRivalOwnerIndex = -1;
            return;
        }

        if (manualRivalActionRoutine != null)
        {
            UpdateManualRivalOffBallSupport();
            return;
        }

        Transform carrier = rivalTeam[manualRivalOwnerIndex];
        manualRivalDecisionTimer -= Time.deltaTime;

        if (manualRivalDecisionTimer <= 0f && IsRivalCloseToShoot(carrier))
        {
            manualRivalActionRoutine = StartCoroutine(ManualRivalShootAtGoal(carrier, manualRivalOwnerIndex));
            return;
        }

        if (manualRivalDecisionTimer <= 0f)
        {
            int receiverIndex = FindBestRivalPassReceiver(carrier);

            if (receiverIndex >= 0)
            {
                manualRivalActionRoutine = StartCoroutine(ManualRivalPassToReceiver(carrier, receiverIndex));
                return;
            }

            manualRivalDecisionTimer = manualRivalDecisionInterval;
        }

        Vector3 target = GetManualRivalCarryTarget(carrier);

        MoveManualPlayerToward(carrier, rivalAnimators[manualRivalOwnerIndex], target, manualRivalPressureSpeed);
        AttachBall(carrier);

        UpdateManualRivalOffBallSupport();
    }

    private void UpdateManualRivalOffBallSupport()
    {
        for (int i = 0; i < rivalTeam.Length; i++)
        {
            if (!IsRivalPlayerAvailable(i) || i == manualRivalOwnerIndex) continue;

            Vector3 supportTarget = GetManualRivalSupportTarget(rivalTeam[i], i);
            MoveManualPlayerToward(rivalTeam[i], rivalAnimators[i], supportTarget, supportMoveSpeed);
        }
    }

    private bool IsRivalCloseToShoot(Transform carrier)
    {
        if (carrier == null) return false;

        return Mathf.Abs(carrier.position.x - GetPlayerGoalLineX()) <= manualRivalShootDistance;
    }

    private int FindBestRivalPassReceiver(Transform passer)
    {
        if (passer == null) return -1;

        int bestIndex = -1;
        float bestScore = -9999f;
        float attackDirection = Mathf.Sign(playerGoalX - rivalGoalX);

        for (int i = 0; i < rivalTeam.Length; i++)
        {
            if (i == manualRivalOwnerIndex || !IsRivalPlayerAvailable(i)) continue;

            Transform receiver = rivalTeam[i];
            float distance = GetFlatDistance(passer.position, receiver.position);

            if (distance < minPassDistance || distance > manualPassMaxDistance) continue;

            float progress = (receiver.position.x - passer.position.x) * attackDirection;
            float width = Mathf.Abs(receiver.position.z - passer.position.z);
            float pressure = GetNearestOpponentDistance(Team.Rival, receiver.position);
            float laneRisk = GetPassLaneRisk(Team.Rival, passer.position, receiver.position);
            float goalLane = Mathf.Abs(receiver.position.z - goalCenterZ);
            float score = progress * 1.9f
                + width * 0.25f
                + pressure * 0.5f
                - distance * 0.12f
                - laneRisk * passLaneRiskPenalty
                - goalLane * 0.05f
                + Random.Range(-0.5f, 0.5f);

            if (progress < -2f)
            {
                score -= 5f;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private IEnumerator ManualRivalPassToReceiver(Transform passer, int receiverIndex)
    {
        Transform receiver = IsRivalPlayerAvailable(receiverIndex) ? rivalTeam[receiverIndex] : null;

        if (passer != null && receiver != null && ball != null)
        {
            Trigger(rivalAnimators[manualRivalOwnerIndex], PassHash);
            AttachBall(passer);

            Vector3 start = ball.position;
            Vector3 end = GetBallPositionNear(receiver);

            yield return MoveBallArc(start, end, manualRivalPassDuration, passArcHeight);

            manualRivalOwnerIndex = receiverIndex;
            ownerIndex = receiverIndex;
            manualRivalDecisionTimer = manualRivalDecisionInterval;
            Trigger(rivalAnimators[receiverIndex], ReceiveHash);
            AttachBall(receiver);
        }

        manualRivalActionRoutine = null;
    }

    private IEnumerator ManualRivalShootAtGoal(Transform shooter, int shooterIndex)
    {
        if (shooter == null || ball == null)
        {
            manualRivalActionRoutine = null;
            yield break;
        }

        Trigger(rivalAnimators[shooterIndex], ShootHash);
        SetSpeed(rivalAnimators[shooterIndex], 0f);
        LookAtFlatSmooth(shooter, new Vector3(GetPlayerGoalLineX(), playerGroundY, goalCenterZ), manualAiTurnSpeed * 1.5f);

        if (manualKickDelay > 0f)
        {
            yield return new WaitForSeconds(manualKickDelay);
        }

        if (shooter != null && ball != null)
        {
            AttachBall(shooter);

            manualRivalHasBall = false;
            manualRivalOwnerIndex = -1;
            manualBallAttached = false;
            manualBallReleaseTimer = manualKickReleaseSeconds;
            manualStealCooldown = manualStealCooldownSeconds;
            playerPossession = false;
            ownerIndex = shooterIndex;

            float attackDirection = Mathf.Sign(playerGoalX - rivalGoalX);
            Vector3 goalTarget = new Vector3(
                GetPlayerGoalLineX() + attackDirection * 3f,
                ball.position.y,
                goalCenterZ + Random.Range(-goalWidth * 0.22f, goalWidth * 0.22f)
            );

            ReleaseBallToward(goalTarget, manualKickForce * manualRivalShotForceMultiplier);

            UpdateUI(
                "- Team Rival remata al arco\n- Recupera el balon si rebota",
                GetName(Team.Rival, shooterIndex),
                "Shoot"
            );
        }

        manualRivalDecisionTimer = manualRivalDecisionInterval;
        manualRivalActionRoutine = null;
    }

    private Vector3 GetManualRivalSupportTarget(Transform rival, int index)
    {
        Vector3 reference = manualRivalHasBall && IsRivalPlayerAvailable(manualRivalOwnerIndex)
            ? rivalTeam[manualRivalOwnerIndex].position
            : ball.position;

        float attackDirection = Mathf.Sign(playerGoalX - rivalGoalX);
        float lane = GetManualRivalLaneOffset(index);
        float depth = manualRivalHasBall ? 9f + index % 3 * 2f : 5f + index % 2 * 2f;

        Vector3 target = new Vector3(
            reference.x + attackDirection * depth,
            playerGroundY,
            Mathf.Lerp(reference.z, goalCenterZ + lane, 0.72f)
        );

        return ClampToField(target);
    }

    private Vector3 GetManualRivalCarryTarget(Transform carrier)
    {
        if (carrier == null)
        {
            return ClampToField(new Vector3(GetPlayerGoalLineX(), playerGroundY, goalCenterZ));
        }

        float attackDirection = Mathf.Sign(playerGoalX - rivalGoalX);
        Vector3 target = new Vector3(
            carrier.position.x + attackDirection * 8f,
            playerGroundY,
            Mathf.Lerp(carrier.position.z, goalCenterZ, 0.35f)
        );

        Transform nearestPlayer = GetNearestOpponent(Team.Rival, carrier.position);

        if (nearestPlayer != null)
        {
            float pressureDistance = GetFlatDistance(carrier.position, nearestPlayer.position);
            float pressure = Mathf.Clamp01((manualRivalDribblePressureDistance - pressureDistance) / manualRivalDribblePressureDistance);

            if (pressure > 0f)
            {
                Vector3 away = carrier.position - nearestPlayer.position;
                away.y = 0f;

                if (away.sqrMagnitude > 0.001f)
                {
                    target += away.normalized * pressure * 4f;
                    target += Vector3.right * attackDirection * pressure * 3f;
                }
            }
        }

        return ClampToField(target);
    }

    private Vector3 GetManualRivalMarkingTarget(Transform rival, Transform targetPlayer)
    {
        if (rival == null || targetPlayer == null)
        {
            return rival != null ? rival.position : Vector3.zero;
        }

        float attackDirection = Mathf.Sign(playerGoalX - rivalGoalX);
        Vector3 offset = Vector3.right * attackDirection * 2.4f;

        return ClampToField(targetPlayer.position + offset);
    }

    private float GetManualRivalLaneOffset(int index)
    {
        int lane = Mathf.Abs(index) % 5;

        if (lane == 0) return 0f;
        if (lane == 1) return -12f;
        if (lane == 2) return 12f;
        if (lane == 3) return -6f;

        return 6f;
    }

    private int FindNearestAvailableRivalTo(Vector3 position)
    {
        int bestIndex = -1;
        float bestDistance = 9999f;

        for (int i = 0; i < rivalTeam.Length; i++)
        {
            if (!IsRivalPlayerAvailable(i)) continue;

            float distance = GetFlatDistance(rivalTeam[i].position, position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void ManualRivalSteal(int rivalIndex)
    {
        if (!IsRivalPlayerAvailable(rivalIndex)) return;

        manualRivalHasBall = true;
        manualRivalOwnerIndex = rivalIndex;
        manualBallAttached = false;
        manualBallReleaseTimer = 0f;
        manualStealCooldown = manualStealCooldownSeconds;
        manualRivalDecisionTimer = manualRivalDecisionInterval * 0.5f;
        playerPossession = false;
        ownerIndex = rivalIndex;

        Trigger(rivalAnimators[rivalIndex], TackleHash);
        AttachBall(rivalTeam[rivalIndex]);

        UpdateUI(
            "- Team Rival roba el balon\n- Acercate para recuperarlo",
            GetName(Team.Rival, rivalIndex),
            "Steal"
        );
    }

    private void MoveManualPlayerToward(Transform player, Animator animator, Vector3 target, float speed)
    {
        if (player == null) return;

        target = ClampToField(target);
        Vector3 toTarget = target - player.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        if (distance > 0.08f)
        {
            Vector3 direction = toTarget / distance;
            RotateFlatSmooth(player, direction, manualAiTurnSpeed);
            player.position = ClampToField(player.position + direction * Mathf.Min(speed * Time.deltaTime, distance));

            float animationSpeed = Mathf.Clamp(speed / Mathf.Max(0.01f, manualRunSpeed), 0.35f, 0.85f);
            SetSpeed(animator, animationSpeed);
        }
        else
        {
            SetSpeed(animator, 0f);
            LookAtFlatSmooth(player, ball != null ? ball.position : target, manualAiTurnSpeed * 0.65f);
        }
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

private float GetFieldMinX()
{
    return playAreaCenter.x - playAreaSize.x * 0.5f;
}

private float GetFieldMaxX()
{
    return playAreaCenter.x + playAreaSize.x * 0.5f;
}

private float GetPlayerGoalLineX()
{
    return Mathf.Max(playerGoalX, GetFieldMinX());
}

private float GetRivalGoalLineX()
{
    return Mathf.Min(rivalGoalX, GetFieldMaxX());
}

private Vector3 ClampToField(Vector3 target)
{
    float halfWidth = playAreaSize.x * 0.5f;
    float halfDepth = playAreaSize.y * 0.5f;

    target.x = Mathf.Clamp(target.x, playAreaCenter.x - halfWidth + playAreaPadding, playAreaCenter.x + halfWidth - playAreaPadding);
    target.z = Mathf.Clamp(target.z, playAreaCenter.z - halfDepth + playAreaPadding, playAreaCenter.z + halfDepth - playAreaPadding);
    return WithGroundY(target);
}

private void CheckManualGoal()
{
    if (ball == null || manualGoalResetRoutine != null) return;

    float halfGoal = goalWidth * 0.5f;

    if (Mathf.Abs(ball.position.z - goalCenterZ) > halfGoal)
    {
        return;
    }

    if (ball.position.x >= GetRivalGoalLineX())
    {
        manualGoalResetRoutine = StartCoroutine(ManualGoalReset(true));
    }
    else if (ball.position.x <= GetPlayerGoalLineX())
    {
        manualGoalResetRoutine = StartCoroutine(ManualGoalReset(false));
    }
}

private IEnumerator ManualGoalReset(bool playerScored)
{
    if (manualRivalActionRoutine != null)
    {
        StopCoroutine(manualRivalActionRoutine);
        manualRivalActionRoutine = null;
    }

    ShowGoalPanel(playerScored);

    if (playerScored)
    {
        ecuadorScore++;
        manualRivalHasBall = false;
        manualRivalOwnerIndex = -1;

        UpdateUI(
            "- GOL de Team Player\n- El balon vuelve al jugador controlado",
            "Team Player",
            "Goal"
        );
    }
    else
    {
        cpuScore++;
        manualRivalHasBall = false;
        manualRivalOwnerIndex = -1;

        UpdateUI(
            "- GOL de Team Rival\n- Recupera la posesion",
            "Team Rival",
            "Goal"
        );
    }

    yield return new WaitForSeconds(Mathf.Max(goalResetDelay, goalPanelSeconds));

    ResetManualMatchPositions();
    manualBallAttached = false;
    manualBallReleaseTimer = 0f;
    manualStealCooldown = manualStealCooldownSeconds;
    manualRivalDecisionTimer = manualRivalDecisionInterval;

    SelectManualPlayer(manualPlayerIndex, false);

    HideGoalPanel();
    manualGoalResetRoutine = null;
}

private void ResetManualMatchPositions()
{
    for (int i = 0; i < playerTeam.Length; i++)
    {
        if (!IsManualPlayerAvailable(i)) continue;

        playerTeam[i].position = playerBase[i];
        playerTeam[i].rotation = playerBaseRotation[i];
        playerBusy[i] = false;
        SetSpeed(playerAnimators[i], 0f);
    }

    for (int i = 0; i < rivalTeam.Length; i++)
    {
        if (!IsRivalPlayerAvailable(i)) continue;

        rivalTeam[i].position = rivalBase[i];
        rivalTeam[i].rotation = rivalBaseRotation[i];
        rivalBusy[i] = false;
        SetSpeed(rivalAnimators[i], 0f);
    }

    if (ballRb != null)
    {
        if (!ballRb.isKinematic)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }

        ballRb.useGravity = false;
        ballRb.isKinematic = true;
    }

    manualRivalHasBall = false;
    manualRivalOwnerIndex = -1;
    manualRivalDecisionTimer = manualRivalDecisionInterval;
    manualBallAttached = false;
    PlaceBallAtInitialTransform();

    if (manualRivalActionRoutine != null)
    {
        StopCoroutine(manualRivalActionRoutine);
        manualRivalActionRoutine = null;
    }
}

    private void AttachBall(Transform owner)
    {
        if (owner == null || ball == null) return;

        if (ballRb != null)
        {
            if (!ballRb.isKinematic)
            {
                ballRb.linearVelocity = Vector3.zero;
                ballRb.angularVelocity = Vector3.zero;
            }

            ballRb.useGravity = false;
            ballRb.isKinematic = true;
        }

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

    private void LookAtFlatSmooth(Transform who, Vector3 target, float turnSpeed)
    {
        if (who == null) return;

        Vector3 direction = target - who.position;
        direction.y = 0f;

        RotateFlatSmooth(who, direction, turnSpeed);
    }

    private void RotateFlatSmooth(Transform who, Vector3 direction, float turnSpeed)
    {
        if (who == null) return;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        float blend = Mathf.Clamp01(turnSpeed * Time.deltaTime);
        who.rotation = Quaternion.Slerp(who.rotation, targetRotation, blend);
    }

    private void SetSpeed(Animator animator, float value)
    {
        if (animator == null) return;
        if (!HasAnimatorParameter(animator, SpeedHash, AnimatorControllerParameterType.Float)) return;

        value = Mathf.Clamp01(value);

        if (animationSpeedDampTime <= 0f || Time.deltaTime <= 0f)
        {
            animator.SetFloat(SpeedHash, value);
            return;
        }

        animator.SetFloat(SpeedHash, value, animationSpeedDampTime, Time.deltaTime);
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

    private void EnsureGoalPanel()
    {
        if (!showGoalPanel || goalPanelObject != null)
        {
            return;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("FootballOS_Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        goalPanelObject = new GameObject("FootballOS_GoalPanel");
        goalPanelObject.transform.SetParent(canvas.transform, false);
        goalPanelObject.transform.SetAsLastSibling();

        RectTransform panelRect = goalPanelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        goalPanelImage = goalPanelObject.AddComponent<Image>();
        goalPanelImage.raycastTarget = false;

        goalPanelGroup = goalPanelObject.AddComponent<CanvasGroup>();
        goalPanelGroup.alpha = 0f;
        goalPanelGroup.blocksRaycasts = false;
        goalPanelGroup.interactable = false;

        GameObject textObject = new GameObject("GoalText");
        textObject.transform.SetParent(goalPanelObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.05f, 0.25f);
        textRect.anchorMax = new Vector2(0.95f, 0.75f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        goalPanelText = textObject.AddComponent<TextMeshProUGUI>();
        goalPanelText.alignment = TextAlignmentOptions.Center;
        goalPanelText.fontSize = 78f;
        goalPanelText.enableAutoSizing = true;
        goalPanelText.fontSizeMin = 36f;
        goalPanelText.fontSizeMax = 92f;
        goalPanelText.fontStyle = FontStyles.Bold;
        goalPanelText.color = Color.white;
        goalPanelText.raycastTarget = false;

        goalPanelObject.SetActive(false);
    }

    private void ShowGoalPanel(bool playerScored)
    {
        if (!showGoalPanel)
        {
            return;
        }

        EnsureGoalPanel();

        if (goalPanelObject == null || goalPanelText == null || goalPanelImage == null || goalPanelGroup == null)
        {
            return;
        }

        goalPanelText.text = playerScored ? "GOOOL\nEquipo Team Player" : "GOOOL\nEquipo Team Rival";
        goalPanelImage.color = playerScored ? playerGoalPanelColor : rivalGoalPanelColor;
        goalPanelGroup.alpha = 1f;
        goalPanelObject.SetActive(true);
    }

    private void HideGoalPanel()
    {
        if (goalPanelGroup != null)
        {
            goalPanelGroup.alpha = 0f;
        }

        if (goalPanelObject != null)
        {
            goalPanelObject.SetActive(false);
        }
    }

    private void UpdateUI(string eventLog, string playerInControl, string action)
    {
        if (uiController == null) return;

        uiController.SetEventLog(
            "TIEMPO: " + minute.ToString("00") + "'  MARCADOR: Team Player " + ecuadorScore + " - " + cpuScore + " Team Rival\n" +
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
