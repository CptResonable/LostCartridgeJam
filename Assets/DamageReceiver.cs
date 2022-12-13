using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour {
    public float damageMultiplier = 1;
    public delegate void DamageReceivedDelegate(float damage);
    public event DamageReceivedDelegate damageReceivedEvent;

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
}
