using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Foot {
    [SerializeField] private Enums.Side side;
    [SerializeField] private Transform tFoot; // Leg2end
    [SerializeField] private Transform tIkTarget; // The ik target tarnsform
    [SerializeField] private Transform tIkPole;

    private Character character;
    private Transform tLeg_1;
    private Transform tLeg_2;

    private float deltaHeight;

    public void Init(Character character) {
        this.character = character;

        if (side == Enums.Side.left) {
            tLeg_1 = character.body.tLegL_1;
            tLeg_2 = character.body.tLegL_2;
        }
        else {
            tLeg_1 = character.body.tLegR_1;
            tLeg_2 = character.body.tLegR_2;
        }

        character.animatorController.animatorUpdatedEvent += AnimatorController_animatorUpdatedEvent;
    }

    private void AnimatorController_animatorUpdatedEvent() {

        tIkTarget.position = tFoot.position;
        tIkTarget.rotation = tFoot.rotation;

        tIkPole.position = tLeg_2.position + character.transform.forward;

        float rawDeltaHeight = 0;
        if (character.locomotion.activeState.stateID == Locomotion.LocomotionState.StateIDEnum.Grounded) {
            RaycastHit downHit;
            if (Physics.Raycast(tIkTarget.position + Vector3.up, Vector3.down, out downHit, 5, LayerMasks.i.environment)) {
                rawDeltaHeight = downHit.point.y - character.locomotion.downHit.point.y;           
            }
        }
        deltaHeight = Mathf.Lerp(deltaHeight, rawDeltaHeight, Time.deltaTime * 8);

        tIkTarget.position += Vector3.up * deltaHeight;

        RaycastHit hipToFootHit;
        if (Physics.Linecast(tLeg_1.position, tIkTarget.position, out hipToFootHit, LayerMasks.i.environment)) {
            tIkTarget.position = hipToFootHit.point;
        }
    }
}
