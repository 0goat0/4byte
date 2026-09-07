using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RefreshButton : MonoBehaviour
{
    private Button refreshButton;
    private void Awake()
    {
        if(refreshButton==null)
        {
            refreshButton = GetComponent<Button>();
        }
        refreshButton.onClick.AddListener(Refresh);
    }

    private void Refresh()
    {
        StartCoroutine(RefreshWait());
    }
    private IEnumerator RefreshWait()
    {
        refreshButton.interactable = false;
        FusionConnection.instance.RefreshSessionListUI();
        yield return new WaitForSeconds(3f);

        refreshButton.interactable = true;
    }

}
