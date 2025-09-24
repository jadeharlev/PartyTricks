// WIP: Not in use yet.
using UnityEngine;

public class GameState : MonoBehaviour
{
    private enum GameStates {
        MainMenu,
        Shop,
        PlayingMinigame,
        Paused,
        EndOfGame
    }

    private GameStates currentState;
        
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        currentState = GameStates.MainMenu;
    }
}
