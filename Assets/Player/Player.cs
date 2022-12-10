using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Head head;
    public FPCamera fpCamera;
    public UpperBody upperBody;
    public PlayerController playerController;
    public Hand hand;
    public HandMovement handMovement;
    public Arm arm;
    public Body body;

    public Rigidbody rb;
    public Animator animator;

    public event Delegates.EmptyDelegate updateEvent;
    public event Delegates.EmptyDelegate fixedUpdateEvent;
    public event Delegates.EmptyDelegate lateUpdateEvent;

    private void Awake() {
        rb = GetComponent<Rigidbody>();

        fpCamera.Initialize(this);
        playerController.Initialize(this);
        upperBody.Init(this);
        arm.Initialize(this);
    }

    Vector3 localVelocity;
    private void Update() {

        Vector3 preAnimPelvisPos = body.tPelvis.position;
        localVelocity = Vector3.Lerp(localVelocity, transform.InverseTransformVector(rb.velocity), Time.deltaTime * 4);
        animator.SetFloat("velForward", localVelocity.z / 6);
        animator.SetFloat("velSide", localVelocity.x / 6);
        //animator.SetFloat("velForward", playerController.inputDir.z);
        //animator.SetFloat("velSide", playerController.inputDir.x);

        animator.Update(Time.deltaTime);
        body.tPelvis.position = Vector3.Lerp(body.tPelvis.position, new Vector3(preAnimPelvisPos.x, body.tPelvis.position.y, preAnimPelvisPos.z), 0.5f);
        updateEvent?.Invoke();
        fpCamera.SetRotation();
    }

    private void FixedUpdate() {
        fixedUpdateEvent?.Invoke();
        handMovement.DoUpdate();
    }

    private void LateUpdate() {
        lateUpdateEvent?.Invoke();
    }
}
