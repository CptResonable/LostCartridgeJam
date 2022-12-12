using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpperBody {
    private Character character;

    public void Init(Character character) {
        this.character = character;

        character.updateEvent += Character_updateEvent;
    }

    private void Character_updateEvent() {
        Quaternion t2Rot = Quaternion.Slerp(character.transform.rotation, character.body.tHead.rotation, 0.7f);
        character.body.tTorso_1.rotation = Quaternion.Slerp(character.transform.rotation, character.body.tHead.rotation, 0.35f);
        character.body.tTorso_2.rotation = t2Rot;
        character.body.tTorso_2.Rotate(Vector3.up * 30, Space.Self);
    }
}
