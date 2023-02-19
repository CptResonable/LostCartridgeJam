using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSoundManager : MonoBehaviour {
    private Character character;
    private void Awake() {
        character = GetComponent<Character>();

        character.locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
    }

    private void WallrunController_verticalRunStarted() {
        AudioManager.i.PlaySoundStatic(AudioManager.i.sfxLibrary.wallClimb_01, character.transform.position);
    }
}
