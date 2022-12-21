using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class EnemySpawnManager {
    [SerializeField] private Spawner[] spawners;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject assaultPrefab;

    public event Delegates.EmptyDelegate enemySpawnedEvent;

    public bool spawningComplete = false;

    public Vector3Int[] waves = new Vector3Int[10] {
        new Vector3Int(6, 0, 12),
        new Vector3Int(0, 2, 8),
        new Vector3Int(6, 2, 20),
        new Vector3Int(12, 0, 24),
        new Vector3Int(0, 4, 16),
        new Vector3Int(6, 0, 12),
        new Vector3Int(6, 0, 12),
        new Vector3Int(6, 0, 12),
        new Vector3Int(6, 0, 12),
        new Vector3Int(6, 0, 12),
    };

    public void SpawnWave(int zombieCount, int assaultCount, float waveDuration) {
        GameManager.i.StartCoroutine(SpawnCorutine(zombieCount, assaultCount, waveDuration));
    }

    public void SpawnWave(int wave) {
        GameManager.i.StartCoroutine(SpawnCorutine(waves[wave].x, waves[wave].y, waves[wave].z));
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
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        spawningComplete = true;
    }
}
