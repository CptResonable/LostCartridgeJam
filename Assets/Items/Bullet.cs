using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    [SerializeField] private LayerMask layerMask;
    public Rigidbody rb;

    private Vector3 lastPosition;
    private TrailRenderer trailRenderer;
    private MeshRenderer meshRenderer;
    private bool isActive;

    [SerializeField] private GameObject prefab_vfxDirtKickup;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Fire(Vector3 velocity) {
        meshRenderer.enabled = true;
        isActive = true;
        rb.isKinematic = false;
        trailRenderer.Clear();
        rb.velocity = velocity;
        lastPosition = transform.position;
    }

    private void FixedUpdate() {
        if (!isActive)
            return;

        RaycastHit hit;
        if (Physics.Linecast(lastPosition, transform.position, out hit, layerMask)) {

            DamageReceiver damageReceiver;
            if (hit.collider.TryGetComponent<DamageReceiver>(out damageReceiver)) {
                damageReceiver.ReceiveDamage(22);
            }
            else {
                GameObject goDirtKickup = EZ_Pooling.EZ_PoolManager.Spawn(prefab_vfxDirtKickup.transform, hit.point, Quaternion.LookRotation(hit.normal)).gameObject;
                Vfx_dirtKickup dirtKickup = goDirtKickup.GetComponent<Vfx_dirtKickup>();
                dirtKickup.Initiate(hit.collider.transform);
            }

            meshRenderer.enabled = false;
            isActive = false;

            rb.isKinematic = true;
            StartCoroutine(DespawnDelayConrutine(3));
        }

        lastPosition = transform.position;
    }

    private IEnumerator DespawnDelayConrutine(float delay) {
        yield return new WaitForSeconds(delay);
        EZ_Pooling.EZ_PoolManager.Despawn(transform);
    }
}
