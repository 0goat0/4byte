using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IPlayerState
{
    void Enter(PlayerStats player);
    void Exit(PlayerStats player);
    void Tick(PlayerStats player);
}
public enum PlayerStateType
{
    Idle,
    Detect,
    Chase,
    Move,
    Attack,
    AttackMove,
    Dead
}

public class PlayerStats : NetworkBehaviour, IDamageable, IHealthSource
{
    [Header("Data")]
    //이름, 크기(소형, 중형, 대형), 공격타입(근접, 원거리, 광역)
    //체력, 공격력, 방어력, 공격속도, 이동속도
    //드랍골드
    //몬스터 프리펩
    [SerializeField] private PlayerData data;
    public PlayerData Data => data;
    public float CurrentHealth => CurrentHp;
    public float MaxHealth => data != null ? data.hp : 0f;
    public event System.Action<float, float> OnHealthChanged;

    //[Networked, OnChangedRender(nameof(OnPlayerNameChanged))]
    //public NetworkString<_32> playerName { get; set; }
    //[SerializeField] private TextMeshPro playerNameLabel;

    private TextMeshProUGUI uiNameLabel;


    [Header("Detection")]
    public float AttackRange { get { return attackRange; } }
    [SerializeField] private float attackRange;
    public float DetectRange { get { return detectRange; } }
    [SerializeField] private float detectRange;
    public float AttackInterval { get { return attackInterval; } }
    [SerializeField] private float attackInterval;

    [Networked] public float attackDamage {  get; set; }
    [Networked] public float defense {  get; set; }
    [Networked] public float attackSpeed {  get; set; }
    [Networked] public float MoveSpeed { get; set; }


    [SerializeField] private LayerMask targetLayerMask;
    public LayerMask TargetLayerMask { get { return targetLayerMask; } }

    //상태 체크
    [Networked, OnChangedRender(nameof(OnStateTypeChanged))]
    public PlayerStateType StateType { get; private set; }
    [Networked, OnChangedRender(nameof(OnCurrentHpChanged))] public float CurrentHp { get; set; }
    [Networked] public NetworkObject Target { get; set; }
    [Networked] public TickTimer AttackCooldown { get; set; } // 쿨타임 타이머
    [Networked] public TickTimer DetectTimer { get; set; }
    [Networked] public TickTimer DeathTimer { get; set; }
    public float DespawnDelay = 2f;

    [Networked]
    public Vector3 CommandDestination { get; private set; }

    public PlayerAnimeController Animator { get; private set; }
    public NetworkNavMeshMover Mover { get; private set; }

    private const int TargetBufferCapacity = 16;

    private readonly Collider[] _targetResults = new Collider[TargetBufferCapacity];
    private Dictionary<PlayerStateType, IPlayerState> _states;
    private IPlayerState _currentState;
    private WorldHealthBarTarget _healthBarTarget;

    [OnChangedRender(nameof(OnKillsChanged))]
    [Networked] public int Kills { get; set; }
    [Networked] public int AttackLevel { get; set; }
    [Networked] public int DefenseLevel { get; set; }

    private void OnKillsChanged()
    {
        var ui = FindAnyObjectByType<PlayerStatsUI>();
        if (ui != null)
        {
            ui.UpdateTotalKillUI();
        }
    }
    private void Awake()
    {
        Mover = GetComponent<NetworkNavMeshMover>();
        Animator = GetComponentInChildren<PlayerAnimeController>();
    }
    public override void Spawned()
    {
        var ui = FindAnyObjectByType<PlayerStatsUI>();
        if (ui != null)
        {
            ui.UpdateTotalKillUI();
        }

        _states = new Dictionary<PlayerStateType, IPlayerState>
        {
            { PlayerStateType.Idle, new PlayerIdleState() },
            { PlayerStateType.Detect, new PlayerDetectState() },
            { PlayerStateType.Chase, new PlayerChaseState() },
            { PlayerStateType.Move, new PlayerMoveState() },
            { PlayerStateType.Attack, new PlayerAttackState() },
            { PlayerStateType.AttackMove, new PlayerAttackMoveState() },
            { PlayerStateType.Dead, new PlayerDeadState() }
        };


        if (Object.HasStateAuthority)
        {
            if (data != null)
            {
                CurrentHp = data.hp;
                attackDamage = data.attack;
                defense = data.defense;
                attackSpeed = data.attackSpeed;
                MoveSpeed = data.moveSpeed;
            }
            else
            {
                MoveSpeed = 3f;
                Debug.LogWarning("PlayerData is not assigned.", this);
            }

            ChangeState(PlayerStateType.Idle);
        }

        ApplyStateVisual(StateType);

        _healthBarTarget = WorldHealthBarTarget.Attach(gameObject, this);

        #region Name
        //if (Object.HasInputAuthority)
        //{
        //    if (FusionConnection.instance != null && !string.IsNullOrEmpty(FusionConnection.instance._playerName))
        //    {
        //        string localName = FusionConnection.instance._playerName;

        //        UpdateNameUI(localName);
        //        RpcSetPlayerName(localName);
        //    }
        //}
        //else
        //{
        //    string currentNetName = playerName.ToString();
        //    UpdateNameUI(string.IsNullOrEmpty(currentNetName) ? "Connecting..." : currentNetName);
        //}
        #endregion
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        _healthBarTarget?.Release();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        _currentState?.Tick(this);
    }

    #region Name
    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    //private void RpcSetPlayerName(string nameInput, RpcInfo info = default)
    //{
    //    playerName = nameInput;
    //}
    //private void OnPlayerNameChanged()
    //{
    //    if(Object.HasInputAuthority)
    //    {
    //        return;
    //    }

    //    UpdateNameUI(playerName.ToString());
    //}
    //private void UpdateNameUI(string nameToDisplay)
    //{
    //    if (string.IsNullOrEmpty(nameToDisplay) || nameToDisplay == "null")
    //    {
    //        nameToDisplay = "Connectinggg...";
    //    }

    //    if (playerNameLabel != null)
    //    {
    //        playerNameLabel.text = nameToDisplay;
    //    }

    //    if (uiNameLabel == null)
    //    {
    //        GameObject mainCanvas = GameObject.Find("Main Canvars");
    //        if (mainCanvas != null)
    //        {
    //            Transform targetTransform = mainCanvas.transform.Find("In Game UI/PlayerTeam/UI Player Name");

    //            if (targetTransform != null)
    //            {
    //                uiNameLabel = targetTransform.GetComponent<TextMeshProUGUI>();
    //            }
    //        }
    //    }
    //    if (uiNameLabel != null)
    //    {
    //        uiNameLabel.text = nameToDisplay;
    //    }
    //}
    #endregion

    public void ChangeState(PlayerStateType stateType)
    {
        if (!HasStateAuthority)
            return;

        if (!_states.TryGetValue(stateType, out IPlayerState nextState))
        {
            return;
        }

        _currentState?.Exit(this);

        StateType = stateType;
        _currentState = nextState;

        _currentState.Enter(this);
    }

    private void OnStateTypeChanged()
    {
        ApplyStateVisual(StateType);
    }

    private void OnCurrentHpChanged()
    {
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    private void ApplyStateVisual(PlayerStateType stateType)
    {
        if (Animator == null)
            return;

        Animator.SetState(stateType);
    }

    //스텟 업글
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcUpgradeStats(bool isAttack)
    {
        if (!HasStateAuthority || data == null) return;

        if (isAttack)
        {
            this.attackDamage += 1f;
        }
        else
        {
            this.defense += 1f;
        }

        Debug.Log($"업그레이드 완료");
    }
    public void CommandMove(Vector3 destination)
    {
        if (!HasStateAuthority ||
            StateType == PlayerStateType.Dead)
        {
            return;
        }

        Target = null;
        CommandDestination = destination;

        ChangeState(PlayerStateType.Move);
    }

    public void CommandAttackTarget(NetworkObject target)
    {
        if (!HasStateAuthority ||
            target == null ||
            StateType == PlayerStateType.Dead)
        {
            return;
        }

        Target = target;

        ChangeState(PlayerStateType.Chase);
    }


    public void CommandAttackMove(Vector3 destination)
    {
        if (!HasStateAuthority ||
            StateType == PlayerStateType.Dead)
        {
            return;
        }

        Target = null;
        CommandDestination = destination;

        ChangeState(PlayerStateType.AttackMove);
    }

    public bool TryFindTarget(out NetworkObject target)
    {
        target = null;

        if (!HasStateAuthority)
            return false;

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            DetectRange,
            _targetResults,
            TargetLayerMask,
            QueryTriggerInteraction.Ignore);

        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            NetworkObject candidate =
                _targetResults[i].GetComponentInParent<NetworkObject>();

            if (candidate == null || candidate == Object)
                continue;

            float distanceSqr =
                (candidate.transform.position - transform.position).sqrMagnitude;

            if (distanceSqr >= nearestDistanceSqr)
                continue;

            nearestDistanceSqr = distanceSqr;
            target = candidate;
        }

        return target != null;
    }

    public void TakeDamage(float damage, NetworkObject attacker)
    {
        if (!HasStateAuthority || StateType == PlayerStateType.Dead)
            return;

        float finalDamage = Mathf.Max((int)damage - data.defense, 1f);
        CurrentHp = Mathf.Clamp(CurrentHp - finalDamage, 0, data.hp);

        if (CurrentHp <= 0f)
        {
            CurrentHp = 0f;
            ChangeState(PlayerStateType.Dead);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
