using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenu : MonoBehaviour {
    [SerializeField]
    private UIDocument mainMenu;
    void Start()
    {
        VisualElement root = mainMenu.rootVisualElement;
        Button QuitButton = root.Query<Button>("QuitButton");
        Button StartGameButton = root.Query<Button>("StartGameButton");
        Button OptionsButton = root.Query<Button>("OptionsButton");
        QuitButton.clicked += Application.Quit;
        StartGameButton.clicked += LoadGameScene;
        OptionsButton.clicked += ShowOptions;
    }

    private void LoadGameScene() {
        SceneManager.LoadScene("Shop");
    }

    private void ShowOptions() {
        Debug.Log("NOT IMPLEMENTED YET");
        throw new System.NotImplementedException();
    }
}
