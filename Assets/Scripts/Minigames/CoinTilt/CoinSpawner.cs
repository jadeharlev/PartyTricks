using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoinSpawner : MonoBehaviour {
    [Header("Coin Types")] [SerializeField]
    private CoinTypeSO[] availableCoinTypes;

    [Header("Spawn Settings")]
    [Tooltip("Coins per second")]
    [SerializeField] private float initialSpawnRate = 0.45f;
    [SerializeField] private float finalSpawnRate = 1f;
    [SerializeField] private float lateGamePhaseStartTimeFromEndInSeconds = 7f;
    [SerializeField] private int maxCoinsOnPlatform = 10;
    [SerializeField] private float coinLifetimeInSeconds = 15f;

    [Header("Spawn Area")] 
    [SerializeField] private float spawnRadiusMin = 2f;
    [SerializeField] private float spawnRadiusMax = 7.5f;
    [SerializeField] private float playerAvoidanceRadius = 3.5f;
    [SerializeField] private float spawnHeight = 3.5f;

    [Header("References")] 
    [SerializeField] private Transform platformTransform;
    [SerializeField] private CoinTiltPlayer assignedPlayer;

    private bool isSpawning;
    private float spawnTimer;
    private float currentSpawnInterval;
    private float gameDuration;
    private float elapsedTime;
    private bool powerupsHaveBeenApplied = false;
    private List<GameObject> activeCoins = new();
    
    private void Update() {
        if (!isSpawning) return;
        
        elapsedTime += Time.deltaTime;
        spawnTimer += Time.deltaTime;
        UpdateSpawnRate();
        
        if (spawnTimer >= currentSpawnInterval) {
            spawnTimer = 0;
            TryToSpawnCoin();
        }
        
        activeCoins.RemoveAll(coin => coin == null);
    }

    public void StartSpawning(float durationInSeconds, int numberOfCoinSpawnPowerups) {
        if (!powerupsHaveBeenApplied) {
            float spawnRateMultiplier = 1;
            for (int i = 0; i < numberOfCoinSpawnPowerups; i++) {
                spawnRateMultiplier *= 1.25f;
            }
            initialSpawnRate *= spawnRateMultiplier;
            finalSpawnRate *= spawnRateMultiplier;
            powerupsHaveBeenApplied = true;
        }
        isSpawning = true;
        gameDuration = durationInSeconds;
        elapsedTime = 0;
        spawnTimer = 0;
        TryToSpawnCoin();
        UpdateSpawnRate();
        DebugLogger.Log(LogChannel.Systems, "Coin spawning started.");
    }

    public void StopSpawning() {
        isSpawning = false;
        DebugLogger.Log(LogChannel.Systems, "Coin spawning stopped.");
    }

    public void DestroyAll() {
        foreach(GameObject coin in activeCoins)
        {
            if (coin) {
                Destroy(coin);
            }
        }
    }

    private void UpdateSpawnRate() {
        float timeRemaining = gameDuration - elapsedTime;
        float spawnRate;
        bool endGameTimeThresholdReached = timeRemaining <= lateGamePhaseStartTimeFromEndInSeconds;
        
        if (endGameTimeThresholdReached) {
            spawnRate = finalSpawnRate;
        }
        else {
            spawnRate = GetSpawnRateBasedOnTime();
        }

        currentSpawnInterval = 1f / spawnRate;
    }

    private float GetSpawnRateBasedOnTime() {
        float progress = elapsedTime / (gameDuration - lateGamePhaseStartTimeFromEndInSeconds);
        float midGameRate = (initialSpawnRate + finalSpawnRate) / 2f;
        var spawnRate = Mathf.Lerp(initialSpawnRate, midGameRate, progress);
        return spawnRate;
    }

    private void TryToSpawnCoin() {
        bool tooManyCoinsAlreadyExist = maxCoinsOnPlatform > 0 && activeCoins.Count >= maxCoinsOnPlatform;
        if (tooManyCoinsAlreadyExist) {
            return;
        }
        
        CoinTypeSO coinType = SelectRandomCoinType();
        
        if (!coinType || !coinType.CoinPrefab) {
            Debug.LogWarning("No valid coin type/prefab available.");
            return;
        }
        
        var spawnPosition = TryToFindValidSpawnLocation(out var maxAttempts, out var attempts);
        
        if (attempts >= maxAttempts) {
            Debug.LogWarning("Could not find valid coin spawn location.");
            return;
        }
        
        var coinObject = CreateCoinInstance(coinType, spawnPosition);
        InitializeAndTrackCoin(coinObject, coinType);
    }

    private void InitializeAndTrackCoin(GameObject coinObject, CoinTypeSO coinType) {
        Coin coin = coinObject.GetComponent<Coin>();
        if(!coin) coin.InitializeWithType(coinType);
        activeCoins.Add(coinObject);
        
        if (coinLifetimeInSeconds > 0) {
            Destroy(coinObject, coinLifetimeInSeconds);
        }
    }

    private Vector3 TryToFindValidSpawnLocation(out int maxAttempts, out int attempts) {
        Vector3 spawnPosition;
        maxAttempts = 10;
        attempts = 0;
        do {
            spawnPosition = GetRandomSpawnPosition();
            attempts++;
        }
        while(attempts < maxAttempts && !IsValidSpawnPosition(spawnPosition));

        return spawnPosition;
    }

    private GameObject CreateCoinInstance(CoinTypeSO coinType, Vector3 spawnPosition) {
        GameObject coinObject = Instantiate(coinType.CoinPrefab, spawnPosition, Quaternion.identity);
        coinObject.transform.SetParent(platformTransform, true);
        coinObject.transform.rotation = Quaternion.identity;

        Vector3 localPosition = platformTransform.InverseTransformPoint(spawnPosition);
        localPosition.y = spawnHeight;
        coinObject.transform.localPosition = localPosition;
        coinObject.transform.localRotation = Quaternion.identity;

        Vector3 desiredWorldScale = new Vector3(0.5f,0.5f, 0.5f);
        Vector3 parentScale = platformTransform.lossyScale;
        coinObject.transform.localScale = new Vector3(
            desiredWorldScale.x / parentScale.x,
            desiredWorldScale.y / parentScale.y,
            desiredWorldScale.z / parentScale.z);
        return coinObject;
    }

    private CoinTypeSO SelectRandomCoinType() {
        if (availableCoinTypes == null || availableCoinTypes.Length == 0) return null;

        float totalWeight = 0f;
        totalWeight = CalculateTotalSpawnWeight(totalWeight);

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        
        return GetRandomCoinType(cumulativeWeight, randomValue);
    }

    private CoinTypeSO GetRandomCoinType(float cumulativeWeight, float randomValue) {
        foreach (var coinType in availableCoinTypes) {
            if (coinType) {
                cumulativeWeight += coinType.SpawnWeight;
                if (randomValue <= cumulativeWeight) {
                    return coinType;
                }
            }
        }

        return availableCoinTypes[0];
    }

    private float CalculateTotalSpawnWeight(float totalWeight) {
        foreach (var coinType in availableCoinTypes) {
            if (coinType) {
                totalWeight += coinType.SpawnWeight;
            }
        }

        return totalWeight;
    }

    private Vector3 GetRandomSpawnPosition() {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        float radius = Random.Range(spawnRadiusMin, spawnRadiusMax);

        Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, spawnHeight, Mathf.Sin(angle) * radius);
        return platformTransform.position + offset;
    }

    private bool IsValidSpawnPosition(Vector3 worldPosition) {
        Vector3 localPosition = platformTransform.InverseTransformPoint(worldPosition);
        localPosition.y = spawnHeight;
        Vector3 finalWorldPosition = platformTransform.TransformPoint(localPosition);
        
        if (assignedPlayer) {
            float distanceToPlayer = Vector3.Distance(finalWorldPosition, assignedPlayer.Position);
            if (distanceToPlayer < playerAvoidanceRadius) {
                return false;
            }
        }

        foreach (var coin in activeCoins) {
            if (coin) {
                float distanceToCoin = Vector3.Distance(finalWorldPosition, coin.transform.position);
                if (distanceToCoin < 1f) {
                    return false;
                }
            }
        }

        return true;
    }
}