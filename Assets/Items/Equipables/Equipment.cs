using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour {
    public uint ID = 0; // Instance id, is 0 if not set

    public enum EquipmentState { Equiped, InInventory, OnGround }
    public EquipmentState equipmentState;

    public Transform tRightHandOffset;
    public Transform tOffHandTarget;
    public Vector3 targetHandPosition;
    public Vector3 targetAdsHandPosition;

    [Header("Hand Joint Drive override values")]
    [SerializeField] private float jointDriveSpring;
    [SerializeField] private float jointDriveDamper;
    public JointDrive handJointDriveOverride;

    [HideInInspector] public Character character;

    protected List<Collider> colliders = new List<Collider>();

    public event Delegates.EmptyDelegate equipedEvent;
    public event Delegates.EmptyDelegate unequipedEvent;

    protected virtual void Awake() {
        FindColliders(transform);
        handJointDriveOverride.positionSpring = jointDriveSpring;
        handJointDriveOverride.positionDamper = jointDriveDamper;
        handJointDriveOverride.maximumForce = float.MaxValue;
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

        gameObject.SetActive(true);
        equipmentState = EquipmentState.Equiped;
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
        equipmentState = EquipmentState.InInventory;
        unequipedEvent?.Invoke();

        gameObject.SetActive(false);
    }

    public virtual void Drop() {
        gameObject.AddComponent<Rigidbody>();
        equipmentState = EquipmentState.OnGround;
    }
}
