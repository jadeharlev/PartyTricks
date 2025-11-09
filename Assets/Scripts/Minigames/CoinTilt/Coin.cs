using UnityEngine;

public class Coin : MonoBehaviour {
    [SerializeField] private CoinTypeSO coinType;
    private int pointValue;
    private bool hasBeenCollected;

    private void Awake() {
        if (coinType != null) {
            pointValue = coinType.PointValue;
        }
        else {
            Debug.LogError("No coin type specified");
        }
    }

    public int Collect() {
        if (hasBeenCollected) return 0;
        hasBeenCollected = true;
        
        Destroy(gameObject);
        return pointValue;
    }

    public void InitializeWithType(CoinTypeSO type) {
        coinType = type;
        if (coinType != null) {
            pointValue = coinType.PointValue;
        }
    }
}