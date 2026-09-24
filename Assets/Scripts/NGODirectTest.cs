using UnityEngine;
using Unity.Netcode;

public class NGODirectTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            NetworkManager.Singleton.StartHost();
            Debug.Log("Started NGO Host.");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            NetworkManager.Singleton.StartClient();
            Debug.Log("Started NGO Client.");
        }
    }
}
