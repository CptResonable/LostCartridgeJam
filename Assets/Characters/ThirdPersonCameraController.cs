using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour {
    [SerializeField] private Transform tCharacter;
    [SerializeField] private Camera camera;

    private float yaw;

    private void Update() {

        if (Input.GetKey(KeyCode.LeftArrow))
            yaw += Time.deltaTime * 120f;

        if (Input.GetKey(KeyCode.RightArrow))
            yaw -= Time.deltaTime * 120;

        transform.position = tCharacter.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, tCharacter.rotation * Quaternion.Euler(0, yaw, 0), Time.deltaTime * 13f);
    }
}
