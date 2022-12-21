using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public void Spawn(GameObject prefab) {
        GameObject goEnemy = Instantiate(prefab, transform.position, transform.rotation);
        NPC enemy = goEnemy.GetComponentInChildren<NPC>();
        GameManager.i.EnemySpawned(enemy);
    }
}
