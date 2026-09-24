using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class SpectatorSystem : NetworkBehaviour
{
    //public static SpectatorManager Instance { get; private set; }

    public NetworkList<ulong> AliveClientIds; // This is already synchronized across every Client; you don't need to add anything to it.

    [SerializeField]
    private int _clientCount;

    private void Awake()
    {
        // Instance = this;

        AliveClientIds = new NetworkList<ulong>();


        // Experiment
        //NetworkManager.Singleton.ConnectedClientsList.PlayerObject.OnListChanged += UpdateAlivePlayers;
        // Experiment
    }

    /*
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Players should be updated");
            UpdateAlivePlayers();
        }
    }
    */

    /*
    void Start()
    {
        if (IsServer)
        {
            Debug.Log("Players should be updated");
            UpdateAlivePlayers(); 
        }
    }
    */

    // Example Code: just for testing and seeing when it fires.
    private void Update()
    {
        //_clientCount = AliveClientIds.Count; // This isn't firing for Clients for some reason
    }
    // Example Code: just for testing and seeing when it fires.

    public void UpdateAlivePlayersOriginal()
    {
        Debug.Log("Update Alive Players is triggered");
        if (!IsServer)
        {
            Debug.Log("Not Called//////");///////////////////
            return;
        }
        //return;
        Debug.Log($"BEFORE CLEAR: {AliveClientIds.Count}");

        AliveClientIds.Clear();

        Debug.Log($"AFTER CLEAR: {AliveClientIds.Count}");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                Debug.Log($"ADDING {client.ClientId}");
                AliveClientIds.Add(client.ClientId);
                //Debug.Log("One Added//////////");///////////////
            }
        }
        Debug.Log($"FINAL SERVER LIST COUNT: {AliveClientIds.Count}"); // This is ending up as "0" when called by void Start()

        //Debug.Log("Client Count: " + AliveClientIds.Count);///////////////

        //_clientCount = AliveClientIds.Count; // This isn't firing for Clients for some reason
    }

    public void UpdateAlivePlayers()
    {
        Debug.Log("Update Alive Players is triggered");
        if (!IsServer)
        {
            Debug.Log("Not Called//////");///////////////////
            return;
        }
        //return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && AliveClientIds.Contains(client.ClientId) == false)
            {
                Debug.Log($"ADDING {client.ClientId}");
                AliveClientIds.Add(client.ClientId);
                //Debug.Log("One Added//////////");///////////////
            }
            else if(client.PlayerObject == null && AliveClientIds.Contains(client.ClientId) == true)
            {
                Debug.Log($"REMOVING {client.ClientId}");
                AliveClientIds.Remove(client.ClientId);
            }
        }
        Debug.Log($"FINAL SERVER LIST COUNT: {AliveClientIds.Count}"); // This is ending up as "0" when called by void Start()

        //Debug.Log("Client Count: " + AliveClientIds.Count);///////////////

        //_clientCount = AliveClientIds.Count; // This isn't firing for Clients for some reason
    }




    /*
    List<GameObject> currentPlayers = new List<GameObject>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnPlayerNumberChanged()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                currentPlayers.Add(client.PlayerObject.gameObject);
            }
        }
    }
    */
}