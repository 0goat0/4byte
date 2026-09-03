using UnityEngine;
using System.Collections.Generic;
using Fusion;
using System.Collections;
using UnityEngine.AI;

public interface IEnemyState
{
    void Enter(EnemyAI enemy);
    void Exit(EnemyAI enemy);
    void Tick(EnemyAI enemy); //FixedUpdateNetwork에서 매 틱 호출됨.

}

//서버로 전송하는 타입이 필요함.
public enum EnemyStateType
{
    Idle,
    Detect,
    Chase,
    Attack,
    Dead
}
public class EnemyAI : NetworkBehaviour
{

    [Header("Data")]
    //이름, 크기(소형, 중형, 대형), 공격타입(근접, 원거리, 광역)
    //체력, 공격력, 방어력, 공격속도, 이동속도
    //드랍골드
    //몬스터 프리펩
    [SerializeField] private EnemyData data;

    [Header("Detection")]
    [SerializeField] private float attackRange = 2f;
    public float AttackRange { get { return attackRange; }}
    [SerializeField] private float detectRange = 4f;
    public float DetectRange { get { return detectRange; }}
    [SerializeField] private LayerMask targetLayerMask;
    public LayerMask TargetLayerMask {  get { return targetLayerMask; }}


    //현재 상태 체크
    [Networked, OnChangedRender(nameof(OnStateTypeChanged))] 
    public EnemyStateType StateType { get; set; }
    //현재 체력 체크
    [Networked] public float CurrentHp { get; set; }
    //어떤 타겟을 따라가는지 체크
    [Networked] public NetworkObject Target { get; set; }
    //공격 쿨타임 체크
    [Networked] TickTimer AttackCooldown { get; set; }

    [Networked] public TickTimer DetectTimer { get; set; }

    public NavMeshAgent agent;

    private Dictionary<EnemyStateType, IEnemyState> stateDic;
    private IEnemyState currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public override void Spawned()
    {
        //각 상태를 매핑
        //enum으로 타입을 만들어준 이유는 Networked로 서버에 상태를 보내줄 경우
        //interface로 만들어서는 보낼 수 없기 때문에 각 상태를 미리 매핑해서
        //기존에 쓰던 방식을 사용할 수 있게 해주었음.
        stateDic = new Dictionary<EnemyStateType, IEnemyState>();
        stateDic.Add(EnemyStateType.Idle, new EnemyIdleState());
        stateDic.Add(EnemyStateType.Detect, new EnemyDetectState());
        stateDic.Add(EnemyStateType.Chase, new EnemyChaseState());
        stateDic.Add(EnemyStateType.Attack, new EnemyAttackState());
        stateDic.Add(EnemyStateType.Dead, new EnemyDeadState());

        //호스트가 초기 적의 상태와 초기체력 설정
        //호스트가 아닌 클라이언트는 실행안됌.
        if (HasStateAuthority)
        {
            CurrentHp = data.hp;
            StateType = EnemyStateType.Idle;
        }
        else
        {
            agent.enabled = false;
        }
        currentState = stateDic[StateType];
        //테스트
        //StartCoroutine(DespawnEnemy());
        currentState.Enter(this);
    }
    //테스트 코드
    IEnumerator DespawnEnemy()
    {
        yield return new WaitForSeconds(1f);
        //Runner.Despawn(Object);
    }
    public void ChangeState(EnemyStateType state)
    {
        if(StateType == state)
        {
            return;
        }
        currentState.Exit(this);
        StateType = state;
        currentState = stateDic[state];
        currentState.Enter(this);
    }

    public override void FixedUpdateNetwork()
    {
        //판정은 호스트만 하도록
        if (!HasStateAuthority)
        {
            return;
        }
        //그 상태를 반복해서 서버에서 검사하도록.
        currentState.Tick(this);

    }

    //클라이언트도 상태가 바뀌면 체크하고 바꿔주기 위함.
    //서버에서 적의 StateType을 바꾸면, 모든 클라이언트에서 네트워크 동기화를 통해
    //자동으로 OnStateTypeChanged()가 실행되어 local에서도 적의 상태 객체가 교체되고
    //각 상태의 Enter가 실행되는 구조.
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
