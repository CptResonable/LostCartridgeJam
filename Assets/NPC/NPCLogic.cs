using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLogic : MonoBehaviour {
    protected NPC character;
    protected Character target;

    protected virtual void Awake() {
        target = GameManager.i.player;
        character = GetComponent<NPC>();
    }

    public virtual void UpdateInput(CharacterInput input) {
    }
}
