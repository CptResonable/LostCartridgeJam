using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSoundManager : MonoBehaviour {
    private Character character;
    private void Awake() {
        character = GetComponent<Character>();

        character.locomotion.jumpStartedEvent += Locomotion_jumpStartedEvent;
        character.locomotion.landedEvent += Locomotion_landedEvent;
        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
    }

    private void Locomotion_jumpStartedEvent() {
        AudioManager.i.PlaySoundStatic(AudioManager.i.sfxLibrary.jumpStart_01, character.transform.position);
    }

    private void Locomotion_landedEvent(float airTime) {
        AudioManager.i.PlaySoundStatic(AudioManager.i.sfxLibrary.land_01, character.transform.position);
    }

    private void WallrunController_verticalRunStarted() {
        AudioManager.i.PlaySoundStatic(AudioManager.i.sfxLibrary.wallClimb_01, character.transform.position);
    }
}
