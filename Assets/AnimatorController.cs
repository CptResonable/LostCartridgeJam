using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimatorController {
    public Animator animator;

    private Vector3 localVelocity;

    private Character character;

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;

        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
        character.locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
        character.locomotion.wallrunController.verticalRunReachStarted += WallrunController_verticalRunReachStarted;
    }

    private void Character_updateEvent() {
        localVelocity = Vector3.Lerp(localVelocity, character.transform.InverseTransformVector(character.rb.velocity), Time.deltaTime * 10);
        animator.SetFloat("VelocityZ", localVelocity.z / 6);
        animator.SetFloat("VelocityX", localVelocity.x / 6);
        animator.SetFloat("VelocityY", localVelocity.y / 6);
        //animator.SetBool("IsHanging", character.locomotion.hand_R.grabingLedge);
        animator.Update(Time.deltaTime);
    }

    private void WallrunController_verticalRunStarted() {
        animator.SetBool("IsWallClimbing", true);
    }

    private void WallrunController_verticalRunStopped() {
        animator.SetBool("IsWallClimbing", false);
        animator.SetBool("IsReaching", false);
    }

    private void WallrunController_verticalRunReachStarted() {
        //animator.SetBool("IsReaching", true);
    }
}
