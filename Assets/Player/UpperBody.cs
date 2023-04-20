using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpperBody {
    private Character character;

    // Additive piich yaw and roll
    private Vector3 pyrTorso1, pyrTorso2;

    private List<UpperBodyRotationModifier> modifiers = new List<UpperBodyRotationModifier>();

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;
    }

    private void Character_updateEvent() {
        float deltaPitch = -Vector3.SignedAngle(character.fpCamera.tCamera.forward, Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, Vector3.up), character.fpCamera.tCamera.right);
        if (character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.WallClimbing)
            deltaPitch = 0;

        float deltaYaw = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.transform.forward, Vector3.up), Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, Vector3.up), Vector3.up);
        float deltaRoll = Vector3.SignedAngle(character.transform.right, Vector3.ProjectOnPlane(character.transform.right, character.fpCamera.tCamera.up), character.transform.forward);

        // Scale down pitch if standing next to wall etc, this is to stop head/torso cliping into geometry
        RaycastHit pitchHit;
        if (deltaPitch > 0) {
            if (Physics.Raycast(character.fpCamera.tCamera.transform.position, character.fpCamera.tCamera.transform.forward, out pitchHit, 1, LayerMasks.i.environment)) {
                deltaPitch *= (pitchHit.distance - 0.25f) * 1.333333333333333f;
            }
        }
        else if (deltaPitch < 0) {
            if (Physics.Raycast(character.fpCamera.tCamera.transform.position, -character.fpCamera.tCamera.transform.forward, out pitchHit, 1, LayerMasks.i.environment)) {
                deltaPitch *= (pitchHit.distance - 0.25f) * 1.333333333333333f;
            }
        }

        // Scale down roll if standing next to wall etc, this is to stop head/torso cliping into geometry
        RaycastHit rollHit;
        if (deltaRoll > 0) {
            if (Physics.Raycast(character.fpCamera.tCamera.transform.position, -character.fpCamera.tCamera.transform.right, out rollHit, 1, LayerMasks.i.environment)) {
                deltaRoll *= (rollHit.distance - 0.25f) * 1.333333333333333f;
            }
        }
        else if (deltaRoll < 0) {
            if (Physics.Raycast(character.fpCamera.tCamera.transform.position, character.fpCamera.tCamera.transform.right, out rollHit, 1, LayerMasks.i.environment)) {
                deltaRoll *= (rollHit.distance - 0.25f) * 1.333333333333333f;
            }
        }

        Vector3 bonusEulers_torso1 = Vector3.zero;
        Vector3 bonusEulers_torso2 = Vector3.zero;
        for (int i = 0; i < modifiers.Count; i++) {
            bonusEulers_torso1 += modifiers[i].bonusEuler_torso1;
            bonusEulers_torso2 += modifiers[i].bonusEuler_torso2;
        }

        //if (character.locomotion.activeStateEnum == Locomotion.LocomotionState.StateIDEnum.WallClimbing) {
        //    character.body.tTorso_1.Rotate(character.transform.right, -20, Space.World);
        //    character.body.tTorso_2.Rotate(character.transform.right, -15, Space.World);
        //}
        //else {
        //    character.body.tTorso_1.Rotate(character.fpCamera.tCamera.right, deltaPitch * 0.35f + bonusEulers_torso1.x, Space.World);
        //    character.body.tTorso_2.Rotate(character.fpCamera.tCamera.right, deltaPitch * 0.35f + bonusEulers_torso2.x, Space.World);
        //}

        pyrTorso1.x = Mathf.Lerp(pyrTorso1.x, deltaPitch * 0.35f + bonusEulers_torso1.x, Time.deltaTime * 12);
        pyrTorso1.y = Mathf.Lerp(pyrTorso1.y, deltaYaw * 0.5f + bonusEulers_torso1.y, Time.deltaTime * 12);
        pyrTorso1.z = Mathf.Lerp(pyrTorso1.z, deltaRoll * 0.5f + bonusEulers_torso1.z, Time.deltaTime * 12);

        pyrTorso2.x = Mathf.Lerp(pyrTorso2.x, deltaPitch * 0.35f + bonusEulers_torso2.x, Time.deltaTime * 12);
        pyrTorso2.y = Mathf.Lerp(pyrTorso2.y, deltaYaw * 0.5f + bonusEulers_torso2.y, Time.deltaTime * 12);
        pyrTorso2.z = Mathf.Lerp(pyrTorso2.z, deltaRoll * 0.5f + bonusEulers_torso2.z, Time.deltaTime * 12);

        character.body.tTorso_1.Rotate(character.fpCamera.tCamera.right, pyrTorso1.x , Space.World);
        character.body.tTorso_2.Rotate(character.fpCamera.tCamera.right, pyrTorso2.x, Space.World);

        character.body.tTorso_1.Rotate(character.fpCamera.tCamera.forward, pyrTorso1.z, Space.World);
        character.body.tTorso_2.Rotate(character.fpCamera.tCamera.forward, pyrTorso2.z, Space.World);

        character.body.tTorso_1.Rotate(character.body.tTorso_1.up, pyrTorso1.y, Space.World);
        character.body.tTorso_2.Rotate(character.body.tTorso_2.up, pyrTorso2.y, Space.World);

        //character.body.tTorso_1.Rotate(character.fpCamera.tCamera.right, deltaPitch * 0.35f + bonusEulers_torso1.x, Space.World);
        //character.body.tTorso_2.Rotate(character.fpCamera.tCamera.right, deltaPitch * 0.35f + bonusEulers_torso2.x, Space.World);

        //character.body.tTorso_1.Rotate(character.fpCamera.tCamera.forward, deltaRoll * 0.5f + bonusEulers_torso1.z, Space.World);
        //character.body.tTorso_2.Rotate(character.fpCamera.tCamera.forward, deltaRoll * 0.5f + bonusEulers_torso2.z, Space.World);

        //character.body.tTorso_1.Rotate(character.body.tTorso_1.up, deltaYaw * 0.5f + bonusEulers_torso1.y, Space.World);
        //character.body.tTorso_2.Rotate(character.body.tTorso_2.up, deltaYaw * 0.5f + bonusEulers_torso2.y, Space.World);

        character.body.tHead.rotation = character.fpCamera.tCamera.rotation; // Set head rotation to camera rotation
        character.damageReactionController.ManualUpdateTest();
        character.fpCamera.tCamera.rotation = character.body.tHead.rotation;
        Debug.Log("WORING ON THIS; THIS NEEDS TIO BE FOODSODOS I MEAN GOOOD");

        character.body.tHead.Rotate(character.body.tHead.forward, Mathf.Lerp(0, -30, character.stanceController.hipAdsInterpolator.t), Space.World);



        character.fpCamera.tCamera.position = Vector3.Lerp(character.fpCamera.tCameraTarget_hip.position, character.fpCamera.tCameraTarget_ads.position, character.stanceController.hipAdsInterpolator.t); // Set camera position
    }

    //private void Character_updateEvent() {
    //    float deltaPitch = Vector3.SignedAngle(character.transform.forward, Vector3.ProjectOnPlane(character.transform.forward, character.fpCamera.tCamera.up), character.transform.right);
    //    float deltaYaw = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.transform.forward, Vector3.up), Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, Vector3.up), Vector3.up);
    //    float deltaRoll = Vector3.SignedAngle(character.transform.right, Vector3.ProjectOnPlane(character.transform.right, character.fpCamera.tCamera.up), character.transform.forward);

    //    // Scale down pitch if standing next to wall etc, this is to stop head/torso cliping into geometry
    //    RaycastHit pitchHit;
    //    if (deltaPitch > 0) {
    //        if (Physics.Raycast(character.fpCamera.tCameraTarget.transform.position, character.fpCamera.tCameraTarget.transform.forward, out pitchHit, 1, LayerMasks.i.environment)) {
    //            deltaPitch *= (pitchHit.distance - 0.25f) * 1.333333333333333f;
    //        }
    //    }
    //    else if (deltaPitch < 0) {
    //        if (Physics.Raycast(character.fpCamera.tCameraTarget.transform.position, -character.fpCamera.tCameraTarget.transform.forward, out pitchHit, 1, LayerMasks.i.environment)) {
    //            deltaPitch *= (pitchHit.distance - 0.25f) * 1.333333333333333f;
    //        }
    //    }

    //    // Scale down roll if standing next to wall etc, this is to stop head/torso cliping into geometry
    //    RaycastHit rollHit;
    //    if (deltaRoll > 0) {
    //        if (Physics.Raycast(character.fpCamera.tCameraTarget.transform.position, -character.fpCamera.tCameraTarget.transform.right, out rollHit, 1, LayerMasks.i.environment)) {
    //            deltaRoll *= (rollHit.distance - 0.25f) * 1.333333333333333f;
    //        }
    //    }
    //    else if (deltaRoll < 0) {
    //        if (Physics.Raycast(character.fpCamera.tCameraTarget.transform.position, character.fpCamera.tCameraTarget.transform.right, out rollHit, 1, LayerMasks.i.environment)) {
    //            deltaRoll *= (rollHit.distance - 0.25f) * 1.333333333333333f;
    //        }
    //    }


    //    character.body.tTorso_1.Rotate(Vector3.up, deltaYaw * 0.35f, Space.Self);
    //    character.body.tTorso_2.Rotate(Vector3.up, deltaYaw * 0.35f, Space.Self);

    //    character.body.tTorso_1.Rotate(Vector3.right, deltaPitch * 0.35f, Space.Self);
    //    character.body.tTorso_2.Rotate(Vector3.right, deltaPitch * 0.35f, Space.Self);

    //    character.body.tTorso_1.Rotate(Vector3.forward, deltaRoll * 0.5f, Space.Self);
    //    character.body.tTorso_2.Rotate(Vector3.forward, deltaRoll * 0.5f, Space.Self);

    //    // Rotate torso when aimng guns
    //    if (character.weaponController.equipedGun != null && !character.locomotion.state_grounded.isSprinting)
    //        equipmentInducedTorsoYaw = Mathf.Lerp(equipmentInducedTorsoYaw, 30, Time.deltaTime * 3f);
    //    else
    //        equipmentInducedTorsoYaw = Mathf.Lerp(equipmentInducedTorsoYaw, 0, Time.deltaTime * 3f);

    //    character.body.tTorso_2.Rotate(Vector3.up * equipmentInducedTorsoYaw, Space.Self);

    //    character.body.tHead.rotation = character.fpCamera.tCamera.rotation; // Set head rotation to camera rotation
    //    character.fpCamera.tCamera.position = character.fpCamera.tCameraTarget.position; // Set camera position
    //}

    private void Character_fixedUpdateEvent() {
        character.fpCamera.tCamera.position = Vector3.Lerp(character.fpCamera.tCameraTarget_hip.position, character.fpCamera.tCameraTarget_ads.position, character.stanceController.hipAdsInterpolator.t); // Set camera position, needs to be done here aswell for camera and hands to move smoothly
    }

    public void AddModifier(UpperBodyRotationModifier modifier) {
        modifiers.Add(modifier);
    }

    public class UpperBodyRotationModifier {
        public Vector3 bonusEuler_torso1;
        public Vector3 bonusEuler_torso2;

        private TWrapper t;

        public UpperBodyRotationModifier(Vector3 bonusEuler_torso1, Vector3 bonusEuler_torso2) {
            this.bonusEuler_torso1 = bonusEuler_torso1;
            this.bonusEuler_torso2 = bonusEuler_torso2;
        }

        public void UpdateBonusEulers(Vector3 newBonusEuler_torso1, Vector3 newBonusEuler_torso2) {
            bonusEuler_torso1 = newBonusEuler_torso1;
            bonusEuler_torso2 = newBonusEuler_torso2;
        }
    }
}
