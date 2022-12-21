using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashTrigger : MonoBehaviour {
    public bool isActive;

    private void OnTriggerEnter(Collider other) {
        if (isActive) {
            DamageReceiver damageReceiver;
            if (other.TryGetComponent<DamageReceiver>(out damageReceiver)) {
                Debug.Log("DASH ATTACK HIT!");
                isActive = false;
                damageReceiver.ReceiveDamage(15);
            }            
        }
    }

    public void DashAttack(float time) {
        StartCoroutine(DashCorutine(time));
    }

    private IEnumerator DashCorutine(float time) {
        isActive = true;
        yield return new WaitForSeconds(time);
        isActive = false;
    }
}
