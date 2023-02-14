using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LocomotionSettings", menuName = "ScriptableObjects/LocomotionSettings", order = 2)]
public class LocomotionSettings : ScriptableObject {
    public float moveSpeed;
    public float sprintSpeed;
    public float moveAcceleration;

    public float bounceUpSpeed;
    public float yVelLerpSpeed;

    public float jumpVelocity;
    public AnimationCurve airTimeToGravityScale;
    public AnimationCurve handDistanceForceCurve;
    public AnimationCurve bounceDownCurve;

    public float targetHeight = 0.8f;
    public float crouchTargetHeight = 0.45f;

}
