using System;
using System.Collections;
using UnityEngine;


public class CoinTiltPlayer : MonoBehaviour {
    public event Action<int, int> OnCoinCollected;
    public event Action<int> OnFallOff;

    [Header("References")]
    [SerializeField] private MeshRenderer meshRenderer;

    [SerializeField] private CoinTiltPlayerStatsSO baseStats;
    
    private float moveSpeed = 15f;
    private float acceleration = 7f;
    private float slipFactor = 1.7f;
    
    private float jumpForce = 8f;
    private float airControlMultiplier = 1;
    private float gravityScale = 3f;
    private float coyoteTime = 0.15f;
    private float momentumCancelPercentageRegular = 0.5f;
    private float momentumCancelPercentageBoosted = 0.75f;
    
    private float fallThresholdY = -10f;
    private float respawnDelayInSeconds = 0.75f;
    private Vector3 respawnPosition = Vector3.zero;

    private int playerIndex;
    private IDirectionalTwoButtonInputHandler navigator;
    private bool isAI;
    private CoinTiltMinigameManager manager;
    private CharacterController characterController;
    private Coroutine respawnCoroutine;
    
    private Vector3 currentVelocity;
    private bool isFrozen;
    private bool isGrounded;
    private bool isFalling;
    private bool inputEnabled;
    private bool magnetEnabled = false;
    private float magnetRadius = 3f;
    private float magnetPullSpeed = 5f;
    private float timeSinceGrounded;

    public int PlayerIndex => playerIndex;
    public Vector3 Position => transform.position;

    private void Awake() {
        characterController = GetComponent<CharacterController>();
        if (characterController == null) {
            Debug.LogError("CoinTiltPlayer could not find CharacterController component.");
        }
    }

    public void Initialize(int index, IDirectionalTwoButtonInputHandler inputHandler, bool isAI, int numberOfMagnetPowerups, int numberOfMoveBoosts) {
        ApplyBaseStats();
        ApplyMoveBoosts(numberOfMoveBoosts);
        this.playerIndex = index;
        this.navigator = inputHandler;
        this.isAI = isAI;
        this.inputEnabled = false;
        this.isFalling = false;

        if (numberOfMagnetPowerups > 0) {
            this.magnetEnabled = true;
            if (numberOfMagnetPowerups > 1) {
                magnetRadius += numberOfMagnetPowerups * 0.4f;
                magnetPullSpeed += numberOfMagnetPowerups * 0.5f;
            }
        }
        respawnPosition = transform.position;
        
        DebugLogger.Log(LogChannel.Systems, $"P{playerIndex+1} initialized. IsAI: {isAI}");
    }

    private void ApplyMoveBoosts(int numberOfMoveBoosts) {
        for (int i = 0; i < numberOfMoveBoosts; i++) {
            slipFactor += 1.2f;
            airControlMultiplier = Mathf.Clamp01(airControlMultiplier + 0.2f);
        }
    }

    private void ApplyBaseStats() {
        moveSpeed = baseStats.MoveSpeed;
        acceleration = baseStats.Acceleration;
        slipFactor = baseStats.SlipFactor;
        jumpForce = baseStats.JumpForce;
        airControlMultiplier = baseStats.AirControlMultiplier;
        gravityScale = baseStats.GravityScale;
        coyoteTime = baseStats.CoyoteTimeInSeconds;
        momentumCancelPercentageRegular = baseStats.MomentumCancelPercentageRegular;
        momentumCancelPercentageBoosted = baseStats.MomentumCancelPercentageBoosted;
        fallThresholdY = baseStats.FallThresholdY;
        respawnDelayInSeconds = baseStats.RespawnDelayInSeconds;
    }

    public void EnableInput() {
        inputEnabled = true;
    }

    public void DisableInput() {
        inputEnabled = false;
    }

    public void Freeze() {
        isFrozen = true;
        currentVelocity = Vector3.zero;
        if (respawnCoroutine != null) {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }

    private void Update() {
        if (isFalling) return;
        CheckForFall();
        HandleInput();
        ApplyMovement();

        if (magnetEnabled && inputEnabled) {
            PullNearbyCoins();
        }
    }

    private void PullNearbyCoins() {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, magnetRadius);
        foreach (Collider nearbyCollider in nearbyColliders) {
            if (nearbyCollider.CompareTag("Coin")) {
                Coin coin = nearbyCollider.GetComponent<Coin>();
                if (coin) {
                    coin.StartPull(transform, magnetPullSpeed);
                }
            }
        }
    }

    private void CheckForFall() {
        if (transform.position.y < fallThresholdY) {
            TriggerFall();
        }
    }

    private void HandleInput() {
        if (!inputEnabled || navigator == null || !navigator.IsActive()) return;

        Vector2 input = navigator.GetNavigate();
        Vector3 inputDirection = new Vector3(input.x, 0, input.y);
        if (isGrounded) {
            ApplyGroundMovement(inputDirection);
        }
        else {
            ApplyAirMovement(inputDirection);
        }

        if (navigator.SelectIsPressed() && (isGrounded || timeSinceGrounded <= coyoteTime)) {
            Jump();
        }
    }

    private void ApplyGroundMovement(Vector3 inputDirection) {
        if (inputDirection.magnitude > 0.1f) {
            ApplyMovementTowardsInputDirection(inputDirection);
        }
        else {
            ApplyDeceleration();
        }
    }

    private void ApplyDeceleration() {
        float deceleration = acceleration * slipFactor;
        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            new Vector3(0, currentVelocity.y, 0),
            deceleration * Time.deltaTime);
    }

    private void ApplyMovementTowardsInputDirection(Vector3 inputDirection) {
        Vector3 targetVelocity = inputDirection.normalized * moveSpeed;
        currentVelocity = Vector3.MoveTowards(
            currentVelocity,
            new Vector3(targetVelocity.x, targetVelocity.y, targetVelocity.z),
            acceleration * Time.deltaTime
        );
    }

    private void ApplyAirMovement(Vector3 inputDirection) {
        if (inputDirection.magnitude > 0.1f) {
            Vector3 airInfluence = inputDirection.normalized * moveSpeed * airControlMultiplier * Time.deltaTime;
            currentVelocity += new Vector3(airInfluence.x, 0, airInfluence.z);
        }
    }

    private void Jump() {
        ApplyMomentumCancellationForJump();
        currentVelocity.y = jumpForce;
        isGrounded = false;
        DebugLogger.Log(LogChannel.Systems, $"P{playerIndex+1} jumped.", LogLevel.Verbose);
    }

    private void ApplyMomentumCancellationForJump() {
        var momentumCancelPercentage = CalculateMomentumCancellation();
        float retainedMomentum = Mathf.Max(1f-momentumCancelPercentage, 0.1f);
        currentVelocity.x *= retainedMomentum;
        currentVelocity.z *= retainedMomentum;
    }

    private float CalculateMomentumCancellation() {
        float momentumCancelPercentage = momentumCancelPercentageRegular;

        if (inputEnabled && navigator != null && navigator.IsActive()) {
            Vector2 input = navigator.GetNavigate();
            Vector3 inputDirection = new Vector3(input.x, 0, input.y);
            
            CancelMoreMomentumWhenMovingOppositeDirection(inputDirection, ref momentumCancelPercentage);
        }

        return momentumCancelPercentage;
    }

    private void CancelMoreMomentumWhenMovingOppositeDirection(Vector3 inputDirection,
        ref float momentumCancelPercentage) {
        if (inputDirection.magnitude > 0.1f) {
            Vector3 horizontalVelocity = new Vector3(inputDirection.x, 0, inputDirection.z);
            float dotProduct = Vector3.Dot(inputDirection.normalized, horizontalVelocity.normalized);
            if (dotProduct < 0) {
                momentumCancelPercentage = momentumCancelPercentageBoosted;
            }
        }
    }

    private void ApplyMovement() {
        if (isFrozen) return;
        if (!isGrounded) {
            currentVelocity.y += Physics.gravity.y * gravityScale * Time.deltaTime;
        }

        if (characterController.enabled) {
            CollisionFlags collisionFlags = characterController.Move(currentVelocity * Time.deltaTime);
            isGrounded = (collisionFlags & CollisionFlags.Below) != 0;
        }

        UpdateGroundedTime();
        
        if (isGrounded && currentVelocity.y < 0) {
            currentVelocity.y = -2f;
        }
    }

    private void UpdateGroundedTime() {
        if (isGrounded) {
            timeSinceGrounded = 0f;
        }
        else {
            timeSinceGrounded += Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Coin")) {
            Coin coin = other.GetComponent<Coin>();
            if (coin != null) {
                int coinValue = coin.Collect();
                OnCoinCollected?.Invoke(playerIndex, coinValue);
            }
        }

        if (other.CompareTag("FallZone")) {
            TriggerFall();
        }
    }

    private void TriggerFall() {
        if (isFalling) return;
        isFalling = true;
        inputEnabled = false;
        OnFallOff?.Invoke(playerIndex);
        DebugLogger.Log(LogChannel.Systems,  $"P{playerIndex+1} falling.", LogLevel.Verbose);
        respawnCoroutine = StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay() {
        characterController.enabled = false;
        meshRenderer.enabled = false;
        yield return new WaitForSeconds(respawnDelayInSeconds);
        Respawn();
        respawnCoroutine = null;
    }

    private void Respawn() {
        transform.position = respawnPosition;
        currentVelocity = Vector3.zero;
        meshRenderer.enabled = true;
        characterController.enabled = true;
        
        isFalling = false;
        if(!isFrozen) inputEnabled = true;
        isGrounded = false;
        
        DebugLogger.Log(LogChannel.Systems, $"P{playerIndex+1} respawned.", LogLevel.Verbose);
    }
}
