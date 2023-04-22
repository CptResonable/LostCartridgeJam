using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MiscAnimationCurves", menuName = "ScriptableObjects/MiscAnimationCurves")]
public class MiscAnimationCurves : ScriptableObject {
    private static MiscAnimationCurves _i;
    public static MiscAnimationCurves i {
        get {
            if (_i == null) {
                _i = Resources.Load("MiscAnimationCurves") as MiscAnimationCurves;
            }
            return _i;
        }
    }

    [Header("Hit reaction")]
    public AnimationCurve damageReactionCurve;

    [Header("Head/camera")]
    public AnimationCurve recoilHeadApplicationCurve;
    public AnimationCurve wallRunVerticalTiltBackCurve;
    public AnimationCurve wallAngleToRollCurve;
}
