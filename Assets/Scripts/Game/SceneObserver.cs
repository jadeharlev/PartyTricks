using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneObserver {
    public static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        foreach (var input in Object.FindObjectsByType<UnityEngine.InputSystem.PlayerInput>(FindObjectsSortMode.None)) {
            input.ActivateInput();
        }
    }
}