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

        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
        character.locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
    }

    private void Character_updateEvent() {
        Vector3 preAnimPelvisPos = character.body.tPelvis.localPosition;
        localVelocity = Vector3.Lerp(localVelocity, character.transform.InverseTransformVector(character.rb.velocity), Time.deltaTime * 5);

        animator.SetFloat("VelocityZ", localVelocity.z / 5);
        animator.SetFloat("VelocityX", localVelocity.x / 5);
        animator.SetFloat("VelocityY", localVelocity.y / 5);
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
}
