using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunController : MonoBehaviour {
    [SerializeField] private Transform tClosestPointMarker;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private AnimationCurve verticalRunCurve;
    [SerializeField] private AnimationCurve mountCurve;
    [SerializeField] private float verticalRunDuration;
    [SerializeField] private float maxVerticalVelocity;

    public bool isWallRunning;
    public bool wallRunUsed = false;
    public bool isReaching;
    public Vector3 runVelocity;
    public float wallCameraAngle;

    private Character character;

    private bool wallDetected;
    public RaycastHit wallHit;
    private Coroutine wallRunCorutine;

    private Vector3 smoothCharacterVelocity;
    private Vector3 smoothCharacterHorizontalVelocity;
    private float velocityWallAngle;

    public float t; // 0 - 1, wall run progress

    private float lastYaw;
    private float angle;

    public event Delegates.EmptyDelegate verticalRunStarted;
    public event Delegates.EmptyDelegate verticalRunStopped;
    public event Delegates.EmptyDelegate verticalRunReachStarted;

    private void Awake() {
        character = GetComponentInParent<Character>();
    }

    private void Update() {
        smoothCharacterVelocity = Vector3.Lerp(smoothCharacterVelocity, character.rb.velocity, Time.deltaTime * 8);
        smoothCharacterHorizontalVelocity = Vector3.ProjectOnPlane(smoothCharacterVelocity, Vector3.up);

        if (character.locomotion.isGrounded && !isWallRunning)
            wallRunUsed = false;

        if (!wallDetected && isWallRunning) {
            StopWallRun();
            //StartCoroutine(DelayedStop(0.1f));
        }

        if (!wallDetected)
            return;

        if (!isWallRunning) {
            if (Input.GetKey(KeyCode.Space) && !wallRunUsed) {
                velocityWallAngle = Vector3.SignedAngle(-wallHit.normal, smoothCharacterHorizontalVelocity, Vector3.up);

                if (Mathf.Abs(velocityWallAngle) < 15)
                    StartVerticalRun();
            }
        }
        else {
            if (Input.GetKeyDown(KeyCode.Space) && t > 0.5f) {
                isReaching = true;
                verticalRunReachStarted?.Invoke();
            }
        }

        Vector3 lastYawVector = Quaternion.Euler(0, lastYaw, 0) * Vector3.forward;

        lastYaw = character.fpCamera.yaw;
    }

    private IEnumerator DelayedStop(float delay) {
        yield return new WaitForSeconds(delay);
        if (isWallRunning)
            StopWallRun();
    }

    private void FixedUpdate() {
        tClosestPointMarker.gameObject.SetActive(false);
        wallDetected = false;
    }

    private void OnTriggerStay(Collider other) {
        if (Physics.Raycast(transform.position + Vector3.down * 0.4f, character.transform.forward, out wallHit, 2, layerMask)) {
            if (Physics.Raycast(transform.position + Vector3.down * 0.4f, -wallHit.normal, out wallHit, 1, layerMask)) {
                wallDetected = true;
            }
        }
    }

    //private void OnTriggerStay(Collider other) {
    //    if (Physics.Raycast(transform.position, character.transform.forward, out wallHit, 2, layerMask)) {
    //        if (Physics.Raycast(transform.position, -wallHit.normal, out wallHit, 1, layerMask)) {
    //            wallDetected = true;
    //        }
    //    }
    //}

    private void StartVerticalRun() {
        angle = Vector3.SignedAngle(wallHit.normal, Quaternion.Euler(0, character.fpCamera.yaw, 0) * Vector3.forward, Vector3.up);
        isWallRunning = true;
        wallRunCorutine = StartCoroutine(VerticalRunCorutine(verticalRunDuration));
        wallRunUsed = true;
        verticalRunStarted?.Invoke();
    }

    public void StopWallRun() {
        if (isWallRunning) {
            StopCoroutine(wallRunCorutine);
            isWallRunning = false;
            isReaching = false;
            verticalRunStopped?.Invoke();
        }
    }

    private IEnumerator VerticalRunCorutine(float duration) {
        t = 0;

        while (t < 1) {
            t += Time.fixedDeltaTime / duration;
            runVelocity = Vector3.up * verticalRunCurve.Evaluate(t) * maxVerticalVelocity;
            runVelocity += -wallHit.normal * 1f;
            wallCameraAngle = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.fpCamera.tCameraTarget.forward, Vector3.up), Vector3.ProjectOnPlane(-wallHit.normal, Vector3.up), Vector3.up);
            yield return new WaitForFixedUpdate();
        }

        if (character.arms.hand_R.grabingLedge || character.arms.hand_L.grabingLedge) {
            float t2 = 0;
            while (t2 < 1) {
                t2 += Time.fixedDeltaTime / 0.75f;
                runVelocity = Vector3.up * mountCurve.Evaluate(t2) * maxVerticalVelocity * 0.5f;
                runVelocity += -wallHit.normal * 1f;
                wallCameraAngle = Vector3.SignedAngle(Vector3.ProjectOnPlane(character.fpCamera.tCameraTarget.forward, Vector3.up), Vector3.ProjectOnPlane(-wallHit.normal, Vector3.up), Vector3.up);
                float f = Mathf.Clamp01(1 - (Mathf.Abs(wallCameraAngle) / 45));
                runVelocity *= f;
                yield return new WaitForFixedUpdate();
            }
        }

        isReaching = false;
        isWallRunning = false;
        verticalRunStopped?.Invoke();
    }
}
