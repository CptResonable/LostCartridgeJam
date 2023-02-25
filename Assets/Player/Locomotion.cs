using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Locomotion {
    [SerializeField] private LocomotionSettings settings;
    [SerializeField] public WallrunController wallrunController;

    [SerializeField] private Transform tTargetRoation;

    public LocomotionState_grounded state_grounded;
    public LocomotionState_inAir state_inAir;
    [HideInInspector] public LocomotionState activeState;

    //public bool isSprinting;
    public event Delegates.EmptyDelegate sprintStartedEvent;
    public event Delegates.EmptyDelegate sprintEndedEvent;

    public bool isCrouching;
    public event Delegates.EmptyDelegate crouchStartedEvent;
    public event Delegates.EmptyDelegate crouchEndedEvent;

    public event Delegates.EmptyDelegate jumpStartedEvent;

    public bool isGrounded = true;
    private float airTime = 0;
    public delegate void LandedDelegate(float airTime);
    public event LandedDelegate landedEvent;
    private bool jumpOnCooldown = false;

    private float currentHeight = 1;
    private Vector3 inputDir;

    private List<Bouncer.BounceInstance> bounceInstances = new List<Bouncer.BounceInstance>();

    private Character character;
    private Rigidbody rb;

    // Dash 
    private bool isDashing;
    private Vector3 dashVector;

    public void Initialize(Character character) {
        this.character = character;
        rb = character.GetComponent<Rigidbody>();
        character.updateEvent += Character_updateEvent;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;
        character.characterInput.action_jump.keyDownEvent += Action_jump_keyDownEvent;
        //character.characterInput.action_sprint.keyDownEvent += Action_sprint_keyDownEvent;
        //character.characterInput.action_sprint.keyUpEvent += Action_sprint_keyUpEvent;

        state_grounded.Init(this);
        state_inAir.Init(this);
        activeState = state_grounded;
        activeState.EnterState();
    }

    //private void Action_sprint_keyDownEvent() {
    //    isSprinting = true;
    //    sprintStartedEvent?.Invoke();
    //}

    //private void Action_sprint_keyUpEvent() {
    //    isSprinting = false;
    //    sprintEndedEvent?.Invoke();
    //}

    private void Character_updateEvent() {
        inputDir = character.transform.TransformDirection(character.characterInput.moveInput);

        if (character.characterInput.action_crouch.isDown) {
            if (!isCrouching) {
                isCrouching = true;
                crouchStartedEvent?.Invoke();
            }
        }
        else {
            if (isCrouching) {
                isCrouching = false;
                crouchEndedEvent?.Invoke();
            }
        }

        character.tRig.localPosition = -0.75f * Vector3.up;
        foreach (var instance in bounceInstances) {
            character.tRig.position += instance.offset;
        }
    }

    private void Character_fixedUpdateEvent() {
        HeightCheck();

        //if (wallrunController.isWallRunning)
        //    airTime = 0;
            
        ////if (!isGrounded) {
        ////    rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
        ////    rb.AddForce(Vector3.down * 9.81f * settings.airTimeToGravityScale.Evaluate(airTime), ForceMode.Acceleration);
        ////}
        ////else {
        ////    rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
        ////}

        //if (wallrunController.isWallRunning) {
        //    character.rb.velocity = wallrunController.runVelocity;
        //}
        //else {
        //    HorizontalMovement();
        //}

        //VerticalMovement();

        rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);

        Quaternion targetRotation;
        if (wallrunController.isWallRunning)
            targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-wallrunController.wallHit.normal, Vector3.up).normalized, Vector3.up);
        else
            targetRotation = Quaternion.Euler(0, character.fpCamera.yaw, 0);

        tTargetRoation.rotation = targetRotation;
    }

    private void HeightCheck() {
        RaycastHit downHit;
        if (Physics.Raycast(character.transform.position, Vector3.down, out downHit, 100, LayerMasks.i.environment)) {
            currentHeight = downHit.distance;
        }
        else {
            currentHeight = 100;
        }
    }

    private void EnterState_inAir() {
        activeState.ExitState();
        state_inAir.EnterState();
        activeState = state_inAir;        
    }

    private void EnterState_grounded() {
        activeState.ExitState();
        state_grounded.EnterState();
        activeState = state_grounded;
    }

    private void Action_jump_keyDownEvent() {

        //if (!jumpOnCooldown && isGrounded) {
        //    state_grounded.JumpKeyPressed();
        //}
        //else if (wallrunController.isWallRunning) {
        //    JumpFromWallClimb();
        //}
    }

    private void VerticalMovement() {
        RaycastHit downHit;
        if (Physics.Raycast(character.transform.position, Vector3.down, out downHit, 100, LayerMasks.i.environment)) {
            currentHeight = downHit.distance;
        }

        float finalTargetHeight = settings.targetHeight;
        if (isCrouching)
            finalTargetHeight = settings.crouchTargetHeight;

        float deltaHeight = currentHeight - finalTargetHeight;

        if (deltaHeight < 0 && !jumpOnCooldown) {
            isGrounded = true;
            float yVelTarget = (-deltaHeight) * settings.bounceUpSpeed;
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(rb.velocity.x, yVelTarget, rb.velocity.z), settings.yVelLerpSpeed * Time.deltaTime);
        }
        else {
            isGrounded = false;
        }
    }

    private void JumpFromWallClimb() {
        wallrunController.StopWallRun();

        Vector3 lookDir = Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, wallrunController.wallUpVector).normalized;
        float lookDirToCameraAngle = Vector3.SignedAngle(lookDir, wallrunController.wallHit.normal, wallrunController.wallUpVector);

        // Get jump vector by rotating wall normal with camera to wall angle
        Vector3 jumpVector = Quaternion.AngleAxis(-lookDirToCameraAngle, Vector3.up) * wallrunController.wallHit.normal;

        // If character has move input
        if (character.characterInput.moveInput.magnitude > 0.5f) {
            float wasdAngle = Vector3.SignedAngle(new Vector3(0, 0, 1), character.characterInput.moveInput, Vector3.up);
            jumpVector = Quaternion.AngleAxis(wasdAngle, Vector3.up) * jumpVector;
        }

        // Stops jump vector from goin into wall, also add some velocity away from wall
        if (Vector3.Dot(jumpVector, wallrunController.wallHit.normal) < 0)
            jumpVector = Vector3.ProjectOnPlane(jumpVector, wallrunController.wallHit.normal) + wallrunController.wallHit.normal * 0.2f;

        character.rb.velocity = jumpVector * settings.wallJumpVelocity + Vector3.up * character.rb.velocity.y;
    }

    public void Dash(float time, Vector3 dashVector) {
        this.dashVector = dashVector;
        character.StartCoroutine(DashCorutine(time));
    }

    private IEnumerator DashCorutine(float time) {
        float defaultMoveSpeed = settings.moveSpeed;
        float defaultAcceleration = settings.moveAcceleration;
        isDashing = true;

        settings.moveSpeed *= 3;
        settings.moveAcceleration *= 2;
        yield return new WaitForSeconds(time);

        settings.moveAcceleration = defaultAcceleration;
        settings.moveSpeed = defaultMoveSpeed;
        isDashing = false;
    }

    //public enum LocomotionState { grounded, wallClimbing, wallGrabing, jumping }
    //public LocomotionState state;
    [System.Serializable]
    public class LocomotionState {
        protected Locomotion locomotion;

        public virtual void Init(Locomotion locomotion) {
            this.locomotion = locomotion;
        }

        public virtual void EnterState() {
        }

        public virtual void ExitState() {
        }
    }

    [System.Serializable]
    public class LocomotionState_grounded : LocomotionState {

        public bool isSprinting;
        private bool isPreparingJump = false;
        private float timeSinceGroundTouched; // Grace period, stops character from exiting ground state for something like going over a bump

        public override void EnterState() {
            base.EnterState();

            if (locomotion.character.characterInput.action_sprint.isDown)
                SprintStarted();

            locomotion.character.fixedUpdateEvent += Character_fixedUpdateEvent;
            locomotion.character.characterInput.action_jump.keyDownEvent += Action_jump_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyDownEvent += Action_sprint_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyUpEvent += Action_sprint_keyUpEvent;
        }

        public override void ExitState() {
            base.ExitState();

            if (isSprinting)
                SprintEnded();

            locomotion.character.fixedUpdateEvent -= Character_fixedUpdateEvent;
            locomotion.character.characterInput.action_jump.keyDownEvent -= Action_jump_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyDownEvent -= Action_sprint_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyUpEvent -= Action_sprint_keyUpEvent;
        }

        private void Character_fixedUpdateEvent() {
            StillOnGroundCheck();
            VerticalMovement();
            HorizontalMovement();
        }

        private void StillOnGroundCheck() {
            if (locomotion.currentHeight > locomotion.settings.targetHeight + 0.1f)
                timeSinceGroundTouched += Time.fixedDeltaTime;
            else
                timeSinceGroundTouched = 0;

            // If time above ground is above the grace period time, then enter inAir state
            if (timeSinceGroundTouched > 0.15f)
                locomotion.EnterState_inAir();

        }

        private void VerticalMovement() {

            // Return if jumping, don't want to intefere with jump stuff
            if (locomotion.jumpOnCooldown)
                return;

            float finalTargetHeight = locomotion.settings.targetHeight;
            //if (isCrouching)
            //    finalTargetHeight = settings.crouchTargetHeight;

            float deltaHeight = locomotion.currentHeight - finalTargetHeight;

            if (deltaHeight < 0) {
                float yVelTarget = (-deltaHeight) * locomotion.settings.bounceUpSpeed;
                locomotion.rb.velocity = Vector3.Lerp(locomotion.rb.velocity, new Vector3(locomotion.rb.velocity.x, yVelTarget, locomotion.rb.velocity.z), locomotion.settings.yVelLerpSpeed * Time.deltaTime);
            }

            //if (deltaHeight < 0 && !locomotion.jumpOnCooldown) {
            //    isGrounded = true;
            //    float yVelTarget = (-deltaHeight) * settings.bounceUpSpeed;
            //    rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(rb.velocity.x, yVelTarget, rb.velocity.z), settings.yVelLerpSpeed * Time.deltaTime);
            //}
            //else {
            //    isGrounded = false;
            //}
        }

        private void HorizontalMovement() {

            // Get forward part of inputDir
            Vector3 inputDir_forward = Vector3.zero;
            if (locomotion.character.characterInput.moveInput.z > 0)
                inputDir_forward = Vector3.Project(locomotion.inputDir, locomotion.character.transform.forward);

            // Get strafe part of input dir my subtracting forward part
            Vector3 inputDir_strafe = locomotion.inputDir - inputDir_forward;

            // Create move vector
            Vector3 moveVector = Vector3.zero;
            if (isSprinting) {
                moveVector += inputDir_forward * locomotion.settings.sprintSpeed;
                moveVector += inputDir_strafe * locomotion.settings.sprintStrafeSpeed;
            }
            else {
                moveVector += inputDir_forward * locomotion.settings.moveSpeed;
                moveVector += inputDir_strafe * locomotion.settings.strafeSpeed;
            }

            // Lerp character velocity towards new target velocity
            locomotion.rb.velocity = Vector3.Lerp(locomotion.rb.velocity, moveVector + Vector3.up * locomotion.rb.velocity.y, locomotion.settings.moveAcceleration * Time.deltaTime);
        }

        #region Sprint
        private void Action_sprint_keyDownEvent() {
            SprintStarted();
        }
        private void Action_sprint_keyUpEvent() {
            SprintEnded();
        }

        private void SprintStarted() {
            isSprinting = true;
            locomotion.sprintStartedEvent?.Invoke();
        }

        private void SprintEnded() {
            isSprinting = false;
            locomotion.sprintEndedEvent?.Invoke();
        }
        #endregion

        #region Jump
        private void Action_jump_keyDownEvent() {

            // Don't jump again before you have jumped
            if (isPreparingJump)
                return;

            Debug.Log("Jump pressed");
            locomotion.bounceInstances.Add(new Bouncer.BounceInstance(OnBounceFinished, locomotion.settings.bounceDownCurve, Vector3.down, 0.3f, 0.15f));
            locomotion.jumpStartedEvent?.Invoke();
            isPreparingJump = true;
        }

        private void OnBounceFinished(Bouncer.BounceInstance bounceInstance) {
            locomotion.bounceInstances.Remove(bounceInstance);
            isPreparingJump = false;
            Debug.Log("Bounced");
            Jump();
        }

        private void Jump() {
            locomotion.rb.velocity = new Vector3(locomotion.rb.velocity.x, locomotion.settings.jumpVelocity, locomotion.rb.velocity.z);
            locomotion.jumpOnCooldown = true;

            Debug.Log("Jump!");

            Utils.DelayedFunctionCall(JumpCooldownDone, 0.3f); // Jump cooldown
        }

        private void JumpCooldownDone() {
            Debug.Log("Jump off cooldown");
            locomotion.jumpOnCooldown = false;
        }

        #endregion
    }

    [System.Serializable]
    public class LocomotionState_inAir : LocomotionState {
        public float airTime;

        public override void EnterState() {
            base.EnterState();
            locomotion.character.fixedUpdateEvent += Character_fixedUpdateEvent;
        }

        public override void ExitState() {
            base.ExitState();
            locomotion.character.fixedUpdateEvent -= Character_fixedUpdateEvent;
            airTime = 0f;
        }

        private void Character_fixedUpdateEvent() {
            Debug.Log("In AIR!");
            StillInAirCheck();

            airTime += Time.deltaTime;
        }

        private void StillInAirCheck() {
            if (locomotion.currentHeight < locomotion.settings.targetHeight)
                locomotion.EnterState_grounded();
        }

        //private void v() {
        //    if (isGrounded) {
        //        if (airTime > 0.2f)
        //            landedEvent?.Invoke(airTime);
        //        airTime = 0;
        //    }
        //    else {
        //        airTime += Time.deltaTime;
        //    }
        //}
    }
}
