using Fusion;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    [SerializeField] private RTSInputReader inputReader;
    [SerializeField] private PlayerInteractionState playerState;

    private void OnEnable()
    {
        inputReader.SelectStarted += HandlePlayerSelect;
    }

    private void OnDisable()
    {
        inputReader.SelectStarted -= HandlePlayerSelect;
    }

    private void HandlePlayerSelect(Vector2 pointerPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(pointerPosition);

        if (!Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            playerState.SetInfoTarget(null, false);
            return;
        }

        UpdateSelection(hitInfo.collider.gameObject);
    }

    private void UpdateSelection(GameObject hitObject)
    {
        ISelectable selected = hitObject.GetComponent<ISelectable>();

        if(selected == null)
        {
            playerState.SetInfoTarget(selected, false);
            return;
        }

        bool canCommand = CanCommandTarget(hitObject);

        playerState.SetInfoTarget(selected, canCommand);

    }

    private bool CanCommandTarget(GameObject hitObject)
    {
        NetworkObject networkObject = hitObject.GetComponent<NetworkObject>();

        bool canCommand = networkObject != null && networkObject.HasInputAuthority;
        return canCommand;
    }
}
