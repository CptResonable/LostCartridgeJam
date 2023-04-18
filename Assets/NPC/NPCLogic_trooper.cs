using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;


public class NPCLogic_trooper : NPCLogic {
    [SerializeField] private TMP_Text stateDebugText;

    private Vector3 targetPoint;
    private Vector3 moveDir;

    private bool targetInSight; // Is target in line of sight?
    private bool toTargetVectorBlocked; // Is the vector between this char and target blocked?
    private float toTargetVectorBlockedDuration; // For how long has vector been blocked?
    private Vector3 lastTargetSpottedPosition;
    private Vector3 toTargetVector;

    public LogicState_randomPatrol state_randomPatrol;
    public LogicState_fighting state_fighting;
    public LogicState_searching state_searching;
    [HideInInspector] public LogicState activeState;

    public delegate void StateChangedDelegate(LogicState newState);
    public event StateChangedDelegate stateChangedEvent;

    protected override void Awake() {
        base.Awake();

        Debug.Log("SDSDJKSDJKSDJKSDJK");

        state_randomPatrol.Init(this);
        state_fighting.Init(this);
        state_searching.Init(this);
        activeState = state_randomPatrol;
        activeState.EnterState();

        Destroy(stateDebugText.gameObject);
    }

    public override void UpdateInput(CharacterInput input) {

        if (Input.GetKeyDown(KeyCode.F7)) {
            Destroy(this);
            Destroy(stateDebugText.gameObject);
        }

        if (target != null) {
            toTargetVector = VectorUtils.FromToVector(transform.position, target.transform.position);
            toTargetVectorBlocked = !CheckLOS(character.body.tHead.position, target.body.tHead.position);

            if (toTargetVectorBlocked) {
                toTargetVectorBlockedDuration += Time.deltaTime;
            }
            else {
                lastTargetSpottedPosition = target.transform.position;
                toTargetVectorBlockedDuration = 0;
            }
        }
        else {
            toTargetVectorBlocked = false;
            toTargetVectorBlockedDuration = 0;
        }

        base.UpdateInput(input);
        this.input = input;

        LookForTarget();
    }

    private bool LookForTarget() {
        if (!Physics.Linecast(character.body.tHead.position, GameManager.i.player.body.tHead.position, LayerMasks.i.environment)) {
            Vector3 headToTargetHead = VectorUtils.FromToVector(character.body.tHead.position, GameManager.i.player.body.tHead.position);

            if (Vector3.Angle(character.fpCamera.tCamera.forward, headToTargetHead) < 60) {
                TargetInSight(GameManager.i.player);
                return true;
            }
        }

        return false;
    }

    /// <summary> Returns true if path is clear, false if blocked </summary>
    public bool CheckLOS(Vector3 fromPoint, Vector3 toPoint) {
        if (Physics.Linecast(fromPoint, toPoint, LayerMasks.i.environment))
            return false;
        else
            return true;
    }

    private void TargetInSight(Character character) {
        target = character;
        lastTargetSpottedPosition = character.transform.position;
        GizmoManager.i.DrawSphere(Time.deltaTime, Color.blue, character.body.tHead.position + Vector3.up * 0.3f, 0.5f);
    }

    private void FindNewTargetPosition() {
        int maxTries = 10;
        NavMeshHit navMeshHit;

        for (int tries = 0; tries < maxTries; tries++) {
            Vector2 randDir = VectorUtils.RandomUnitVector();
            float randRange = Random.Range(2f, 20f);

            //NavMeshPath path = new NavMeshPath();
            //path.
            if (NavMesh.SamplePosition(transform.position + new Vector3(randDir.x, 0, randDir.y) * randRange, out navMeshHit, 1, NavMesh.AllAreas)) {
                targetPoint = navMeshHit.position;
                break;
            }
        }
    }

    private void FindNewTargetPosition(Vector3 origin, float radius) {


        Debug.Log("waaaaaaaa");
        int maxTries = 10;
        NavMeshHit navMeshHit;

        bool success = false;

        for (int tries = 0; tries < maxTries; tries++) {
            Vector2 randDir = VectorUtils.RandomUnitVector();

            if (NavMesh.SamplePosition(origin + new Vector3(randDir.x, 0, randDir.y) * radius, out navMeshHit, 1, NavMesh.AllAreas)) {
                targetPoint = navMeshHit.position;
                success = true;
                break;
            }
        }

        if (!success)
            targetPoint = transform.position;
    }

    private void EnterState_fighting() {
        activeState.ExitState();
        state_fighting.EnterState();
        activeState = state_fighting;
        stateChangedEvent?.Invoke(activeState);

        //stateDebugText.text = "F";

        Debug.Log("New state: FIGHTING");
    }

    private void EnterState_seraching() {
        activeState.ExitState();
        state_searching.EnterState();
        activeState = state_searching;
        stateChangedEvent?.Invoke(activeState);

        //stateDebugText.text = "S";

        Debug.Log("New state: SEARCHING");
    }

    private void EnterState_patrol() {
        activeState.ExitState();
        state_randomPatrol.EnterState();
        activeState = state_randomPatrol;
        stateChangedEvent?.Invoke(activeState);

        //stateDebugText.text = "P";

        Debug.Log("New state: PATROL");
    }

    [System.Serializable]
    public class LogicState {
        protected NPCLogic_trooper logic;

        public enum StateIDEnum { RandomPatrol, Fighting, Searching }
        [HideInInspector] public StateIDEnum stateID;

        public virtual void Init(NPCLogic_trooper logic) {
            this.logic = logic;
        }

        public virtual void EnterState() {
            logic.character.health.bulletHitEvent += Health_bulletHitEvent;
        }

        public virtual void ExitState() {
            logic.character.health.bulletHitEvent -= Health_bulletHitEvent;
        }

        protected virtual void Health_bulletHitEvent(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
        }
    }

    [System.Serializable]
    public class LogicState_randomPatrol : LogicState {

        private bool targetReached = false;

        public override void Init(NPCLogic_trooper logic) {
            base.Init(logic);
            stateID = StateIDEnum.RandomPatrol;
        }

        public override void EnterState() {
            base.EnterState();
            FindNewTargetPosition();
            logic.updateInputEvent += Logic_updateInputEvent;
        }

        public override void ExitState() {
            base.ExitState();
            logic.updateInputEvent -= Logic_updateInputEvent;
        }

        private void Logic_updateInputEvent() {
            if (targetReached)
                return;

            logic.navMeshAgent.SetDestination(logic.targetPoint);

            logic.moveDir = logic.transform.InverseTransformVector(logic.navMeshAgent.desiredVelocity.normalized);
            Vector3 toTargetVector = VectorUtils.FromToVector(logic.input.transform.position, logic.targetPoint);

            logic.input.moveInput = logic.moveDir;
            if (toTargetVector.magnitude > 1) {
                logic.input.moveInput = logic.moveDir;
            }
            else {
                logic.input.moveInput = Vector3.zero;
                TargetReached();
            }

            Rotation();

            if (logic.LookForTarget()) {
                logic.EnterState_fighting();
            }

            logic.navMeshAgent.transform.position = logic.transform.position;
        }

        protected override void Health_bulletHitEvent(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
            base.Health_bulletHitEvent(damage, hitPoint, bulletPathVector);

            logic.lastTargetSpottedPosition = logic.transform.position - bulletPathVector * 3f;
            Utils.DelayedFunctionCall(logic.EnterState_seraching, 0.5f);
        }

        private void Rotation() {
            Vector3 lookFlatDirection = Vector3.ProjectOnPlane(logic.character.fpCamera.tCamera.forward, Vector3.up);
            float dAngle = Vector3.SignedAngle(lookFlatDirection, logic.navMeshAgent.desiredVelocity.normalized, Vector3.up);
            logic.input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;

            logic.input.mouseMovement.yDelta = logic.character.fpCamera.pitch / Settings.MOUSE_SENSITIVITY;
        }

        private void TargetReached() {
            targetReached = true;
            Utils.DelayedFunctionCall(FindNewTargetPosition, 3);
        }

        private void FindNewTargetPosition() {
            Debug.Log("Why? " + logic.transform.position);
            logic.FindNewTargetPosition(logic.transform.position, Random.Range(1f, 4));
            targetReached = false;
        }
    }

    [System.Serializable]
    public class LogicState_searching : LogicState {

        private bool targetReached = false;
        private Vector3 lastSearchVector;

        private int spotsChecked = 0;

        public override void Init(NPCLogic_trooper logic) {
            base.Init(logic);
            stateID = StateIDEnum.Searching;
        }

        public override void EnterState() {
            base.EnterState();
            logic.FindNewTargetPosition(logic.lastTargetSpottedPosition, 1);
            lastSearchVector = Vector3.ProjectOnPlane(VectorUtils.FromToVector(logic.transform.position, logic.targetPoint), Vector3.up);
            logic.updateInputEvent += Logic_updateInputEvent;
        }

        public override void ExitState() {
            base.ExitState();
            logic.updateInputEvent -= Logic_updateInputEvent;
        }

        private void Logic_updateInputEvent() {

            if (logic.LookForTarget()) {
                logic.EnterState_fighting();
            }
            else if (logic.toTargetVectorBlockedDuration > 30) {
                logic.EnterState_patrol();
            }

            if (targetReached)
                return;

            logic.navMeshAgent.SetDestination(logic.targetPoint);

            logic.moveDir = logic.transform.InverseTransformVector(logic.navMeshAgent.desiredVelocity.normalized);
            Vector3 toTargetVector = VectorUtils.FromToVector(logic.input.transform.position, logic.targetPoint);

            logic.input.moveInput = logic.moveDir;
            if (toTargetVector.magnitude > 1) {
                logic.input.moveInput = logic.moveDir;
            }
            else {
                logic.input.moveInput = Vector3.zero;
                TargetReached();
            }

            Rotation();

            logic.navMeshAgent.transform.position = logic.transform.position;
        }

        private void Rotation() {
            Vector3 lookFlatDirection = Vector3.ProjectOnPlane(logic.character.fpCamera.tCamera.forward, Vector3.up);
            float dAngle = Vector3.SignedAngle(lookFlatDirection, logic.navMeshAgent.desiredVelocity.normalized, Vector3.up);
            logic.input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;

            logic.input.mouseMovement.yDelta = logic.character.fpCamera.pitch / Settings.MOUSE_SENSITIVITY;
        }

        private void TargetReached() {
            targetReached = true;
            spotsChecked++;
            Utils.DelayedFunctionCall(FindNewTargetPosition, 1);
        }

        private void FindNewTargetPosition() {
            if (spotsChecked >= 3) {
                logic.EnterState_patrol();
                return;
            }

            logic.FindNewTargetPosition(logic.transform.position + lastSearchVector * 5, Random.Range(2f, 10f));
            lastSearchVector = Vector3.ProjectOnPlane(VectorUtils.FromToVector(logic.transform.position, logic.targetPoint), Vector3.up);
            targetReached = false;
        }
    }

    [System.Serializable]
    public class LogicState_fighting : LogicState {

        private bool targetReached = false;
        private bool isBursting;
        private bool burstOnCooldown;

        private float aimSwaySpeed = 2;
        private float aimSwayAmount_horizontal = 0.5f;
        private float aimSwayAmount_vertical = 0.3f;

        private Gun gun;

        public override void Init(NPCLogic_trooper logic) {
            base.Init(logic);
            stateID = StateIDEnum.Fighting;
        }

        public override void EnterState() {
            base.EnterState();
            logic.input.action_equipSlot2.Click();
            gun = (Gun)logic.character.equipmentManager.equipedItem;
            Utils.DelayedFunctionCall(FindNewTargetPosition, 1);
            logic.updateInputEvent += Logic_updateInputEvent;
        }

        public override void ExitState() {
            base.ExitState();
            logic.updateInputEvent -= Logic_updateInputEvent;
        }

        private void Logic_updateInputEvent() {
            logic.targetInSight = logic.LookForTarget();

            LookAtTarget();

            if (!isBursting && !gun.bulletInChaimber && gun.bulletsInMagCount == 0 && !gun.isReloading)
                logic.input.action_reload.Click();

            if (logic.targetInSight && !isBursting && !burstOnCooldown)
                logic.StartCoroutine(BurstCorutine());

            logic.navMeshAgent.SetDestination(logic.targetPoint);

            logic.moveDir = logic.transform.InverseTransformVector(logic.navMeshAgent.desiredVelocity.normalized);
            Vector3 toTargetVector = VectorUtils.FromToVector(logic.input.transform.position, logic.targetPoint);

            logic.input.moveInput = logic.moveDir;
            if (toTargetVector.magnitude > 1) {
                logic.input.moveInput = logic.moveDir;
            }
            else {
                logic.input.moveInput = Vector3.zero;

                if (!targetReached)
                    TargetReached();
            }


            if (logic.toTargetVectorBlockedDuration > 6) {
                logic.EnterState_seraching();
            }

            //Rotation();

            logic.navMeshAgent.transform.position = logic.transform.position;
        }

        private void TargetReached() {
            targetReached = true;
            Utils.DelayedFunctionCall(FindNewTargetPosition, 3);
        }

        private void FindNewTargetPosition() {

            if (!TryFindNewTargetPosition(logic.target.transform.position, Random.Range(2f, 10f)))
                Utils.DelayedFunctionCall(FindNewTargetPosition, 2); // Failed to find good position, try again in 2 sec
        }

        private bool TryFindNewTargetPosition(Vector3 origin, float radius) {
            int maxTries = 10;
            NavMeshHit navMeshHit;

            for (int tries = 0; tries < maxTries; tries++) {
                Vector2 randDir = VectorUtils.RandomUnitVector();

                if (NavMesh.SamplePosition(origin + new Vector3(randDir.x, 0, randDir.y) * radius, out navMeshHit, 1, NavMesh.AllAreas)) {

                    // Make sure new position has LOS to target
                    if (logic.CheckLOS(navMeshHit.position + Vector3.up * 1.75f, logic.target.body.tHead.position)) {

                        logic.targetPoint = navMeshHit.position;
                        Debug.Log("New position found!");
                        targetReached = false;
                        return true;
                    }
                }
            }

            return false;
        }


        private IEnumerator BurstCorutine() {
            Debug.Log("burstStart");

            logic.character.locomotion.aiSpeedMod = 0.5f;
            burstOnCooldown = true;

            yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
            isBursting = true;
            float burstDuration = Random.Range(0.2f, 1.5f);
            logic.input.action_attack.isDown = true;
            yield return new WaitForSeconds(burstDuration);
            logic.input.action_attack.isDown = false;
            isBursting = false;
            float burstCooldown = Random.Range(0.3f, 1.2f) * burstDuration;

            logic.character.locomotion.aiSpeedMod = 1f;

            yield return new WaitForSeconds(burstCooldown);
            burstOnCooldown = false;
        }

        private float smoothDX, smoothDY;

        private void LookAtTarget() {
            Vector3 lookFlatDirection = Vector3.ProjectOnPlane(logic.character.fpCamera.tCamera.forward, Vector3.up);
            float dAngle = Vector3.SignedAngle(lookFlatDirection, logic.toTargetVector, Vector3.up);

            float dX = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f);
            dX += Perlin.CustomFbm(Time.time + 324, 0.75f, aimSwayAmount_horizontal, 3, 2, 0.75f); // Aim sway
            smoothDX = Mathf.Lerp(smoothDX, dX, Time.deltaTime * 6);

            logic.input.mouseMovement.xDelta = smoothDX / Settings.MOUSE_SENSITIVITY;

            float dY;
            if (dAngle > 80) {
                dY = logic.character.fpCamera.pitch;
            }
            else {
                Vector3 gunProjVec = Vector3.ProjectOnPlane(gun.transform.up, Vector3.Cross(logic.toTargetVector.normalized, Vector3.up));
                float gunPitch = Vector3.SignedAngle(gunProjVec, logic.toTargetVector.normalized, Vector3.Cross(logic.toTargetVector.normalized, Vector3.up));
                dY = gunPitch * 0.05f;
            }

            dY += Perlin.CustomFbm(Time.time + 674, 0.75f, aimSwayAmount_vertical, 3, 2, 0.75f); // Aim sway
            dY -= 0.01f;
            smoothDY = Mathf.Lerp(smoothDY, dY, Time.deltaTime * 6);

            logic.input.mouseMovement.yDelta = smoothDY / Settings.MOUSE_SENSITIVITY;
        }

    }
}
