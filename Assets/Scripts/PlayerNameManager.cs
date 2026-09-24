/*
using System;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
*/

using System;
using TMPro;
using Unity.Services.CloudCode.GeneratedBindings;
using Unity.Services.CloudCode.GeneratedBindings.UsernameRegistry;
using UnityEngine;

public class PlayerNameManager : Unity.Netcode.NetworkBehaviour
{
    [Header("Player Name")]
    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Settings")]
    [SerializeField] private int minimumNameLength = 1;

    [SerializeField] private int maximumNameLength = 20;

    public string CurrentPlayerName { get; private set; }

    private UsernameRegistryModuleBindings usernameRegistry;

    private void Awake()
    {
        usernameRegistry =
            new UsernameRegistryModuleBindings();
    }

    public async void SetPlayerName() // The Script that is triggered by the Input Button
    {
        Debug.Log("SetPlayerName() triggered.");

        // Make sure AuthenticationManager exists.
        if (AuthenticationManager.Instance == null)
        {
            Debug.LogError(
                "AuthenticationManager does not exist."
            );

            return;
        }

        // Wait until Authentication has finished initializing.
        while (!AuthenticationManager.Instance.IsReady)
        {
            await System.Threading.Tasks.Task.Yield();
        }

        string requestedName =
            playerNameInput.text.Trim();

        if (string.IsNullOrEmpty(requestedName))
        {
            Debug.LogWarning(
                "Player name cannot be empty."
            );

            return;
        }

        if (requestedName.Length < minimumNameLength ||
            requestedName.Length > maximumNameLength)
        {
            Debug.LogWarning(
                $"Player name must be between " +
                $"{minimumNameLength} and " +
                $"{maximumNameLength} characters."
            );

            return;
        }

        if (requestedName.Contains(" "))
        {
            Debug.LogWarning(
                "Player name cannot contain spaces."
            );

            return;
        }

        try
        {
            Debug.Log(
                $"Requesting username claim: {requestedName}"
            );

            UsernameRegistryModule_UsernameResult result =
                await usernameRegistry.ClaimUsername(
                    requestedName
                );

            if (!result.Success)
            {
                Debug.LogWarning(
                    $"Username could not be claimed. " +
                    $"Reason: {result.Error}"
                );

                return;
            }

            CurrentPlayerName =
                result.Username;

            Debug.Log(
                $"Username successfully claimed: " +
                $"{CurrentPlayerName}"
            );

            playerNameInput.text =
                CurrentPlayerName;

            UIManager uiManager =
                FindAnyObjectByType<UIManager>();

            if (uiManager != null)
            {
                uiManager.ChangeToSessionList();
            }
            else
            {
                Debug.LogWarning(
                    "UIManager could not be found."
                );
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

}