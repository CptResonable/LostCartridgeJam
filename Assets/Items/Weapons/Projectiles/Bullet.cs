using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LayerMask fleshLayerMask;

    public Rigidbody rb;
    public ProjectileParams projectileParams;

    private Vector3 lastPosition;
    private TrailRenderer trailRenderer;
    private bool isActive;

    [SerializeField] private GameObject prefab_vfxDirtKickup;
    [SerializeField] private Color bloodColor;
    [SerializeField] private Color groundColor;
    [SerializeField] private Color wallColor;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    public void Fire(ProjectileParams projectileParams) {
        this.projectileParams = projectileParams;
        isActive = true;
        rb.isKinematic = false;
        trailRenderer.Clear();
        rb.velocity = projectileParams.muzzleVelocity;
        lastPosition = transform.position;
    }

    private void FixedUpdate() {
        if (!isActive)
            return;

        RaycastHit hit;
        if (Physics.Linecast(lastPosition, transform.position, out hit, layerMask)) {
            ProjectileManager.i.ProjectileHit(this, hit, VectorUtils.FromToVector(lastPosition, transform.position).normalized);
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
