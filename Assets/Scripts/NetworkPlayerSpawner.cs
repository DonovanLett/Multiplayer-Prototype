using UnityEngine;
using Unity.Netcode;

public class NetworkPlayerSpawner : MonoBehaviour
{
    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager could not be found.");
            return;
        }

        // Listen for Clients who connect after this script starts.
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Handle Clients who connected BEFORE this script started.
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            OnClientConnected(clientId);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        SpawnPlayer(clientId);
    }

    public void SpawnPlayer(ulong clientId) // Originally Private before creating the respawn point for Players.
    {
        Debug.Log("SpawnPlayer triggered");

        GameObject playerPrefab =
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab;

        if (playerPrefab == null)
        {
            Debug.LogError("No Player Prefab is assigned to NetworkManager.");
            return;
        }

        GameObject player = Instantiate(
            playerPrefab,
            transform.position, // originally _spawnPoint.position,
            transform.rotation  // originally _spawnPoint.rotation
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
    }
}