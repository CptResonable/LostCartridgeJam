using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAnchour : MonoBehaviour {
    [SerializeField] private Character character;

    private void Update() {
        transform.position = character.transform.position - character.handMovement.targetDeltaPos;
    }
}
