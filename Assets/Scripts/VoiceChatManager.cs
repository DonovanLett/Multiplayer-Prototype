using System;
using System.Threading.Tasks;
using Unity.Multiplayer.Widgets;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class VoiceChatManager : MonoBehaviour
{
    public static VoiceChatManager Instance { get; private set; }

    [Header("Voice Settings")]
    [SerializeField] private string _channelNamePrefix = "Proximity_";

    [Header("Debug")]
    [SerializeField] private bool _logVoiceEvents = true;

    private string _currentChannelName;
    private bool _isPositionTrackingReady; // Added

    public bool IsInitialized { get; private set; }
    public bool IsLoggedIn { get; private set; }
    public bool IsInProximityChannel { get; private set; }


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

    private void OnEnable()
    {
        VoiceChatSessionEvents.SessionJoined += OnSessionJoined;
        VoiceChatSessionEvents.SessionLeft += OnSessionLeft;
    }

    private void OnDisable()
    {
        VoiceChatSessionEvents.SessionJoined -= OnSessionJoined;
        VoiceChatSessionEvents.SessionLeft -= OnSessionLeft;
    }


    private async void Start()
    {
        await InitializeVoiceChat();
    }

    private async void OnSessionJoined(string sessionCode)
    {
        Log($"Received Session Joined event. Code: {sessionCode}");

        await JoinProximityChannel(sessionCode);
    }

    private async void OnSessionLeft()
    {
        Log("Received Session Left event.");

        await LeaveProximityChannel();
    }


    private async Task InitializeVoiceChat()
    {
        try
        {
            // Multiplayer Widgets is responsible for initializing:
            // - Unity Services
            // - Authentication
            // - Vivox
            //
            // Wait until Widgets has finished that process.
            while (!WidgetServiceInitialization.IsInitialized)
            {
                await Task.Yield();
            }

            Log("Multiplayer Widgets services are initialized.");

            // Vivox itself has now been initialized by Widgets.
            IsInitialized = true;

            // Log into Vivox.
            VivoxService.Instance.ChannelJoined += OnVivoxChannelJoined; // Added
            VivoxService.Instance.ChannelLeft += OnVivoxChannelLeft; // Added

            if (!VivoxService.Instance.IsLoggedIn)
            {
                Log("Logging into Vivox...");
                await VivoxService.Instance.LoginAsync();
            }

            IsLoggedIn = VivoxService.Instance.IsLoggedIn;

            if (IsLoggedIn)
            {
                Log("Logged into Vivox.");
            }
            else
            {
                Debug.LogWarning(
                    "[VoiceChat] Vivox LoginAsync completed, but Vivox reports that the user is not logged in."
                );
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[VoiceChat] Initialization failed: {exception}"
            );
        }
    }


    public async Task JoinProximityChannel(string sessionCode)
    {
        if (!IsInitialized || !IsLoggedIn)
        {
            Debug.LogWarning(
                "[VoiceChat] Cannot join proximity channel. Vivox is not ready."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(sessionCode))
        {
            Debug.LogError(
                "[VoiceChat] Cannot join proximity channel. Session code is empty."
            );

            return;
        }

        try
        {
            _currentChannelName = $"{_channelNamePrefix}{sessionCode}";

            Log($"Joining proximity channel: {_currentChannelName}");

            // Configure the 3D positional audio.
            Channel3DProperties positionalProperties =
                new Channel3DProperties(
                    32,
                    1,
                    1.0f,
                    AudioFadeModel.InverseByDistance
                );

            // Join the Vivox positional voice channel.
            await VivoxService.Instance.JoinPositionalChannelAsync(
                _currentChannelName,
                ChatCapability.AudioOnly,
                positionalProperties
            );

            IsInProximityChannel = true;

            Log($"Joined proximity channel: {_currentChannelName}");
        }
        catch (Exception exception)
        {
            IsInProximityChannel = false;

            Debug.LogError(
                $"[VoiceChat] Failed to join proximity channel: {exception}"
            );
        }


        /*
        if (!IsInitialized || !IsLoggedIn)
        {
            Debug.LogWarning(
                "[VoiceChat] Cannot join proximity channel. Vivox is not ready."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(sessionCode))
        {
            Debug.LogError(
                "[VoiceChat] Cannot join proximity channel. Session code is empty."
            );

            return;
        }

        try
        {
            _currentChannelName =
                $"{_channelNamePrefix}{sessionCode}";

            Log($"Joining proximity channel: {_currentChannelName}");

            /*
             * Positional Vivox channel will be implemented here.
             *
             * Example structure:
             *
             * await VivoxService.Instance.JoinPositionalChannelAsync(...);
             *

            // DO NOT mark the channel as joined yet.
            //
            // This will be set to true only after the real
            // JoinPositionalChannelAsync call succeeds.

            Log(
                "Proximity channel join is not implemented yet."
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[VoiceChat] Failed to join proximity channel: {exception}"
            );
        }
        */
    }

    private void OnVivoxChannelJoined(string channelName)
    {
        if (channelName != _currentChannelName)
            return;

        _isPositionTrackingReady = true;

        Log($"Vivox channel is ready for 3D positioning: {channelName}");
    }

    private void OnVivoxChannelLeft(string channelName)
    {
        if (channelName != _currentChannelName)
            return;

        _isPositionTrackingReady = false;

        Log($"Vivox channel is no longer ready for 3D positioning: {channelName}");
    }

    public async Task LeaveProximityChannel()
    {
        if (!IsInProximityChannel)
            return;

        _isPositionTrackingReady = false;

        try
        {
            Log($"Leaving proximity channel: {_currentChannelName}");

            await VivoxService.Instance.LeaveChannelAsync(
                _currentChannelName
            );

            IsInProximityChannel = false;
            _currentChannelName = null;

            Log("Left proximity channel.");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[VoiceChat] Failed to leave proximity channel: {exception}"
            );
        }
    }


    public void SetMicrophoneTransmission(bool transmitting)
    {
        if (!IsLoggedIn)
            return;

        /*
         * Vivox microphone transmission will be implemented here.
         *
         * This will eventually be called by your
         * Push-To-Talk Input Action.
         */

        Log($"Microphone transmission: {transmitting}");
    }


    /*
    public void UpdateVoicePosition(
        Vector3 position,
        Vector3 forward,
        Vector3 up)
    {
        if (!IsInProximityChannel)
            return;


        if (string.IsNullOrEmpty(_currentChannelName))
            return;

        VivoxService.Instance.Set3DPosition(
            position,
            position,
            forward,
            up,
            _currentChannelName
        );


        /*
         * Vivox positional voice update will be implemented here.
         *
         * This will eventually receive the position and orientation
         * of the local player's VoiceOrigin.
         
    }*/

    public void UpdateVoicePosition(Vector3 position, Vector3 forward, Vector3 up)
    {
        if (!IsInProximityChannel)
            return;

        if (!_isPositionTrackingReady)
            return;

        if (string.IsNullOrEmpty(_currentChannelName))
            return;

        VivoxService.Instance.Set3DPosition(
            position,
            position,
            forward,
            up,
            _currentChannelName
        );
    }


    public async Task Logout()
    {
        if (!IsLoggedIn)
            return;

        try
        {
            if (IsInProximityChannel)
            {
                await LeaveProximityChannel();
            }

            await VivoxService.Instance.LogoutAsync();

            IsLoggedIn = false;
            IsInitialized = false;

            Log("Logged out of Vivox.");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[VoiceChat] Logout failed: {exception}"
            );
        }
    }


    private void Log(string message)
    {
        if (_logVoiceEvents)
        {
            Debug.Log($"[VoiceChat] {message}");
        }
    }


    private void OnDestroy()
    {
        if (Instance != this)
            return;

        Instance = null;
    }



    /*
    public static VoiceChatManager Instance { get; private set; }

    [Header("Voice Settings")]
    [SerializeField] private string _channelNamePrefix = "Proximity_";

    [Header("Debug")]
    [SerializeField] private bool _logVoiceEvents = true;

    private string _currentChannelName;

    public bool IsInitialized { get; private set; }
    public bool IsLoggedIn { get; private set; }
    public bool IsInProximityChannel { get; private set; }

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

    private async void Start()
    {
        await InitializeVivox();
    }

    private async Task InitializeVivox()
    {
        try
        {
            // Make sure Unity Services are initialized.
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                await UnityServices.InitializeAsync();
            }

            // Make sure the player is authenticated.
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Log("Unity Services and Authentication ready.");

            // Initialize Vivox.
            await VivoxService.Instance.InitializeAsync();

            IsInitialized = true;

            Log("Vivox initialized.");

            // Log into Vivox.
            await VivoxService.Instance.LoginAsync();

            IsLoggedIn = true;

            Log("Logged into Vivox.");
        }
        catch (Exception exception)
        {
            Debug.LogError($"[VoiceChat] Initialization failed: {exception}");
        }
    }

    public async Task JoinProximityChannel(string sessionCode)
    {
        if (!IsInitialized || !IsLoggedIn)
        {
            Debug.LogWarning(
                "[VoiceChat] Cannot join proximity channel. Vivox is not ready."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(sessionCode))
        {
            Debug.LogError(
                "[VoiceChat] Cannot join proximity channel. Session code is empty."
            );

            return;
        }

        try
        {
            _currentChannelName = $"{_channelNamePrefix}{sessionCode}";

            Log($"Joining proximity channel: {_currentChannelName}");

            /*
             * Prototype:
             *
             * Join the Vivox positional channel here.
             *
             * The exact arguments/method signature should be matched
             * to the Vivox package version installed in the project.
             *

            //await VivoxService.Instance.JoinPositionalChannelAsync(...);

            IsInProximityChannel = true;

            Log("Joined proximity channel.");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[VoiceChat] Failed to join proximity channel: {exception}"
            );
        }
    }

    public async Task LeaveProximityChannel()
    {
        if (!IsInProximityChannel)
            return;

        try
        {
            Log($"Leaving proximity channel: {_currentChannelName}");

            /*
             * Prototype:
             *
             * Leave the current Vivox channel here.
             *
             * The exact method/signature should match the installed
             * Vivox package version.
             *

            // await VivoxService.Instance.LeaveChannelAsync(_currentChannelName);

            IsInProximityChannel = false;
            _currentChannelName = null;

            Log("Left proximity channel.");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[VoiceChat] Failed to leave proximity channel: {exception}"
            );
        }
    }

    public void SetMicrophoneTransmission(bool transmitting)
    {
        if (!IsLoggedIn)
            return;

        /*
         * Prototype:
         *
         * Enable/disable Vivox microphone transmission here.
         *
         * This will eventually be called by your Push-To-Talk
         * Input Action.
         *

        Log($"Microphone transmission: {transmitting}");
    }

    public void UpdateVoicePosition(
        Vector3 position,
        Vector3 forward,
        Vector3 up)
    {
        if (!IsInProximityChannel)
            return;

        /*
         * Prototype:
         *
         * Update Vivox with the local player's 3D position and
         * orientation.
         *
         * This is what will make the voice positional.
         *
    }

    public async Task Logout()
    {
        if (!IsLoggedIn)
            return;

        try
        {
            if (IsInProximityChannel)
            {
                await LeaveProximityChannel();
            }

            /*
             * Vivox logout will go here.
             *

            IsLoggedIn = false;

            Log("Logged out of Vivox.");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[VoiceChat] Logout failed: {exception}"
            );
        }
    }

    private void Log(string message)
    {
        if (_logVoiceEvents)
        {
            Debug.Log($"[VoiceChat] {message}");
        }
    }

    private async void OnDestroy()
    {
        if (Instance != this)
            return;

        /*
         * In the finished version, we will be careful about
         * asynchronous Vivox shutdown here.
         
    }
*/

}
