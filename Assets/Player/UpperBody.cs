using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpperBody {
    private Character character;

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;
        //character.fixedUpdateEvent += Character_updateEvent;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;
    }

    private void Character_fixedUpdateEvent() {
        character.fpCamera.tCamera.position = character.fpCamera.tCameraTarget.position; // Set camera position
    }

    private void Character_updateEvent() {

        //float deltaPitch = Vector3.SignedAngle(character.transform.forward, Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, character.transform.right), character.transform.right);
        float deltaPitch = Vector3.SignedAngle(character.transform.forward, Vector3.ProjectOnPlane(character.transform.forward, character.fpCamera.tCamera.up), character.transform.right);
        float deltaYaw = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.transform.forward, Vector3.up), Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, Vector3.up), Vector3.up);

        character.body.tTorso_1.Rotate(Vector3.up, deltaYaw * 0.35f, Space.Self);
        character.body.tTorso_2.Rotate(Vector3.up, deltaYaw * 0.35f, Space.Self);

        character.body.tTorso_1.Rotate(Vector3.right, deltaPitch * 0.35f, Space.Self);
        character.body.tTorso_2.Rotate(Vector3.right, deltaPitch * 0.35f, Space.Self);

        //// Follow head roation with torso
        //character.body.tTorso_1.rotation = Quaternion.Slerp(character.transform.rotation, character.fpCamera.tCamera.rotation, 0.35f);
        //character.body.tTorso_2.rotation = Quaternion.Slerp(character.transform.rotation, character.fpCamera.tCamera.rotation, 0.7f);

        // Rotate torso when aimng guns
        if (character.weaponController.equipedGun != null) {
            if (character.weaponController.equipedGun.isAuto)
                character.body.tTorso_2.Rotate(Vector3.up * 30, Space.Self);
        }

        character.body.tHead.rotation = character.fpCamera.tCamera.rotation; // Set head rotation to camera rotation
        character.fpCamera.tCamera.position = character.fpCamera.tCameraTarget.position; // Set camera position
    }

    //private void Character_updateEvent() {

    //    // Follow head roation with torso
    //    character.body.tTorso_1.rotation = Quaternion.Slerp(character.transform.rotation, character.fpCamera.tCamera.rotation, 0.35f);
    //    character.body.tTorso_2.rotation = Quaternion.Slerp(character.transform.rotation, character.fpCamera.tCamera.rotation, 0.7f);

    //    // Rotate torso when aimng guns
    //    if (character.weaponController.equipedGun != null) {
    //        if (character.weaponController.equipedGun.isAuto)
    //            character.body.tTorso_2.Rotate(Vector3.up * 30, Space.Self);
    //    }

    //    character.body.tHead.rotation = character.fpCamera.tCamera.rotation; // Set head rotation to camera rotation
    //    character.fpCamera.tCamera.position = character.fpCamera.tCameraTarget.position; // Set camera position
    //}

    //private void Character_updateEvent() {
    //    Quaternion t2Rot = Quaternion.Slerp(character.transform.rotation, character.body.tHead.rotation, 0.7f);
    //    character.body.tTorso_1.rotation = Quaternion.Slerp(character.transform.rotation, character.body.tHead.rotation, 0.35f);
    //    character.body.tTorso_2.rotation = t2Rot;
    //    character.fpCamera.tCamera.position = character.fpCamera.tHead.position;
    //    //character.body.tTorso_1.rotation = Quaternion.Slerp(character.body.tTorso_1.rotation, Quaternion.Slerp(character.transform.rotation, character.body.tHead.rotation, 0.35f), Time.deltaTime * 2);
    //    //character.body.tTorso_2.rotation = Quaternion.Slerp(character.body.tTorso_2.rotation, t2Rot, Time.deltaTime * 2);

    //    if (character.weaponController.equipedGun != null) {
    //        if (character.weaponController.equipedGun.isAuto)
    //            character.body.tTorso_2.Rotate(Vector3.up * 30, Space.Self);
    //    }
    //}
}
