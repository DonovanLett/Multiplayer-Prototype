using UnityEngine;

namespace Unity.Multiplayer.Widgets
{
    public class ChatManager : MonoBehaviour
    {
        public static ChatManager Instance { get; private set; }

        public bool IsChatJoined { get; private set; }
        public string CurrentChatId { get; private set; }

        private void Awake()
        {
            Debug.Log("ChatManager: Awake()");

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        public void SetChatJoined(string chatId)
        {
            IsChatJoined = true;
            CurrentChatId = chatId;

            Debug.Log($"ChatManager: Chat joined ({chatId})");
        }

        public void SetChatLeft(string chatId)
        {
            if (CurrentChatId != chatId)
                return;

            IsChatJoined = false;
            CurrentChatId = null;

            Debug.Log($"ChatManager: Chat left ({chatId})");
        }
    }
}