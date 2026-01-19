using System.Linq;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneObserver {
    private static readonly string[] scenesWithPausingDisabled = { "MainMenu"};
    public static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        var pauseService = ServiceLocatorAccessor.GetService<IPauseService>();
        foreach (var input in Object.FindObjectsByType<UnityEngine.InputSystem.PlayerInput>(FindObjectsSortMode.None)) {
            input.ActivateInput();
        }

        if (pauseService != null) {
            bool shouldBeAbleToPause = !scenesWithPausingDisabled.Contains(scene.name); 
            if (shouldBeAbleToPause) {
                pauseService.EnablePause();
            }
            else {
                DebugLogger.Log(LogChannel.Systems, $"Disabling pause; scene is {scene.name}.", LogLevel.Verbose);
                pauseService.DisablePause();
            }
        }
    }
}