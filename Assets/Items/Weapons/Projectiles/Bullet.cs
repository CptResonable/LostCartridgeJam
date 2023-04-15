using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LayerMask fleshLayerMask;
    public Rigidbody rb;

    private Vector3 lastPosition;
    private TrailRenderer trailRenderer;
    private MeshRenderer meshRenderer;
    private bool isActive;

    private float damage;


    [SerializeField] private GameObject prefab_vfxDirtKickup;
    [SerializeField] private Color bloodColor;
    [SerializeField] private Color groundColor;
    [SerializeField] private Color wallColor;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        trailRenderer = GetComponent<TrailRenderer>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Fire(Vector3 velocity, float damage) {
        this.damage = damage;
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
            EffectMaterialKeeper materialKeeper;
            if (hit.transform.TryGetComponent<EffectMaterialKeeper>(out materialKeeper)) {

                materialKeeper.PlayBulletHitEffects(hit.point, hit.normal);
            }

            DamageReceiver damageReceiver;
            if (hit.collider.TryGetComponent<DamageReceiver>(out damageReceiver)) {
                damageReceiver.ReceiveDamage_bulletHit(damage, hit.point, VectorUtils.FromToVector(lastPosition, transform.position).normalized);
            }

            meshRenderer.enabled = false;
            isActive = false;

            rb.isKinematic = true;
            StartCoroutine(DespawnDelayConrutine(3));
        }

        lastPosition = transform.position;
    }
    //private void FixedUpdate() {
    //    if (!isActive)
    //        return;

    //    RaycastHit hit;
    //    if (Physics.Linecast(lastPosition, transform.position, out hit, layerMask)) {

    //        Debug.Log("HIT! " + hit.collider.gameObject);
    //        if (fleshLayerMask.Contains(hit.collider.gameObject.layer)) {
    //            //GameObject goDirtKickup = EZ_Pooling.EZ_PoolManager.Spawn(prefab_vfxDirtKickup.transform, hit.point, Quaternion.LookRotation(hit.normal)).gameObject;
    //            //Vfx_dirtKickup dirtKickup = goDirtKickup.GetComponent<Vfx_dirtKickup>();
    //            //dirtKickup.Initiate(hit.collider.transform, true, bloodColor);
    //        }
    //        else {

    //            EffectMaterialKeeper materialKeeper;
    //            if (hit.transform.TryGetComponent<EffectMaterialKeeper>(out materialKeeper)) {
    //                materialKeeper.PlayBulletHitEffects(hit.point, hit.normal);
    //            }

    //            //GameObject goDirtKickup = EZ_Pooling.EZ_PoolManager.Spawn(prefab_vfxDirtKickup.transform, hit.point, Quaternion.LookRotation(hit.normal)).gameObject;
    //            //Vfx_dirtKickup dirtKickup = goDirtKickup.GetComponent<Vfx_dirtKickup>();

    //            //if (hit.collider.transform.name == "Terrain")
    //            //    dirtKickup.Initiate(hit.collider.transform, true, groundColor);
    //            //else
    //            //    dirtKickup.Initiate(hit.collider.transform, true, wallColor);
    //        }

    //        DamageReceiver damageReceiver;
    //        if (hit.collider.TryGetComponent<DamageReceiver>(out damageReceiver)) {
    //            damageReceiver.ReceiveDamage_bulletHit(damage, hit.point, VectorUtils.FromToVector(lastPosition, transform.position).normalized);
    //        }

    //        meshRenderer.enabled = false;
    //        isActive = false;

    //        rb.isKinematic = true;
    //        StartCoroutine(DespawnDelayConrutine(3));
    //    }

    //    lastPosition = transform.position;
    //}

    private IEnumerator DespawnDelayConrutine(float delay) {
        yield return new WaitForSeconds(delay);
        EZ_Pooling.EZ_PoolManager.Despawn(transform);
    }
}
