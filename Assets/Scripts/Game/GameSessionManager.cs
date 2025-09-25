using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSessionManager : MonoBehaviour
{
   public static GameSessionManager Instance { get; private set; }
   public PlayerSlot[] PlayerSlots = new PlayerSlot[4];

   private void OnEnable() {
      InputSystem.onDeviceChange += OnDeviceChange;
      SceneManager.sceneLoaded += OnSceneLoaded;
   }

   private void OnDisable() {
      InputSystem.onDeviceChange -= OnDeviceChange;
      SceneManager.sceneLoaded -= OnSceneLoaded;
   }

   private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
      foreach (PlayerSlot playerSlot in PlayerSlots) {
         var playerInput = playerSlot.GetComponentInChildren<PlayerInput>();
         if(playerInput != null) playerInput.ActivateInput();
      }
   }

   private void OnDeviceChange(InputDevice device, InputDeviceChange change) {
      if (change == InputDeviceChange.Disconnected) {
         Debug.Log("Device disconnected: " + device);
         Time.timeScale = 0;
      }
      else if (change == InputDeviceChange.Reconnected) {
         Debug.Log("Device reconnected: " + device);
         Time.timeScale = 1f;
      }
   }

   private void Awake() {
      if (Instance != null) {
         Destroy(gameObject);
      }
      Instance = this;
      DontDestroyOnLoad(gameObject);
      InitializeSlots();
   }

   private void InitializeSlots() {
      for (int i = 0; i < PlayerSlots.Length; i++) {
         if (PlayerSlots[i] == null) {
            Debug.LogError("GameSessionManager: PlayerSlot at index " + i + " is not assigned in the inspector.");
            continue;
         }
         SetUpAIShopInput(i);
      }
   }

   private void SetUpAIShopInput(int playerSlotIndex) {
      var aiGameObject = new GameObject("AIInput_P" + playerSlotIndex);
      aiGameObject.transform.SetParent(PlayerSlots[playerSlotIndex].transform);
      var aiHandler = aiGameObject.AddComponent<AIShopInputHandler>();
      PlayerSlots[playerSlotIndex].Initialize(playerSlotIndex, aiHandler, true);
   }

   public void OnPlayerJoined(PlayerInput playerInput) {
      Debug.Log("Player joined: " + playerInput.playerIndex);
      int slotIndex = FindFirstAI();
      if (slotIndex == -1) {
         Debug.Log("All slots are already human!!");
         Destroy(playerInput.gameObject);
         return;
      }

      playerInput.transform.SetParent(PlayerSlots[slotIndex].transform);
      var shopHandler = playerInput.gameObject.AddComponent<PlayerShopInputHandler>();
      shopHandler.Initialize(playerInput);
      PlayerSlots[slotIndex].ReplaceShopNavigator(shopHandler, false);
   }

   private int FindFirstAI() {
      for (int i = 0; i < PlayerSlots.Length; i++) {
         if (PlayerSlots[i].Navigator is AIShopInputHandler) {
            return i;
         }
      }

      return -1;
   }
}
