using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UI; // Up-Down Input SetMode

public class SpectatorController : MonoBehaviour
{
    
    public enum SpectatorMode
    {
        POV,
        ThirdPerson,
        FreeRoam
    }

    [Header("Current State")]
    [SerializeField] private SpectatorMode currentMode;
    [SerializeField] private ulong currentTargetClientId;
    [SerializeField] private SpectatorSystem _spectatorSystem;


    private bool isSpectating;
    private bool allPlayersDead = false;

    // Up-Down Input
    [SerializeField] private Toggle[] spectatorToggles;
    private int currentModeIndex = 0;

    /*
    private void OnEnable() //// Where the _spectatorSystem was stored originally
    {
        _spectatorSystem = FindAnyObjectByType<SpectatorSystem>();

        if (_spectatorSystem != null)
        {
            _spectatorSystem.AliveClientIds.OnListChanged += OnAlivePlayersChanged; // This IS being executed
            Debug.Log(
    $"SpectatorController subscribed to SpectatorSystem " +
    $"{_spectatorSystem.gameObject.name} | " +
    $"AliveClientIds Count: {_spectatorSystem.AliveClientIds.Count}"
);
            //Debug.Log("OnAlivePlayersChanged is connected to _spectatorSystem.AliveClientIds.OnListChanged"); // This IS being called
        }
        else
        {
            Debug.Log("OnAlivePlayersChanged is NOT connected to _spectatorSystem.AliveClientIds.OnListChanged"); // This ISN'T being called
        }
    }
    */

    private void Start()
    {
        _spectatorSystem = FindAnyObjectByType<SpectatorSystem>();

        if (_spectatorSystem != null)
        {
            _spectatorSystem.AliveClientIds.OnListChanged += OnAlivePlayersChanged; // This IS being executed
            Debug.Log(
    $"SpectatorController subscribed to SpectatorSystem " +
    $"{_spectatorSystem.gameObject.name} | " +
    $"AliveClientIds Count: {_spectatorSystem.AliveClientIds.Count}"
);
            //Debug.Log("OnAlivePlayersChanged is connected to _spectatorSystem.AliveClientIds.OnListChanged"); // This IS being called
        }
        else
        {
            Debug.Log("OnAlivePlayersChanged is NOT connected to _spectatorSystem.AliveClientIds.OnListChanged"); // This ISN'T being called
        }
    }

    private void OnDisable()
    {
        if (_spectatorSystem != null)
        {
            _spectatorSystem.AliveClientIds.OnListChanged -= OnAlivePlayersChanged;
        }
    }

    private void Update()
    {
        if (!isSpectating)
            return;

        HandleTargetSwitching();

        // Up-Down Behaviour
        List<ulong> alivePlayers = GetAlivePlayers();

        // Nobody is alive.
        if (alivePlayers.Count > 0)/// Added Improvement
        {///Added Improvement
            HandleModeSwitching();
        }///Added Improvement



        //HandleModeSwitching();
    }



    // =========================================================
    // ENTER / EXIT SPECTATOR MODE
    // =========================================================

    public void EnterSpectatorMode()
    {
        isSpectating = true;

        List<ulong> alivePlayers = GetAlivePlayers();

        // Nobody is alive.
        if (alivePlayers.Count == 0)
        {
            Debug.Log("The Script doesn't think anyone's alive");
            // Added Improvement
            /*
            foreach (Toggle toggle in spectatorToggles)
            {
                toggle.gameObject.SetActive(false);
            }
            */
            // Added Improvement
            ForceFreeRoam();
            currentModeIndex = 2;
            //return;
        }
        else
        {
            // currentTargetClientId = alivePlayers[0];
            currentTargetClientId = alivePlayers[0];
            // Normal spectator mode starts in Third Person.
            // SetThirdPerson();
            SetMode(SpectatorMode.ThirdPerson);
            currentModeIndex = 0; /// This might be unnecessary
            // spectatorToggles[currentModeIndex].isOn = true; /// This might be unnecessary
        }

        spectatorToggles[currentModeIndex].isOn = true; /// This might be unnecessary

        // Start with the lowest Client ID.
        //currentTargetClientId = alivePlayers[0];

        // Normal spectator mode starts in Third Person.
        //SetMode(SpectatorMode.ThirdPerson);
        

        // UI
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        UIManager uIManager = FindAnyObjectByType<UIManager>();
        if(uIManager != null)
        {
            uIManager.DeathScreenLeft(clientId);
            uIManager.Spectating(clientId);
        }

        // Up-Down Input; make sure this is right
        //currentModeIndex = 0; // This might be unnecessary, but be certain/// taken out
        //spectatorToggles[currentModeIndex].isOn = true; /// taken out

        /*
         isSpectating = true;

        List<ulong> alivePlayers = GetAlivePlayers();

        // Nobody is alive.
        if (alivePlayers.Count == 0)
        {
            Debug.Log("The Script doesn't think anyone's alive");
            EnterFreeRoam();
            return;
        }

        // Start with the lowest Client ID.
        currentTargetClientId = alivePlayers[0];

        // Normal spectator mode starts in Third Person.
        SetMode(SpectatorMode.ThirdPerson);

        // UI
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        UIManager uIManager = FindAnyObjectByType<UIManager>();
        if(uIManager != null)
        {
            uIManager.DeathScreenLeft(clientId);
            uIManager.Spectating(clientId);
        }

        // Up-Down Input; make sure this is right
        currentModeIndex = 0;
        spectatorToggles[currentModeIndex].isOn = true;
         
         */
    }

    public void ExitSpectatorMode()
    {
        isSpectating = false;
    }

    // =========================================================
    // SPECTATOR MODES
    // =========================================================

    // Up-Down Behaviour

    /*
    private void HandleModeSwitching() // The One that doesn't rely on Toggles
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentModeIndex--;

            if (currentModeIndex < 0)
                currentModeIndex = 2;

            switch (currentModeIndex)
            {
                case 0:
                    SetPOV();
                    break;

                case 1:
                    SetThirdPerson();
                    break;

                case 2:
                    SetFreeRoam();
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentModeIndex++;

            if (currentModeIndex > 2)
                currentModeIndex = 0;

            switch (currentModeIndex)
            {
                case 0:
                    SetPOV();
                    break;

                case 1:
                    SetThirdPerson();
                    break;

                case 2:
                    SetFreeRoam();
                    break;
            }
        }
    }
    */

    // Up-Down Behaviour
    public void HandleModeSwitching() // The one that works alongside toggles, and thereby, effects them in order to trigger the POVs
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentModeIndex--;

            if (currentModeIndex < 0)
                currentModeIndex = spectatorToggles.Length - 1;

            spectatorToggles[currentModeIndex].isOn = true;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentModeIndex++;

            if (currentModeIndex >= spectatorToggles.Length)
                currentModeIndex = 0;

            spectatorToggles[currentModeIndex].isOn = true;
        }
    }

    public void SetPOV() // These are for button Presses
    {
        SetMode(SpectatorMode.POV);

        if(currentModeIndex != 1) // This needs to be fixed; unnecessary hard-code
        {
            currentModeIndex = 1;
            // spectatorToggles[currentModeIndex].isOn = true;
        }
        
    }

    public void SetThirdPerson() // These are for button Presses
    {
        SetMode(SpectatorMode.ThirdPerson);

        if (currentModeIndex != 0) // This needs to be fixed; unnecessary hard-code
        {
            currentModeIndex = 0;
            // spectatorToggles[currentModeIndex].isOn = true;
        }
        
    }

    public void SetFreeRoam() // These are for button Presses
    {
        SetMode(SpectatorMode.FreeRoam);

        if (currentModeIndex != 2) // This needs to be fixed; unnecessary hard-code
        {
            currentModeIndex = 2;
            // spectatorToggles[currentModeIndex].isOn = true;
        }
        
    }

    public void SetMode(SpectatorMode newMode)
    {
        if (!isSpectating)
            return;

        // POV and Third Person require at least one living Player.
        if (newMode != SpectatorMode.FreeRoam &&
            GetAlivePlayers().Count == 0)
        {
            EnterFreeRoam();
            return;
        }

        currentMode = newMode;

        Debug.Log($"Spectator Mode: {currentMode}");

        // The camera system will respond to this later.
        SpectatorCameraController cameraController =
            GetComponent<SpectatorCameraController>();

        if (cameraController != null)
        {
            cameraController.SetMode(currentMode, currentTargetClientId); // This is where it is set. !!!!!!!!!!!!!!
        }
    }

    public void EnterFreeRoam()
    {
        isSpectating = true;

        currentMode = SpectatorMode.FreeRoam;

        Debug.Log("Spectator: Entering Free-Roam.");

        SpectatorCameraController cameraController =
            GetComponent<SpectatorCameraController>();

        if (cameraController != null)
        {
            cameraController.EnterFreeRoam();
        }
    }

    // Added Improvement
    public void ForceFreeRoam()
    {
        isSpectating = true;

        if (currentModeIndex != 2) // This needs to be fixed; unnecessary hard-code
        {
            currentModeIndex = 2;
            // spectatorToggles[currentModeIndex].isOn = true;
        }
        spectatorToggles[currentModeIndex].isOn = true;

        foreach (Toggle toggle in spectatorToggles)
        {
            toggle.gameObject.SetActive(false);
        }

        currentMode = SpectatorMode.FreeRoam;

        Debug.Log("Spectator: Entering Free-Roam.");

        SpectatorCameraController cameraController =
            GetComponent<SpectatorCameraController>();

        if (cameraController != null)
        {
            cameraController.ForceFreeRoam();
        }
    }
    // Added Improvement

    // =========================================================
    // PLAYER SWITCHING
    // =========================================================

    private void HandleTargetSwitching()
    {
        // Arrow keys do nothing in Free-Roam.
        if (currentMode == SpectatorMode.FreeRoam)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SwitchTarget(1);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SwitchTarget(-1);
        }
    }

    private void SwitchTarget(int direction)
    {
        List<ulong> alivePlayers = GetAlivePlayers();

        if (alivePlayers.Count == 0)
        {
            EnterFreeRoam();
            return;
        }

        int currentIndex =
            alivePlayers.IndexOf(currentTargetClientId);

        // Current target somehow isn't in the list anymore.
        if (currentIndex == -1)
        {
            currentTargetClientId = alivePlayers[0];
        }
        else
        {
            currentIndex += direction;

            // Wrap around.
            if (currentIndex >= alivePlayers.Count)
            {
                currentIndex = 0;
            }
            else if (currentIndex < 0)
            {
                currentIndex = alivePlayers.Count - 1;
            }

            currentTargetClientId = alivePlayers[currentIndex];
        }

        Debug.Log(
            $"Spectator Target: Client {currentTargetClientId}"
        );

        SpectatorCameraController cameraController =
            GetComponent<SpectatorCameraController>();

        if (cameraController != null)
        {
            cameraController.SetTarget(currentTargetClientId);
        }
    }

    // =========================================================
    // PLAYER REMOVAL / RESPAWN
    // =========================================================

    private void OnAlivePlayersChanged( // This isn't being triggered on anything for some reason, even when AliveClientIds is changed. Figure out why that is.
        NetworkListEvent<ulong> changeEvent)
    {
        Debug.Log("OnAlivePlayerChanged is called in this Client...");

        if (!isSpectating)
            return;

        Debug.Log("...and it knows we're spectating");
        List<ulong> alivePlayers = GetAlivePlayers();

        // Nobody remains alive.
        if (alivePlayers.Count == 0)
        {
            ForceFreeRoam(); // Originally EnterFreemRoam();

            // Added Improvement
            
            // Added Improvement

            allPlayersDead = true;
        
            return;

            /// All of this is being triggered on void Start, when they're really are no 
        }
        else if (allPlayersDead)
        {

            // !!!Return the ability to choose spectator modes, yet have them still defaulted to FreeRoam
            //currentTargetClientId = alivePlayers[0];
            // Normal spectator mode starts in Third Person.
            // SetThirdPerson();
            // SetMode(SpectatorMode.ThirdPerson);
            //currentModeIndex = 0;
            //EnterFreeRoam();
            //FindAnyObjectOfType<SpectatorCameraController>().SetMode()
            // SetMode(currentMode); // Maybe Fix This
            //spectatorToggles[currentModeIndex].isOn = true;
            // SetFreeRoam(); // Make sure this is right.
            // UI Code
            UIManager uIManager = FindAnyObjectByType<UIManager>();
            uIManager.UpdateSpectatorUI(currentTargetClientId, currentMode);
            foreach (Toggle toggle in spectatorToggles)
            {
                toggle.gameObject.SetActive(true);
            }
            //spectatorToggles[currentModeIndex].isOn = true;
            allPlayersDead = true;
        }

        // Our current target is still alive. // This is where we're having problems; the Spectator for each Client does appear to be updating, but it isn't switching properly if watching a Player that has since died.
        if (alivePlayers.Contains(currentTargetClientId))
            return;

        Debug.Log("It is aware we are watching a Client that should now be dead");
        // Our target died/despawned.
        //
        // Try to select the next Client ID after the old target.
        ulong newTarget = GetNextAvailableTarget(
            currentTargetClientId,
            alivePlayers
        );

        currentTargetClientId = newTarget;

        Debug.Log(
            $"Spectator target removed. " +
            $"Now watching Client {currentTargetClientId}."
        );

        SpectatorCameraController cameraController =
            GetComponent<SpectatorCameraController>();

        if (cameraController != null)
        {
            cameraController.SetTarget(currentTargetClientId);
        }
    }


    /////////////

    private ulong GetNextAvailableTarget(
        ulong oldTarget,
        List<ulong> alivePlayers)
    {
        // Find the first Client ID greater than the
        // previous target.
        foreach (ulong clientId in alivePlayers)
        {
            if (clientId > oldTarget)
                return clientId;
        }

        // If there isn't one, wrap around to the
        // lowest available Client ID.
        return alivePlayers[0];
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private List<ulong> GetAlivePlayers()
    {
        List<ulong> players = new List<ulong>();

        if (_spectatorSystem == null)
            return players;

        foreach (ulong clientId in
                 _spectatorSystem.AliveClientIds)
        {
            players.Add(clientId);
        }

        // NetworkList normally preserves insertion order,
        // but explicitly sort by Client ID so spectator
        // order is guaranteed.
        players.Sort();

        return players;
    }

    // =========================================================
    // PUBLIC ACCESSORS
    // =========================================================

    public SpectatorMode CurrentMode
    {
        get { return currentMode; }
    }

    public ulong CurrentTargetClientId
    {
        get { return currentTargetClientId; }
    }

    public bool IsSpectating
    {
        get { return isSpectating; }
    }
    
}
