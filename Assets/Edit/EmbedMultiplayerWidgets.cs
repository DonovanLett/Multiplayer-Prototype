using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class EmbedMultiplayerWidgets
{
    private static EmbedRequest embedRequest;

    [MenuItem("Tools/Embed Multiplayer Widgets")]
    private static void Embed()
    {
        embedRequest = Client.Embed("com.unity.multiplayer.widgets");

        EditorApplication.update += CheckRequest;

        Debug.Log("Embedding Multiplayer Widgets...");
    }

    private static void CheckRequest()
    {
        if (!embedRequest.IsCompleted)
            return;

        EditorApplication.update -= CheckRequest;

        if (embedRequest.Status == StatusCode.Success)
        {
            Debug.Log(
                $"Multiplayer Widgets embedded successfully: " +
                $"{embedRequest.Result.packageId}"
            );
        }
        else
        {
            Debug.LogError(
                $"Failed to embed Multiplayer Widgets: " +
                $"{embedRequest.Error?.message}"
            );
        }
    }
}