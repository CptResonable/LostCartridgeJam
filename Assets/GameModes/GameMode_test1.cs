using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameMode_test1 : GameMode {
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] tSpawners;
    public override void StartGameMode() {
        base.StartGameMode();
        StartCoroutine(SpawnEnemiesCorutine());
    }

    public IEnumerator SpawnEnemiesCorutine() {
        int count = 0;
        while (count < 31) {
            SpawnEnemyAtRandomSpawnPoint(count);
            count++;
            yield return new WaitForSeconds(0.1f);
        }
    }


    public void SpawnEnemyAtRandomSpawnPoint(int id) {
        int maxTries = 10;
        for (int tries = 0; tries < maxTries; tries++) {

            Transform tSpawner = tSpawners[Random.Range(0, tSpawners.Length)];

            // Make sure new position has LOS to target
            if (!CheckLOS(tSpawner.position + Vector3.up * 1.75f, GameManager.i.player.body.tHead.position)) {
                GameObject go = Instantiate(enemyPrefab, tSpawner.position, Quaternion.identity);
                go.name = "Enemy AI #" + id;
                return;
            }
        }
    }

    //public void SpawnEnemyAtRandomPoint() {
    //    int maxTries = 10;
    //    NavMeshHit navMeshHit;

    //    for (int tries = 0; tries < maxTries; tries++) {
    //        if (NavMesh.SamplePosition(Vector3.zero, out navMeshHit, 400, NavMesh.AllAreas)) {

    //            // Make sure new position has LOS to target
    //            if (!CheckLOS(navMeshHit.position + Vector3.up * 1.75f, GameManager.i.player.body.tHead.position)) {
    //                Instantiate(enemyPrefab, navMeshHit.position, Quaternion.identity);
    //                return;
    //            }
    //        }
    //    }
    //}

    /// <summary> Returns true if path is clear, false if blocked </summary>
    private bool CheckLOS(Vector3 fromPoint, Vector3 toPoint) {
        if (Physics.Linecast(fromPoint, toPoint, LayerMasks.i.environment))
            return false;
        else
            return true;
    }
}
