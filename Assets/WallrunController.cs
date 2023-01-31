using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallrunController : MonoBehaviour {
    [SerializeField] private Transform tClosestPointMarker;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private AnimationCurve verticalRunCurve;
    [SerializeField] private float verticalRunDuration;
    [SerializeField] private float maxVerticalVelocity;

    public bool isWallRunning;
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

    private float t; // 0 - 1, wall run progress

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

        //if (isWallRunning) {
        //    float newAngle = Vector3.SignedAngle(wallHit.normal, Quaternion.Euler(0, character.head.yaw, 0) * Vector3.forward, Vector3.up);
        //    if (Mathf.Sign(newAngle) != Mathf.Sign(angle) && Mathf.Abs(angle) < 30) {
        //        character.head.yaw += Mathf.Sign(newAngle) * (Mathf.Abs(newAngle + 1));
        //        angle = Vector3.SignedAngle(wallHit.normal, Quaternion.Euler(0, character.head.yaw, 0) * Vector3.forward, Vector3.up);
        //    }
        //    else {
        //        angle = newAngle;
        //    }
        //}

        if (!wallDetected && isWallRunning) {
            StopWallRun();
        }

        if (!wallDetected)
            return;

        if (!isWallRunning) {
            if (Input.GetKey(KeyCode.Space)) {
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

    private void FixedUpdate() {
        tClosestPointMarker.gameObject.SetActive(false);
        wallDetected = false;
    }

    private void OnTriggerStay(Collider other) {
        Vector3 closestPoint = other.ClosestPointOnBounds(transform.position);
        tClosestPointMarker.gameObject.SetActive(true);
        tClosestPointMarker.position = closestPoint;

        Vector3 characterToPointVector = VectorUtils.FromToVector(transform.position, closestPoint);

        if (Physics.Raycast(transform.position, characterToPointVector.normalized, out wallHit, 1, layerMask)) {
            wallDetected = true;
        }
    }

    private void StartVerticalRun() {
        angle = Vector3.SignedAngle(wallHit.normal, Quaternion.Euler(0, character.fpCamera.yaw, 0) * Vector3.forward, Vector3.up);
        isWallRunning = true;
        wallRunCorutine = StartCoroutine(VerticalRunCorutine(verticalRunDuration));
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

        isReaching = false;
        isWallRunning = false;
        verticalRunStopped?.Invoke();
    }
}
