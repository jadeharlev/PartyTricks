using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "DireDodgingPlayerStatsSO", menuName = "Scriptable Objects/DireDodgingPlayerStatsSO")]
public class DireDodgingPlayerStatsSO : ScriptableObject {
    [Header("Movement Settings")] 
    public float MoveSpeed = 15f;
    public float ProjectileScale = 1f;
    public float ProjectileSpeed = 15f;
    public float ProjectileShootRate = 1f;
    public float BaseDamage = 1f;
    public float BaseHealth = 15f;
    public float DamageAnimationTimeInSeconds = 0.2f;
    public float DeathAnimationTimeInSeconds = 0.05f;
    
    [Header("Charge Attack Settings")]
    public float ChargeTimeRequired = 2f;
    public float ChargedProjectileScale = 2f;
    public float ChargedProjectileSpeed = 2f;
    
    [Header("Ghost Mode Settings")]
    public float GhostChargeTime = 1f;
    public float GhostProjectileSpeed = 1.2f;
    public float GhostMoveSpeedMultiplier = 0.3f;
    
    [Header("Stun Settings")]
    public float StunDuration = 1f;


    [Header("Sound Events")] 
    public EventReference GetHitEvent;
    public EventReference DeathEvent;
    public EventReference ChargeLoopEvent;
    public EventReference ChargeReleaseEvent;
    public EventReference ChargeShootEvent;
}
