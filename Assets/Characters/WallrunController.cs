using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunController : MonoBehaviour {
    [SerializeField] private WallrunSettings settings;

    public bool isWallRunning;
    public bool wallRunUsed = false;

    [HideInInspector] public Vector3 runVelocity;
    [HideInInspector] public float wallCameraAngle;

    private Character character;

    private bool wallDetected;
    public RaycastHit wallHit;
    public Vector3 wallUpVector; // Just (0, 1, 0) for now. TODO make it actually up vector;
    private Coroutine wallRunCorutine;

    private Vector3 smoothCharacterVelocity;
    private Vector3 smoothCharacterHorizontalVelocity;
    private float velocityWallAngle;

    [HideInInspector] public float t; // 0 - 1, wall run progress

    public event Delegates.EmptyDelegate verticalRunStarted;
    public event Delegates.EmptyDelegate verticalRunStopped;

    private void Awake() {
        character = GetComponentInParent<Character>();
    }

    private void Update() {
        smoothCharacterVelocity = Vector3.Lerp(smoothCharacterVelocity, character.rb.velocity, Time.deltaTime * 8);
        smoothCharacterHorizontalVelocity = Vector3.ProjectOnPlane(smoothCharacterVelocity, Vector3.up);

        if (character.locomotion.activeState.stateID == Locomotion.LocomotionState.StateIDEnum.Grounded && !isWallRunning)
            wallRunUsed = false;

        //if (!wallDetected && isWallRunning) {
        //    StopWallRun();
        //}

        //if (!wallDetected)
        //    return;

        //if (!isWallRunning) {
        //    if (Input.GetKey(KeyCode.Space) && !wallRunUsed && !character.locomotion.isGrounded) {
        //        velocityWallAngle = Vector3.SignedAngle(-wallHit.normal, smoothCharacterHorizontalVelocity, Vector3.up);

        //        if (Mathf.Abs(velocityWallAngle) < 15)
        //            StartVerticalRun();
        //    }
        //}
    }

    private void FixedUpdate() {
        wallDetected = false;
    }

    private void OnTriggerStay(Collider other) {
        if (Physics.Raycast(transform.position + Vector3.down * 0.4f, character.transform.forward, out wallHit, 2, LayerMasks.i.wall)) {
            if (Physics.Raycast(transform.position + Vector3.down * 0.4f, -wallHit.normal, out wallHit, 1, LayerMasks.i.wall)) {
                wallDetected = true;
            }
        }
    }

    public void AttemptWallRun() {

        if (!wallDetected) // No wall to run/climb
            return;

        if (!isWallRunning) {
            if (!wallRunUsed) {
                velocityWallAngle = Vector3.SignedAngle(-wallHit.normal, smoothCharacterHorizontalVelocity, Vector3.up);
                if (Mathf.Abs(velocityWallAngle) < 15)
                    StartVerticalRun();
            }
        }
    }

    private void StartVerticalRun() {
        isWallRunning = true;
        wallRunCorutine = StartCoroutine(VerticalRunCorutine(settings.verticalRunDuration));
        wallRunUsed = true;
        wallUpVector = Vector3.up;
        verticalRunStarted?.Invoke();
    }

    public void StopWallRun() {
        if (isWallRunning) {
            StopCoroutine(wallRunCorutine);
            isWallRunning = false;
            verticalRunStopped?.Invoke();
        }
    }

    private IEnumerator VerticalRunCorutine(float duration) {

        float scale = settings.yVelToVerticalRunScaleCurve.Evaluate(character.rb.velocity.y);
        duration *= scale;

        t = 0;

        while (t < 1) {

            // No longer any wall to run/climb
            if (!wallDetected) {
                StopWallRun();
            }

            t += Time.fixedDeltaTime / duration;
            runVelocity = Vector3.up * settings.verticalRunCurve.Evaluate(t) * settings.maxVerticalVelocity * scale;
            runVelocity += -wallHit.normal * 1f;
            wallCameraAngle = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.fpCamera.tCameraTarget.forward, Vector3.up), Vector3.ProjectOnPlane(-wallHit.normal, Vector3.up), Vector3.up);

            yield return new WaitForFixedUpdate();
        }

        if (character.arms.hand_R.grabingLedge || character.arms.hand_L.grabingLedge) {
            float t2 = 0;
            while (t2 < 1) {

                // No longer any wall to run/climb
                if (!wallDetected) {
                    StopWallRun();
                }

                t2 += Time.fixedDeltaTime / 0.75f;
                runVelocity = Vector3.up * settings.mountCurve.Evaluate(t2) * settings.maxVerticalVelocity * 0.5f;
                runVelocity += -wallHit.normal * 1f;
                wallCameraAngle = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.fpCamera.tCameraTarget.forward, Vector3.up), Vector3.ProjectOnPlane(-wallHit.normal, Vector3.up), Vector3.up);
                float f = Mathf.Clamp01(1 - (Mathf.Abs(wallCameraAngle) / 45));
                runVelocity *= f;
                yield return new WaitForFixedUpdate();
            }
        }

        isWallRunning = false;
        verticalRunStopped?.Invoke();
    }
}
