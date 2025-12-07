using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
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
    private float damageAnimationTimeInSeconds;
    private float deathAnimationTimeInSeconds;

    [SerializeField] private DireDodgingPlayerStatsSO PlayerStatsSO;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private Collider2D Collider2D;
    [SerializeField] private Rigidbody2D Rigidbody2D;
    [SerializeField] private Color PlayerColor;
    [SerializeField] private GameObject ProjectilePrefab;
    [SerializeField] private Transform PoolParent;
    [SerializeField] private DireDodgingHealthBar HealthBar;
    
    private int playerIndex;
    private Sequence colorChangeSequence;
    private Color baseColor;
    private IDirectionalTwoButtonInputHandler navigator;
    private bool isAI;
    private bool inputEnabled;
    private bool isAlive = true;
    private ObjectPool<DireDodgingProjectile>[] projectilePools;
    private List<DireDodgingProjectile> activeProjectiles = new();

    private Coroutine damageCoroutineInstance = null;
    private Coroutine intensityCoroutineInstance = null;
    private Camera mainCamera;
    private readonly Quaternion leftRotation = Quaternion.Euler(0, 0, 90);
    private readonly Quaternion rightRotation = Quaternion.Euler(0, 0, 270);
    private EventReference hitEvent;
    private EventReference deathEvent;

    private void Awake() {
        baseColor = SpriteRenderer.color;
    }

    public void Initialize(int index, IDirectionalTwoButtonInputHandler inputHandler, bool isAI, int numberOfIncreasedHPPowerups, int numberOfIncreasedAttackSpeedPowerups) {
        mainCamera = Camera.main;
        ApplyBaseStats();
        ApplyStatBuffs(numberOfIncreasedHPPowerups, numberOfIncreasedAttackSpeedPowerups);
        this.playerIndex = index;
        this.navigator = inputHandler;
        this.isAI = isAI;
        this.inputEnabled = false;
        spriteHalfWidth = SpriteRenderer.bounds.size.x;
        spriteHalfHeight = SpriteRenderer.bounds.extents.y + 0.2f; // offset added for health bar
        
        InitializePools();
        DebugLogger.Log(LogChannel.Systems, $"P{playerIndex+1} initialized. IsAI: {isAI}");
    }

    private void ApplyStatBuffs(int numberOfIncreasedHpPowerups, int numberOfIncreasedAttackSpeedPowerups) {
        this.maxHealth += numberOfIncreasedHpPowerups;
        this.currentHealth = this.maxHealth;
        for (int i = 0; i < numberOfIncreasedAttackSpeedPowerups; i++) {
            this.projectileShootRate *= 0.75f;
        }
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
        this.projectileScale = PlayerStatsSO.ProjectileScale * 0.36f; // scale to prefab size
        this.projectileSpeed = PlayerStatsSO.ProjectileSpeed;
        this.baseDamage = PlayerStatsSO.BaseDamage;
        this.maxHealth = PlayerStatsSO.BaseHealth;
        this.projectileShootRate = PlayerStatsSO.ProjectileShootRate;
        this.damageAnimationTimeInSeconds = PlayerStatsSO.DamageAnimationTimeInSeconds;
        this.deathAnimationTimeInSeconds = PlayerStatsSO.DeathAnimationTimeInSeconds;
        this.hitEvent = PlayerStatsSO.GetHitEvent;
        this.deathEvent = PlayerStatsSO.DeathEvent;
        currentHealth = maxHealth;
    }

    private IEnumerator ShootingCoroutine() {
        float nextShootTime = 0f;
        while (inputEnabled && isAlive) {
            if (Time.time >= nextShootTime) {
                Shoot();
                nextShootTime = Time.time + projectileShootRate;
            }

            yield return null;
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
        projectile.transform.localScale = Vector3.one * projectileScale;
        
        projectile.Initialize(playerIndex, baseDamage, projectileSpeed, true);
    }

    private void ShootLeft() {
        var projectile = projectilePools[0].Get();
        Vector2 position = transform.position;
        position.x -= spriteHalfWidth;
        projectile.transform.position = position;
        projectile.transform.rotation = leftRotation;
        projectile.transform.localScale = Vector3.one * projectileScale;
        
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
        HealthBar.UpdateDisplay(currentHealth, maxHealth);
        if (PlayerIsDead) {
            DireDodgingMinigameManager.Instance.RegisterDeath(projectile.OwnerIndex, playerIndex);
            Die();
            return;
        } else {
            RuntimeManager.PlayOneShot(hitEvent);
            mainCamera.DOShakePosition(duration: 0.05f, strength: 0.2f, vibrato: 1, randomness: 90f, fadeOut: false).SetUpdate(true);
        }
        if (damageCoroutineInstance != null) {
            StopCoroutine(damageCoroutineInstance);
        }
        damageCoroutineInstance = StartCoroutine(DamageCoroutine());
    }

    private IEnumerator DamageCoroutine() {
        Debug.Log($"P{playerIndex+1} took damage!");
        var fadeInTween = SpriteRenderer.DOColor(Color.white, damageAnimationTimeInSeconds / 2f);
        var fadeOutTween = SpriteRenderer.DOColor(baseColor, damageAnimationTimeInSeconds / 2f);
        colorChangeSequence = DOTween.Sequence();
        colorChangeSequence.Append(fadeInTween);
        colorChangeSequence.Append(fadeOutTween);
        colorChangeSequence.onComplete = ResetColorChangeSequence;
        yield return new DOTweenCYInstruction.WaitForKill(fadeOutTween);
        damageCoroutineInstance = null;
    }

    private void ResetColorChangeSequence() {
        colorChangeSequence = null;
    }

    private void Die() {
        inputEnabled = false;
        isAlive = false;
        if (intensityCoroutineInstance != null) {
            StopCoroutine(intensityCoroutineInstance);
            intensityCoroutineInstance = null;
        }
        if(colorChangeSequence != null) colorChangeSequence.Kill();
        DisableColliderComponent();
        var color = baseColor;
        color.a = 0.1f;
        SpriteRenderer.DOColor(color, deathAnimationTimeInSeconds).SetUpdate(true);
        RuntimeManager.PlayOneShot(deathEvent);
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

    public void StartIncreasingIntensity(int remainingTimeInSeconds) {
        intensityCoroutineInstance = StartCoroutine(IntensityCoroutine(remainingTimeInSeconds));
    }

    private IEnumerator IntensityCoroutine(int remainingTimeInSeconds) {
        float startTime = Time.time;
        float timeAtFullyRampedUpSpeed = 5f;
        float duration = remainingTimeInSeconds - timeAtFullyRampedUpSpeed;
        float initialProjectileSpeed = projectileSpeed;
        float initialShootRate = projectileShootRate;
        float initialProjectileScale = projectileScale;

        float targetProjectileSpeed = initialProjectileSpeed * 2.5f;
        float targetShootRate = initialShootRate * 0.3f;
        float targetProjectileScale = projectileScale * 2f;

        while (Time.time - startTime < duration) {
            float elapsed = Time.time - startTime;
            float t = elapsed / duration;
            float easedT = t * t; // quadratic easing
            projectileSpeed = Mathf.Lerp(initialProjectileSpeed, targetProjectileSpeed, easedT);
            projectileShootRate = Mathf.Lerp(initialShootRate, targetShootRate, easedT);
            projectileScale = Mathf.Lerp(initialProjectileScale, targetProjectileScale, easedT);
            yield return null;
        }

        projectileSpeed = targetProjectileSpeed;
        projectileShootRate = targetShootRate;
        projectileScale = targetProjectileScale;
        intensityCoroutineInstance = null;
    }
}
