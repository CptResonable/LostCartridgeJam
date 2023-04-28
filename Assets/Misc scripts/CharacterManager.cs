using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {
    private static CharacterManager _i;
    public static CharacterManager i {
        get {
            if (_i == null) {
                GameObject go = new GameObject("CharacterManager");
                _i = go.AddComponent<CharacterManager>();
            }

            return _i;
        }
    }

    public Dictionary<uint, Character> characters = new Dictionary<uint, Character>();

    public void AddExistingCharacter(Character character) {
        character.ID = IdManager.CreateNewID();
        characters.Add(character.ID, character);
    }
}
