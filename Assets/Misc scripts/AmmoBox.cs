using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoBox : MonoBehaviour {
    [SerializeField] private int ammo = 30;
    private void Update() {
        if (Vector3.Distance(transform.position, GameManager.i.player.transform.position) < 1.2f) {
            //GameManager.i.player.weaponController.rifle.ammoReserve += ammo;
            Destroy(gameObject);
        }
    }
}
