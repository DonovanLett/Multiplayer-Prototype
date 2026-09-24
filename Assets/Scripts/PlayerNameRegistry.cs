using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNameRegistry : NetworkBehaviour
{
    public static PlayerNameRegistry Instance { get; private set; }

    private Dictionary<ulong, string> playerNames =
        new Dictionary<ulong, string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public bool TryClaimName(ulong clientId, string requestedName)
    {
        if (!IsServer)
            return false;

        // Case-insensitive comparison.
        foreach (var pair in playerNames)
        {
            if (pair.Value.Equals(
                requestedName,
                StringComparison.OrdinalIgnoreCase))
            {
                // The player already owns this name.
                if (pair.Key == clientId)
                    return true;

                // Somebody else owns it.
                return false;
            }
        }

        // Name is available.
        playerNames[clientId] = requestedName;

        Debug.Log(
            $"Player {clientId} claimed name: {requestedName}"
        );

        return true;
    }

    public string GetPlayerName(ulong clientId)
    {
        if (playerNames.TryGetValue(clientId, out string name))
            return name;

        return null;
    }

    public bool HasName(string name)
    {
        foreach (string existingName in playerNames.Values)
        {
            if (existingName.Equals(
                name,
                StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public void RemovePlayer(ulong clientId)
    {
        if (!IsServer)
            return;

        if (playerNames.TryGetValue(clientId, out string name))
        {
            playerNames.Remove(clientId);

            Debug.Log(
                $"Player {clientId} disconnected. " +
                $"Released name: {name}"
            );
        }
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        NetworkManager.Singleton.OnClientDisconnectCallback
            += HandleClientDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback
                -= HandleClientDisconnect;
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        RemovePlayer(clientId);
    }
}
