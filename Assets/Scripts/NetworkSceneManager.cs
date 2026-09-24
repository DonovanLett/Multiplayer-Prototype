using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneManager : MonoBehaviour
{
    public static NetworkSceneManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] 
    private string _mainMenuScene;
    // [SerializeField] private string _waitingRoomScene = "Game";
    [SerializeField] 
    private string _lobbyScene; // "Originally "LobbyScene";
    [SerializeField] 
    private string _gameScene;

    /*// Controlling which Players cane enter code
    [Header("Other Variables")]
    private NetworkVariable<bool> gameStarted = new NetworkVariable<bool>( // Make sure this is right
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server
);
    // Controlling which Players cane enter code*/

    private void Awake()
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        // NEW
        if (NetworkManager.Singleton != null)
        {
            UnityEngine.Debug.Log(
        "Connection Approval Enabled: " +
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval
    );

            NetworkManager.Singleton.ConnectionApprovalCallback =
                ApprovalCheck; // Make sure the "Connection Approval" Toggle in the Network Manager is set to true, otherwise this won't work.
        }
    }

    private void Start()
    {
        UnityEngine.Debug.Log($"Main Menu Name: '{_mainMenuScene}'");
        UnityEngine.Debug.Log($"Lobby Scene Name: '{_lobbyScene}'");
        UnityEngine.Debug.Log($"Game Scene Name: '{_gameScene}'");

        UnityEngine.SceneManagement.SceneManager.LoadScene(_mainMenuScene);
    }

    private void ApprovalCheck(
    NetworkManager.ConnectionApprovalRequest request,
    NetworkManager.ConnectionApprovalResponse response)
    {
        /*
        // Controlling which Players cane enter code
        if (gameStarted.Value)
        {
            response.Approved = false;
            response.Reason = "The game has already started.";
            return;
        }
        // Controlling which Players cane enter code
        */

        response.Approved = true;

        // This is the important line:
        response.CreatePlayerObject = false;

        response.Pending = false;
        UnityEngine.Debug.Log("ApprovalCheck is called.");
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    // Called after the Host successfully creates a Session.
    public void StartSession()
    {
        //SessionInfoController.Instance.SetSessionLocked(true);
        UnityEngine.Debug.Log($"Main Menu Name: '{_mainMenuScene}'");
        UnityEngine.Debug.Log($"Lobby Scene Name: '{_lobbyScene}'");
        UnityEngine.Debug.Log($"Game Scene Name: '{_gameScene}'");
        //_isLeavingGame = false; // Bool Code

        if (NetworkManager.Singleton == null)
        {
            UnityEngine.Debug.LogError("NetworkManager could not be found.");
            return;
        }

        // Only the Server/Host should initiate
        // the networked scene transition.
        if (!NetworkManager.Singleton.IsServer)
        {
            UnityEngine.Debug.LogWarning("Only the Host/Server can start the Game.");
            return;
        }

        /*
        NetworkManager.Singleton.SceneManager.LoadScene(
            _gameScene,
            LoadSceneMode.Single
        );
        */

        UnityEngine.Debug.Log($"Main Menu Name: '{_mainMenuScene}'");
        UnityEngine.Debug.Log($"Lobby Scene Name: '{_lobbyScene}'");
        UnityEngine.Debug.Log($"Game Scene Name: '{_gameScene}'");


        NetworkManager.Singleton.SceneManager.LoadScene(
            _lobbyScene,
            LoadSceneMode.Single
        );

        // gameStarted.Value = true; // BLIP CODE
    }

    public async void LeaveGame()
    {
        //if (_isLeavingGame)
        //    return;

        //_isLeavingGame = true;

        await LeaveSession();

        LoadMainMenu();
    }

    /*
    public void LeaveGame() Works
    {
        if (NetworkManager.Singleton == null)
        {
            LoadMainMenu();
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.Shutdown();
            LoadMainMenu();
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown();
            LoadMainMenu();
        }
        else
        {
            LoadMainMenu();
        }
    }
    */

    private async void OnClientDisconnect(ulong clientId) // Works
    {
        if (NetworkManager.Singleton == null)
            return;

        if (clientId != NetworkManager.Singleton.LocalClientId)
            return;

        UnityEngine.Debug.Log("LOCAL CLIENT DISCONNECTED");

        await LeaveSession();

        LoadMainMenu();
    }

    private void LoadMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(_mainMenuScene);
    }

    public void LoadGame() // Make this proper
    {
        SessionInfoController.Instance.SetSessionLocked(true); // Added code
        NetworkManager.Singleton.SceneManager.LoadScene(
    _gameScene,
    LoadSceneMode.Single
);
    }

    public void ReturnToLobby() // Make this Proper
    {
        DespawnAllPlayers();

        /*
        NetworkManager.Singleton.SceneManager.LoadScene(
    "SampleScene",
    LoadSceneMode.Single
);
        */
        SessionInfoController.Instance.SetSessionLocked(false); // Added code (maybe put this after "Load Lobby")
        UnityEngine.Debug.Log($"Lobby Scene Name: '{_lobbyScene}'");

        NetworkManager.Singleton.SceneManager.LoadScene(
            _lobbyScene,
            LoadSceneMode.Single
        );
        //SessionInfoController.Instance.SetSessionLocked(false); // Added code (maybe put this before "Load Lobby"

        // gameStarted.Value = false; // BLIP CODE
    }

    private void DespawnAllPlayers()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            NetworkClient client =
                NetworkManager.Singleton.ConnectedClients[clientId];

            if (client.PlayerObject != null)
            {
                client.PlayerObject.Despawn();
            }
        }
    }

    private async Task LeaveSession()
    {
        foreach (ISession session in MultiplayerService.Instance.Sessions.Values)
        {
            UnityEngine.Debug.Log($"Leaving session: {session.Id}");

            try
            {
                await session.LeaveAsync();

                UnityEngine.Debug.Log($"Successfully left session: {session.Id}");
            }
            catch (SessionException e)
            {
                if (e.Error == SessionError.SessionNotFound)
                {
                    UnityEngine.Debug.Log("Session already gone; no cleanup necessary.");
                }
                else
                {
                    UnityEngine.Debug.LogError($"Failed to leave session: {e}");
                }
            }
            return;
        }

        UnityEngine.Debug.Log("No active session found.");
    }
}