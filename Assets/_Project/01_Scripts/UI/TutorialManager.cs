using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject tutorialCanvas;
    public TMP_Text tutorialText;
    public Button nextButton;

    [Header("Tutorial Texts")]
    [TextArea(3, 5)]
    public List<string> startTexts;

    [Header("Scene Settings")]
    public string nextSceneName = "MainMenu";

    private int currentTextIndex = 0;
    private bool isStartTutorialOver = false;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        StartTutorial();
    }

    public void StartTutorial()
    {
        if (startTexts == null || startTexts.Count == 0) return;

        tutorialCanvas.SetActive(true);
        nextButton.gameObject.SetActive(true);
        tutorialText.gameObject.SetActive(true);
        currentTextIndex = 0;
        isStartTutorialOver = false;

        ShowText(currentTextIndex);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNextButtonClick);
    }

    private void ShowText(int index)
    {
        tutorialText.text = startTexts[index];
    }

    private void OnNextButtonClick()
    {
        currentTextIndex++;

        if (currentTextIndex < startTexts.Count)
        {
            ShowText(currentTextIndex);
        }
        else
        {
            //마지막 텍스트 종료
            EndStartTutorial();
        }
    }
    private void EndStartTutorial()
    {
        isStartTutorialOver = true;

        nextButton.gameObject.SetActive(false);
        tutorialCanvas.SetActive(false);

        Debug.Log("시작 튜토리얼 종료");
    }

    //보스 처치
    public void OnBossDefeated()
    {
        if (!isStartTutorialOver) return;

        tutorialCanvas.SetActive(true);
        tutorialText.text = "Boss Die";

        nextButton.gameObject.SetActive(false);
        Invoke("ExitTutorial", 2.0f);
    }

    private void ExitTutorial()
    {
        Debug.Log("씬 종료: " + nextSceneName);
        SceneManager.LoadScene(nextSceneName);
    }
}