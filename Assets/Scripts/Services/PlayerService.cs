using System;
using System.Collections.Generic;
using System.Linq;
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

      public event Action<int, PlayerProfile> OnPlayerJoined;
      public event Action<int> OnPlayerLeft;
      public event Action<int, int> OnPlayerFundsChanged;
      
      public IReadOnlyList<PlayerSlot> PlayerSlots => playerSlots;

      private void Awake() {
         InitializeSlots();
         SetUpEventListeners();
      }

      private void InitializeSlots() {
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
         var aiSlot = playerSlots.FirstOrDefault(slot => slot.IsAI);
         if (aiSlot == null) {
            DebugLogger.Log(LogChannel.Systems, $"No AI slots available for human player!", LogLevel.Warning);
            Destroy(input.gameObject);
            return false;
         }

         int slotIndex = aiSlot.SlotIndex;
         aiSlot.SetUpAsHuman(input);

         if (debugMode) {
            DebugLogger.Log(LogChannel.Systems, $"Player joined at slot {slotIndex}.");
         }
         
         OnPlayerJoined?.Invoke(slotIndex, aiSlot.Profile);
         return true;
      }

      public void RemovePlayer(int playerIndex) {
         if (playerIndex < 0 || playerIndex >= playerSlots.Length) {
            DebugLogger.Log(LogChannel.Systems, $"Invalid Player Index {playerIndex}", LogLevel.Error);
            return;
         }
         
         var slot = playerSlots[playerIndex];
         if (!slot.IsAI) {
            slot.SetUpAsAI();
            OnPlayerLeft?.Invoke(playerIndex);

            if (debugMode) {
               DebugLogger.Log(LogChannel.Systems, $"Player {playerIndex} left, reverted to AI.");
            }
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

      public bool SlotIsOccupied(int slotIndex) {
         if(slotIndex < 0 || slotIndex >= playerSlots.Length) return false;
         return playerSlots[slotIndex].IsOccupied;
      }

      public int GetPlayerCount() {
         return playerSlots.Length;
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

      public void ResetAllPlayers() {
         foreach (var slot in PlayerSlots) {
            slot.Reset();
         }
      }
   }
}
