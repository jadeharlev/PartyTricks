using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DireDodgingPlayer : MonoBehaviour {
    private float maxMoveSpeed;
    private float projectileScale;
    private float projectileSpeed;
    private float baseDamage;
    private float maxHealth;
    private float currentHealth;
    private float projectileShootRate;
    private float spriteHalfWidth;
    private float spriteHalfHeight;

    [SerializeField] private DireDodgingPlayerStatsSO PlayerStatsSO;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private Collider2D Collider2D;
    [SerializeField] private Rigidbody2D Rigidbody2D;
    [SerializeField] private Color PlayerColor;
    [SerializeField] private GameObject ProjectilePrefab;
    [SerializeField] private Transform PoolParent;
    
    private int playerIndex;
    private IDirectionalTwoButtonInputHandler navigator;
    private bool isAI;
    private bool inputEnabled;
    private bool isAlive = true;
    private ObjectPool<DireDodgingProjectile>[] projectilePools;
    private List<DireDodgingProjectile> activeProjectiles = new();

    private Coroutine damageCoroutineInstance = null;
    private Camera mainCamera;
    private Quaternion leftRotation = Quaternion.Euler(0, 0, 90);
    private Quaternion rightRotation = Quaternion.Euler(0, 0, 270);

    public void Initialize(int index, IDirectionalTwoButtonInputHandler inputHandler, bool isAI) {
        mainCamera = Camera.main;
        ApplyBaseStats();
        this.playerIndex = index;
        this.navigator = inputHandler;
        this.isAI = isAI;
        this.inputEnabled = false;
        
        spriteHalfWidth = SpriteRenderer.bounds.size.x;
        spriteHalfHeight = SpriteRenderer.bounds.extents.y;
        
        InitializePools();
        DebugLogger.Log(LogChannel.Systems, $"P{playerIndex+1} initialized. IsAI: {isAI}");
    }

    private void InitializePools() {
        projectilePools = new ObjectPool<DireDodgingProjectile>[2];
        projectilePools[0] = new ObjectPool<DireDodgingProjectile>(
            () => CreateProjectile(projectilePools[0]),
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile
        );
        projectilePools[1] = new ObjectPool<DireDodgingProjectile>(
            () => CreateProjectile(projectilePools[1]),
            OnGetProjectile,
            OnReleaseProjectile,
            OnDestroyProjectile
        );
    }
    private DireDodgingProjectile CreateProjectile(IObjectPool<DireDodgingProjectile> projectilePool) {
        GameObject projectileObject = Instantiate(ProjectilePrefab, PoolParent);
        projectileObject.SetActive(false);
        DireDodgingProjectile projectile = projectileObject.GetComponent<DireDodgingProjectile>();
        projectile.SetPool(projectilePool);
        projectile.SetColor(PlayerColor);
        return projectile;
    }

    private void OnGetProjectile(DireDodgingProjectile projectile) {
        projectile.gameObject.SetActive(true);
        activeProjectiles.Add(projectile);
    }

    private void OnReleaseProjectile(DireDodgingProjectile projectile) {
        projectile.gameObject.SetActive(false);
        activeProjectiles.Remove(projectile);
    }

    private void OnDestroyProjectile(DireDodgingProjectile projectile) {
        Destroy(projectile.gameObject);
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
    }
    
    private float ClampYPosition(float yPosition) {
        float screenBottom = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;
        float screenTop = mainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
        return Mathf.Clamp(yPosition, screenBottom + spriteHalfHeight, screenTop - spriteHalfHeight);
    }

    private void ShootRight() {
        var projectile = projectilePools[1].Get();
        Vector2 position = transform.position;
        position.x += spriteHalfWidth;
        projectile.transform.position = position;
        projectile.transform.rotation = rightRotation;
        
        projectile.Initialize(playerIndex, baseDamage, projectileSpeed, true);
    }

    private void ShootLeft() {
        var projectile = projectilePools[0].Get();
        Vector2 position = transform.position;
        position.x -= spriteHalfWidth;
        projectile.transform.position = position;
        projectile.transform.rotation = leftRotation;
        
        projectile.Initialize(playerIndex, baseDamage, projectileSpeed, false);
    }

    public void Freeze() {
        inputEnabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        GameObject other = collision.gameObject;
        if(PlayerIsDead) return;
        DireDodgingProjectile projectile = other.GetComponent<DireDodgingProjectile>();
        if (projectile != null) {
            if (projectile.OwnerIndex == playerIndex) return;
            TakeDamage(projectile);
            projectile.ReturnToPool();
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

    public void DestroyVisibleProjectiles() {
        var projectilesToDestroy = new List<DireDodgingProjectile>(activeProjectiles);
        foreach (var projectile in projectilesToDestroy) {
            Destroy(projectile.gameObject);
        }
    }
}
