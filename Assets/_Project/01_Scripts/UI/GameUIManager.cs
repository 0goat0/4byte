using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;
    private Animator animator;

    private void Awake()
    {
        if (instance == null) instance = this;
        animator = GetComponent<Animator>();
    }

    public void Close()
    {
        StartCoroutine(CloseAfterDelay(this.gameObject, this.animator));
    }

    public void CloseTargetUI(GameObject targetUI, Animator targetAnimator)
    {
        StartCoroutine(CloseAfterDelay(targetUI, targetAnimator));
    }

    // 매개변수 받음
    private IEnumerator CloseAfterDelay(GameObject targetUI, Animator targetAnimator)
    {
        if (targetAnimator != null)
        {
            targetAnimator.SetTrigger("Close");
        }

        yield return new WaitForSeconds(0.5f);

        if (targetAnimator != null)
        {
            targetAnimator.ResetTrigger("Close");
        }

        if (targetUI != null)
        {
            targetUI.SetActive(false);
        }
    }
}