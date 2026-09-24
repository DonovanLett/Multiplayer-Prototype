using System.Globalization;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerNetworkData : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> PlayerName =
        new NetworkVariable<FixedString64Bytes>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner
        );

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        PlayerName.Value = AuthenticationService.Instance.PlayerName;
    }
}