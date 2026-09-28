using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 순차 진행되는 스포너들이 공통으로 구현 (EnemySpawner, BossSpawner)
public interface IStageSpawner
{
    NetworkBool IsDestroyed { get; }
    void SetSpawnerActive(bool active);
}

// 플레이어 베이스 건물이 구현 (전부 파괴되면 게임 패배)
public interface IBaseBuilding
{
    NetworkBool IsDestroyed { get; }
}

public enum GameManagerState
{
    WaitingForTutorial, // (옵션) 튜토리얼 종료 대기
    Preparing,          // 카운트다운 중
    StageInProgress,    // 일반 스포너 진행 중
    BossStage,          // 마지막 스포너(보스) 진행 중
    Cleared,
    GameOver
}

// 게임 진행(네트워크) + 시작 튜토리얼 UI(로컬)를 하나로 합친 매니저.
// 진행 로직은 호스트(StateAuthority)만, UI는 모든 클라이언트가 각자 로컬로 처리.
public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    // ───────────── 게임 진행 ─────────────
    [Header("게임 진행")]
    [SerializeField] private float startDelay = 30f;
    [Tooltip("true: 튜토리얼 종료 시 카운트다운 시작 (싱글 추천) / false: 게임 시작과 동시에 카운트다운 (멀티 추천)")]
    [SerializeField] private bool startCountdownAfterTutorial = false;

    [Tooltip("순서대로 등록: 일반 스포너들 + 마지막에 BossSpawner")]
    [SerializeField] private MonoBehaviour[] stageSpawnerBehaviours;
    private readonly List<IStageSpawner> stageSpawners = new List<IStageSpawner>();

    [Networked, OnChangedRender(nameof(OnGameStateChanged))]
    public GameManagerState State { get; set; }
    [Networked] public int StageIndex { get; set; }
    [Networked] private TickTimer StartTimer { get; set; }

    private EnemyAI bossEnemyAI;

    [Header("패배 조건: 여기 등록한 플레이어 건물이 전부 파괴되면 게임 오버")]
    [SerializeField] private MonoBehaviour[] playerBuildingBehaviours;
    private readonly List<IBaseBuilding> playerBuildings = new List<IBaseBuilding>();

    // ───────────── 튜토리얼 UI (로컬) ─────────────
    [Header("UI")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private Button nextButton;

    [Header("카운트다운 UI (화면 상단 중앙 / tutorialCanvas와 별개 오브젝트에 둘 것)")]
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private string firstSpawnFormat = "First Spawn {0} Second";
    [SerializeField] private string nextWaveFormat = "Next Wave {0} Second";
    private int lastShownSeconds = -1;
    private int lastShownMode;

    [Header("Tutorial Texts")]
    [TextArea(3, 5)]
    [SerializeField] private List<string> startTexts;

    [Header("종료 메시지 / 씬")]
    [SerializeField] private string clearMessage = "Congratulations! You have completed the tutorial by defeating the boss";
    [SerializeField] private string gameOverMessage = "Game Over";
    [SerializeField] private float exitDelay = 10f;
    [SerializeField] private string nextSceneName = "MainMenuScene";

    private int currentTextIndex;
    private bool isExiting;

    // ───────────── 초기화 ─────────────
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        stageSpawners.Clear();
        foreach (var b in stageSpawnerBehaviours)
        {
            if (b == null) continue;

            if (b is IStageSpawner s)
            {
                stageSpawners.Add(s);
            }
            else if (b.TryGetComponent(out IStageSpawner found))
            {
                // NetworkObject 등이 들어갔어도 같은 오브젝트의 스포너를 찾아 등록
                stageSpawners.Add(found);
            }
            else
            {
                Debug.LogWarning($"[GameManager] {b.name}({b.GetType().Name})에서 IStageSpawner를 찾지 못함");
            }
        }

        playerBuildings.Clear();
        foreach (var b in playerBuildingBehaviours)
        {
            if (b == null) continue;

            if (b is IBaseBuilding building)
            {
                playerBuildings.Add(building);
            }
            else if (b.TryGetComponent(out IBaseBuilding found))
            {
                playerBuildings.Add(found);
            }
            else
            {
                Debug.LogWarning($"[GameManager] {b.name}({b.GetType().Name})에서 IBaseBuilding을 찾지 못함");
            }
        }
    }

    private void Start()
    {
        StartTutorial();
    }

    public override void Spawned()
    {
        if (!HasStateAuthority) return;

        StageIndex = 0;
        if (startCountdownAfterTutorial)
        {
            State = GameManagerState.WaitingForTutorial;
        }
        else
        {
            BeginCountdown();
        }
    }
    private bool IsStageSpawnerDone(IStageSpawner s)
    {
        if (s is NetworkBehaviour nb)
        {
            if (nb == null) return true;                                // Unity에서 Destroy됨
            if (nb.Object == null || !nb.Object.IsValid) return true;   // Fusion에서 디스폰됨
        }
        return s.IsDestroyed;
    }
    private void BeginCountdown()
    {
        State = GameManagerState.Preparing;
        StartTimer = TickTimer.CreateFromSeconds(Runner, startDelay);
    }

    // ───────────── 카운트다운 UI (모든 클라이언트, 매 프레임) ─────────────
    public override void Render()
    {
        if (countdownText == null) return;

        float? remaining = null;
        string format = null;
        int mode = 0;

        if (State == GameManagerState.Preparing)
        {
            // 게임 시작 후 첫 스포너가 켜지기까지
            remaining = StartTimer.RemainingTime(Runner);
            format = firstSpawnFormat;
            mode = 1;
        }
        else if (State == GameManagerState.StageInProgress && StageIndex < stageSpawners.Count)
        {
            // 현재 진행 중인 스포너의 다음 웨이브까지
            IStageSpawner current = stageSpawners[StageIndex];
            if (!IsStageSpawnerDone(current) && current is EnemySpawner enemySpawner)
            {
                remaining = enemySpawner.GetNextWaveRemainingTime();
                format = nextWaveFormat;
                mode = 2;
            }
        }

        if (remaining == null || format == null)
        {
            if (countdownText.gameObject.activeSelf) countdownText.gameObject.SetActive(false);
            lastShownSeconds = -1;
            lastShownMode = 0;
            return;
        }

        if (!countdownText.gameObject.activeSelf) countdownText.gameObject.SetActive(true);

        int seconds = Mathf.CeilToInt(remaining.Value);
        if (seconds == lastShownSeconds && mode == lastShownMode) return; // 값이 같으면 갱신 생략

        lastShownSeconds = seconds;
        lastShownMode = mode;
        countdownText.text = string.Format(format, seconds);
    }

    // ───────────── 진행 로직 (호스트 전용) ─────────────
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        switch (State)
        {
            case GameManagerState.Preparing:
                if (StartTimer.Expired(Runner))
                {
                    StartTimer = TickTimer.None;
                    ActivateStage(0);
                }
                break;

            case GameManagerState.StageInProgress:
                TickStageProgress();
                CheckAllPlayerBuildingsDestroyed();
                break;

            case GameManagerState.BossStage:
                TickBossStage();
                CheckAllPlayerBuildingsDestroyed();
                break;
        }
    }

    private void ActivateStage(int index)
    {
        if (index >= stageSpawners.Count)
        {
            Debug.LogWarning("[GameManager] 등록된 스포너가 없거나 인덱스 초과");
            return;
        }

        StageIndex = index;
        bool isLastStage = index == stageSpawners.Count - 1;
        State = isLastStage ? GameManagerState.BossStage : GameManagerState.StageInProgress;

        stageSpawners[index].SetSpawnerActive(true);
        Debug.Log($"[GameManager] {index}번째 스포너 활성화 (보스: {isLastStage})");
    }

    private void TickStageProgress()
    {
        if (!IsStageSpawnerDone(stageSpawners[StageIndex])) return;
        ActivateStage(StageIndex + 1);
    }

    private void TickBossStage()
    {
        if (!IsStageSpawnerDone(stageSpawners[StageIndex])) return; // 보스 스포너 아직 안 부서짐
        if (bossEnemyAI == null) return;                    // 보스 등록 대기

        if (!bossEnemyAI.IsAlive)
        {
            State = GameManagerState.Cleared;
            Debug.Log("[GameManager] 보스 처치 - 게임 클리어");
        }
    }

    private bool IsPlayerBuildingDestroyed(IBaseBuilding b)
    {
        if (b is NetworkBehaviour nb)
        {
            if (nb == null) return true;                                // Unity에서 Destroy됨
            if (nb.Object == null || !nb.Object.IsValid) return true;   // Fusion에서 디스폰됨
        }
        return b.IsDestroyed;
    }

    // 등록된 플레이어 건물이 전부 파괴되면 게임 오버
    private void CheckAllPlayerBuildingsDestroyed()
    {
        if (playerBuildings.Count == 0) return; // 등록 안 했으면 판정 안 함

        // 같은 틱에 보스 클리어가 먼저 처리됐다면 덮어쓰지 않음
        if (State != GameManagerState.StageInProgress && State != GameManagerState.BossStage) return;

        foreach (var b in playerBuildings)
        {
            if (!IsPlayerBuildingDestroyed(b)) return; // 하나라도 살아있으면 계속 진행
        }

        State = GameManagerState.GameOver;
        Debug.Log("[GameManager] 플레이어 건물 전부 파괴 - 게임 오버");
    }

    public void RegisterBoss(NetworkObject bossObject)
    {
        if (bossObject != null) bossEnemyAI = bossObject.GetComponent<EnemyAI>();
    }

    // 튜토리얼 종료 후 카운트다운 시작 (startCountdownAfterTutorial 옵션용)
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_BeginCountdown()
    {
        if (State != GameManagerState.WaitingForTutorial) return;
        BeginCountdown();
    }

    // ───────────── 상태 변경 → 모든 클라이언트 UI ─────────────
    private void OnGameStateChanged()
    {
        if (State == GameManagerState.Cleared)
        {
            ShowEndMessage(clearMessage);
        }
        else if (State == GameManagerState.GameOver)
        {
            ShowEndMessage(gameOverMessage);
        }
    }

    // ───────────── 튜토리얼 UI ─────────────
    private void StartTutorial()
    {
        if (startTexts == null || startTexts.Count == 0)
        {
            // 튜토리얼 텍스트가 없으면 바로 종료 처리
            NotifyTutorialEnded();
            return;
        }

        tutorialCanvas.SetActive(true);
        tutorialText.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);

        currentTextIndex = 0;
        tutorialText.text = startTexts[0];

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNextButtonClick);
    }

    private void OnNextButtonClick()
    {
        currentTextIndex++;

        if (currentTextIndex < startTexts.Count)
        {
            tutorialText.text = startTexts[currentTextIndex];
        }
        else
        {
            EndStartTutorial();
        }
    }

    private void EndStartTutorial()
    {
        nextButton.gameObject.SetActive(false);
        tutorialCanvas.SetActive(false);
        Debug.Log("시작 튜토리얼 종료");
        NotifyTutorialEnded();
    }

    private void NotifyTutorialEnded()
    {
        if (startCountdownAfterTutorial && Object != null && Object.IsValid)
        {
            RPC_BeginCountdown();
        }
    }

    // 튜토리얼 진행 여부와 관계없이 강제로 종료 메시지 표시
    private void ShowEndMessage(string message)
    {
        if (isExiting) return;
        isExiting = true;

        tutorialCanvas.SetActive(true);
        tutorialText.gameObject.SetActive(true);
        tutorialText.text = message;
        nextButton.gameObject.SetActive(false);

        ExitAfterDelay();
    }

    // ───────────── 씬 종료 ─────────────
    private async void ExitAfterDelay()
    {
        await Task.Delay(Mathf.RoundToInt(exitDelay * 1000f));

        // Runner를 먼저 정리한 뒤 씬 이동 (오브젝트가 파괴돼도 async 흐름은 계속 진행됨)
        NetworkRunner runner = Runner;
        if (runner != null)
        {
            await runner.Shutdown();
        }

        Debug.Log("씬 이동: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }
}