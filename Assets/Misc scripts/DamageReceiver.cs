using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour {
    public float damageMultiplier = 1;
    public delegate void DamageReceivedDelegate(float damage);
    public delegate void BulletHitDelegate(float damage, Vector3 hitPoint, Vector3 bulletPathVector);
    public event DamageReceivedDelegate damageReceivedEvent;
    public event BulletHitDelegate bulletHitEvent;

    private Health health;

    public void Init(Health health) {
        this.health = health;
    }

    /// <summary> Returns true if resulting hp is below 0 </summary>
    public bool ReceiveDamage(float damage) {
        damage *= damageMultiplier;
        damageReceivedEvent?.Invoke(damage);

        Debug.Log("DAMAGE RECIEVED!");

        if (health != null) {

            if (health.HP < 0)
                return true;
            else
                return false;
        }
        else {
            return false;
        }
    }

    /// <summary> Returns true if resulting hp is below 0 </summary>
    public bool ReceiveDamage_bulletHit(float damage, Vector3 hitPoint, Vector3 bulletPathVector) {
        damage *= damageMultiplier;
        damageReceivedEvent?.Invoke(damage);
        bulletHitEvent?.Invoke(damage, hitPoint, bulletPathVector);

        Debug.Log("DAMAGE RECIEVED!");

        if (health != null) {

            if (health.HP < 0)
                return true;
            else
                return false;
        }
        else {
            return false;
        }
    }
}
