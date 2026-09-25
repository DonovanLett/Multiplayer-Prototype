using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiSpotSpawner : MonoBehaviour
{
    public static MultiSpotSpawner Instance { get; private set; }

    [Header("Player Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private bool _playersSpawned = false;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        ////
        
        Debug.Log("MultiSpotSpawner Awake() is called.");

        // Only the server should decide where Players spawn.
        if (NetworkManager.Singleton == null)
        {
            Debug.Log("NetworkManager.Singleton doesn't exist.");
            return;
        }

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("NetworkManager.Singleton.IsServer is false.");
            return;
        }

        // Make sure the NetworkManager's SceneManager exists.
        if (NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnGameSceneLoaded;

            Debug.Log(
                $"MultiSpotSpawner subscribed to OnLoadEventCompleted in Awake(). " +
                $"Frame: {Time.frameCount}"
            );
        }
        else
        {
            Debug.LogError(
                "MultiSpotSpawner: The NetworkManager's SceneManager doesn't exist."
            );
        }

        ////
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        Debug.Log(
            "[MultiSpotSpawner] Subscribed to OnClientConnectedCallback."
        );

    }

    private void OnClientConnected(ulong clientId)
    {
        NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientId];

        Debug.Log(
            $"[MultiSpotSpawner] OnClientConnected | " +
            $"ClientId = {clientId} | " +
            $"Active Scene = {SceneManager.GetActiveScene().name} | " +
            $"PlayerObject = {(client.PlayerObject != null ? "EXISTS" : "NULL")} | " +
            $"ConnectedClientsIds.Count = {NetworkManager.Singleton.ConnectedClientsIds.Count}"
        );
    }

    private void Start()
    {
        /*
        // Only the server should decide where Players spawn.
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsServer)
            return;

        // Make sure the NetworkManager's SceneManager exists.
        if (NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnGameSceneLoaded;
        }
        */

        /*
        Debug.Log(
            $"========== MultiSpotSpawner.Start() =========="
        );

        Debug.Log(
            $"Start() Time: {Time.frameCount} | " +
            $"Active Scene: {SceneManager.GetActiveScene().name}"
        );
        */
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnGameSceneLoaded;
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

        if (Instance == this)
        {
            Instance = null;
        }

        // NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnGameSceneLoaded(
    string sceneName,
    LoadSceneMode loadSceneMode,
    System.Collections.Generic.List<ulong> clientsCompleted,
    System.Collections.Generic.List<ulong> clientsTimedOut)
    {
        // Make absolutely sure this is the Game Scene.
        if (sceneName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            return;

        // New
        Debug.Log(
    $"[MultiSpotSpawner] Game Scene finished loading. " +
    $"ConnectedClientsIds.Count = {NetworkManager.Singleton.ConnectedClientsIds.Count}, " +
    $"clientsCompleted.Count = {clientsCompleted.Count}, " +
    $"clientsTimedOut.Count = {clientsTimedOut.Count}"
);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            NetworkClient client = NetworkManager.Singleton.ConnectedClients[clientId];

            Debug.Log(
                $"[MultiSpotSpawner] Connected Client {clientId} | " +
                $"PlayerObject = {(client.PlayerObject != null ? "EXISTS" : "NULL")}"
            );
        }

        Debug.Log("[MultiSpotSpawner] clientsCompleted:");

        foreach (ulong clientId in clientsCompleted)
        {
            Debug.Log(
                $"[MultiSpotSpawner] Scene-load completed for Client {clientId}."
            );
        }

        Debug.Log("[MultiSpotSpawner] clientsTimedOut:");

        foreach (ulong clientId in clientsTimedOut)
        {
            Debug.Log(
                $"[MultiSpotSpawner] Scene-load timed out for Client {clientId}."
            );
        }

        SpawnPlayersInACircle(
            new Vector3(0, 0.5700001f, 0),
            3.25f,
            0f
        );
        // New
        /*
        Debug.Log("Game Scene finished loading. Waiting for PlayerObjects...");

        //SpawnPlayers();
        SpawnPlayersInACircle(new Vector3(0, 0.5700001f, 0), 3.25f, 0f);
        //StartCoroutine(WaitForPlayersAndSpawn());
        */
    }

    private void SpawnPlayers()
    {
        GameObject playerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;

        if (_playersSpawned)
            return;

        if (playerPrefab == null)
        {
            Debug.LogError(
                "MultiSpotSpawner: Player Prefab has not been assigned!"
            );

            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError(
                "MultiSpotSpawner: No Spawn Points have been assigned!"
            );

            return;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError(
                "MultiSpotSpawner: NetworkManager does not exist!"
            );

            return;
        }

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError(
                "MultiSpotSpawner: SpawnPlayers was called on a Client!"
            );

            return;
        }

        int spawnIndex = 0;

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (spawnIndex >= spawnPoints.Length)
            {
                Debug.LogError(
                    "MultiSpotSpawner: There are more Players than Spawn Points!"
                );

                break;
            }

            Transform spawnPoint = spawnPoints[spawnIndex];

            NetworkObject playerObject = client.PlayerObject;

            // --------------------------------------------------
            // CASE 1:
            // The Player already exists as a NetworkObject.
            // --------------------------------------------------

            if (playerObject != null)
            {
                playerObject.transform.SetPositionAndRotation(
                    spawnPoint.position,
                    spawnPoint.rotation
                );

                Debug.Log(
                    $"Player {client.ClientId} moved to Spawn Point {spawnIndex}."
                );
            }

            // --------------------------------------------------
            // CASE 2:
            // The Player does not exist.
            // Create and network-spawn it.
            // --------------------------------------------------

            else
            {
                GameObject playerInstance = Instantiate(
                    playerPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

                playerObject = playerInstance.GetComponent<NetworkObject>();

                if (playerObject == null)
                {
                    Debug.LogError(
                        "MultiSpotSpawner: The Player Prefab does not " +
                        "have a NetworkObject component!"
                    );

                    Destroy(playerInstance);
                    continue;
                }

                playerObject.SpawnAsPlayerObject(client.ClientId);

                Debug.Log(
                    $"Player {client.ClientId} was instantiated and " +
                    $"spawned at Spawn Point {spawnIndex}."
                );
            }

            spawnIndex++;
        }

        _playersSpawned = true;

        Debug.Log("MultiSpotSpawner: All Players have been assigned spawn points.");
    }

    private void SpawnPlayersInACircle(Vector3 circleCenter, float radius, float startingAngle)
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        GameObject playerPrefab =
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab;

        if (playerPrefab == null)
        {
            Debug.LogError("No Player Prefab is assigned to NetworkManager.");
            return;
        }

        int playerCount =
            NetworkManager.Singleton.ConnectedClientsIds.Count;

        if (playerCount == 0)
            return;

        float angleBetweenPlayers = 360f / playerCount;

        int spawnIndex = 0;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            float angle =
                startingAngle + (angleBetweenPlayers * spawnIndex);

            float angleRadians = angle * Mathf.Deg2Rad;

            float x = circleCenter.x + Mathf.Cos(angleRadians) * radius;
            float z = circleCenter.z + Mathf.Sin(angleRadians) * radius;

            Vector3 spawnPosition =
                new Vector3(x, circleCenter.y, z);

            Vector3 directionToCenter = circleCenter - spawnPosition;

            Quaternion spawnRotation =
                Quaternion.LookRotation(directionToCenter, Vector3.up);

            GameObject player =
                Instantiate(
                    playerPrefab,
                    spawnPosition,
                    spawnRotation
                );

            Debug.Log(
    $"SERVER SPAWN | Client {clientId} | " +
    $"Position immediately after Instantiate: {player.transform.position}"
); // Experiment

            NetworkObject networkObject =
                player.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                Debug.LogError(
                    "Player Prefab does not contain a NetworkObject."
                );

                Destroy(player);
                continue;
            }

            networkObject.SpawnAsPlayerObject(clientId);

            spawnIndex++;
        }

        SpectatorSystem spectatorSystem = FindAnyObjectByType<SpectatorSystem>(); // Added
        spectatorSystem.UpdateAlivePlayers(); // Added
    }

}
