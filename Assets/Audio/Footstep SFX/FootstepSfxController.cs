using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSfxController : MonoBehaviour {
    [SerializeField] private Character character;

    public void OnPlayFootStepEvent_left() {
        if (character.locomotion.isGrounded && character.rb.velocity.magnitude > 2)
            AudioManager.i.PlaySoundStatic(AudioManager.i.sfxLibrary.footstepWalkDirt, character.body.tLegL_2.position);
    }
    public void OnPlayFootStepEvent_right() {
        if (character.locomotion.isGrounded && character.rb.velocity.magnitude > 2)
            AudioManager.i.PlaySoundStatic(AudioManager.i.sfxLibrary.footstepWalkDirt, character.body.tLegR_2.position);
    }
}
