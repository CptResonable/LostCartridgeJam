using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Locomotion {
    [SerializeField] private LocomotionSettings settings;
    [SerializeField] public WallrunController wallrunController;

    [SerializeField] private Transform tTargetRoation;

    [SerializeField] private float handStuckCorrectionForce;

    public LocomotionState_grounded state_grounded;
    public LocomotionState_inAir state_inAir;
    public LocomotionState_wallClimbing state_wallClimbing;
    public LocomotionState_wallRunning state_wallRunning;
    [HideInInspector] public LocomotionState activeState;

    private List<Bouncer.BounceInstance> bounceInstances = new List<Bouncer.BounceInstance>();

    private Character character;
    private Rigidbody rb;

    public RaycastHit downHit;
    private float currentHeight = 1;
    private Vector3 inputDir;
    private Vector3 localVelocity;

    private Vector3 meshHandToPhysicalHandError;

    public event Delegates.EmptyDelegate sprintStartedEvent;
    public event Delegates.EmptyDelegate sprintEndedEvent;

    public event Delegates.EmptyDelegate crouchStartedEvent;
    public event Delegates.EmptyDelegate crouchEndedEvent;

    public event Delegates.EmptyDelegate slideStartedEvent;
    public event Delegates.EmptyDelegate slideEndedEvent;

    public event Delegates.EmptyDelegate jumpStartedEvent;

    public delegate void LandedDelegate(float airTime);
    public event LandedDelegate landedEvent;


    public void Initialize(Character character) {
        this.character = character;
        rb = character.GetComponent<Rigidbody>();
        character.updateEvent += Character_updateEvent;
        character.fixedUpdateEvent += Character_fixedUpdateEvent;

        state_grounded.Init(this);
        state_inAir.Init(this);
        state_wallClimbing.Init(this);
        state_wallRunning.Init(this);
        activeState = state_grounded;
        activeState.EnterState();
    }

    private void Character_updateEvent() {

        // Get local velocity
        localVelocity = character.transform.InverseTransformVector(rb.velocity);

        // Convert movement input into world space vector
        inputDir = character.transform.TransformDirection(character.characterInput.moveInput);

        if (meshHandToPhysicalHandError.magnitude > 0.03f) {
            if (Vector3.Dot(inputDir, meshHandToPhysicalHandError.normalized) < 0)
                inputDir -= Vector3.Project(inputDir, meshHandToPhysicalHandError.normalized);
        }

        // Set rig base height (Default model is not at the correct height)
        character.tRig.localPosition = -0.75f * Vector3.up;

        // Bounces the model, now only used just before character jumps
        foreach (var instance in bounceInstances) {
            character.tRig.position += instance.offset;
        }

        //GizmoManager.i.DrawSphere(Time.deltaTime, Color.red, character.transform.TransformPoint(rb.centerOfMass), 0.2f);
    }

    private void Character_fixedUpdateEvent() {

        meshHandToPhysicalHandError = VectorUtils.FromToVector(character.body.tHandR.position, character.arms.hand_R.transform.position);
        GizmoManager.i.DrawSphere(Time.deltaTime, Color.green, character.body.tHandR.position, 0.05f);
        GizmoManager.i.DrawSphere(Time.deltaTime, Color.red, character.arms.hand_R.transform.position, 0.05f);

        if (meshHandToPhysicalHandError.magnitude > 0.1) {
            rb.velocity += meshHandToPhysicalHandError * handStuckCorrectionForce;
        }

        HeightCheck();
    }

    private void HeightCheck() {
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

        Debug.Log("New state: AIR");
    }

    private void EnterState_grounded() {

        // Invoke landed event
        if (activeState.stateID == LocomotionState.StateIDEnum.InAir)
            landedEvent?.Invoke(state_inAir.airTime);

        activeState.ExitState();
        state_grounded.EnterState();
        activeState = state_grounded;

        Debug.Log("New state: GROUND");
    }

    private void EnterState_wallClimbing() {
        activeState.ExitState();
        state_wallClimbing.EnterState();
        activeState = state_wallClimbing;

        Debug.Log("New state: Wall climb");
    }

    private void EnterState_wallRunning() {
        activeState.ExitState();
        state_wallRunning.EnterState();
        activeState = state_wallRunning;

        Debug.Log("New state: Wall running");
    }

    [System.Serializable]
    public class LocomotionState {
        public enum StateIDEnum { Grounded, InAir, WallClimbing, WallRunning }
        [HideInInspector] public StateIDEnum stateID;

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
        private UpperBody.UpperBodyRotationModifier upperBodyRotationModifier;

        public bool isSprinting;
        public bool isCrouching;
        public bool isSliding;

        private TWrapper crouchBodyRotationInterpolator = new TWrapper(0, 1, 0);
        private Coroutine crouchInterpolationCorutine;

        private bool isPreparingJump;
        private bool jumpOnCooldown;
        private float timeSinceGroundTouched; // Grace period, stops character from exiting ground state for something like going over a bump

        private float deltaHeight;

        public override void Init(Locomotion locomotion) {
            base.Init(locomotion);
            stateID = StateIDEnum.Grounded;

            upperBodyRotationModifier = new UpperBody.UpperBodyRotationModifier(Vector3.zero, Vector3.zero);
            Debug.Log(locomotion.character.upperBody);
            locomotion.character.upperBody.AddModifier(upperBodyRotationModifier);
        }

        public override void EnterState() {
            base.EnterState();

            if (locomotion.character.characterInput.action_sprint.isDown)
                SprintStarted();

            if (locomotion.character.characterInput.action_crouch.isDown) {
                if (locomotion.localVelocity.z > 2.5f)
                    StartSlide();
                else
                    StartCrouch();
            }

            locomotion.character.fixedUpdateEvent += Character_fixedUpdateEvent;
            locomotion.character.characterInput.action_jump.keyDownEvent += Action_jump_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyDownEvent += Action_sprint_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyUpEvent += Action_sprint_keyUpEvent;
            locomotion.character.characterInput.action_crouch.keyDownEvent += Action_crouch_keyDownEvent;
            locomotion.character.characterInput.action_crouch.keyUpEvent += Action_crouch_keyUpEvent;
        }

        public override void ExitState() {
            base.ExitState();

            if (isSprinting)
                SprintEnded();

            if (isCrouching)
                StopCrouch();

            locomotion.character.fixedUpdateEvent -= Character_fixedUpdateEvent;
            locomotion.character.characterInput.action_jump.keyDownEvent -= Action_jump_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyDownEvent -= Action_sprint_keyDownEvent;
            locomotion.character.characterInput.action_sprint.keyUpEvent -= Action_sprint_keyUpEvent;
            locomotion.character.characterInput.action_crouch.keyDownEvent -= Action_crouch_keyDownEvent;
            locomotion.character.characterInput.action_crouch.keyUpEvent -= Action_crouch_keyUpEvent;
        }

        private void Character_fixedUpdateEvent() {
            StillOnGroundCheck();
            VerticalMovement();

            if (isSliding)
                SlideUpdate();
            else
                HorizontalMovement();

            SetTargetRotation();
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

        private void SetTargetRotation() {
            if (!isSliding)
                locomotion.tTargetRoation.rotation = Quaternion.Euler(0, locomotion.character.fpCamera.yaw, 0);
        }

        private void VerticalMovement() {

            // Return if jumping, don't want to intefere with jump stuff
            if (jumpOnCooldown)
                return;

            float finalTargetHeight = locomotion.settings.targetHeight;
            if (isCrouching)
                finalTargetHeight = locomotion.settings.crouchTargetHeight;

            deltaHeight = locomotion.currentHeight - finalTargetHeight;

            if (deltaHeight < 0) {
                float yVelTarget = (-deltaHeight) * locomotion.settings.bounceUpSpeed;
                locomotion.rb.velocity = Vector3.Lerp(locomotion.rb.velocity, new Vector3(locomotion.rb.velocity.x, yVelTarget, locomotion.rb.velocity.z), locomotion.settings.yVelLerpSpeed * Time.deltaTime);
            }
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

            // Project vector onto ground
            moveVector = Vector3.ProjectOnPlane(moveVector, locomotion.downHit.normal);

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

            // End slide
            if (isSliding)
                StopSlide();

            // Don't jump again before you have jumped
            if (isPreparingJump)
                return;

            locomotion.bounceInstances.Add(new Bouncer.BounceInstance(OnBounceFinished, locomotion.settings.bounceDownCurve, Vector3.down, 0.3f, 0.15f));
            locomotion.jumpStartedEvent?.Invoke();
            isPreparingJump = true;
        }

        private void OnBounceFinished(Bouncer.BounceInstance bounceInstance) {
            locomotion.bounceInstances.Remove(bounceInstance);
            isPreparingJump = false;
            Jump();
        }

        private void Jump() {
            locomotion.rb.velocity = new Vector3(locomotion.rb.velocity.x, locomotion.settings.jumpVelocity, locomotion.rb.velocity.z);
            jumpOnCooldown = true;

            Utils.DelayedFunctionCall(JumpCooldownDone, 0.3f); // Jump cooldown

            locomotion.EnterState_inAir();
        }

        private void JumpCooldownDone() {
            jumpOnCooldown = false;
        }

        #endregion

        #region Crouch/Slide
        private void Action_crouch_keyDownEvent() {
            if (locomotion.localVelocity.z > 4f)
                StartSlide();
            else if (!isCrouching)
                StartCrouch();
        }

        private void Action_crouch_keyUpEvent() {
            if (isSliding)
                StopSlide();

            if (isCrouching)
                StopCrouch();
        }

        private void StartCrouch() {
            if (isSprinting)
                SprintEnded();

            isCrouching = true;

            if (crouchInterpolationCorutine != null)
                locomotion.character.StopCoroutine(crouchInterpolationCorutine);

            crouchInterpolationCorutine = locomotion.character.StartCoroutine(InterpolationUtils.i.SmoothStepUpdateCallback(crouchBodyRotationInterpolator.t, 1, 4, crouchBodyRotationInterpolator, CrouchInterpolationUpdateCallback));
        }

        private void StopCrouch() {
            isCrouching = false;

            if (crouchInterpolationCorutine != null)
                locomotion.character.StopCoroutine(crouchInterpolationCorutine);

            crouchInterpolationCorutine = locomotion.character.StartCoroutine(InterpolationUtils.i.SmoothStepUpdateCallback(crouchBodyRotationInterpolator.t, 0, 4, crouchBodyRotationInterpolator, CrouchInterpolationUpdateCallback));
        }

        private void StartSlide() {
            if (isSprinting)
                SprintEnded();

            isSliding = true;

            Vector3 velocityProjectedOnGround = Vector3.ProjectOnPlane(locomotion.rb.velocity, locomotion.downHit.normal);
            locomotion.rb.velocity += velocityProjectedOnGround * 1;

            locomotion.slideStartedEvent?.Invoke();
        }

        private void StopSlide() {
            isSliding = false;
            locomotion.slideEndedEvent?.Invoke();
        }

        private void SlideUpdate() {

            Debug.Log("dh: " + deltaHeight);
            if (deltaHeight < 0) {
                locomotion.rb.velocity = Vector3.ProjectOnPlane(locomotion.rb.velocity, -locomotion.downHit.normal);
                //if (Vector3.Dot(locomotion.rb.velocity, locomotion.downHit.normal) < 0) {
                //    locomotion.rb.velocity += Vector3.Project(locomotion.rb.velocity, locomotion.downHit.normal);
                //}
            }

            Vector3 velocityProjectedOnGround = Vector3.ProjectOnPlane(locomotion.rb.velocity, locomotion.downHit.normal);
            locomotion.rb.velocity -= velocityProjectedOnGround.normalized * 0.15f;

            float slopeAngle = Vector3.Angle(Vector3.up, locomotion.downHit.normal);
            if (slopeAngle > 0) {

                Vector3 slopeDownVector = VectorUtils.RotateVectorAroundVector(locomotion.downHit.normal, Vector3.Cross(Vector3.up, locomotion.downHit.normal), 90).normalized;
                locomotion.rb.velocity += slopeDownVector * locomotion.settings.slideAngleToAccelerationCurve.Evaluate(slopeAngle) * locomotion.settings.slideMaxSlopeAcceleration;
                //locomotion.rb.velocity += slopeDownVector * 0.05f * Mathf.Sqrt(slopeAngle);
            }

            if (velocityProjectedOnGround.magnitude < 1.5f)
                StopSlide();
            //locomotion.downHit.normal
        }

        //private void SlideUpdate() {

        //    Vector3 velocityProjectedOnGround = Vector3.ProjectOnPlane(locomotion.rb.velocity, locomotion.downHit.normal);
        //    locomotion.rb.velocity -= velocityProjectedOnGround.normalized * 0.15f;
        //    Debug.Log(velocityProjectedOnGround.magnitude);
        //    if (velocityProjectedOnGround.magnitude < 1.5f)
        //        StopSlide();
        //    //locomotion.downHit.normal
        //}


        //private void SlideUpdate() {
        //    float frictionCoef = 0.8f;

        //    Vector3 velocityProjectedOnGround = Vector3.ProjectOnPlane(locomotion.rb.velocity, locomotion.downHit.normal);
        //    Vector3 rightAxis = Vector3.Cross(velocityProjectedOnGround.normalized, locomotion.downHit.normal);
        //    float slopeAngle = Vector3.SignedAngle(Vector3.up, locomotion.downHit.normal, rightAxis);
        //    Debug.Log("angle: " + slopeAngle);
        //    Debug.Log(Mathf.Cos(slopeAngle * Mathf.Deg2Rad));
        //    float normalForce = 50 * 9.81f * Mathf.Cos(slopeAngle * Mathf.Deg2Rad);
        //    float friction = frictionCoef * normalForce;

        //    locomotion.rb.AddForce(-velocityProjectedOnGround.normalized * friction);
        //    //locomotion.rb.velocity -= velocityProjectedOnGround.normalized * 0.15f;
        //    //Debug.Log(velocityProjectedOnGround.magnitude);
        //    if (velocityProjectedOnGround.magnitude < 1.5f)
        //        StopSlide();
        //    //locomotion.downHit.normal
        //}

        // Will be called from the crouch interpolation corutine
        private void CrouchInterpolationUpdateCallback() {

            // Set crouch body rotation adjustments
            upperBodyRotationModifier.UpdateBonusEulers(Vector3.Lerp(Vector3.zero, new Vector3(60, 0, 0), crouchBodyRotationInterpolator.t), Vector3.Lerp(Vector3.zero, new Vector3(-10, 0, 0), crouchBodyRotationInterpolator.t));
            //upperBodyRotationModifier.UpdateBonusEulers(Vector3.Lerp(Vector3.zero, new Vector3(0, 0, 0), crouchBodyRotationInterpolator.t), new Vector3(0, 0, 0));
            locomotion.character.tRig.localRotation = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(-20, 0, 0), crouchBodyRotationInterpolator.t);
        }

        #endregion
    }

    [System.Serializable]
    public class LocomotionState_inAir : LocomotionState {
        public float airTime;

        public override void Init(Locomotion locomotion) {
            base.Init(locomotion);
            stateID = StateIDEnum.InAir;
        }

        public override void EnterState() {
            base.EnterState();

            locomotion.character.updateEvent += Character_updateEvent;
            locomotion.character.fixedUpdateEvent += Character_fixedUpdateEvent;
            locomotion.wallrunController.verticalRunStarted += WallrunController_verticalRunStarted;
            locomotion.wallrunController.horizontalRunStarted += WallrunController_horizontalRunStarted;
        }

        public override void ExitState() {
            base.ExitState();

            locomotion.character.updateEvent -= Character_updateEvent;
            locomotion.character.fixedUpdateEvent -= Character_fixedUpdateEvent;
            locomotion.wallrunController.verticalRunStarted -= WallrunController_verticalRunStarted;
            locomotion.wallrunController.horizontalRunStarted -= WallrunController_horizontalRunStarted;

            airTime = 0f;
        }

        private void Character_updateEvent() {

            // Check for wall to run if pressing jump key
            if (locomotion.character.characterInput.action_jump.isDown)
                locomotion.wallrunController.AttemptWallRun();
        }

        private void Character_fixedUpdateEvent() {
            StillInAirCheck();
            VerticalMovement();
            SetTargetRotation();

            airTime += Time.deltaTime;
        }

        private void StillInAirCheck() {
            if (locomotion.currentHeight < locomotion.settings.targetHeight && airTime > 0.1f)
                locomotion.EnterState_grounded();
        }

        private void SetTargetRotation() {
            locomotion.tTargetRoation.rotation = Quaternion.Euler(0, locomotion.character.fpCamera.yaw, 0);
        }

        private void VerticalMovement() {
            locomotion.rb.AddForce(Vector3.down * 9.81f * locomotion.settings.airTimeToGravityScale.Evaluate(airTime), ForceMode.Acceleration);
        }

        private void WallrunController_verticalRunStarted() {
            locomotion.EnterState_wallClimbing();
        }

        private void WallrunController_horizontalRunStarted() {
            locomotion.EnterState_wallRunning();
        }
    }

    [System.Serializable]
    public class LocomotionState_wallClimbing : LocomotionState {
        public override void Init(Locomotion locomotion) {
            base.Init(locomotion);
            stateID = StateIDEnum.WallClimbing;
        }

        public override void EnterState() {
            base.EnterState();
            locomotion.character.fixedUpdateEvent += Character_fixedUpdateEvent;
            locomotion.wallrunController.verticalRunStopped += WallrunController_verticalRunStopped;
            locomotion.character.characterInput.action_jump.keyDownEvent += Action_jump_keyDownEvent;
        }

        public override void ExitState() {
            base.ExitState();
            locomotion.character.fixedUpdateEvent -= Character_fixedUpdateEvent;
            locomotion.wallrunController.verticalRunStopped -= WallrunController_verticalRunStopped;
            locomotion.character.characterInput.action_jump.keyDownEvent -= Action_jump_keyDownEvent;
        }

        private void Character_fixedUpdateEvent() {
            locomotion.rb.velocity = locomotion.wallrunController.runVelocity;

            SetTargetRotation();
        }

        private void SetTargetRotation() {
            locomotion.tTargetRoation.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-locomotion.wallrunController.wallHit.normal, Vector3.up).normalized, Vector3.up);
        }

        private void WallrunController_verticalRunStopped() {
            locomotion.EnterState_inAir();
        }

        private void Action_jump_keyDownEvent() {
            locomotion.wallrunController.StopWallClimb();

            Vector3 lookDir = Vector3.ProjectOnPlane(locomotion.character.fpCamera.tCamera.forward, locomotion.wallrunController.wallUpVector).normalized;
            float lookDirToCameraAngle = Vector3.SignedAngle(lookDir, locomotion.wallrunController.wallHit.normal, locomotion.wallrunController.wallUpVector);

            // Get jump vector by rotating wall normal with camera to wall angle
            Vector3 jumpVector = Quaternion.AngleAxis(-lookDirToCameraAngle, Vector3.up) * locomotion.wallrunController.wallHit.normal;

            // If character has move input
            if (locomotion.character.characterInput.moveInput.magnitude > 0.5f) {
                float wasdAngle = Vector3.SignedAngle(new Vector3(0, 0, 1), locomotion.character.characterInput.moveInput, Vector3.up);
                jumpVector = Quaternion.AngleAxis(wasdAngle, Vector3.up) * jumpVector;
            }

            // Stops jump vector from goin into wall, also add some velocity away from wall
            if (Vector3.Dot(jumpVector, locomotion.wallrunController.wallHit.normal) < 0)
                jumpVector = Vector3.ProjectOnPlane(jumpVector, locomotion.wallrunController.wallHit.normal) + locomotion.wallrunController.wallHit.normal * 0.2f;

            locomotion.character.rb.velocity = jumpVector * locomotion.settings.wallJumpVelocity + Vector3.up * locomotion.character.rb.velocity.y;
        }
    }

    [System.Serializable]
    public class LocomotionState_wallRunning : LocomotionState {
        public override void Init(Locomotion locomotion) {
            base.Init(locomotion);
            stateID = StateIDEnum.WallRunning;
        }

        public override void EnterState() {
            base.EnterState();
            locomotion.character.fixedUpdateEvent += Character_fixedUpdateEvent;
            locomotion.wallrunController.horizontalRunStopped += WallrunController_horizontalRunStopped;
            locomotion.character.characterInput.action_jump.keyDownEvent += Action_jump_keyDownEvent;
        }

        public override void ExitState() {
            base.ExitState();
            locomotion.character.fixedUpdateEvent -= Character_fixedUpdateEvent;
            locomotion.wallrunController.horizontalRunStopped -= WallrunController_horizontalRunStopped;
            locomotion.character.characterInput.action_jump.keyDownEvent -= Action_jump_keyDownEvent;
        }

        private void Character_fixedUpdateEvent() {
            HorizontalMovement();

            SetTargetRotation();
        }

        private void SetTargetRotation() {
            //locomotion.tTargetRoation.rotation = Quaternion.LookRotation(locomotion.wallrunController.wallForwardVector, Vector3.up);
            locomotion.tTargetRoation.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(-locomotion.wallrunController.wallHit.normal, Vector3.up).normalized, Vector3.up);
        }

        private void WallrunController_horizontalRunStopped() {
            locomotion.EnterState_inAir();
            locomotion.rb.velocity += locomotion.wallrunController.wallHit.normal * 2f;
        }

        private void Action_jump_keyDownEvent() {
            locomotion.wallrunController.StopWallRun();

            Vector3 lookDir = Vector3.ProjectOnPlane(locomotion.character.fpCamera.tCamera.forward, locomotion.wallrunController.wallUpVector).normalized;
            float lookDirToCameraAngle = Vector3.SignedAngle(lookDir, locomotion.wallrunController.wallHit.normal, locomotion.wallrunController.wallUpVector);

            // Get jump vector by rotating wall normal with camera to wall angle
            Vector3 jumpVector = Quaternion.AngleAxis(-lookDirToCameraAngle, Vector3.up) * locomotion.wallrunController.wallHit.normal;

            // If character has move input
            if (locomotion.character.characterInput.moveInput.magnitude > 0.5f) {
                float wasdAngle = Vector3.SignedAngle(new Vector3(0, 0, 1), locomotion.character.characterInput.moveInput, Vector3.up);
                jumpVector = Quaternion.AngleAxis(wasdAngle, Vector3.up) * jumpVector;
            }

            // Stops jump vector from goin into wall, also add some velocity away from wall
            if (Vector3.Dot(jumpVector, locomotion.wallrunController.wallHit.normal) < 0)
                jumpVector = Vector3.ProjectOnPlane(jumpVector, locomotion.wallrunController.wallHit.normal) + locomotion.wallrunController.wallHit.normal * 0.2f;

            locomotion.character.rb.velocity = jumpVector * locomotion.settings.wallJumpVelocity + Vector3.up * locomotion.character.rb.velocity.y;
        }

        private void HorizontalMovement() {

            // Get forward part of inputDir
            Vector3 inputDir_forward = Vector3.zero;
            if (locomotion.character.characterInput.moveInput.z > 0)
                inputDir_forward = Vector3.Project(locomotion.inputDir, locomotion.character.transform.forward);

            // Get strafe part of input dir my subtracting forward part
            Vector3 inputDir_strafe = locomotion.inputDir - inputDir_forward;

            Vector3 moveVector = locomotion.wallrunController.wallForwardVector * locomotion.settings.sprintSpeed;

            // Lerp character velocity towards new target velocity
            locomotion.rb.velocity = Vector3.Lerp(locomotion.rb.velocity, moveVector + Vector3.up * locomotion.rb.velocity.y, locomotion.settings.moveAcceleration * Time.deltaTime);
        }
    }
}
