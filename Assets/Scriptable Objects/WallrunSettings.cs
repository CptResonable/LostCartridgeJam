using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WallrunSettings", menuName = "ScriptableObjects/WallrunSettings", order = 3)]
public class WallrunSettings : ScriptableObject {

    [Header("Vertical Run")]
    public AnimationCurve verticalRunCurve;
    public AnimationCurve mountCurve;
    public AnimationCurve yVelToVerticalRunScaleCurve;
    public float verticalRunDuration;
    public float maxVerticalVelocity;
    public float velocityNeededForWallClimb;
    public float maxAngleForWallClimb;

    [Header("Horizontal Run")]
    public AnimationCurve horizontalForwardRunCurve;
    public AnimationCurve horizontalInclinationCurve;
    public float horizontalRunDuration;
    public float maxHorizontalVelocity;
}
