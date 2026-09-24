using UnityEngine;
using Unity.Netcode;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private GameObject cameraObject;

    public override void OnNetworkSpawn()
    {
        cameraObject.SetActive(IsOwner);
    }
}
