using UnityEngine;

public class TiltingPlatform : MonoBehaviour {
    [Header("Tilt Settings")] 
    [SerializeField] private float maxTiltAngle = 30f;

    [SerializeField] private float tiltSpeed = 2f;
    [SerializeField] private float centerDeadzone = 2f;

    [Header("Platform Settings")] 
    [SerializeField] private float platformRadius = 7.5f;

    private CoinTiltPlayer assignedPlayer;
    private Quaternion targetRotation;
    private Vector3 platformCenter;

    private void Awake() {
        platformCenter = transform.position;
        targetRotation = transform.rotation;
    }

    public void Initialize(CoinTiltPlayer player) {
        assignedPlayer = player;
        DebugLogger.Log(LogChannel.Systems, $"Platform initiated for P{player.PlayerIndex+1}");
    }

    private void Update() {
        if (!assignedPlayer) return;
        CalculateTilt();
        ApplyTilt();
    }

    private void CalculateTilt() {
        Vector3 playerPosition = assignedPlayer.Position;
        Vector3 relativePosition = playerPosition - platformCenter;
        FlattenToXYPlane(ref relativePosition);
        float distanceFromCenter = relativePosition.magnitude;
        if (distanceFromCenter < centerDeadzone) {
            targetRotation = Quaternion.identity;
            return;
        }

        Vector3 tiltDirection = relativePosition.normalized;
        float tiltAmount = Mathf.Clamp01((distanceFromCenter - centerDeadzone) / (platformRadius - centerDeadzone));
        float tiltAngle = tiltAmount * maxTiltAngle;
        
        Vector3 tiltAxis = new Vector3(tiltDirection.z, 0, -tiltDirection.x);
        targetRotation = Quaternion.AngleAxis(tiltAngle, tiltAxis);
    }

    private void ApplyTilt() {
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
    }

    private static void FlattenToXYPlane(ref Vector3 position) {
        position.y = 0;
    }
}