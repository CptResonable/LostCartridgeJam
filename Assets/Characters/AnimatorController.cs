using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnimatorController {
    public Animator animator;

    public event Delegates.EmptyDelegate animatorUpdatedEvent;

    private Vector3 localVelocity;

    private Character character;

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;
        character.lateUpdateEvent += Character_lateUpdateEvent;

        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
        character.locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
        character.locomotion.wallrunController.horizontalRunStarted += WallrunController_horizontalRunStarted;
        character.locomotion.wallrunController.horizontalRunStopped += WallrunController_horizontalRunStopped;

        character.locomotion.slideStartedEvent += Locomotion_slideStartedEvent;
        character.locomotion.slideEndedEvent += Locomotion_slideEndedEvent;
        character.locomotion.jumpStartedEvent += Locomotion_jumpStartedEvent;
    }

    private void Character_lateUpdateEvent() {
        //animator.Update(0);
    }

    private void Character_updateEvent() {
        Vector3 preAnimPelvisPos = character.body.tPelvis.localPosition;
        localVelocity = Vector3.Lerp(localVelocity, character.transform.InverseTransformVector(character.rb.velocity), Time.deltaTime * 5);

        animator.SetFloat("VelocityZ", localVelocity.z / 5);
        animator.SetFloat("VelocityX", localVelocity.x / 5);
        animator.SetFloat("VelocityY", localVelocity.y / 5);

        float bodyToHeadDeltaYaw = Vector3.SignedAngle(character.transform.forward, Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, character.transform.up), character.transform.up);
        bodyToHeadDeltaYaw = (Mathf.Clamp(bodyToHeadDeltaYaw, -90, 90) + 90) / 180;
        animator.SetFloat("SlideBodyToCameraAngle", bodyToHeadDeltaYaw);

        //animator.SetBool("IsHanging", character.locomotion.hand_R.grabingLedge);
        animator.Update(Time.deltaTime);
        //character.body.tPelvis.localPosition *= 4;
        animatorUpdatedEvent?.Invoke();
    }
    //private void Character_updateEvent() {
    //    Vector3 preAnimPelvisPos = character.body.tPelvis.position;
    //    localVelocity = Vector3.Lerp(localVelocity, character.transform.InverseTransformVector(character.rb.velocity), Time.deltaTime * 10);
    //    animator.SetFloat("VelocityZ", localVelocity.z / 3);
    //    animator.SetFloat("VelocityX", localVelocity.x / 3);
    //    animator.SetFloat("VelocityY", localVelocity.y / 3);
    //    //animator.SetBool("IsHanging", character.locomotion.hand_R.grabingLedge);
    //    animator.Update(Time.deltaTime);
    //    //character.body.tPelvis.position = Vector3.Lerp(character.body.tPelvis.position, new Vector3(preAnimPelvisPos.x, character.body.tPelvis.position.y, preAnimPelvisPos.z), 0.5f);
    //    animatorUpdatedEvent?.Invoke();

    //}

    private void WallrunController_verticalRunStarted() {
        animator.SetBool("IsWallClimbing", true);
    }

    private void WallrunController_verticalRunStopped() {
        animator.SetBool("IsWallClimbing", false);
        animator.SetBool("IsReaching", false);
    }

    private void WallrunController_horizontalRunStarted() {
        if (character.locomotion.wallrunController.wallRunSide == Enums.Side.left)
            animator.SetFloat("LeftOrRightWallRun", 0);
        else
            animator.SetFloat("LeftOrRightWallRun", 1);

        animator.SetBool("IsWallRunning", true);
    }

    private void WallrunController_horizontalRunStopped() {
        animator.SetBool("IsWallRunning", false);
    }

    private void Locomotion_slideStartedEvent() {
        animator.SetBool("IsSliding", true);
    }

    private void Locomotion_slideEndedEvent() {
        animator.SetBool("IsSliding", false);
    }

    private void Locomotion_jumpStartedEvent() {
        animator.SetTrigger("Jump");
    }
}
