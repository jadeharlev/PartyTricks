using UnityEngine;

public class Coin : MonoBehaviour {
    [SerializeField] private CoinTypeSO coinType;
    private int pointValue;
    private bool hasBeenCollected;
    private Transform pullTarget;
    private bool isBeingPulled;
    private float pullSpeed;

    public void StartPull(Transform target, float speed) {
        if (!isBeingPulled) {
            isBeingPulled = true;
            pullTarget = target;
            pullSpeed = speed;
        }
    }

    private void Update() {
        if (isBeingPulled && pullTarget != null) {
            Vector3 direction = pullTarget.position - transform.position;
            transform.position += direction * pullSpeed * Time.deltaTime;
        }
    }

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