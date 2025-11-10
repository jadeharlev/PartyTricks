using UnityEngine;

[CreateAssetMenu(fileName = "CoinTiltPlayerStatsSO", menuName = "Scriptable Objects/Coin Tilt Player Stats")]
public class CoinTiltPlayerStatsSO : ScriptableObject {
    [Header("Movement Settings")] 
    public float MoveSpeed = 15f;
    public float Acceleration = 7f;
    public float SlipFactor = 1.7f;
    
    [Header("Jump Settings")]
    public float JumpForce = 8f;
    public float AirControlMultiplier = 1;
    public float GravityScale = 3f;
    public float CoyoteTimeInSeconds = 0.15f;
    
    [Header("Fall Settings")] 
    public float FallThresholdY = -10f;
    public float RespawnDelayInSeconds = 0.75f;
}
