using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Locomotion {
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float moveAcceleration;

    [SerializeField] private float bounceUpSpeed;
    [SerializeField] private float yVelLerpSpeed;

    [SerializeField] private float jumpVelocity;
    [SerializeField] private AnimationCurve airTimeToGravityScale;
    [SerializeField] private AnimationCurve handDistanceForceCurve;

    [SerializeField] private float targetHeight = 0.8f;

    [SerializeField] private LayerMask layerMask;
    [SerializeField] public WallrunController wallrunController;

    public bool isSprinting;
    public event Delegates.EmptyDelegate sprintStartedEvent;
    public event Delegates.EmptyDelegate sprintEndedEvent;

    private float currentHeight = 1;

    private Vector3 inputDir;

    private Rigidbody rb;
    public bool isGrounded = true;
    private float airTime = 0;
    private bool jumpOnCooldown = false;

    private Character character;

    // Dash 
    private bool isDashing;
    private Vector3 dashVector;

    public void Initialize(Character character) {
        this.character = character;
        rb = character.GetComponent<Rigidbody>();
        character.updateEvent += Character_updateEvent;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;
        character.characterInput.action_jump.keyDownEvent += Action_jump_keyDownEvent;
        character.characterInput.action_sprint.keyDownEvent += Action_sprint_keyDownEvent;
        character.characterInput.action_sprint.keyUpEvent += Action_sprint_keyUpEvent;
    }

    private void Action_sprint_keyDownEvent() {
        isSprinting = true;
        sprintStartedEvent?.Invoke();
    }

    private void Action_sprint_keyUpEvent() {
        isSprinting = false;
        sprintEndedEvent?.Invoke();
    }

    private void Character_updateEvent() {
        inputDir = character.transform.TransformDirection(character.characterInput.moveInput);

        //BodyTilt();
    }

    private float yRotationError;
    private float yDeltaError;
    [SerializeField] private float rotateForce;
    [SerializeField] private float rotateDamperCoef;
    [SerializeField] private Transform tTargetRoation;
    private void Character_fixedUpdateEvent() {
        if (isGrounded)
            airTime = 0;
        else 
            airTime += Time.deltaTime;
            
        if (!isGrounded) {

            rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            rb.AddForce(Vector3.down * 9.81f * airTimeToGravityScale.Evaluate(airTime), ForceMode.Acceleration);
        }
        else {
            rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
        }

        if (wallrunController.isWallRunning) {
            character.rb.velocity = wallrunController.runVelocity;
        }
        else {
            if (isGrounded)
                HorizontalMovement();
        }

        VerticalMovement();

        Quaternion targetRotation;
        if (wallrunController.isWallRunning)
            targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-wallrunController.wallHit.normal, Vector3.up).normalized, Vector3.up);
        else
            targetRotation = Quaternion.Euler(0, character.fpCamera.yaw, 0);

        tTargetRoation.rotation = targetRotation;
    }

    private void Rotation() {

        // Character rotation
        float newYRotationError = -Vector3.SignedAngle(Vector3.ProjectOnPlane(character.fpCamera.tCameraTarget.forward, Vector3.up).normalized, Vector3.ProjectOnPlane(character.transform.forward, Vector3.up).normalized, Vector3.up);
        //newYRotationError = -Vector3.SignedAngle(Vector3.ProjectOnPlane(character.head.tTarget.forward, Vector3.up).normalized, Vector3.ProjectOnPlane(character.transform.forward, Vector3.up).normalized, Vector3.up);

        yDeltaError = (newYRotationError - yRotationError) / Time.fixedDeltaTime;
        yRotationError = newYRotationError;
        float damper = yDeltaError * rotateDamperCoef;
        character.rb.AddTorque(Vector3.up * (yRotationError + damper) * rotateForce);
    }

    private void Action_jump_keyDownEvent() {


        if (!jumpOnCooldown && isGrounded)
            Jump();
        else if (wallrunController.isWallRunning) {

            wallrunController.StopWallRun();

            Vector3 jumpVector = Vector3.Lerp(Vector3.Lerp(character.fpCamera.tCamera.forward, wallrunController.wallHit.normal, 0.5f), Vector3.up, 0.25f).normalized;
            //Vector3 jumpVector = Vector3.Lerp(Vector3.ProjectOnPlane(character.fpCamera.tCameraTarget.forward, Vector3.up).normalized, wallrunController.wallHit.normal, 0.5f).normalized;
            //character.rb.velocity = character.fpCamera.tCamera.forward * jumpVelocity * 2f;
            //Debug.Log("WALL JUMP!");
            character.rb.velocity += (jumpVector * jumpVelocity * 0.7f) + Vector3.up * jumpVelocity * 0.4f;

            //// Head bounce
            //character.head.HeadBounce();
        }
    }

    private void VerticalMovement() {
        RaycastHit downHit;
        if (Physics.Raycast(character.transform.position, Vector3.down, out downHit, 100, layerMask)) {
            currentHeight = downHit.distance;
        }

        float deltaHeight = currentHeight - targetHeight;

        if (deltaHeight < 0 && !jumpOnCooldown) {
            isGrounded = true;
            float yVelTarget = (-deltaHeight) * bounceUpSpeed;
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(rb.velocity.x, yVelTarget, rb.velocity.z), yVelLerpSpeed * Time.deltaTime);
        }
        else {
            isGrounded = false;
        }
    }

    private void HorizontalMovement() {
        Vector3 moveDir_forwardPart = Vector3.zero;
        if (character.characterInput.moveInput.z > 0)
            moveDir_forwardPart = Vector3.Project(inputDir, character.transform.forward);

        Vector3 moveDir_otherPart = inputDir - moveDir_forwardPart;
        Vector3 moveVector = moveDir_otherPart * moveSpeed;

        if (isSprinting)
            moveVector += moveDir_forwardPart * sprintSpeed;
        else
            moveVector += moveDir_forwardPart * moveSpeed;

        if (isDashing)
            rb.velocity = Vector3.Lerp(rb.velocity, dashVector * moveSpeed + Vector3.up * rb.velocity.y, moveAcceleration * Time.deltaTime);
        else
            rb.velocity = Vector3.Lerp(rb.velocity, moveVector + Vector3.up * rb.velocity.y, moveAcceleration * Time.deltaTime);
    }

    private void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        jumpOnCooldown = true;
        character.StartCoroutine(JumpCorutine());
    }

    // Tilt while climbing walls
    private float tiltAmount = 0f;
    private Vector3 tiltAxel;
    private void BodyTilt() {
        if (wallrunController.isWallRunning) {
            tiltAmount = Mathf.Lerp(tiltAmount, 55, Time.deltaTime * 3);
            tiltAxel = Vector3.Cross(-wallrunController.wallHit.normal, Vector3.up);
        }
        else {
            tiltAmount = Mathf.Lerp(tiltAmount, 0, Time.deltaTime * 2);
        }
        character.tRig.localRotation = Quaternion.identity;
        character.tRig.Rotate(tiltAxel, tiltAmount, Space.World);
        //character.tRig.rotation = Quaternion.Slerp(character.tRig.rotation, targetBodyRotation, Time.deltaTime * 4);
    }


    private IEnumerator JumpCorutine() {
        yield return new WaitForSeconds(0.3f);
        jumpOnCooldown = false;
    }

    public void Dash(float time, Vector3 dashVector) {
        this.dashVector = dashVector;
        character.StartCoroutine(DashCorutine(time));
    }

    private IEnumerator DashCorutine(float time) {
        float defaultMoveSpeed = moveSpeed;
        float defaultAcceleration = moveAcceleration;
        isDashing = true;

        moveSpeed *= 3;
        moveAcceleration *= 2;
        yield return new WaitForSeconds(time);

        moveAcceleration = defaultAcceleration;
        moveSpeed = defaultMoveSpeed;
        isDashing = false;
    }
}
