using System;
using System.Collections.Generic;
using System.Linq;
using CoreData;
using Debug;
using Input;
using Options;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Services {
   public class PlayerService : MonoBehaviour, IPlayerService {

      [Header("Player Configuration")] 
      [SerializeField] private int maxPlayers = 4;
      [SerializeField] private int startingFunds = 300;

      [Header("Player Slot Setup")] 
      [SerializeField] private PlayerSlot[] playerSlotPrefabs;
      [SerializeField] private Transform playerSlotsContainer;

      [Header("Debug")] 
      [SerializeField] private bool debugMode = false;

      private PlayerSlot[] playerSlots;
      private readonly Dictionary<int, PlayerProfile> playerProfiles = new();
      private Transform unseatedInputsContainer;

      public event Action<int, PlayerProfile> OnPlayerJoined;
      public event Action<int> OnPlayerLeft;
      public event Action<int, int> OnPlayerFundsChanged;
      public event Action<PlayerInput> OnInputConnected;

      public IReadOnlyList<PlayerSlot> PlayerSlots => playerSlots;

      public IEnumerable<PlayerInput> UnassignedInputs
      {
         get
         {
            return PlayerInput.all.Where(input => playerSlots.All(slot => slot.PlayerInput != input));
         }
      }

      private void Awake() {
         InitializeSlots();
         SetUpEventListeners();
         unseatedInputsContainer = new GameObject("UnseatedInputs").transform;
         unseatedInputsContainer.SetParent(transform);
         GameSettings.OnApplySettings += SyncOneHandedMode;
      }

      private void InitializeSlots() {
         var colorConfig = ServiceLocatorAccessor.GetService<PlayerColorConfig>();
         if (playerSlotsContainer == null) {
            playerSlotsContainer = transform;
         }
         playerSlots = new PlayerSlot[maxPlayers];
         for (int i = 0; i < maxPlayers; i++) {
            PlayerSlot playerSlot;
            if (playerSlotPrefabs != null && i < playerSlotPrefabs.Length && playerSlotPrefabs[i] != null) {
               playerSlot = Instantiate(playerSlotPrefabs[i], playerSlotsContainer);
            }
            else {
               GameObject playerSlotGameObject = new GameObject($"PlayerSlot_{i}");
               playerSlotGameObject.transform.SetParent(playerSlotsContainer);
               playerSlot = playerSlotGameObject.AddComponent<PlayerSlot>();
            }

            playerSlot.Initialize(i);
            
            if (colorConfig != null) {
               playerSlot.SetPlayerColor(colorConfig.GetMainColor(i));
            }

            var profile = new PlayerProfile(startingFunds);
            playerSlot.AssignProfile(profile);
            playerProfiles[i] = profile;
            
            playerSlots[i] = playerSlot;
            
            if (debugMode) {
               DebugLogger.Log(LogChannel.Systems, $"Initialized slot {i} as AI.");
            } 
         }
      }

      private void SetUpEventListeners() {
         foreach (var profile in playerProfiles.Values) {
            int index = GetPlayerIndexForProfile(profile);
            profile.Wallet.OnFundsChanged += (newAmount) =>
            {
               OnPlayerFundsChanged?.Invoke(index, newAmount);
            };
         }
      }
      
      public bool TryJoinPlayer(PlayerInput input) {
         input.transform.SetParent(unseatedInputsContainer);
         
         var handler = input.gameObject.GetComponent<PlayerUITwoButtonInputHandler>();
         if (handler == null) {
            handler = input.gameObject.AddComponent<PlayerUITwoButtonInputHandler>();
            handler.Initialize(input);
         }
         handler.OneHandedMode = GameSettings.Accessibility.OneHandedMode;
         
         OnInputConnected?.Invoke(input);
         return true;
      }

      public bool TryAssignInputToSlot(PlayerInput input, int slotIndex) {
         if (input == null) {
            return false;
         }

         if (slotIndex < 0 || slotIndex >= playerSlots.Length) {
            DebugLogger.Log(LogChannel.Systems, $"Slot index was invalid: {slotIndex}", LogLevel.Warning);
            return false;
         }
         
         var slot = playerSlots[slotIndex];
         if (!slot.IsAI) return false;

         slot.SetUpAsHuman(input);
         OnPlayerJoined?.Invoke(slotIndex, slot.Profile);
         return true;
      }

      public bool TryReleaseSlot(int slotIndex) {
         if (slotIndex < 0 || slotIndex >= playerSlots.Length) {
            DebugLogger.Log(LogChannel.Systems, $"Slot index was invalid: {slotIndex}", LogLevel.Warning);
            return false;
         }
         
         var slot = playerSlots[slotIndex];
         if (slot.IsAI) return false;

         var released = slot.ReleaseInput();
         if(released != null) released.transform.SetParent(unseatedInputsContainer);
         OnPlayerLeft?.Invoke(slotIndex);
         return true;
      }

      public void RemovePlayer(int playerIndex) => TryReleaseSlot(playerIndex);
      public void DestroyUnassignedInputs() {
         foreach (var input in UnassignedInputs.ToArray()) {
            Destroy(input.gameObject);
         }
      }

      public PlayerProfile GetPlayerProfile(int playerIndex) {
         if (playerIndex < 0 || playerIndex >= playerSlots.Length) {
            DebugLogger.Log(LogChannel.Systems, $"Invalid Player Index {playerIndex}", LogLevel.Error);
            return null;
         }

         return playerSlots[playerIndex].Profile;
      }

      public bool PlayerIsHuman(int playerIndex) {
         if (playerIndex < 0 || playerIndex >= playerSlots.Length) return false;
         return !playerSlots[playerIndex].IsAI;
      }

      public int GetPlayerCount() {
         return playerSlots.Length;
      }

      public int GetMaxPlayers() {
         return maxPlayers;
      }

      public int GetHumanPlayerCount() {
         return playerSlots.Count(slot => !slot.IsAI);
      }
      
      public int GetAIPlayerCount() {
         return playerSlots.Count(slot => slot.IsAI);
      }

      private int GetPlayerIndexForProfile(PlayerProfile profile) {
         for (int i = 0; i < playerSlots.Length; i++) {
            if (playerSlots[i].Profile == profile) {
               return i;
            }
         }

         return -1;
      }
      
      private void SyncOneHandedMode() {
         foreach (var input in PlayerInput.all) {
            var handler = input.gameObject.GetComponent<PlayerUITwoButtonInputHandler>();
            if (handler != null) {
               handler.OneHandedMode = GameSettings.Accessibility.OneHandedMode;
            }
         }
      }
      
      private void OnDestroy() {
         GameSettings.OnApplySettings -= SyncOneHandedMode;
      }


      public void ResetAllPlayers() {
         foreach (var slot in PlayerSlots) {
            slot.Reset();
         }
      }
   }
}
