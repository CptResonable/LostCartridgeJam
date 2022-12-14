using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLogic : MonoBehaviour {
    protected NPC character;

    protected virtual void Awake() {    
        character = GetComponent<NPC>();
    }

    public virtual void UpdateInput(CharacterInput input) {
    }
}
