using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Legs {
    public Foot foot_L;
    public Foot foot_R;

    private Character character;

    public void Init(Character character) {
        this.character = character;

        foot_L.Init(character);
        foot_R.Init(character);
    }
}
