using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneDamageReaction {
    private Transform tBone;
    private Character character;

    private float pitch, yaw, roll;
    private Vector3 rotation;

    public BoneDamageReaction(Transform tBone, Character character) {
        this.tBone = tBone;
        this.character = character;
    }

    public void AddReaction(float pitch) {
        this.pitch += pitch;
    }

    //public void ApplyAndUpdate() {
    //    //tBone.Rotate(new Vector3(0, yaw, pitch), Space.Self);
    //    tBone.Rotate(character.transform.up, yaw, Space.World);
    //    pitch = Mathf.Lerp(pitch, 0, Time.deltaTime * 2);
    //    yaw = Mathf.Lerp(yaw, 0, Time.deltaTime * 2);
    //}

    public void ApplyAndUpdate() {
        //tBone.Rotate(new Vector3(0, yaw, pitch), Space.Self);
        if (tBone == character.body.tTorso_1)
            Debug.Log(rotation);
        tBone.Rotate(rotation, Space.World);
        rotation = Vector3.Lerp(rotation, Vector3.zero, 2 * Time.deltaTime);
    }

    public void AddReaction(Vector3 euler) {
        rotation += euler;
    }

    public void AddReaction(float pitch, float yaw) {
        this.pitch += pitch;
        this.yaw += yaw;
    }

    //public void AddReaction(Quaternion rotation, float duration) {

    //}
}
