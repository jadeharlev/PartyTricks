using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DireDodgingPlayer : MonoBehaviour {
    private float maxMoveSpeed;
    private float projectileScale;
    private float projectileSpeed;
    private float baseDamage;
    private float maxHealth;
    private float currentHealth;
    private float projectileShootRate;

    [SerializeField] private DireDodgingPlayerStatsSO PlayerStatsSO;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private Collider2D Collider2D;
    [SerializeField] private Rigidbody2D Rigidbody2D;
    [SerializeField] private Color PlayerColor;
    
    private int playerIndex;
    private IDirectionalTwoButtonInputHandler navigator;
    private bool isAI;
    private bool inputEnabled;
    private bool isAlive = true;

    private Coroutine damageCoroutineInstance = null;
    private Camera mainCamera;
    
    public void Initialize(int index, IDirectionalTwoButtonInputHandler inputHandler, bool isAI) {
        mainCamera = Camera.main;
        ApplyBaseStats();
        this.playerIndex = index;
        this.navigator = inputHandler;
        this.isAI = isAI;
        this.inputEnabled = false;
        
        DebugLogger.Log(LogChannel.Systems, $"P{playerIndex+1} initialized. IsAI: {isAI}");
    }

    public void EnableInput() {
        inputEnabled = true;
    }

    public void StartShooting() {
        StartCoroutine(ShootingCoroutine());
    }
    
    private void Update() {
        if (!isAlive) return;
        HandleInput();
    }

    private void HandleInput() {
        if (!inputEnabled) return;
        Vector2 input = navigator.GetNavigate();
        ApplyMovement(input);
    }

    private void ApplyMovement(Vector2 input) {
        switch (input.y) {
            case > 0:
                MoveUp();
                break;
            case < 0:
                MoveDown();
                break;
        }
    }

    private void MoveUp() {
        var vector3 = Rigidbody2D.position;
        vector3.y += maxMoveSpeed * Time.deltaTime;
        vector3.y = ClampYPosition(vector3.y);
        Rigidbody2D.MovePosition(vector3);
    }

    private void MoveDown() {
        var vector3 = Rigidbody2D.position;
        vector3.y -= maxMoveSpeed * Time.deltaTime;
        vector3.y = ClampYPosition(vector3.y);
        Rigidbody2D.MovePosition(vector3);
    }

    private void ApplyBaseStats() {
        this.maxMoveSpeed = PlayerStatsSO.MoveSpeed;
        this.projectileScale = PlayerStatsSO.ProjectileScale;
        this.projectileSpeed = PlayerStatsSO.ProjectileSpeed;
        this.baseDamage = PlayerStatsSO.BaseDamage;
        this.maxHealth = PlayerStatsSO.BaseHealth;
        this.projectileShootRate = PlayerStatsSO.ProjectileShootRate;
        currentHealth = maxHealth;
    }

    private IEnumerator ShootingCoroutine() {
        while (inputEnabled && isAlive) {
            Shoot();
            yield return new WaitForSeconds(projectileShootRate);
        }
    }

    private void Shoot() {
        ShootRight();
        ShootLeft();
        // TODO make this object pooled
    }
    
    private float ClampYPosition(float yPosition) {
        float spriteHalfHeight = SpriteRenderer.bounds.extents.y;
        float screenBottom = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;
        float screenTop = mainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
        return Mathf.Clamp(yPosition, screenBottom + spriteHalfHeight, screenTop - spriteHalfHeight);
    }

    private void ShootRight() {
        Vector2 position = transform.position;
        position.x += SpriteRenderer.bounds.size.x;
        Quaternion rotation = Quaternion.Euler(0, 0, 270);
        var gameObject = Instantiate(PlayerStatsSO.ProjectilePrefab, position, rotation);
        DireDodgingProjectile projectileComponent = gameObject.GetComponent<DireDodgingProjectile>();
        if (projectileComponent != null) {
            projectileComponent.Initialize(playerIndex, baseDamage, projectileSpeed,PlayerColor, true);
        }
    }

    private void ShootLeft() {
        Vector2 position = transform.position;
        position.x -= SpriteRenderer.bounds.size.x;
        Quaternion rotation = Quaternion.Euler(0, 0, 90);
        var gameObject = Instantiate(PlayerStatsSO.ProjectilePrefab, position, rotation);
        DireDodgingProjectile projectileComponent = gameObject.GetComponent<DireDodgingProjectile>();
        if (projectileComponent != null) {
            projectileComponent.Initialize(playerIndex, baseDamage, projectileSpeed, PlayerColor, false);
        }
    }

    public void Freeze() {
        inputEnabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject other = collision.gameObject;
        if(PlayerIsDead) return;
        DireDodgingProjectile projectile = other.GetComponent<DireDodgingProjectile>();
        if (projectile.OwnerIndex == playerIndex) return;
        if (projectile != null) {
            TakeDamage(projectile);
            Destroy(other);
        }
    }

    private void TakeDamage(DireDodgingProjectile projectile) {
        currentHealth -= projectile.Damage;
        if (PlayerIsDead) {
            DireDodgingMinigameManager.Instance.RegisterDeath(projectile.OwnerIndex, playerIndex);
            Die();
            return;
        }
        if (damageCoroutineInstance != null) {
            StopCoroutine(damageCoroutineInstance);
        }
        damageCoroutineInstance = StartCoroutine(DamageCoroutine());
    }

    private IEnumerator DamageCoroutine() {
        Debug.Log($"P{playerIndex+1} took damage!");
        // TODO change color, probably with dotween
        yield return null;
        damageCoroutineInstance = null;
    }

    private void Die() {
        inputEnabled = false;
        isAlive = false;
        DisableColliderComponent();
        var color = SpriteRenderer.color;
        color.a = 0.1f;
        SpriteRenderer.color = color;
    }

    private void DisableColliderComponent() {
        Collider2D.enabled = false;
    }

    private bool PlayerIsDead => currentHealth <= 0;
}
