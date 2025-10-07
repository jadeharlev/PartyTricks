using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneObserver {
    private static readonly string[] scenesWithPausingDisabled = { "MainMenu"};
    public static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        foreach (var input in Object.FindObjectsByType<UnityEngine.InputSystem.PlayerInput>(FindObjectsSortMode.None)) {
            input.ActivateInput();
        }

        if (PauseManager.Instance != null) {
            bool shouldBeAbleToPause = !scenesWithPausingDisabled.Contains(scene.name); 
            if (shouldBeAbleToPause) {
                PauseManager.Instance.EnablePause();
            }
            else {
                DebugLogger.Log(LogChannel.Systems, $"Disabling pause; scene is {scene.name}.", LogLevel.Verbose);
                PauseManager.Instance.DisablePause();
            }
        }
    }
}