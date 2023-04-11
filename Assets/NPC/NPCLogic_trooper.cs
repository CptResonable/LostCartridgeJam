using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class NPCLogic_trooper : NPCLogic {

    private Vector3 targetPoint;
    private Vector3 moveDir;

    private bool tarrgetInSight;
    private Vector3 lastTargetSpottedPosition;
    private Vector3 toTargetVector;

    public LogicState_randomPatrol state_randomPatrol;
    public LogicState_fighting state_fighting;
    [HideInInspector] public LogicState activeState;

    public delegate void StateChangedDelegate(LogicState newState);
    public event StateChangedDelegate stateChangedEvent;

    protected override void Awake() {
        base.Awake();

        state_randomPatrol.Init(this);
        state_fighting.Init(this);
        activeState = state_randomPatrol;
        activeState.EnterState();
    }

    public override void UpdateInput(CharacterInput input) {

        if (target != null)
            toTargetVector = VectorUtils.FromToVector(transform.position, target.transform.position);

        base.UpdateInput(input);
        this.input = input;

        LookForTarget();
    }

    private bool LookForTarget() {
        if (!Physics.Linecast(character.body.tHead.position, GameManager.i.player.body.tHead.position, LayerMasks.i.environment)) {
            Vector3 headToTargetHead = VectorUtils.FromToVector(character.body.tHead.position, GameManager.i.player.body.tHead.position);

            if (Vector3.Angle(character.fpCamera.tCameraTarget.forward, headToTargetHead) < 60) {
                TargetInSight(GameManager.i.player);
                return true;
            }
        }

        return false;
    }

    public bool CheckLOS(Vector3 fromPoint, Vector3 toPoint) {
        if (Physics.Linecast(fromPoint, toPoint, LayerMasks.i.environment))
            return false;
        else
            return true;
    }

    private void TargetInSight(Character character) {
        Debug.Log("ISEEYOU");
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
        int maxTries = 10;
        NavMeshHit navMeshHit;

        for (int tries = 0; tries < maxTries; tries++) {
            Vector2 randDir = VectorUtils.RandomUnitVector();

            if (NavMesh.SamplePosition(origin + new Vector3(randDir.x, 0, randDir.y) * radius, out navMeshHit, 1, NavMesh.AllAreas)) {
                targetPoint = navMeshHit.position;
                break;
            }
        }
    }

    private void EnterState_fighting() {
        activeState.ExitState();
        state_fighting.EnterState();
        activeState = state_fighting;
        stateChangedEvent?.Invoke(activeState);

        Debug.Log("New state: FIGHTING");
    }

    [System.Serializable]
    public class LogicState {
        protected NPCLogic_trooper logic;

        public enum StateIDEnum { RandomPatrol, Fighting }
        [HideInInspector] public StateIDEnum stateID;

        public virtual void Init(NPCLogic_trooper logic) {
            this.logic = logic;
        }

        public virtual void EnterState() {
        }

        public virtual void ExitState() {
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
            logic.updateInputEvent += Logic_updateInputEvent;
            FindNewTargetPosition();
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

        private void Rotation() {
            Vector3 lookFlatDirection = Vector3.ProjectOnPlane(logic.character.fpCamera.tCameraTarget.forward, Vector3.up);
            float dAngle = Vector3.SignedAngle(lookFlatDirection, logic.navMeshAgent.desiredVelocity.normalized, Vector3.up);
            logic.input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;

            logic.input.mouseMovement.yDelta = logic.character.fpCamera.pitch / Settings.MOUSE_SENSITIVITY;
        }

        private void TargetReached() {
            targetReached = true;
            Utils.DelayedFunctionCall(FindNewTargetPosition, 3);
        }

        private void FindNewTargetPosition() {
            logic.FindNewTargetPosition(logic.transform.position, Random.Range(2f, 20f));
            targetReached = false;
        }
    }

    [System.Serializable]
    public class LogicState_fighting : LogicState {

        private bool targetReached = false;
        private bool isBursting;
        private bool burstOnCooldown;

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
            logic.tarrgetInSight = logic.LookForTarget();

            LookAtTarget();

            if (!isBursting && !gun.bulletInChaimber && gun.bulletsInMagCount == 0 && !gun.isReloading)
                logic.input.action_reload.Click();


            //if (logic.tarrgetInSight && !isBursting && !burstOnCooldown)
            //    logic.StartCoroutine(BurstCorutine());

            //if (targetReached)
            //    return;

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
            yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
            isBursting = true;
            float burstDuration = Random.Range(0.2f, 1.5f);
            logic.input.action_attack.isDown = true;
            yield return new WaitForSeconds(burstDuration);
            logic.input.action_attack.isDown = false;
            isBursting = false;
            float burstCooldown = Random.Range(0.3f, 1.2f) * burstDuration;
            yield return new WaitForSeconds(burstCooldown);
            burstOnCooldown = false;
        }

        private void LookAtTarget() {
            Vector3 lookFlatDirection = Vector3.ProjectOnPlane(logic.character.fpCamera.tCameraTarget.forward, Vector3.up);
            float dAngle = Vector3.SignedAngle(lookFlatDirection, logic.toTargetVector, Vector3.up);
            logic.input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;

            logic.input.mouseMovement.yDelta = logic.character.fpCamera.pitch / Settings.MOUSE_SENSITIVITY;
        }

        //private void Rotation() {
        //    Vector3 lookFlatDirection = Vector3.ProjectOnPlane(logic.character.fpCamera.tCameraTarget.forward, Vector3.up);
        //    float dAngle = Vector3.SignedAngle(lookFlatDirection, logic.navMeshAgent.desiredVelocity.normalized, Vector3.up);
        //    logic.input.mouseMovement.xDelta = (Mathf.Sign(dAngle) * Mathf.Sqrt(Mathf.Abs(dAngle)) * 0.2f) / Settings.MOUSE_SENSITIVITY;

        //    logic.input.mouseMovement.yDelta = logic.character.fpCamera.pitch / Settings.MOUSE_SENSITIVITY;
        //}

        //private void TargetReached() {
        //    targetReached = true;
        //    Utils.DelayedFunctionCall(FindNewTargetPosition, 3);
        //}

        //private void FindNewTargetPosition() {
        //    logic.FindNewTargetPosition(logic.transform.position, Random.Range(2f, 20f));
        //    targetReached = false;
        //}
    }
}
