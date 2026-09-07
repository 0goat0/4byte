using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameEntry : MonoBehaviour
{
    [SerializeField] GameObject cavas;
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] Button submitButton;

    public void SubmitName()
    {
        string enteredName = nameInputField.text;

        // 닉네임이 비어있지 않은 경우에만 로비 연결 및 이름 전달 실행
        if (!string.IsNullOrEmpty(enteredName))
        {
            if (FusionConnection.instance != null)
            {
                FusionConnection.instance.ConnectedToLobby(enteredName);
            }
            cavas.SetActive(false);
        }
        else
        {
            Debug.LogWarning("닉네임을 입력해주세요");
        }
    }
    public void ActivateButton()
    {
        submitButton.interactable = true;
    }
}
