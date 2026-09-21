using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NameEntry : MonoBehaviour
{
    private Animator animator;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] Button submitButton;

    private void Awake()
    {
            animator = GetComponent<Animator>();
    }

    public void SubmitName()
    {
        string enteredName = nameInputField.text;

        // 닉네임이 비어있지 않으면 로비 연결
        if (!string.IsNullOrEmpty(enteredName))
        {
            if (LobbyConnection.Instance != null)
            {
                LobbyConnection.Instance.ConnectedToLobby(enteredName);
            }
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
            Debug.LogWarning("닉네임을 입력해주세요");
        }
    }
    public void Close()
    {
        if (MainMenuManager.instance != null)
        {
            MainMenuManager.instance.CloseTargetUI(this.gameObject, this.animator);
        }
    }

    public void ActivateButton()
    {
        submitButton.interactable = true;
    }
}
