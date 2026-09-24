using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    public bool IsReady { get; private set; }

    public string PlayerId
    {
        get
        {
            if (!IsReady)
                return null;

            return AuthenticationService.Instance.PlayerId;
        }
    }

    /*
    public string PlayerName
    {
        get
        {
            if (!IsReady)
                return null;

            return AuthenticationService.Instance.PlayerName;
        }
    }
    */

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Debug.Log("Player Name: " + PlayerName); // TEST ACTION, DELETE LATER

        await Initialize();
    }

    private async Task Initialize()
    {
        try
        {
            Debug.Log("Initializing Unity Services...");

            await UnityServices.InitializeAsync();

            Debug.Log("Unity Services initialized.");

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Signing in anonymously...");

                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                Debug.Log(
                    $"Authentication successful. " +
                    $"Player ID: {AuthenticationService.Instance.PlayerId}"
                );
            }

            IsReady = true;

            Debug.Log("AuthenticationManager is ready.");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    /*
    private void Update() // TEST METHOD, DELETE LATER
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("Player Name: " + PlayerName); // Added
        }
    }
    */


    /* // Original, removed code
    public static AuthenticationManager Instance { get; private set; }

    public bool IsAuthenticated =>
        AuthenticationService.Instance.IsSignedIn;

    public string PlayerId =>
        AuthenticationService.Instance.PlayerId;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log(
                $"Authentication successful. " +
                $"Player ID: {AuthenticationService.Instance.PlayerId}"
            );
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }
    */
}
