using GameDevHQ.FileBase.Plugins.FPS_Character_Controller;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class DeathTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision Triggered!");

        if (!other.CompareTag("Player"))
            return;

        NetworkObject networkObject = other.GetComponent<NetworkObject>();

        if (networkObject == null)
            return;

        Debug.Log("Player accessed");

        FPS_Controller player = other.GetComponent<FPS_Controller>();

        if (player == null)
        {
            Debug.LogError("Player does not have an FPS_Controller component.");
            return;
        }

        player.RequestDeathRpc();
    }



    /*
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision Triggered!");
        if(other.gameObject.tag == "Player" && other.gameObject.GetComponent<NetworkObject>() != null)
        {
            Debug.Log("Player accessed");

            UIManager uIManager = FindAnyObjectByType<UIManager>();
            uIManager.Death(other.gameObject.GetComponent<NetworkObject>().OwnerClientId);

            // Delete Player
            NetworkClient client =
               NetworkManager.Singleton.ConnectedClients[other.gameObject.GetComponent<NetworkObject>().OwnerClientId]; // There is probably an easier way to go about doing this
            if (client.PlayerObject != null)
            {
                client.PlayerObject.Despawn();
                SpectatorSystem _spectatorSystem = FindAnyObjectByType<SpectatorSystem>();
                _spectatorSystem.UpdateAlivePlayers();
            }
        }
    }
    */
}