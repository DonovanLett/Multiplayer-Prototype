using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections; // 


public class FixScript : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<ISession> joinedSession;

    private void Awake()
    {
        Debug.Log("Awake Called");
        if (NetworkSceneManager.Instance == null)
        {
            Debug.LogError("NetworkSceneManager.Instance is null.");
            return;
        }

        StartCoroutine(PauseAdd());

        // joinedSession.AddListener(OnJoinedSession);
    }

    IEnumerator PauseAdd()
    {
        Debug.Log("Pause Called"); 
        yield return null;
        joinedSession.AddListener(OnJoinedSession);
        //NetworkSceneManager.Instance.StartGame();
    }

    
    private void OnJoinedSession(ISession session)
    {
        NetworkSceneManager.Instance.StartSession();
        Debug.Log("Join Session Called");
    }



    /*
    private int previousListenerCount;

    void /Update()
    {
        CheckJoinedSessionEvent();
        int currentCount = joinedSession.GetPersistentEventCount();

        if (currentCount != previousListenerCount)
        {
            Debug.Log(
                $"Joined Session listener count changed: " +
                $"{previousListenerCount} ? {currentCount}"
            );

            previousListenerCount = currentCount;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            if(joinedSession is NetworkSceneManager)
            {

            }
        }
    }

    void CheckJoinedSessionEvent()
    {
        int count = joinedSession.GetPersistentEventCount();

        for (int i = 0; i < count; i++)
        {
            UnityEngine.Object target = joinedSession.GetPersistentTarget(i);
            string method = joinedSession.GetPersistentMethodName(i);

            if (target == null)
            {
                Debug.LogError(
                    $"JOINED SESSION EVENT BROKE! " +
                    $"Listener {i} has a missing target. " +
                    $"Method was: {method}"
                );
            }
        }
    }
    */
}
