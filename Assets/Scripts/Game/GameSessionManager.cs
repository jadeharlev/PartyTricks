using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
   public static GameSessionManager Instance { get; private set; }

   [Header("Pause Settings")] [SerializeField]
   private GameObject pauseMenuPrefab;
   
   public PlayerSlot[] PlayerSlots = new PlayerSlot[4];
   private PlayerSlotManager playerSlotManager;
   private DeviceDisconnectService deviceDisconnectService;
   public Action<PlayerInput> OnPlayerJoined;

   private void OnEnable() {
      InputSystem.onDeviceChange += deviceDisconnectService.OnDeviceChange;
      SceneManager.sceneLoaded += SceneObserver.OnSceneLoaded;
   }

   private void OnDisable() {
      InputSystem.onDeviceChange -= deviceDisconnectService.OnDeviceChange;
      SceneManager.sceneLoaded -= SceneObserver.OnSceneLoaded;
   }

   private void Awake() {
      if (Instance != null && Instance != this) {
         Destroy(gameObject);
         return;
      }
      Instance = this;
      DontDestroyOnLoad(gameObject);
      playerSlotManager = new PlayerSlotManager(PlayerSlots);
      foreach (var playerSlot in PlayerSlots) {
         var profile = new PlayerProfile(300);
         playerSlot.AssignProfile(profile);
      }
      deviceDisconnectService = new DeviceDisconnectService();
      SetUpPauseManager();
   }

   private void SetUpPauseManager() {
      if (PauseManager.Instance == null) {
         GameObject pauseManagerObject = new GameObject("PauseManager");
         pauseManagerObject.transform.SetParent(transform);
         var pauseManager = pauseManagerObject.AddComponent<PauseManager>();
         if (pauseMenuPrefab != null) {
            pauseManager.SetPauseMenuPrefab(pauseMenuPrefab);
         }
         else {
            Debug.LogWarning("GameSessionManager: pauseMenuPrefab is not assigned.");
         }
      }
   }


   public void HandlePlayerJoin(PlayerInput playerInput) {
      playerSlotManager.AssignPlayer(playerInput);
      OnPlayerJoined?.Invoke(playerInput);
   }

}
