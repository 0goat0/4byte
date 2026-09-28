using Fusion;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private RTSInputReader inputReader;
    [SerializeField] private PlayerInteractionState playerState;

    [Header("Command Indicator")]
    [SerializeField] private ParticleSystem commandIndicatorEffect;
    [SerializeField] private Color moveIndicatorColor = Color.green;
    [SerializeField] private Color attackMoveIndicatorColor = Color.red;

    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private LayerMask _enemyMask;
    [SerializeField] private LayerMask _uiMask;
    [SerializeField] private LayerMask _selectionMask;

    private void OnEnable()
    {
        inputReader.SelectStarted += HandlePlayerSelect;
        inputReader.MoveCommandRequested += HandleMoveCommand;
        inputReader.AttackCommandRequested += HandleAttackCommand;
    }

    private void OnDisable()
    {
        inputReader.SelectStarted -= HandlePlayerSelect;
        inputReader.MoveCommandRequested -= HandleMoveCommand;
        inputReader.AttackCommandRequested -= HandleAttackCommand;
    }

    private void HandlePlayerSelect(Vector2 pointerPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _selectionMask))
        {
            // 공격 모드에서 빈 곳을 클릭했을 때 선택 유닛이 초기화되는 것을 방지
            if (playerState.Mode == PlayerInteractionMode.Default)
                playerState.SetInfoTarget(null, null);

            return;
        }

        // 현재 모드 별 클릭 처리
        switch (playerState.Mode)
        {
            case PlayerInteractionMode.Default:
                UpdateSelection(hitInfo.collider.gameObject);
                break;

            case PlayerInteractionMode.AttackTargeting:
                HandleAttackClick(hitInfo);
                break;
        }

    }

    private void UpdateSelection(GameObject hitObject)
    {
        ISelectable selected = hitObject.GetComponent<ISelectable>();

        if(selected == null)
        {
            playerState.SetInfoTarget(selected, null);
            return;
        }

        PartyMember partyMember = hitObject.GetComponent<PartyMember>();
        NetworkParty party = null;

        if (partyMember != null)
        {
            party = partyMember.Party;
        }

        playerState.SetInfoTarget(selected, party);

    }

    private void HandleMoveCommand(Vector2 pointerPosition)
    {
        if (!playerState.CanCommand)
            return;

        if (playerState.Mode == PlayerInteractionMode.AttackTargeting)
        {
            playerState.CancelPendingCommand();
        }

        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _groundMask))
            return;

        Vector3 destination = hitInfo.point;

        ShowCommandIndicator(destination, moveIndicatorColor);

        playerState.CommandParty.RequestMove(destination);
    }

    /// <summary>
    /// A 키 입력 시 호출되어 공격모드로 변경 시도
    /// </summary>
    private void HandleAttackCommand()
    {
        if (playerState.InfoTarget is Unit)
            playerState.TryBeingAttackTargeting();
    }

    private void HandleAttackClick(RaycastHit hitInfo)
    {
        if (!playerState.CanCommand)
            return;

        int hitLayerMask = 1 << hitInfo.collider.gameObject.layer;

        // Layer 에 따라 유닛의 함수 호출
        if ((_enemyMask.value & hitLayerMask) != 0)
        {
            // 적 판정은 레이어로 하고, RPC에는 네트워크 대상을 전달합니다.
            NetworkObject target = hitInfo.collider.GetComponentInParent<NetworkObject>();

            if (target == null)
                return;

            if (target.TryGetComponent(out EnemyAI enemy))
            {
                enemy.PlayAttackTargetFeedback();
            }

            // 적을 클릭 할 경우 타겟 적의 Transform
            playerState.CommandParty.RequestAttackTarget(target);
        }
        else if ((_groundMask.value & hitLayerMask) != 0)
        {
            Vector3 destination = hitInfo.point;

            ShowCommandIndicator(destination, attackMoveIndicatorColor);

            // 땅을 클릭한 경우 클릭한 곳의 월드 좌표
            playerState.CommandParty.RequestAttackMove(destination);
        }
        else
        {
            return;
        }

        // 선택 대상은 유지하고 다음 클릭의 입력 모드만 복귀합니다.
        playerState.CancelPendingCommand();
    }

    private void ShowCommandIndicator(Vector3 position, Color color)
    {
        if (commandIndicatorEffect == null)
            return;

        Transform effectTransform = commandIndicatorEffect.transform;

        effectTransform.position = position + Vector3.up * 0.03f;

        ParticleSystem.MainModule main = commandIndicatorEffect.main;
        main.startColor = color;

        commandIndicatorEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        commandIndicatorEffect.Play(true);
    }
}
