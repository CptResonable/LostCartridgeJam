using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public Rigidbody rb;

    private Vector3 lastPosition;

    public void Fire(Vector3 velocity) {
        rb = GetComponent<Rigidbody>();
        rb.velocity = velocity;
        lastPosition = transform.position;
    }

    private void FixedUpdate() {
        RaycastHit hit;
        if (Physics.Linecast(transform.position, lastPosition, out hit)) {

            DamageReceiver damageReceiver;
            if (hit.collider.TryGetComponent<DamageReceiver>(out damageReceiver)) {
                Debug.Log("DAMAGE RECIEVER HIT!");
                damageReceiver.ReceiveDamage(22);
            }

            EZ_Pooling.EZ_PoolManager.Despawn(transform);
        }

        lastPosition = transform.position;
    }
}
