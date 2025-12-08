using UnityEngine;
using UnityEngine.Pool;

public class DireDodgingProjectile : MonoBehaviour {
    public int OwnerIndex { get; private set; }
    public float Damage { get; private set; }

    private float Speed { get; set; }

    // positive 1: right, negative 1: left
    private int direction;
    
    [SerializeField] private Rigidbody2D Rigidbody2D;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    
    private IObjectPool<DireDodgingProjectile> projectilePool;

    public void SetPool(IObjectPool<DireDodgingProjectile> pool) {
        projectilePool = pool;
    }

    public void Initialize(int ownerIndex, float damage, float speed, bool movingRight) {
        OwnerIndex = ownerIndex;
        Damage = damage;
        Speed = speed;
        if (movingRight) {
            direction = 1;
        }
        else {
            direction = -1;
        }
    }

    public void SetColor(Color playerColor) {
        SpriteRenderer.color = playerColor;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("ProjectileDespawnBounds")) {
            ReturnToPool();
        }
    }

    public void ReturnToPool() {
        if (projectilePool != null) {
            projectilePool.Release(this);
        }
        else {
            Debug.LogWarning("Projectile pool not set up in DireDodgingProjectile.");
        }
    }

    private void Update() {
        transform.position += new Vector3(Speed * direction * Time.deltaTime, 0, 0);
    }
}