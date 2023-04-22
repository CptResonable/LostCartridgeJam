using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpCameraController : MonoBehaviour {
    [HideInInspector] public Camera camera;
    [SerializeField] private Character character;

    private void Awake() {
        camera = GetComponent<Camera>();
    }
    private void LateUpdate() {
        //Quaternion headDEulerTest = character.body.tHead.rotation * Quaternion.Inverse(Quaternion.Lerp(character.body.tHead.rotation, character.head.tCameraBase.transform.rotation, 0.25f));
        //character.head.camera.transform.rotation *= character.head.cameraRotationOffset;
       
        //transform.rotation = Quaternion.Lerp(character.head.tCameraBase.rotation, character.body.tHead.rotation, 0.25f);Quaternion.Euler(character.damageReactionController.bdrHead.rotation)
        //transform.rotation *= Quaternion.Inverse(Quaternion.Euler(character.damageReactionController.bdrHead.rotation));
        transform.Rotate(character.damageReactionController.bdrHead.rotation, Space.World);
    }
}
