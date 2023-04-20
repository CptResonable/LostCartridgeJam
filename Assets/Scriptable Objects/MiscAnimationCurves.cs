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

    public AnimationCurve damageReactionCurve;
}
