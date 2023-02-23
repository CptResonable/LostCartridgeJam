using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour {

    public Transform tWeaponTarget; // Target position and rotation when holding a gun
    public Transform tPhysicalTarget; // Target position and rotation, it is the target after blending between diffrent states. Updates in fixedUpdate
    public Transform tIkTarget; // The ik target tarnsform

    public Transform tElbowPole; // The ik pole target tarnsform
    public Transform tElbowNoAnimPoleTarget; // The ik pole target when not following animation

    [SerializeField] private Transform tCOM;

    [SerializeField] protected float errorAdjustmentCoef;
    [SerializeField] protected float velocityChangeCoef;

    protected Character character;
    protected Arms arms;
    [SerializeField] protected KinematicMeasures kmTarget;
    public Rigidbody rb;

    protected Vector3 velocity;
    protected Vector3 targetVelocity;
    public Vector3 targetDeltaPos;

    public bool grabingLedge;
    public Vector3 grabPoint;
    public Quaternion grabRotation;
    protected float ledgeGrabInterpolator = 0;

    public virtual void Init(Character character) {
        this.character = character;
        this.arms = character.arms;

        //kmTarget = tPhysicalTarget.GetComponent<KinematicMeasures>();
        rb = GetComponent<Rigidbody>();

        character.locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;
    }

    protected virtual void Character_fixedUpdateEvent() {
    }

    public virtual void Update() {
        if (grabingLedge)
            ledgeGrabInterpolator = 1;
        else
            ledgeGrabInterpolator = Mathf.Lerp(ledgeGrabInterpolator, 0, Time.deltaTime * 8);     
    }

    //public virtual void ManualFixedUpdate() { }


    private void WallrunController_verticalRunStopped() {
        if (grabingLedge)
            StartCoroutine(LetGoOfLedgeCorutine());

    }

    private IEnumerator LetGoOfLedgeCorutine() {
        yield return new WaitForSeconds(0.25f);
        grabingLedge = false;
    }
}
