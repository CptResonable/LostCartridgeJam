using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipable : MonoBehaviour {
    [HideInInspector] public Character character;

    public enum EquipableState { Equiped, InInventory, OnGround }
    public EquipableState equipmentState;

    protected List<Collider> colliders = new List<Collider>();

    public event Delegates.EmptyDelegate equipedEvent;
    public event Delegates.EmptyDelegate unequipedEvent;

    protected virtual void Awake() {
        FindColliders(transform);
    }

    protected virtual void Update() {
    }

    protected virtual void LateUpdate() {
    }

    protected virtual void FixedUpdate() {
    }

    void FindColliders(Transform t) {
        Collider col;
        if (t.TryGetComponent<Collider>(out col))
            colliders.Add(col);

        for (int i = 0; i < t.childCount; i++) {
            FindColliders(t.GetChild(i));
        }
    }

    public virtual void Equip(Character character) {
        this.character = character;
        equipedEvent?.Invoke();

        // Disable collission between equpable object and character
        for (int i = 0; i < character.body.tBones.Length; i++) {
            Collider col;
            if (character.body.tBones[i].TryGetComponent<Collider>(out col)) {
                for (int j = 0; j < colliders.Count; j++) {
                    Physics.IgnoreCollision(col, colliders[j], true);
                }
            }
        }
    }

    public virtual void Unequip() {

        // Enable collission between equpable object and character
        if (character != null) {
            for (int i = 0; i < character.body.tBones.Length; i++) {
                Collider col;
                if (character.body.tBones[i].TryGetComponent<Collider>(out col)) {
                    for (int j = 0; j < colliders.Count; j++) {
                        Physics.IgnoreCollision(col, colliders[j], false);
                    }
                }
            }
        }

        character = null;
        unequipedEvent?.Invoke();
    }

}
