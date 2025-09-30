using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
   public static GameSessionManager Instance { get; private set; }
   public PlayerSlot[] PlayerSlots = new PlayerSlot[4];
   private PlayerSlotManager playerSlotManager;
   private DeviceDisconnectService deviceDisconnectService;
   private SceneObserver sceneObserver;

   private void OnEnable() {
      InputSystem.onDeviceChange += deviceDisconnectService.OnDeviceChange;
      SceneManager.sceneLoaded += sceneObserver.OnSceneLoaded;
   }

   private void OnDisable() {
      InputSystem.onDeviceChange -= deviceDisconnectService.OnDeviceChange;
      SceneManager.sceneLoaded -= sceneObserver.OnSceneLoaded;
   }

   private void Awake() {
      if (Instance != null) {
         Destroy(gameObject);
      }
      Instance = this;
      DontDestroyOnLoad(gameObject);
      playerSlotManager = new PlayerSlotManager(PlayerSlots);
      deviceDisconnectService = new DeviceDisconnectService();
      sceneObserver = new SceneObserver();
   }
   

   public void OnPlayerJoined(PlayerInput playerInput) {
      playerSlotManager.AssignPlayer(playerInput);
   }

}
