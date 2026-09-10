using System;
using UnityEngine;

public enum PlayerInteractionMode
{
    Default,
    AttackTargeting
}

public class PlayerInteractionState : MonoBehaviour
{
    public ISelectable InfoTarget { get; private set; }
    public bool CanCommand { get; private set; }

    public PlayerInteractionMode Mode { get; private set; }

    public event Action OnSelectionChanged;

    /// <summary>
    /// 대상 좌클릭
    /// </summary>
    /// <param name="target"></param>
    public void SetInfoTarget(ISelectable target, bool canCommand)
    {
        if (ReferenceEquals(InfoTarget, target) && CanCommand == canCommand)
        {
            return;
        }

        InfoTarget = target;
        CanCommand = canCommand;
        Mode = PlayerInteractionMode.Default;

        OnSelectionChanged?.Invoke();
    }

    /// <summary>
    /// A키 입력 시 공격 목표 지정 모드 진입
    /// </summary>
    /// <returns></returns>
    public bool TryBeingAttackTargeting()
    {
        if (!CanCommand)
            return false;

        if (Mode == PlayerInteractionMode.AttackTargeting)
            return true;

        Mode = PlayerInteractionMode.AttackTargeting;
        OnSelectionChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// 공격 목표 지정 모드 취소
    /// </summary>
    public void CancelPendingCommand()
    {
        if (Mode == PlayerInteractionMode.Default)
            return;

        Mode = PlayerInteractionMode.Default;
        OnSelectionChanged?.Invoke();
    }

    /// <summary>
    /// 조작 상태 초기화
    /// </summary>
    public void Reset()
    {
        if (InfoTarget == null &&
            !CanCommand &&
            Mode == PlayerInteractionMode.Default)
        {
            return;
        }

        InfoTarget = null;
        CanCommand = false;
        Mode = PlayerInteractionMode.Default;

        OnSelectionChanged?.Invoke();
    }
}
