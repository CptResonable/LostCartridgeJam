using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PhysicsMaterials", menuName = "ScriptableObjects/PhysicsMaterials")]
public class PhysicsMaterials : ScriptableObject {
    private static PhysicsMaterials _i;
    public static PhysicsMaterials i {
        get {
            if (_i == null) {
                _i = Resources.Load("PhysicsMaterials") as PhysicsMaterials;
            }
            return _i;
        }
    }

    public PhysicMaterial noFriction;
    public PhysicMaterial lowFriction;
}
