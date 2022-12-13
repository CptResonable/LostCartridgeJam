using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Health {
    [SerializeField] private float maxHP;
    [SerializeField] private DamageReceiver[] damageReceivers;

    public float HP;

    private Character character;

    public event Delegates.EmptyDelegate diedEvent;

    public void Init(Character character) {
        this.character = character;

        HP = maxHP;

        foreach (DamageReceiver damageReceiver in damageReceivers) {
            damageReceiver.Init(this);
            damageReceiver.damageReceivedEvent += DamageReceiver_damageReceivedEvent;
        }
    }

    private void DamageReceiver_damageReceivedEvent(float damage) {
        HP -= damage;

        Debug.Log("HEACÖTJ DAMAGE RECIEVED!: " + damage);

        if (HP < 0) {
            HP = 0;
            Die();
        }
    }

    public void Die() {
        diedEvent?.Invoke();
    }
}
