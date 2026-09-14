using Fusion;
using System.Collections.Generic;
using TMPro;
using Unity.IO.LowLevel.Unsafe;
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
    Attack,
    Dead
}

public class PlayerStats : NetworkBehaviour
{
    [Header("Data")]
    //이름, 크기(소형, 중형, 대형), 공격타입(근접, 원거리, 광역)
    //체력, 공격력, 방어력, 공격속도, 이동속도
    //드랍골드
    //몬스터 프리펩
    [SerializeField] private PlayerData data;

    [Networked, OnChangedRender(nameof(OnPlayerNameChanged))]
    public NetworkString<_32> playerName { get; set; }
    [SerializeField] private TextMeshPro playerNameLabel;


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

    [SerializeField] private LayerMask targetLayerMask;
    public LayerMask TargetLayerMask { get { return targetLayerMask; } }

    //상태 체크
    [Networked, OnChangedRender(nameof(OnStateTypeChanged))]
    public PlayerStateType StateType { get; set; }
    [Networked] public float CurrentHp { get; set; }
    [Networked] public NetworkObject Target { get; set; }
    [Networked] public TickTimer AttackCooldown { get; set; } // 쿨타임 타이머
    [Networked] public TickTimer DetectTimer { get; set; }
    [Networked] public TickTimer DeathTimer { get; set; }
    public float DespawnDelay = 2f;

    public PlayerAnimeController Animator { get; set; }
    public NetworkNavMeshMover Mover { get; set; }

    private Dictionary<PlayerStateType, IPlayerState> stateDic;
    private IPlayerState currentState;

    private bool _isFsmInitialized = false;

    private void Awake()
    {
        Mover = GetComponent<NetworkNavMeshMover>();
        Animator = GetComponentInChildren<PlayerAnimeController>();
    }
    public override void Spawned()
    {
            stateDic = new Dictionary<PlayerStateType, IPlayerState>();
            stateDic.Add(PlayerStateType.Idle, new PlayerIdleState());
            stateDic.Add(PlayerStateType.Detect, new PlayerDetectState());
            stateDic.Add(PlayerStateType.Chase, new PlayerChaseState());
            stateDic.Add(PlayerStateType.Attack, new PlayerAttackState());
            stateDic.Add(PlayerStateType.Dead, new PlayerDeadState());
        
        if (Object.HasStateAuthority)
        {
            ChangeState(PlayerStateType.Idle);
        }
        #region Name
        if (Object.HasInputAuthority)
        {
            if (FusionConnection.instance != null && !string.IsNullOrEmpty(FusionConnection.instance._playerName))
            {
                string localName = FusionConnection.instance._playerName;
                UpdateNameUI(localName);
                RpcSetPlayerName(localName);
            }
        }
        else
        {
            UpdateNameUI(string.IsNullOrEmpty(playerName.Value) ? "Connecting..." : playerName.Value);
        }
        #endregion
    }

    public override void FixedUpdateNetwork()
    {
        //판정은 호스트만 하도록
        if (!HasStateAuthority)
        {
            return;
        }
        //그 상태를 반복해서 서버에서 검사하도록.
        if (currentState != null)
        {
            currentState.Tick(this);
        }
    }

    #region Name
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcSetPlayerName(string nameInput, RpcInfo info = default)
    {
        playerName = nameInput;
    }
    private void OnPlayerNameChanged()
    {
        UpdateNameUI(playerName.Value);
    }
    private void UpdateNameUI(string nameToDisplay)
    {
        if (string.IsNullOrEmpty(nameToDisplay) || nameToDisplay == "null")
        {
            nameToDisplay = "Connecting...";
        }

        if (playerNameLabel != null)
        {
            playerNameLabel.text = nameToDisplay;
        }
    }
    #endregion

    public void ChangeState(PlayerStateType state)
    {
        if (StateType == state && currentState != null)
        {
            return;
        }
        if(currentState != null)
        {
            currentState.Exit(this);
        }
        StateType = state;
        currentState = stateDic[state];

        if (currentState != null)
        {
            currentState.Enter(this);
        }
    }
    private void OnStateTypeChanged()
    {
        //이미 호스트는 change에서 변경을 했으므로 생략
        if (HasStateAuthority)
        {
            return;
        }
        currentState = stateDic[StateType];
        currentState.Enter(this);
    }

    public void TakeDamage(float damage)
    {
        //접근 권한은 호스트에게
        if (!HasStateAuthority)
        {
            return;
        }
        //이미 죽은 상태일 때 무시
        if (StateType == PlayerStateType.Dead)
        {
            return;
        }
        CurrentHp -= damage;

        if (CurrentHp <= 0f)
        {
            CurrentHp = 0f;
            ChangeState(PlayerStateType.Dead);
        }
    }

    //스텟 업글
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcUpdataStats(float addDamage, float addDefense)
    {
        attackDamage += addDamage;
        defense += addDefense;
    }
    public void ChangeState(IPlayerState newState)
    {
        if (currentState != null) currentState.Exit(this);
        currentState = newState;
        if (currentState != null) currentState.Enter(this);
    }
}