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
    protected ConfigurableJoint joint;
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
        joint = GetComponent<ConfigurableJoint>();

        character.equipmentManager.itemEquipedEvent += EquipmentManager_itemEquipedEvent;
        character.locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;
    }

    private void EquipmentManager_itemEquipedEvent(Equipment item) {
        joint.slerpDrive = item.handJointDriveOverride;
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
    protected bool LookForGrip() {

        if (character.locomotion.wallrunController.ledgeGrabPoints.Count == 0)
            return false;

        if (tPhysicalTarget.position.y + 0.05f > character.locomotion.wallrunController.topGrabPoint.y) {

            Vector3 v3 = Vector3.ProjectOnPlane(VectorUtils.FromToVector(character.locomotion.wallrunController.topGrabPoint, tPhysicalTarget.position), character.transform.forward);
            Vector3 point = character.locomotion.wallrunController.topGrabPoint + new Vector3(v3.x, 0, v3.z);

            RaycastHit hit;
            if (Physics.Raycast(point + Vector3.up * 0.1f, Vector3.down, out hit, 0.2f, LayerMasks.i.environment)) {
                grabPoint = hit.point;
                grabRotation = Quaternion.LookRotation(-Vector3.Cross(character.locomotion.wallrunController.wallHit.normal, hit.normal), -character.locomotion.wallrunController.wallHit.normal);
            }
            else {
                grabPoint = character.locomotion.wallrunController.topGrabPoint;
                grabRotation = Quaternion.LookRotation(-Vector3.Cross(character.locomotion.wallrunController.wallHit.normal, Vector3.up), -character.locomotion.wallrunController.wallHit.normal);
            }
            grabingLedge = true;

            return true;
        }
        else {
            return false;
        }
    }

    private void WallrunController_verticalRunStopped() {
        if (grabingLedge)
            StartCoroutine(LetGoOfLedgeCorutine());

    }

    private IEnumerator LetGoOfLedgeCorutine() {
        yield return new WaitForSeconds(0.25f);
        grabingLedge = false;
    }
}
