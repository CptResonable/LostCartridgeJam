using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WallrunSettings", menuName = "ScriptableObjects/WallrunSettings", order = 3)]
public class WallrunSettings : ScriptableObject {
    public AnimationCurve verticalRunCurve;
    public AnimationCurve mountCurve;
    public AnimationCurve yVelToVerticalRunScaleCurve;
    public float verticalRunDuration;
    public float maxVerticalVelocity;
}
