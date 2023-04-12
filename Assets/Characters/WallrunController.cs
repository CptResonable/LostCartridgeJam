using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunController : MonoBehaviour {
    [SerializeField] private WallrunSettings settings;

    public bool isWallRunning;
    //public bool isWallRunningVertical;
    //public bool isWallRunningHorizontal;
    private bool wallClimbOnCooldown;
    public Enums.Side wallRunSide;

    [HideInInspector] public Vector3 runVelocity;
    [HideInInspector] public float wallCameraAngle;
    [HideInInspector] public float wallForwardCameraAngle;

    private Character character;

    private bool wallDetected;
    public RaycastHit wallHit;
    public Vector3 wallUpVector; // Just (0, 1, 0) for now. TODO make it actually up vector;
    public Vector3 wallForwardVector;
    private Coroutine wallRunCorutine;

    private Vector3 smoothCharacterVelocity;
    private Vector3 smoothCharacterHorizontalVelocity;
    private float velocityWallAngle;

    [HideInInspector] public float t; // 0 - 1, wall run progress

    public List<Vector3> ledgeGrabPoints;
    public Vector3 topGrabPoint;

    public event Delegates.EmptyDelegate verticalRunStarted;
    public event Delegates.EmptyDelegate verticalRunStopped;

    public event Delegates.EmptyDelegate horizontalRunStarted;
    public event Delegates.EmptyDelegate horizontalRunStopped;

    private void Awake() {
        character = GetComponentInParent<Character>();
    }

    private void Update() {
        smoothCharacterVelocity = Vector3.Lerp(smoothCharacterVelocity, character.rb.velocity, Time.deltaTime * 8);
        smoothCharacterHorizontalVelocity = Vector3.ProjectOnPlane(smoothCharacterVelocity, Vector3.up);

        LookForEdge();
    }

    private void FixedUpdate() {
        wallDetected = false;
    }

    private void OnTriggerStay(Collider other) {
        if (isWallRunning) {
            if (Physics.Raycast(transform.position + Vector3.down * 0.4f, -wallHit.normal, out wallHit, 1, LayerMasks.i.wall)) {
                wallDetected = true;
            }
        }
        else if (Physics.Raycast(transform.position + Vector3.down * 0.4f, character.transform.forward, out wallHit, 2, LayerMasks.i.wall) || isWallRunning) {
            if (Physics.Raycast(transform.position + Vector3.down * 0.4f, -wallHit.normal, out wallHit, 1, LayerMasks.i.wall)) {
                wallDetected = true;
            }
        }
    }

    public void AttemptWallRun() {

        if (!wallDetected) // No wall to run/climb
            return;

        if (!isWallRunning && !wallClimbOnCooldown) {
            Vector3 cameraVelocityMiddleVector = Vector3.Slerp(Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, Vector3.up), smoothCharacterHorizontalVelocity, 0.5f);
            velocityWallAngle = Vector3.SignedAngle(-wallHit.normal, cameraVelocityMiddleVector, Vector3.up);
            if (Mathf.Abs(velocityWallAngle) < settings.maxAngleForWallClimb) {
                Vector3 proj = Vector3.Project(smoothCharacterVelocity, -wallHit.normal);
                if (proj.magnitude > settings.velocityNeededForWallClimb)
                    StartVerticalRun();
            }
            else if (Mathf.Abs(velocityWallAngle) < settings.maxAngleForWallRun) {
                if (smoothCharacterHorizontalVelocity.magnitude > settings.velocityNeededForWallRun)
                    StartHorizontalRun();
            }
        }
    }

    private void WallClimbCooldownDone() {
        wallClimbOnCooldown = false;
    }

    private void StartHorizontalRun() {

        float initVerticalVelocity = character.rb.velocity.y + Vector3.Project(character.rb.velocity, wallHit.normal).magnitude;

        // Cant start wall run if ill go downwards
        if (initVerticalVelocity < 0)
            return;

        initVerticalVelocity = Mathf.Sqrt(initVerticalVelocity) * 2;

        isWallRunning = true;
        wallRunCorutine = StartCoroutine(HorizontalRunCorutine(settings.horizontalRunDuration));
        wallUpVector = Vector3.up;
        wallForwardVector = Vector3.ProjectOnPlane(smoothCharacterHorizontalVelocity, wallHit.normal).normalized;

        //character.rb.velocity += Vector3.Project(character.rb.velocity, wallHit.normal).magnitude * Vector3.up;
        character.rb.velocity = new Vector3(character.rb.velocity.x, initVerticalVelocity, character.rb.velocity.z);

        if (Vector3.Angle(wallHit.normal, character.transform.right) < 90)
            wallRunSide = Enums.Side.left;
        else
            wallRunSide = Enums.Side.right;

        horizontalRunStarted?.Invoke();
    }

    private IEnumerator HorizontalRunCorutine(float duration) {

        //float scale = settings.yVelToVerticalRunScaleCurve.Evaluate(character.rb.velocity.y);
        //duration *= scale;
        float scale = 1;

        t = 0;

        while (t < 1) {

            // No longer any wall to run/climb
            if (!wallDetected)
                StopWallRun();

            wallCameraAngle = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, Vector3.up), Vector3.ProjectOnPlane(-wallHit.normal, Vector3.up), Vector3.up);

            Vector3 camForwardProj = Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, character.locomotion.wallrunController.wallUpVector).normalized;
            wallForwardCameraAngle = Vector3.SignedAngle(wallForwardVector, camForwardProj, character.locomotion.wallrunController.wallUpVector);
            yield return new WaitForFixedUpdate();
        }

        StopWallRun();
        horizontalRunStopped?.Invoke();
    }


    private void StartVerticalRun() {
        isWallRunning = true;
        wallRunCorutine = StartCoroutine(VerticalRunCorutine(settings.verticalRunDuration));
        wallUpVector = Vector3.up;
        verticalRunStarted?.Invoke();
    }

    public void StopWallClimb() {

        if (isWallRunning) {
            StopCoroutine(wallRunCorutine);
            isWallRunning = false;
            wallClimbOnCooldown = true;
            Utils.DelayedFunctionCall(WallClimbCooldownDone, 0.15f); // Wall climb cooldown
            verticalRunStopped?.Invoke();
        }
    }

    public void StopWallRun() {

        if (isWallRunning) {
            StopCoroutine(wallRunCorutine);
            isWallRunning = false;
            wallClimbOnCooldown = true;
            Utils.DelayedFunctionCall(WallClimbCooldownDone, 0.15f); // Wall climb cooldown
            horizontalRunStopped?.Invoke();
        }
    }

    private IEnumerator VerticalRunCorutine(float duration) {

        float scale = settings.yVelToVerticalRunScaleCurve.Evaluate(character.rb.velocity.y);
        duration *= scale;

        t = 0;

        while (t < 1) {

            // No longer any wall to run/climb
            if (!wallDetected) {
                StopWallClimb();
            }

            t += Time.fixedDeltaTime / duration;
            runVelocity = wallUpVector * settings.verticalRunCurve.Evaluate(t) * settings.maxVerticalVelocity * scale;
            runVelocity += -wallHit.normal * 1f;
            wallCameraAngle = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, Vector3.up), Vector3.ProjectOnPlane(-wallHit.normal, Vector3.up), Vector3.up);

            yield return new WaitForFixedUpdate();
        }

        if (character.arms.hand_R.grabingLedge || character.arms.hand_L.grabingLedge) {
            float t2 = 0;
            while (t2 < 1) {

                // No longer any wall to run/climb
                if (!wallDetected) {
                    StopWallClimb();
                }

                t2 += Time.fixedDeltaTime / 0.75f;
                runVelocity = Vector3.up * settings.mountCurve.Evaluate(t2) * settings.maxVerticalVelocity * 0.5f;
                runVelocity += -wallHit.normal * 1f;
                wallCameraAngle = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.fpCamera.tCamera.forward, Vector3.up), Vector3.ProjectOnPlane(-wallHit.normal, Vector3.up), Vector3.up);
                float f = Mathf.Clamp01(1 - (Mathf.Abs(wallCameraAngle) / 45));
                runVelocity *= f;
                yield return new WaitForFixedUpdate();
            }
        }

        StopWallClimb();
        verticalRunStopped?.Invoke();
    }

    private void LookForEdge() {
        int rayCount = 10;
        float raySeperation = 0.2f;

        bool didLastRayHit = false;
        RaycastHit lastHitHorizontal = new RaycastHit();
        RaycastHit hitHorizontal;

        ledgeGrabPoints.Clear();

        for (int i = 0; i < rayCount; i++) {

            Vector3 origin = character.transform.position + (Vector3.up * raySeperation * i);
            if (Physics.Raycast(origin, character.transform.forward, out hitHorizontal, 1, LayerMasks.i.environment)) {
                if (didLastRayHit) {
                    if (hitHorizontal.distance > lastHitHorizontal.distance + 0.1f) {
                        Vector3 verticalEnd = lastHitHorizontal.point + character.transform.forward * 0.05f;
                        Vector3 verticalOrigin =  new Vector3(verticalEnd.x, origin.y, verticalEnd.z);
                        VerticalCheck(verticalOrigin, lastHitHorizontal.point + character.transform.forward * 0.05f);
                    }
                }
                didLastRayHit = true;
                lastHitHorizontal = hitHorizontal;
                GizmoManager.i.DrawSphere(Time.deltaTime, Color.red, hitHorizontal.point, 0.07f);
            }
            else {
                if (didLastRayHit) {
                    Vector3 verticalEnd = lastHitHorizontal.point + character.transform.forward * 0.05f;
                    Vector3 verticalOrigin = new Vector3(verticalEnd.x, origin.y, verticalEnd.z);
                    VerticalCheck(verticalOrigin, lastHitHorizontal.point + character.transform.forward * 0.05f);
                    didLastRayHit = false;
                }
            }
        }

        void VerticalCheck(Vector3 origin, Vector3 end) {
            RaycastHit hit;
            if (Physics.Linecast(origin, end, out hit, LayerMasks.i.environment)) {
                GizmoManager.i.DrawSphere(Time.deltaTime, Color.green, hit.point, 0.1f);
                ledgeGrabPoints.Add(hit.point);
                topGrabPoint = hit.point;
            }
        }
    }
}
