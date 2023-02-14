using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpperBody {
    private Character character;
    private float equipmentInducedTorsoYaw;

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;
    }


    private void Character_updateEvent() {
        float deltaPitch = Vector3.SignedAngle(character.transform.forward, Vector3.ProjectOnPlane(character.transform.forward, character.fpCamera.tCamera.up), character.transform.right);
        float deltaYaw = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.transform.forward, Vector3.up), Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, Vector3.up), Vector3.up);
        float deltaRoll = Vector3.SignedAngle(character.transform.right, Vector3.ProjectOnPlane(character.transform.right, character.fpCamera.tCamera.up), character.transform.forward);

        // Scale down pitch if standing next to wall etc, this is to stop head/torso cliping into geometry
        RaycastHit pitchHit;
        if (deltaPitch > 0) {
            if (Physics.Raycast(character.fpCamera.tCameraTarget.transform.position, character.fpCamera.tCameraTarget.transform.forward, out pitchHit, 1, LayerMasks.i.environment)) {
                deltaPitch *= (pitchHit.distance - 0.25f) * 1.333333333333333f;
            }
        }
        else if (deltaPitch < 0) {
            if (Physics.Raycast(character.fpCamera.tCameraTarget.transform.position, -character.fpCamera.tCameraTarget.transform.forward, out pitchHit, 1, LayerMasks.i.environment)) {
                deltaPitch *= (pitchHit.distance - 0.25f) * 1.333333333333333f;
            }
        }

        // Scale down roll if standing next to wall etc, this is to stop head/torso cliping into geometry
        RaycastHit rollHit;
        if (deltaRoll > 0) {
            if (Physics.Raycast(character.fpCamera.tCameraTarget.transform.position, -character.fpCamera.tCameraTarget.transform.right, out rollHit, 1, LayerMasks.i.environment)) {
                deltaRoll *= (rollHit.distance - 0.25f) * 1.333333333333333f;
            }
        }
        else if (deltaRoll < 0) {
            if (Physics.Raycast(character.fpCamera.tCameraTarget.transform.position, character.fpCamera.tCameraTarget.transform.right, out rollHit, 1, LayerMasks.i.environment)) {
                deltaRoll *= (rollHit.distance - 0.25f) * 1.333333333333333f;
            }
        }


        character.body.tTorso_1.Rotate(Vector3.up, deltaYaw * 0.35f, Space.Self);
        character.body.tTorso_2.Rotate(Vector3.up, deltaYaw * 0.35f, Space.Self);

        character.body.tTorso_1.Rotate(Vector3.right, deltaPitch * 0.35f, Space.Self);
        character.body.tTorso_2.Rotate(Vector3.right, deltaPitch * 0.35f, Space.Self);

        character.body.tTorso_1.Rotate(Vector3.forward, deltaRoll * 0.5f, Space.Self);
        character.body.tTorso_2.Rotate(Vector3.forward, deltaRoll * 0.5f, Space.Self);

        // Rotate torso when aimng guns
        if (character.weaponController.equipedGun != null && !character.locomotion.isSprinting)
            equipmentInducedTorsoYaw = Mathf.Lerp(equipmentInducedTorsoYaw, 30, Time.deltaTime * 3f);
        else
            equipmentInducedTorsoYaw = Mathf.Lerp(equipmentInducedTorsoYaw, 0, Time.deltaTime * 3f);

        character.body.tTorso_2.Rotate(Vector3.up * equipmentInducedTorsoYaw, Space.Self);

        character.body.tHead.rotation = character.fpCamera.tCamera.rotation; // Set head rotation to camera rotation
        character.fpCamera.tCamera.position = character.fpCamera.tCameraTarget.position; // Set camera position
    }

    private void Character_fixedUpdateEvent() {
        character.fpCamera.tCamera.position = character.fpCamera.tCameraTarget.position; // Set camera position, needs to be done here aswell for camera and hands to move smoothly
    }
}
