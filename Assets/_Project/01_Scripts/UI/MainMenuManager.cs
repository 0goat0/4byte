using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;


public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }



    public void MainMenu() => SceneManager.LoadScene("StartScenes");
    public void SinglePlay() => SceneManager.LoadScene("SingleGame");
    public void MultiPlay() => SceneManager.LoadScene("Lobby");
    public void TutorialPlay() => SceneManager.LoadScene("TutorialScene");

    public void Close()
    {
        StartCoroutine(CloseAfterDelay());
    }

    private IEnumerator CloseAfterDelay()
    {
        animator.SetTrigger("Close");
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
        animator.ResetTrigger("Close");
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#endif
        Application.Quit();
    }


}
