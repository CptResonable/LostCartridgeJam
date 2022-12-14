using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Health {
    [SerializeField] public float maxHP;
    [SerializeField] private DamageReceiver[] damageReceivers;

    public float HP;
    public bool isAlive = true;

    private Character character;

    public event Delegates.FloatDelegate damageTakenEvent;
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
        damageTakenEvent?.Invoke(damage);

        if (HP < 0) {
            HP = 0;
            Die();
        }
    }

    public void Die() {
        isAlive = false;
        diedEvent?.Invoke();
    }
}
