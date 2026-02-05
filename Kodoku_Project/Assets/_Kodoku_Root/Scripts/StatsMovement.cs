using UnityEngine;
using System.Collections.Generic;
using System.Collections;


[CreateAssetMenu(menuName = "Player Movement")]
public class StatsMovement : ScriptableObject
{
    [Header("Caminar")]
    [Range(0f, 1f)] public float moveThreshold = 0.25f;
    [Range(1f, 100f)] public float maxWalkSpeed = 12.5f;
    [Range(0.25f, 50f)] public float groundAcceleration = 5f;
    [Range(0.25f, 50f)] public float groundDeceleration = 20f;
    [Range(0.25f, 50f)] public float airAcceleration = 5f;
    [Range(0.25f, 50f)] public float airDeceleration = 5f;

    [Header("Correr")]
    [Range(1f, 100f)] public float maxRunSpeed = 20f;

    [Header("GroundColissionCheck")]
    public LayerMask groundLayer;

    [Header("Head Bump Slide")]
    public bool useHeadBumpSlide = true;
    [Range(1f, 50f)] public float headBumpSlideSpeed = 13f;
    [Range(0.01f, 0.5f)] public float headBumpBoxWidth = 0.3f;
    [Range(0.01f, 0.5f)] public float headBumpBoxHeight = 0.1f;

    [Header("Salto")]
    public float jumpHeight = 6.5f;
    [Range(1f, 1.1f)] public float jumpHeightCompensationFactor = 1.054f;
    public float timeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)] public float gravitOnRelaseMultiplier = 2f;
    public float maxFallSpeed = 26f;
    [Range(1, 5)] public int jumpsAllowed = 2;

    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)] public float timeForUpwardsCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)] public float apexThreshold = 0.97f;
    [Range(0.01f, 1f)] public float apexHangTime = 0.075f;

    [Header("Jump Buffer")]
    [Range(0f, 1f)] public float jumpBufferTime = 0.125f;

    [Header("Coyote Time")]
    [Range(0f, 1f)] public float coyoteTime = 0.1f;

    [Header("Gravity")]
    public float GravityUp = -20f;
    public float GravityDown = -40f;

    [Header("Debug")]
    public bool debugShowIsGrounded;
    public bool debugShowHeadRays;
    public bool debugShowWallHit;
    public bool debugShowHeadBumpBox;
    [Range(0f, 1f)] public float extraRayDebugDistance = 0.25f;

    [Header("Visualizacion de salto")]
    public bool showWalkJumpArc;
    public bool showRunJumpArc;
    public bool stopOnCollision = true;
    public bool drawRight = true;
    [Range(5, 100)] public int arcResolution = 20;
    [Range(0, 500)] public int visualizationSteps = 90;

    public float Gravity { get; private set; }
    public float initialJumpVelocity { get; private set; }
    public float adjustedJumpHeight { get;  private set; }

    private void OnValidate()
    {
        CalculateValues();
    }

    private void OnEnable()
    {
        CalculateValues();
    }

    private void CalculateValues()
    {
        adjustedJumpHeight = jumpHeight * jumpHeightCompensationFactor;
        //Gravity = -(2f * adjustedJumpHeight) / Mathf.Pow(timeTillJumpApex, 2f);
        //initialJumpVelocity = Mathf.Abs(Gravity) * timeTillJumpApex;
        initialJumpVelocity = Mathf.Sqrt(2f * adjustedJumpHeight * -GravityUp);

    }
}
