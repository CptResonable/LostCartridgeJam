using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "LayerMasks", menuName = "ScriptableObjects/LayerMasks", order = 1)]
public class LayerMasks : ScriptableObject {
    private static LayerMasks _i;
    public static LayerMasks i {
        get {
            if (_i == null) {
                _i = Resources.Load("LayerMasks") as LayerMasks;
            }
            return _i;
        }
    }

    public LayerMask environment;
    public LayerMask wall;
    public LayerMask characters;
    public LayerMask equipment;

    public static bool IsInLayerMask(int layer, LayerMask layerMask) {
        return ((layerMask.value & (1 << layer)) > 0);
    }
}
