using Unity.Netcode;
using UnityEngine;

public class RespawnPoint : NetworkBehaviour // A Script meant to respawn dead Players
{
    public void Respawn() // Triggered by a button
    {
        RequestRespawnRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)] // originally InvokePermission = RpcInvokePermission.Owner
    private void RequestRespawnRpc(RpcParams rpcParams = default) // RespawnPoint Script; Script that handle Spawning a Player back in if they choose not to enter Spectator Mode. // RpcParams rpcParams = default wasn't there before
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        //ulong clientId = OwnerClientId;

        Debug.Log(
            $"Server received respawn request from Player {clientId}"
        );

        GameObject playerPrefab =
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab;

        if (playerPrefab == null)
        {
            Debug.LogError(
                "No Player Prefab is assigned to NetworkManager."
            );

            return;
        }

        GameObject player = Instantiate(
            playerPrefab,
            transform.position,
            transform.rotation
        );

        NetworkObject networkObject =
            player.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError(
                "The Player Prefab does not contain a NetworkObject."
            );

            Destroy(player);
            return;
        }

        networkObject.SpawnAsPlayerObject(clientId, true);

        SpectatorSystem spectatorSystem =
            FindAnyObjectByType<SpectatorSystem>();

        if (spectatorSystem != null)
        {
            spectatorSystem.UpdateAlivePlayers();
        }

        HideDeathScreenRpc(clientId);
    }

    [Rpc(SendTo.Everyone)]
    private void HideDeathScreenRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId)
            return;

        UIManager uiManager =
            FindAnyObjectByType<UIManager>();

        if (uiManager != null)
        {
            uiManager.DeathScreenLeft(clientId);
        }
    }

    /*
    public void Respawn() // Lined out Code; not being used right now.
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;

        GameObject playerPrefab =
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab;

        if (playerPrefab == null)
        {
            Debug.LogError("No Player Prefab is assigned to NetworkManager.");
            return;
        }

        GameObject player = Instantiate(
            playerPrefab,
            transform.position, 
            transform.rotation  
        );

        NetworkObject networkObject =
            player.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError(
                "The Player Prefab does not contain a NetworkObject."
            );

            Destroy(player);
            return;
        }

        networkObject.SpawnAsPlayerObject(clientId, true);
        SpectatorSystem _spectatorSystem = FindAnyObjectByType<SpectatorSystem>(); // 
        _spectatorSystem.UpdateAlivePlayers();


        // UIManager Code; be sure this is correct
        UIManager uIManager = FindAnyObjectByType<UIManager>();
        uIManager.DeathScreenLeft(clientId);
    }
    */
}
