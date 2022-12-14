using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public Rigidbody rb;

    private Vector3 lastPosition;
    private TrailRenderer trailRenderer;
    private MeshRenderer meshRenderer;
    private bool isActive;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Fire(Vector3 velocity) {
        meshRenderer.enabled = true;
        isActive = true;
        trailRenderer.Clear();
        rb.velocity = velocity;
        lastPosition = transform.position;
    }

    private void FixedUpdate() {
        if (!isActive)
            return;

        RaycastHit hit;
        if (Physics.Linecast(transform.position, lastPosition, out hit)) {

            DamageReceiver damageReceiver;
            if (hit.collider.TryGetComponent<DamageReceiver>(out damageReceiver)) {
                damageReceiver.ReceiveDamage(22);
            }

            meshRenderer.enabled = false;
            isActive = false;

            StartCoroutine(DespawnDelayConrutine(3));
        }

        lastPosition = transform.position;
    }

    private IEnumerator DespawnDelayConrutine(float delay) {
        yield return new WaitForSeconds(delay);
        EZ_Pooling.EZ_PoolManager.Despawn(transform);
    }
}
