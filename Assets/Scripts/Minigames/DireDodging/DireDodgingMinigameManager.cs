using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class DireDodgingMinigameManager : MonoBehaviour, IMinigameManager {
    private EventInstance musicInstance;
    public event Action<PlayerMinigameResult[]> OnMinigameFinished;
    public bool IsDoubleRound { get; private set; }
    private IDireDodgingState currentState;
    public static DireDodgingMinigameManager Instance { get; private set; }

    [Header("Minigame Settings")] 
    [SerializeField] private int GameTimeoutDurationInSeconds = 25;
    [SerializeField] private int CountdownDurationInSeconds = 5;
    [SerializeField] private int ResultsDisplayDurationInSeconds = 5;
    [SerializeField] private int[] BaseFundsPerRank = new[] { 100, 80, 60, 50 };
    [SerializeField] private int FundsPerKill = 30;
    [SerializeField] private EventReference MusicEvent;

    [Header("References")] 
    [SerializeField] private DireDodgingPlayer[] Players = new DireDodgingPlayer[4];
    [SerializeField] private MinigameStartCountdown StartCountdown;
    [SerializeField] private MinigameTimer MinigameTimer;
    [SerializeField] private PlayerCornerDisplay[] PlayerCornerDisplays = new PlayerCornerDisplay[4];
    [SerializeField] private PlacesDisplay PlacesDisplay;
    [SerializeField] private Camera MainCamera;
    
    private bool hasBeenInitialized = false;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        this.musicInstance = RuntimeManager.CreateInstance(MusicEvent);
    }

    private void Start() {
        SetUpVariables();
        StartCoroutine(WaitForInitialization());
    }

    private void SetUpVariables() {
        InitializePlayerDisplays();
        PlacesDisplay.Hide();
        StartCountdown.Initialize(CountdownDurationInSeconds);
        if (IsDoubleRound) GameTimeoutDurationInSeconds *= 2;
        MinigameTimer.Initialize(GameTimeoutDurationInSeconds, null);
        for (int i = 0; i < Players.Length; i++) {
            if (Players[i] == null) continue;
            PlayerSlot slot = GameSessionManager.Instance.PlayerSlots[i];
            int increasedHPPowerupCount = 0;
            int increasedAttackSpeedPowerupCount = 0;
            foreach (var itemDefinition in slot.Profile.Inventory.Items) {
                if (itemDefinition.Id == "increasedHP") {
                    increasedHPPowerupCount++;
                }

                if (itemDefinition.Id == "increasedAttackSpeed") {
                    increasedAttackSpeedPowerupCount++;
                }
            }
            Players[i].Initialize(i, slot.Navigator, slot.IsAI, increasedHPPowerupCount, increasedAttackSpeedPowerupCount);
        }
    }

    private void InitializePlayerDisplays() {
        for (int i = 0; i < 4; i++) {
            var slot = GameSessionManager.Instance.PlayerSlots[i];
            if (slot?.Profile != null) {
                PlayerCornerDisplays[i].Initialize(slot.Profile, PlayerCornerDisplay.DisplayMode.Eliminations);
            }
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
        ChangeState(new DireDodgingGameplayState(MinigameTimer, PlayerCornerDisplays, MainCamera));
    }

    public void TransitionToResults(int[] playerPlaces, int[] playerKills) {
        Debug.Log("Game ended. Places: " + string.Join(", ", playerPlaces) + ", kills: " + string.Join(", ", playerKills));
        DireDodgingResultsState resultsState = new DireDodgingResultsState(playerPlaces, playerKills, BaseFundsPerRank, FundsPerKill, PlacesDisplay, MinigameTimer);
        ChangeState(resultsState);
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

    public void StartMusic() {
        musicInstance.start();
    }

    public void SetMusicIntensity(float intensity) {
        musicInstance.setParameterByName("Intensity", intensity);
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

    private void OnDisable() {
        musicInstance.stop(STOP_MODE.IMMEDIATE);
    }

    public void ReturnAllProjectiles() {
        foreach (var player in Players) {
            player.DestroyVisibleProjectiles();
        }
    }

    public void StartIncreasingIntensity(int remainingTimeInSeconds) {
        foreach (var player in Players) {
            player.StartIncreasingIntensity(remainingTimeInSeconds);
        }
    }

    public void OnGameEnd(PlayerMinigameResult[] playerResults) {
        StartCoroutine(WaitAndEndMinigame(playerResults));
    }

    private IEnumerator WaitAndEndMinigame(PlayerMinigameResult[] playerResults) {
        yield return new WaitForSeconds(ResultsDisplayDurationInSeconds);
        OnMinigameFinished?.Invoke(playerResults);
    }
}
