using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoinSpawner : MonoBehaviour {
    [Header("Coin Types")] [SerializeField]
    private CoinTypeSO[] availableCoinTypes;

    [Header("Spawn Settings")]
    [Tooltip("Coins per second")]
    [SerializeField] private float initialSpawnRate = 0.5f;
    [SerializeField] private float finalSpawnRate = 2f;
    [SerializeField] private float lateGamePhaseStartTimeFromEndInSeconds = 5f;
    [SerializeField] private int maxCoinsOnPlatform = 20;
    [SerializeField] private float coinLifetimeInSeconds = 10f;

    [Header("Spawn Area")] 
    [SerializeField] private float spawnRadiusMin = 2f;
    [SerializeField] private float spawnRadiusMax = 7f;
    [SerializeField] private float playerAvoidanceRadius = 2f;
    [SerializeField] private float spawnHeight = 1f;

    [Header("References")] 
    [SerializeField] private Transform platformTransform;
    [SerializeField] private CoinTiltPlayer assignedPlayer;

    private bool isSpawning;
    private float spawnTimer;
    private float currentSpawnInterval;
    private float gameDuration;
    private float elapsedTime;
    private List<GameObject> activeCoins = new();
    private void Update() {
        if (!isSpawning) return;
        
        elapsedTime += Time.deltaTime;
        UpdateSpawnRate();
        
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentSpawnInterval) {
            spawnTimer = 0;
            TryToSpawnCoin();
        }
        
        activeCoins.RemoveAll(coin => coin == null);
    }

    public void StartSpawning(float durationInSeconds) {
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
        if (timeRemaining <= lateGamePhaseStartTimeFromEndInSeconds) {
            spawnRate = finalSpawnRate;
        }
        else {
            float progress = elapsedTime / (gameDuration - lateGamePhaseStartTimeFromEndInSeconds);
            float midGameRate = (initialSpawnRate + finalSpawnRate) / 2f;
            spawnRate = Mathf.Lerp(initialSpawnRate, midGameRate, progress);
        }

        currentSpawnInterval = 1f / spawnRate;
    }

    private void TryToSpawnCoin() {
        if (maxCoinsOnPlatform > 0 && activeCoins.Count >= maxCoinsOnPlatform) {
            return;
        }

        CoinTypeSO coinType = SelectRandomCoinType();
        if (!coinType || !coinType.CoinPrefab) {
            Debug.LogWarning("No valid coin type/prefab available.");
            return;
        }

        Vector3 spawnPosition;
        int maxAttempts = 10;
        int attempts = 0;
        do {
            spawnPosition = GetRandomSpawnPosition();
            attempts++;
        }
        while(attempts < maxAttempts && !IsValidSpawnPosition(spawnPosition));

        if (attempts >= maxAttempts) {
            Debug.LogWarning("Could not find valid coin spawn location.");
            return;
        }
        
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
        
        Coin coin = coinObject.GetComponent<Coin>();
        if(!coin) coin.InitializeWithType(coinType);
        
        activeCoins.Add(coinObject);

        if (coinLifetimeInSeconds > 0) {
            Destroy(coinObject, coinLifetimeInSeconds);
        }
    }

    private CoinTypeSO SelectRandomCoinType() {
        if (availableCoinTypes == null || availableCoinTypes.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var coinType in availableCoinTypes) {
            if (coinType != null) {
                totalWeight += coinType.SpawnWeight;
            }
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        foreach (var coinType in availableCoinTypes) {
            if (coinType != null) {
                cumulativeWeight += coinType.SpawnWeight;
                if (randomValue <= cumulativeWeight) {
                    return coinType;
                }
            }
        }
        return availableCoinTypes[0];
    }

    private Vector3 GetRandomSpawnPosition() {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        float radius = Random.Range(spawnRadiusMin, spawnRadiusMax);

        Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, spawnHeight, Mathf.Sin(angle) * radius);
        return platformTransform.position + offset;
    }

    private bool IsValidSpawnPosition(Vector3 position) {
        if (assignedPlayer != null) {
            float distanceToPlayer = Vector3.Distance(position, assignedPlayer.Position);
            if (distanceToPlayer < playerAvoidanceRadius) {
                return false;
            }
        }

        foreach (var coin in activeCoins) {
            if (coin) {
                float distanceToCoin = Vector3.Distance(position, coin.transform.position);
                if (distanceToCoin < 1f) {
                    return false;
                }
            }
        }

        return true;
    }
}