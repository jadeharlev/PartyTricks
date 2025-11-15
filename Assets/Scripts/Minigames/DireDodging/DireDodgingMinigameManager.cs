using System;
using System.Collections;
using UnityEngine;

public class DireDodgingMinigameManager : MonoBehaviour, IMinigameManager
{
    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    public bool IsDoubleRound { get; private set; }
    private IDireDodgingState currentState;
    public static DireDodgingMinigameManager Instance { get; private set; }

    [Header("Minigame Settings")] 
    [SerializeField] private int GameTimeoutDurationInSeconds = 45;
    [SerializeField] private int CountdownDurationInSeconds = 5;
    [SerializeField] private int ResultsDisplayDurationInSeconds = 5;
    [SerializeField] private int[] BaseFundsPerRank = new[] { 100, 80, 60, 50 };

    [Header("References")] 
    [SerializeField] private DireDodgingPlayer[] Players = new DireDodgingPlayer[4];
    [SerializeField] private MinigameStartCountdown StartCountdown;
    
    private bool hasBeenInitialized = false;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    private void Start() {
        SetUpVariables();
        StartCoroutine(WaitForInitialization());
    }

    private void SetUpVariables() {
        StartCountdown.Initialize(CountdownDurationInSeconds);
        for (int i = 0; i < Players.Length; i++) {
            if (Players[i] == null) continue;
            PlayerSlot slot = GameSessionManager.Instance.PlayerSlots[i];
            Players[i].Initialize(i, slot.Navigator, slot.IsAI);
        }
    }

    private void StartGameFlow() {
        currentState = new DireDodgingCountdownState(StartCountdown);
        currentState.Enter();
    }

    private void ChangeState(IDireDodgingState newState) {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void EnableAllPlayerInput() {
        foreach (var player in Players) {
            if (player != null) {
                player.EnableInput();
            }
        }
    }

    public void StartPlayerShooting() {
        foreach (var player in Players) {
            if (player != null) {
                player.StartShooting();
            }
        }
    }
    
    public void TransitionToGameplay() {
        ChangeState(new DireDodgingGameplayState());
    }

    public void TransitionToResults(int[] playerPlaces, int[] playerKills) {
        Debug.Log("Game ended. Places: " + string.Join(", ", playerPlaces) + ", kills: " + string.Join(", ", playerKills));
        ChangeState(new DireDodgingResultsState());
    }

    private IEnumerator WaitForInitialization() {
        while (!hasBeenInitialized) {
            yield return null;
        }

        StartGameFlow();
    }
    
    public void Initialize(bool isDoubleRound) {
        this.IsDoubleRound = isDoubleRound;
        hasBeenInitialized = true;
        DebugLogger.Log(LogChannel.Systems, $"DireDodgingMinigame initialized. Double round: {isDoubleRound}");
    }

    public void RegisterDeath(int killerID, int killedID) {
        DebugLogger.Log(LogChannel.Systems, $"P{killerID+1} eliminated P{killedID+1}!");
        if (currentState is DireDodgingGameplayState gameplayState) {
            gameplayState.HandlePlayerKill(killerID);
            gameplayState.HandlePlayerDeath(killedID);
        }
        else {
            Debug.Log("Wrong state! See DireDodgingMinigameManager, RegisterDeath");
        }
    }

    public void FreezeAllPlayers() {
        foreach (var player in Players) {
            player.Freeze();
        }
    }

    private void Update() {
        if (!hasBeenInitialized) return;
        currentState?.OnUpdate();
    }

    private void OnDestroy() {
        Instance = null;
    }

    public void ReturnAllProjectiles() {
        foreach (var player in Players) {
            player.DestroyVisibleProjectiles();
        }
    }
}
