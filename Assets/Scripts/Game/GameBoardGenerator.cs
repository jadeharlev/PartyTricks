using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoardGenerator {
    private readonly int numberOfMinigames;
    public List<(MinigameType minigameType, bool IsDouble)> GameBoard { get; private set; }

    private readonly MinigameType[] availableTypes = new[]
    {
        MinigameType.Combat,
        MinigameType.Gambling,
        MinigameType.Movement
    };

    public GameBoardGenerator(int gameLength = 5) {
        this.numberOfMinigames = gameLength;
        GameBoard = new List<(MinigameType minigameType, bool IsDouble)>();
    }

    public void GenerateRandomBoard() {
        GameBoard.Clear();
        int endIndex = numberOfMinigames - 1;
        // note that the last minigame cannot be double
        int indexOfDoubleMinigame = Random.Range(0, endIndex);
        List<MinigameType> minigameTypes = availableTypes.ToList();
        int remainingSlots = endIndex - minigameTypes.Count;
        for (int i = 0; i < remainingSlots; i++) {
            minigameTypes.Add(minigameTypes[Random.Range(0, minigameTypes.Count)]);
        }

        minigameTypes = minigameTypes.OrderBy(x => Random.value).ToList();
        for (int i = 0; i < endIndex; i++) {
            bool isDoubleMinigame = (i == indexOfDoubleMinigame);
            GameBoard.Add((minigameTypes[i], isDoubleMinigame));
        }
        GameBoard.Add((MinigameType.Final, false));
    }

    public void GenerateSpecificBoard(params (MinigameType minigameType, bool IsDouble)[] gameBoard) {
        this.GameBoard = gameBoard.ToList();
    }
}