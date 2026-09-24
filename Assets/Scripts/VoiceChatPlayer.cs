using UnityEngine;
using Unity.Netcode;

public class VoiceChatPlayer : NetworkBehaviour
{
    [Header("Voice Position")]
    [SerializeField] private Transform _voiceOrigin;

    [Header("Voice Listener")]
    [SerializeField] private Transform _listenerTransform;

    private void LateUpdate()
    {
        if (!IsOwner)
            return;

        if (VoiceChatManager.Instance == null)
            return;

        if (!VoiceChatManager.Instance.IsInProximityChannel)
            return;

        if (_voiceOrigin == null || _listenerTransform == null)
            return;

        VoiceChatManager.Instance.UpdateVoicePosition(
            _voiceOrigin.position,
            _listenerTransform.forward,
            _listenerTransform.up
        );
    }
}