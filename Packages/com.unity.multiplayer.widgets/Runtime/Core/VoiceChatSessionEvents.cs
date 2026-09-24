using System;
using UnityEngine;

namespace Unity.Multiplayer.Widgets // Newly Added
{
    public static class VoiceChatSessionEvents
    {
        public static event Action<string> SessionJoined;
        public static event Action SessionLeft;

        public static void InvokeSessionJoined(string sessionCode)
        {
            SessionJoined?.Invoke(sessionCode);
        }

        public static void InvokeSessionLeft()
        {
            SessionLeft?.Invoke();
        }
    }
}
