using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;


public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance;
    private Animator animator;

    private void Awake()
    {
        if (instance == null) instance = this;

        animator = GetComponent<Animator>();
    }
    public void SinglePlay() => SceneManager.LoadScene("SingleGame");
    public void MultiPlay() => SceneManager.LoadScene("MultiGame");
    public void TutorialPlay() => SceneManager.LoadScene("TutorialScene_ArtSource");

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

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#endif
        Application.Quit();
    }
}
