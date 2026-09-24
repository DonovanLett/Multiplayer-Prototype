using UnityEngine;

public class NetworkPlayerDespawner : MonoBehaviour // Gives Event Dispatches bridged access to DontDetroyOnLoad Elements, that is if they exist in your Scene; if it is a Timeline or Button that triggers the end of your name, this will come in handy
{
    public void Despawn()
    {
        Debug.Log("Despawner Called");
        NetworkSceneManager networkSceneManager = NetworkSceneManager.Instance;

        if(networkSceneManager != null)
        {
            networkSceneManager.LeaveGame();
            Debug.Log("NetworkManager should be called!");
        }
        else
        {
            Debug.LogError("NetworkSceneManager not found!!");
        }
    }

    public void ReturnToLobby()
    {
        //Debug.Log("Despawner Called");
        NetworkSceneManager networkSceneManager = NetworkSceneManager.Instance;

        if (networkSceneManager != null)
        {
            networkSceneManager.ReturnToLobby();
            Debug.Log("NetworkManager should be called!");
        }
        else
        {
            Debug.LogError("NetworkSceneManager not found!!");
        }
    }

    public void LoadGame()
    {
        //Debug.Log("Despawner Called");
        NetworkSceneManager networkSceneManager = NetworkSceneManager.Instance;

        if (networkSceneManager != null)
        {
            networkSceneManager.LoadGame();
            Debug.Log("NetworkManager should be called!");
        }
        else
        {
            Debug.LogError("NetworkSceneManager not found!!");
        }
    }

    public void StartSession()
    {
        //Debug.Log("Despawner Called");
        NetworkSceneManager networkSceneManager = NetworkSceneManager.Instance;

        if (networkSceneManager != null)
        {
            networkSceneManager.StartSession();
            Debug.Log("NetworkManager should be called!");
        }
        else
        {
            Debug.LogError("NetworkSceneManager not found!!");
        }
    }

    public void EnterChatMode()
    {
        InputManager inputManager = InputManager.Instance;

        if (inputManager != null)
        {
            inputManager.EnterChatMode();
            Debug.Log("InputManager should be called!");
        }
        else
        {
            Debug.LogError("InputManager not found!!");
        }
    }

    public void ExitChatMode()
    {
        InputManager inputManager = InputManager.Instance;

        if (inputManager != null)
        {
            inputManager.ExitChatMode();
            Debug.Log("InputManager should be called!");
        }
        else
        {
            Debug.LogError("InputManager not found!!");
        }
    }
}
