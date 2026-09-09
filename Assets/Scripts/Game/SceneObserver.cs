using System.Linq;
using Debug;
using Options;
using Services;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game {
    public class SceneObserver {
        private static readonly string[] scenesWithPausingDisabled = { "MainMenu" };
        public static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            GameSettings.Display.ApplyAntiAliasing();
            var pauseService = ServiceLocatorAccessor.GetService<IPauseService>();
            foreach (var input in PlayerInput.all) {
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
}