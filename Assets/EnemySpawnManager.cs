using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class EnemySpawnManager {
    [SerializeField] private Spawner[] spawners;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject assaultPrefab;
    public GameObject spawnSfxPrefab;

    public event Delegates.EmptyDelegate enemySpawnedEvent;

    public bool spawningComplete = false;

    [HideInInspector] public Vector3Int[] waves = new Vector3Int[10] {
        new Vector3Int(6, 0, 12),
        new Vector3Int(0, 2, 8),
        new Vector3Int(6, 2, 16),
        new Vector3Int(12, 0, 20),
        new Vector3Int(0, 4, 16),
        new Vector3Int(14, 2, 22),
        new Vector3Int(10, 4, 22),
        new Vector3Int(30, 0, 18),
        new Vector3Int(0, 6, 12),
        new Vector3Int(50, 10, 30),
    };

    public void SpawnWave(int zombieCount, int assaultCount, float waveDuration) {
        GameManager.i.StartCoroutine(SpawnCorutine(zombieCount, assaultCount, waveDuration));
    }

    public void SpawnWave(int wave) {
        Debug.Log("START WAVE " + waves[wave].x);
        GameManager.i.StartCoroutine(SpawnCorutine(waves[wave].x, waves[wave].y, waves[wave].z));
    }

    //public IEnumerator SpawnCorutine(int zombieCount, int assaultCount, float waveDuration) {

    //    int totalCount = zombieCount + assaultCount;
    //    float timeBetweenSpawns = waveDuration / totalCount;

    //    List<int> list = new List<int>();
    //    for (int i = 0; i < zombieCount; i++)
    //        list.Add(0);
    //    for (int i = 0; i < assaultCount; i++)
    //        list.Add(1);

    //    //list = list.OrderBy(x => Random.value).ToList();

    //    spawningComplete = false;

    //    for (int i = 0; i < totalCount; i++) {
    //        Spawner spawner = spawners[Random.Range(0, spawners.Length)];
    //        if (list[i] == 0) {
    //            spawner.Spawn(zombiePrefab);
    //        }
    //        else {
    //            spawner.Spawn(assaultPrefab);
    //        }

    //        enemySpawnedEvent?.Invoke();

    //        if (i == totalCount - 1)
    //            spawningComplete = true;

    //        yield return new WaitForSeconds(timeBetweenSpawns);
    //    }
    //}

    public IEnumerator SpawnCorutine(int zombieCount, int assaultCount, float waveDuration) {

        int totalCount = zombieCount + assaultCount;
        float timeBetweenSpawns = waveDuration / totalCount;

        List<int> list = new List<int>();
        for (int i = 0; i < zombieCount; i++)
            list.Add(0);
        for (int i = 0; i < assaultCount; i++)
            list.Add(1);

        list = list.OrderBy(x => Random.value).ToList();

        spawningComplete = false;

        for (int i = 0; i < totalCount; i++) {
            Spawner spawner = spawners[Random.Range(0, spawners.Length)];
            if (list[i] == 0) {
                spawner.Spawn(zombiePrefab);
            }
            else {
                spawner.Spawn(assaultPrefab);
            }

            enemySpawnedEvent?.Invoke();

            if (i == totalCount - 1)
                spawningComplete = true;

            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }
}
