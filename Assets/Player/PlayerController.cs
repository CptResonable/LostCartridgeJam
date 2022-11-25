using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveAcceleration;

    [SerializeField] private float bounceUpSpeed;
    [SerializeField] private float yVelLerpSpeed;

    [SerializeField] private float jumpVelocity;

    [SerializeField] private Head head;

    private float targetHeight = 1;
    private float currentHeight = 1;

    private Vector3 inputDir;

    private Rigidbody rb;
    private bool isGrounded = true;
    private bool jumpOnCooldown = false;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        VerticalMovement();

        inputDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            inputDir.z += 1;
        if (Input.GetKey(KeyCode.S))
            inputDir.z -= 1;
        if (Input.GetKey(KeyCode.A))
            inputDir.x -= 1;
        if (Input.GetKey(KeyCode.D))
            inputDir.x += 1;

        if (inputDir.magnitude > 1)
            inputDir.Normalize();

        inputDir = transform.TransformDirection(inputDir);

        if (!jumpOnCooldown && isGrounded && Input.GetKeyDown(KeyCode.Space))
            Jump();

        HorizontalMovement();
        head.vUpdate();
    }

    private void VerticalMovement() {
        RaycastHit downHit;
        if (Physics.Raycast(transform.position, Vector3.down, out downHit, 100 )) {
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
        rb.velocity = Vector3.Lerp(rb.velocity, inputDir * moveSpeed + Vector3.up * rb.velocity.y, moveAcceleration * Time.deltaTime);
    }

    private void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        jumpOnCooldown = true;
        StartCoroutine(JumpCorutine());
    }

    private IEnumerator JumpCorutine() {
        yield return new WaitForSeconds(0.3f);
        jumpOnCooldown = false;
    }
}
