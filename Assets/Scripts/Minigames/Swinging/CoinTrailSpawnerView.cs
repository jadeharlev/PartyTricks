using UnityEngine;
using VineSwinging.Core;
using Random = System.Random;

namespace Minigames.Swinging {
    public class CoinTrailSpawnerView : MonoBehaviour {
        public void SpawnCoinsForTrack(CoinPosition[][] allTrails, float[] vinePositions, float vineAnchorY,
            CoinTypeSO[] coinTypes, Random randomNumberGenerator) {
            float totalWeight = 0f;
            foreach (var coinType in coinTypes) {
                totalWeight += coinType.SpawnWeight;
            }

            for (int gap = 0; gap < allTrails.Length; gap++) {
                float startXPosition = vinePositions[gap];
                float gapWidth = vinePositions[gap + 1] - startXPosition;

                foreach (var coinPosition in allTrails[gap]) {
                    float worldXPosition = startXPosition + coinPosition.RelativeXPosition * gapWidth;
                    float worldYPosition = vineAnchorY + coinPosition.RelativeYPosition;

                    CoinTypeSO coinType = SelectCoinType(coinTypes, totalWeight, randomNumberGenerator);
                    var gameObject = Instantiate(coinType.CoinPrefab, transform);
                    gameObject.transform.localPosition = new Vector3(worldXPosition, worldYPosition);
                    gameObject.GetComponent<SwingingCoinView>().Initialize(coinType.PointValue);
                }
            }
        }

        private CoinTypeSO SelectCoinType(CoinTypeSO[] coinTypes, float totalWeight, Random randomNumberGenerator) {
            float roll = (float)(randomNumberGenerator.NextDouble() * totalWeight);
            float cumulativeWeight = 0f;
            foreach (var coinType in coinTypes) {
                cumulativeWeight += coinType.SpawnWeight;
                if (roll <= cumulativeWeight) return coinType;
            }

            return coinTypes[^1];
        }
    }
}