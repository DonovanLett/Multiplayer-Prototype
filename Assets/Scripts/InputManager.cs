using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public enum InputMode
    {
        Gameplay,
        Spectator,
        Chat
    }

    [Header("Input Actions")]
    [SerializeField]
    private InputActionAsset _inputActions;

    private InputActionMap _gameplayMap;
    private InputActionMap _spectatorMap;
    //private InputActionMap _uiMap;

    public InputMode CurrentMode { get; private set; }

    private InputMode _previousMode;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeInputMaps();
    }

    private void InitializeInputMaps()
    {
        if (_inputActions == null)
        {
            Debug.LogError(
                "InputManager: No Input Action Asset has been assigned."
            );

            return;
        }

        _gameplayMap = _inputActions.FindActionMap("PlayerMovement");
        _spectatorMap = _inputActions.FindActionMap("Spectator");
        //_uiMap = _inputActions.FindActionMap("UI");

        if (_gameplayMap == null)
        {
            Debug.LogError(
                "InputManager: Could not find the 'Gameplay' Action Map."
            );
        }

        if (_spectatorMap == null)
        {
            Debug.LogError(
                "InputManager: Could not find the 'Spectator' Action Map."
            );
        }

        /*
        if (_uiMap == null)
        {
            Debug.LogError(
                "InputManager: Could not find the 'UI' Action Map."
            );
        }
        */

        // UI input should always remain available.
        //_uiMap?.Enable();

        // Start in normal Gameplay mode.
        EnterGameplayMode();
    }

    public void EnterGameplayMode()
    {
        CurrentMode = InputMode.Gameplay;

        _gameplayMap?.Enable();
        _spectatorMap?.Disable();

        Debug.Log("InputManager: Gameplay Input Enabled.");
    }

    public void EnterSpectatorMode()
    {
        CurrentMode = InputMode.Spectator;

        _gameplayMap?.Disable();
        _spectatorMap?.Enable();

        Debug.Log("InputManager: Spectator Input Enabled.");
    }

    public void EnterChatMode()
    {
        // Remember what mode we were in before Chat.
        _previousMode = CurrentMode;

        CurrentMode = InputMode.Chat;

        // Disable all gameplay-related input.
        _gameplayMap?.Disable();
        _spectatorMap?.Disable();

        // UI remains enabled.
        //_uiMap?.Enable();

        Debug.Log("InputManager: Chat Input Enabled. Gameplay Input Disabled.");
    }

    public void ExitChatMode()
    {
        Debug.Log("InputManager: Exiting Chat Mode.");

        // Restore whatever mode we were using before opening Chat.
        switch (_previousMode)
        {
            case InputMode.Gameplay:
                EnterGameplayMode();
                break;

            case InputMode.Spectator:
                EnterSpectatorMode();
                break;

            case InputMode.Chat:
                EnterGameplayMode();
                break;

            default:
                EnterGameplayMode();
                break;
        }
    }

    public bool IsGameplayInputEnabled()
    {
        return _gameplayMap != null && _gameplayMap.enabled;
    }

    public bool IsSpectatorInputEnabled()
    {
        return _spectatorMap != null && _spectatorMap.enabled;
    }

    public bool IsChatMode()
    {
        return CurrentMode == InputMode.Chat;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
