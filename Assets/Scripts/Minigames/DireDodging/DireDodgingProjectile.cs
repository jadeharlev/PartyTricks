using UnityEngine;
using UnityEngine.Pool;

public class DireDodgingProjectile : MonoBehaviour {
    public int OwnerIndex { get; private set; }
    public float Damage { get; private set; }
    public bool IsGhostProjectile { get; private set; }

    private float Speed { get; set; }

    // positive 1: right, negative 1: left
    private Vector2 moveDirection;
    
    [SerializeField] private Rigidbody2D Rigidbody2D;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    
    private IObjectPool<DireDodgingProjectile> projectilePool;

    public void SetPool(IObjectPool<DireDodgingProjectile> pool) {
        projectilePool = pool;
    }

    public void Initialize(int ownerIndex, float damage, float speed, Vector2 direction, bool isGhost = false) {
        OwnerIndex = ownerIndex;
        Damage = damage;
        Speed = speed;
        moveDirection = direction.normalized;
        IsGhostProjectile = isGhost;
    
        // Make ghost projectiles a different color
        if (isGhost && SpriteRenderer != null) {
            Color ghostColor = SpriteRenderer.color;
            ghostColor.a = 0.5f; // Semi-transparent
            SpriteRenderer.color = ghostColor;
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
    
    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Wall")) {
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
        transform.position += (Vector3)(moveDirection * Speed * Time.deltaTime);
    }
}