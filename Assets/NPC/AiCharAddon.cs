using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiCharAddon : MonoBehaviour {
    private Character character;
    private NPCInput input;

    private void Awake() {
        character = GetComponent<Character>();
        input = GetComponent<NPCInput>();

        input.Init(character);
    }
}
