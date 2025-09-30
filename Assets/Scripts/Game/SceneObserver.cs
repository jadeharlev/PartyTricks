using UnityEngine.SceneManagement;

public class SceneObserver {
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        foreach (var input in UnityEngine.Object.FindObjectsOfType<UnityEngine.InputSystem.PlayerInput>()) {
            input.ActivateInput();
        }
    }
}