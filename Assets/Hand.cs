using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour {

    public Transform tWeaponTarget; // Target position and rotation when holding a gun
    public Transform tPhysicalTarget; // Target position and rotation, it is the target after blending between diffrent states. Updates in fixedUpdate
    public Transform tIkTarget; // The ik target tarnsform

    [SerializeField] private Transform tCOM;

    [SerializeField] protected float errorAdjustmentCoef;
    [SerializeField] protected float velocityChangeCoef;

    protected Character character;
    protected Arms arms;
    protected KinematicMeasures kmTarget;
    public Rigidbody rb;

    protected Vector3 velocity;
    protected Vector3 targetVelocity;
    public Vector3 targetDeltaPos;

    public virtual void Init(Character character) {
        this.character = character;
        this.arms = character.arms;

        kmTarget = tPhysicalTarget.GetComponent<KinematicMeasures>();
        rb = GetComponent<Rigidbody>();
    }

    public virtual void Update() { }

    public virtual void ManualFixedUpdate() { }
}
