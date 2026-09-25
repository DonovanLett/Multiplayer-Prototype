using UnityEngine;
using Unity.Services.Multiplayer;
using System.Collections;
using System.Threading.Tasks; // Is Started Code

public class SessionInfoController : MonoBehaviour
{
    public static SessionInfoController Instance { get; private set; }

    private ISession CurrentSession { get; set; }

    private bool _eventsSubscribed = false;

    private void Awake()
    {
        // Singleton protection
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
        StartCoroutine(WaitForMultiplayerService());
    }

    private IEnumerator WaitForMultiplayerService()
    {
        // Wait until MultiplayerService.Instance is available.
        while (MultiplayerService.Instance == null)
        {
            yield return null;
        }

        // Subscribe to session events.
        MultiplayerService.Instance.SessionAdded += OnSessionAdded;
        MultiplayerService.Instance.SessionRemoved += OnSessionRemoved;

        _eventsSubscribed = true;

        Debug.Log("[SessionInfoController] Successfully subscribed to session events.");
    }

    private void OnDisable()
    {
        if (!_eventsSubscribed)
            return;

        if (MultiplayerService.Instance != null)
        {
            MultiplayerService.Instance.SessionAdded -= OnSessionAdded;
            MultiplayerService.Instance.SessionRemoved -= OnSessionRemoved;
        }

        _eventsSubscribed = false;
    }

    private void OnSessionAdded(ISession session)
    {
        CurrentSession = session;

        Debug.Log(
            $"[SessionInfoController] Current Session set: {CurrentSession.Id}"
        );
    }

    private void OnSessionRemoved(ISession session)
    {
        // Only clear our reference if this is the session we're currently using.
        if (CurrentSession == session)
        {
            CurrentSession = null;

            Debug.Log("[SessionInfoController] Current Session cleared.");
        }
    }

    public async Task SetSessionLocked(bool locked) // orignally void; Is Started Code
    {
        if (CurrentSession == null)
        {
            Debug.LogWarning("[SessionInfoController] No current Session.");
            return;
        }

        if (!CurrentSession.IsHost)
        {
            Debug.LogWarning(
                "[SessionInfoController] Only the Host can lock or unlock the Session."
            );
            return;
        }

        IHostSession hostSession = CurrentSession.AsHost();

        hostSession.IsLocked = locked;

        Debug.Log(
            $"[SessionInfoController] Session lock state set locally to: {hostSession.IsLocked}"
        );

        await hostSession.SavePropertiesAsync();

        Debug.Log(
            $"[SessionInfoController] Session lock state saved: {hostSession.IsLocked}"
        );
    }


    /*
    public void SetSessionLocked(bool locked)
    {
        if (CurrentSession == null)
        {
            Debug.LogWarning("[SessionInfoController] No current Session.");
            return;
        }

        if (!CurrentSession.IsHost)
        {
            Debug.LogWarning(
                "[SessionInfoController] Only the Host can lock or unlock the Session."
            );
            return;
        }

        IHostSession hostSession = CurrentSession.AsHost();
        hostSession.IsLocked = locked;

        Debug.Log(
            $"[SessionInfoController] Session lock state set to: {locked}"
        );
    }

    /*
    public static SessionInfoController Instance { get; private set; }

    private ISession CurrentSession { get; set; }

    private void Awake()
    {
        // Singleton protection
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
        // Listen for a session becoming available.
        MultiplayerService.Instance.SessionAdded += OnSessionAdded;
        MultiplayerService.Instance.SessionRemoved += OnSessionRemoved;
    }

    private void OnDisable()
    {
        if (MultiplayerService.Instance == null)
            return;

        MultiplayerService.Instance.SessionAdded -= OnSessionAdded;
        MultiplayerService.Instance.SessionRemoved -= OnSessionRemoved;
    }

    private void OnSessionAdded(ISession session)
    {
        CurrentSession = session;

        Debug.Log(
            $"Current Session set: {CurrentSession.Id}"
        );
    }

    private void OnSessionRemoved(ISession session)
    {
        // Only clear our reference if this is the session we're currently using.
        if (CurrentSession == session)
        {
            CurrentSession = null;

            Debug.Log("Current Session cleared.");
        }
    }

    /*
    public void ClearCurrentSession()
    {
        CurrentSession = null;
    }

    public void SetCurrentSession(ISession session)
    {
        CurrentSession = session;
    }
    */

    /*
    public void SetCurrentSession()
    {
        CurrentSession = MultiplayerService.Instance.SessionManager.CurrentSession;
    }
    

    public void SetSessionLocked(bool locked)
    {
        if (CurrentSession == null)
        {
            Debug.LogWarning("[SessionInfoManager] No current Session.");
            return;
        }

        if (!CurrentSession.IsHost)
        {
            Debug.LogWarning("[SessionInfoManager] Only the Host can lock or unlock the Session.");
            return;
        }

        IHostSession hostSession = CurrentSession.AsHost();
        hostSession.IsLocked = locked;
    }
    */
}