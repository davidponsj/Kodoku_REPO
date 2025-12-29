using UnityEngine;
using System.Collections.Generic;
using System.Collections;


[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerMovementStats : ScriptableObject
{
    [Header("Caminar")]
    [Range(1f, 100f)] public float initialWalkSpeed = 12.5f;
    [Range(1f, 100f)] public float maxWalkSpeed = 20f;
    [Range(0.25f, 50f)] public float groundAcceleration = 5f;
    [Range(0.25f, 50f)] public float groundDeceleration = 20f;
    [Range(0.25f, 50f)] public float airAcceleration = 5f;
    [Range(0.25f, 50f)] public float airDeceleration = 5f;
    [Range(0.25f, 50f)] public float timeToMaxSpeed = 5f;

    [Header("GroundColissionCheck")]
    public LayerMask groundLayer;
    public float groundDetectionRayLength = 0.02f;
    public float headDetectionRayLength = 0.02f;
    [Range(0f,1f)] public float headwidth= 0.75f;
    public bool debugShowIsGroundedBox;
}