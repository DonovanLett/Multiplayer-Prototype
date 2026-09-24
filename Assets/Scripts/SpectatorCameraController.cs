using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class SpectatorCameraController : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [SerializeField] private CinemachineCamera povCamera;
    [SerializeField] private CinemachineCamera thirdPersonCamera;
    [SerializeField] private CinemachineCamera freeRoamCamera;

    [Header("Main Unity Camera")]
    [SerializeField] private Camera mainCamera;

    private SpectatorController.SpectatorMode currentMode;

    private ulong currentTargetClientId;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        DisableAllCameras();
    }

    // =========================================================
    // MODE
    // =========================================================



    public void SetMode(
        SpectatorController.SpectatorMode newMode,
        ulong targetClientId)
    {
        currentMode = newMode;
        currentTargetClientId = targetClientId;

        switch (newMode)
        {
            case SpectatorController.SpectatorMode.POV:
                EnterPOV(targetClientId);
                break;

            case SpectatorController.SpectatorMode.ThirdPerson:
                EnterThirdPerson(targetClientId);
                break;

            case SpectatorController.SpectatorMode.FreeRoam:
                EnterFreeRoam();
                break;
        }

        // UI Code
        UIManager uIManager = FindAnyObjectByType<UIManager>();
        uIManager.UpdateSpectatorUI(currentTargetClientId, currentMode);
    }

    // =========================================================
    // TARGET
    // =========================================================

    public void SetTarget(ulong targetClientId)
    {
        currentTargetClientId = targetClientId;

        if (currentMode == SpectatorController.SpectatorMode.POV)
        {
            SetPOVTarget(targetClientId);
        }
        else if (currentMode ==
                 SpectatorController.SpectatorMode.ThirdPerson)
        {
            SetThirdPersonTarget(targetClientId);
        }

        // UI Code
        UIManager uIManager = FindAnyObjectByType<UIManager>();
        uIManager.UpdateSpectatorUI(currentTargetClientId, currentMode);
    }

    // =========================================================
    // THIRD PERSON
    // =========================================================

    private void EnterThirdPerson(ulong targetClientId)
    {
        NetworkObject playerObject =
            GetPlayerObject(targetClientId);

        if (playerObject == null)
        {
            Debug.LogWarning(
                $"Spectator: Could not find Player Object " +
                $"for Client {targetClientId}."
            );

            return;
        }

        SetThirdPersonTarget(targetClientId);

        DisableAllCameras();

        thirdPersonCamera.gameObject.SetActive(true);

        thirdPersonCamera.enabled = true;

        Debug.Log(
            $"Spectator Camera: Third Person ? Client {targetClientId}"
        );
    }

    private void SetThirdPersonTarget(ulong targetClientId)
    {
        NetworkObject playerObject =
            GetPlayerObject(targetClientId);

        if (playerObject == null)
            return;

        Transform playerTransform =
            playerObject.transform;

        /*
         * Cinemachine 3:
         *
         * CameraTarget contains the TrackingTarget.
         *
         * We are intentionally assigning the Player Prefab's
         * ROOT Transform here, exactly as you requested.
         */
        thirdPersonCamera.Target.TrackingTarget =
            playerTransform;
    }

    // =========================================================
    // POV
    // =========================================================

    private void EnterPOV(ulong targetClientId)
    {
        NetworkObject playerObject =
            GetPlayerObject(targetClientId);

        if (playerObject == null)
        {
            Debug.LogWarning(
                $"Spectator: Could not find Player Object " +
                $"for Client {targetClientId}."
            );

            return;
        }

        DisableAllCameras();

        povCamera.gameObject.SetActive(true);

        povCamera.enabled = true;

        Debug.Log(
            $"Spectator Camera: POV ? Client {targetClientId}"
        );

        SetPOVTarget(targetClientId);
    }

    private void SetPOVTarget(ulong targetClientId)
    {
        POVSpectatorCamera _camera = FindAnyObjectByType<POVSpectatorCamera>();

        _camera.SetTarget(targetClientId);


        /*
        NetworkObject playerObject =
            GetPlayerObject(targetClientId);

        if (playerObject == null)
            return;

        Transform playerTransform =
            playerObject.transform;

        povCamera.Target.TrackingTarget =
            playerTransform;
        */
    }

    // =========================================================
    // FREE ROAM
    // =========================================================

    public void EnterFreeRoam()
    {
        if (mainCamera == null)
        {
            Debug.LogError(
                "Spectator Camera: Main Camera is missing."
            );

            return;
        }

        /*
         * Capture the EXACT position and rotation of whatever
         * camera is currently being displayed.
         */
        Vector3 previousPosition =
            mainCamera.transform.position;

        Quaternion previousRotation =
            mainCamera.transform.rotation;

        /*
         * Disable the Cinemachine cameras before enabling
         * Free-Roam.
         */
        DisableAllCameras();

        /*
         * Free-Roam is a Cinemachine Camera with no tracking
         * target. We manually control its Transform.
         */
        freeRoamCamera.transform.SetPositionAndRotation(
            previousPosition,
            previousRotation
        );

        freeRoamCamera.gameObject.SetActive(true); 

        freeRoamCamera.enabled = true;

        //InputManager.Instance.EnterSpectatorMode(); // Added

        Debug.Log(
            "Spectator Camera: Entered Free-Roam."
        );
    }

    // =========================================================
    // CAMERA MANAGEMENT
    // =========================================================

    private void DisableAllCameras()
    {
        if (povCamera != null)
            povCamera.gameObject.SetActive(false);
            povCamera.enabled = false;

        if (thirdPersonCamera != null)
            thirdPersonCamera.gameObject.SetActive(false);
            thirdPersonCamera.enabled = false;

        if (freeRoamCamera != null)
            freeRoamCamera.gameObject.SetActive(false);
            freeRoamCamera.enabled = false;
    }

    // =========================================================
    // FIND PLAYER
    // =========================================================

    private NetworkObject GetPlayerObject(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
            return null;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(
                clientId,
                out NetworkClient client))
        {
            return null;
        }

        return client.PlayerObject;
    }


    // Added Improvement
    public void ForceFreeRoam()
    {
        EnterFreeRoam();

        // UI Code
        UIManager uIManager = FindAnyObjectByType<UIManager>();
        uIManager.OnAllPlayersDead();
    }
    // Added Improvement

    // =========================================================
    // ACCESSORS
    // =========================================================

    public CinemachineCamera POVCamera
    {
        get { return povCamera; }
    }

    public CinemachineCamera ThirdPersonCamera
    {
        get { return thirdPersonCamera; }
    }

    public CinemachineCamera FreeRoamCamera
    {
        get { return freeRoamCamera; }
    }
}
