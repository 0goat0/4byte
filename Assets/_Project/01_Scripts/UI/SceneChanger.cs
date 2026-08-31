using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneChanger : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1f;
    void Start()
    {
        //LoadScene("AppScene");
    }

    void Update()
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            LoadNextScene();
        }
    }

    public void LoadNextScene()
    {
        StartCoroutine(SceneLoad(SceneManager.GetActiveScene().buildIndex + 1));
    }

    IEnumerator SceneLoad(int SceneIndex)
    {
        //play animation
        transition.SetTrigger("Start");
        //Wait
        yield return new WaitForSeconds(transitionTime);
        //Load scene
        SceneManager.LoadScene(SceneIndex);
    }

    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


    public static void LoadScene(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex);
    }


    public static Scene GetActiveScene()
    {
        return SceneManager.GetActiveScene();
    }
}

