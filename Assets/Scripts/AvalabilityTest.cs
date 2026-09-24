using Unity.Services.Multiplayer;
using UnityEngine;

public class AvalabilityTest : MonoBehaviour
{
    private ISession CurrentSession { get; set; }

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
    */

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
}
