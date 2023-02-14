using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IkAnimationPoint : MonoBehaviour {
    [SerializeField] private Body.BoneEnums parentBone;

    [SerializeField] private Character character;

    private void Awake() {
        transform.parent = character.body.tBones[(int)parentBone];
    }
}
