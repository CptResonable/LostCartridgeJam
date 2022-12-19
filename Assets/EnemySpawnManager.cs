using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class EnemySpawnManager {
    [SerializeField] private Spawner[] spawners;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject assaultPrefab;

    public void SpawnWave(int zombieCount, int assaultCount, float waveDuration) {
        GameManager.i.StartCoroutine(SpawnCorutine(zombieCount, assaultCount, waveDuration));
    }

    public IEnumerator SpawnCorutine(int zombieCount, int assaultCount, float waveDuration) {

        int totalCount = zombieCount + assaultCount;
        float timeBetweenSpawns = waveDuration / totalCount;

        List<int> list = new List<int>();
        for (int i = 0; i < zombieCount; i++)
            list.Add(0);
        for (int i = 0; i < assaultCount; i++)
            list.Add(1);

        list = list.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < totalCount; i++) {
            Spawner spawner = spawners[Random.Range(0, spawners.Length)];
            if (list[i] == 0) {
                spawner.Spawn(zombiePrefab);
            }
            else {
                spawner.Spawn(assaultPrefab);
            }
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }
}
