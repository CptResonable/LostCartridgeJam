using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public void Spawn(GameObject prefab) {
        GameObject goEnemy = Instantiate(prefab, transform.position, transform.rotation);
        NPC enemy = goEnemy.GetComponentInChildren<NPC>();
        GameManager.i.EnemySpawned(enemy);

        SFX sfx_spawn = EZ_Pooling.EZ_PoolManager.Spawn(GameManager.i.enemySpawnManager.spawnSfxPrefab.transform, transform.position, transform.rotation).gameObject.GetComponent<SFX>();
        sfx_spawn.Play();
    }
}
