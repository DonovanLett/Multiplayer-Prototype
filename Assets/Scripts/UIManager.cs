using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //public UIManager Instance { get; private set; }

    [Header("Main Menu Panels")]
    [SerializeField] private GameObject userNameUI;
    [SerializeField] private GameObject sessionUI;

    [Header("Panels")]
    [SerializeField] private GameObject hostUI;
    [SerializeField] private GameObject deathUI;
    [SerializeField] private GameObject spectatorUI;

    [Header("Extra UI")]
    [SerializeField] private TMP_Text _spectatorText;

    private void Start()
    {
        if (NetworkManager.Singleton == null)
            return;

        hostUI.SetActive(NetworkManager.Singleton.IsHost);
    }

    public void Death(ulong clientID)
    {
        deathUI.SetActive(
            NetworkManager.Singleton.LocalClientId == clientID
        );
    }

    public void DeathScreenLeft(ulong clientID)
    {
        deathUI.SetActive(
            NetworkManager.Singleton.LocalClientId != clientID
        );
    }

    public void Spectating(ulong clientID)
    {
        spectatorUI.SetActive(
            NetworkManager.Singleton.LocalClientId == clientID
        );
    }

    public void UpdateSpectatorUI(ulong watchedClientID, SpectatorController.SpectatorMode currentMode)
    {
        if(currentMode == SpectatorController.SpectatorMode.FreeRoam)
        {
            _spectatorText.text = "You are in free-roam";
            // You are in free-roam
        }
        else
        {
            // _spectatorText.text = "You are spectating " + watchedClientID; // Original
            _spectatorText.text = "You are spectating " + GetPlayerName(watchedClientID); // New
            // string name = session.CurrentPlayer.GetPlayerName();

            // Debug.Log("You are spectating " + watchedClientID); // Change this to an actual Player-Provided name later.
        }
    }

    public void OnAllPlayersDead()
    {
        _spectatorText.text = "All Players are dead.";
    }


    public string GetPlayerName(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(
            clientId,
            out NetworkClient client))
        {
            return null;
        }

        if (client.PlayerObject == null)
            return null;

        PlayerNetworkData playerData =
            client.PlayerObject.GetComponent<PlayerNetworkData>();

        if (playerData == null)
            return null;

        return playerData.PlayerName.Value.ToString();
    }

    public void ChangeToSessionList()
    {
        Debug.Log("UI is called");
        userNameUI.SetActive(false);
        sessionUI.SetActive(true);
    }

    public void ChangeToUsername()
    {
        sessionUI.SetActive(false);
        userNameUI.SetActive(true);
    }
}