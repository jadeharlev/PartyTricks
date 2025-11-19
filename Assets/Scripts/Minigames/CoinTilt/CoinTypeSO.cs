using UnityEngine;

[CreateAssetMenu(fileName = "CoinTypeSO", menuName = "Scriptable Objects/CoinTypeSO")]
public class CoinTypeSO : ScriptableObject {
    public int PointValue;
    [Tooltip("0-1 scale of least common to common")]
    [Range(0f, 1f)]
    public float SpawnWeight;
    public GameObject CoinPrefab;
    [Tooltip("Worth more than base coins?")]
    public bool IsSpecialCoin;
}
