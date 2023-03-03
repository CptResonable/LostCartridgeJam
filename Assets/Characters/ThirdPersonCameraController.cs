using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour {
    [SerializeField] private Transform tCharacter;
    [SerializeField] private Camera camera;

    private void Update() {
        transform.position = tCharacter.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, tCharacter.rotation, Time.deltaTime * 3f);
    }
}
