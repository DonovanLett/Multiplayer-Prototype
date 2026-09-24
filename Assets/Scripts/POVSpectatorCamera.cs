using Unity.Netcode;
using UnityEngine;

public class POVSpectatorCamera : MonoBehaviour
{
    [Header("POV Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    public void SetTarget(ulong newTarget)
    {
        target = GetPlayerPOVTarget(newTarget);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        transform.position = target.position + positionOffset;
        transform.rotation = target.rotation;
    }

    private Transform GetPlayerPOVTarget(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out NetworkClient client))
        {
            if (client.PlayerObject != null)
            {
                Transform povTarget = client.PlayerObject.GetComponentInChildren<Camera>(true).transform;
                return povTarget;
            }
        }

        return null;
    }
}
