using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemySpawner : MonoBehaviourPun
{
    public string enemyPrefabPath;
    public float maxEnemies;
    public float spawnRadius;
    public float spawnCheckTime;


    private float lastSpawnCheckTime;
    private List<GameObject> curEnemies = new List<GameObject>();


    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        if (Time.time - lastSpawnCheckTime > spawnCheckTime)
        {
            lastSpawnCheckTime = Time.time;
            TrySpawn();
        }
    }

    void TrySpawn()
    {
        //remove dead enemies
        for (int _enemy = 0; _enemy < curEnemies.Count; _enemy++)
        {
            if (!curEnemies[_enemy])
            {
                curEnemies.RemoveAt(_enemy);
            }
        }

        //Check if maxed out spawn count
        if (curEnemies.Count >= maxEnemies)
        {
            return;
        }

        //Can spawn
        Vector3 randomInCircle = Random.insideUnitCircle * spawnRadius;
        GameObject enemy = PhotonNetwork.Instantiate(enemyPrefabPath, transform.position + randomInCircle, Quaternion.identity);
        curEnemies.Add(enemy);
    }
}
