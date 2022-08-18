using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerController : MonoBehaviour
{
    // enemy spawn
    [SerializeField] GameObject m_enemy;

    List<GameObject> spawnedEnemies = new List<GameObject>();
    InputHelper m_input = new InputHelper();

    Queue<GameObject> typeQueue = new Queue<GameObject>();
    Queue<int> nbQueue = new Queue<int>();

    [SerializeField] float spawnTime = 1.0f;

    [SerializeField] bool waveActive;

    [SerializeField]
    int waveReward = 30;
    int nb_spawned;
    int max_spawned;

    const float spawnTimeReset = 1.0f;

    void Update()
    {
        if (waveActive)
        {
            timer();
            if (spawnTime <= 0.0f && nb_spawned < max_spawned)
            {
                SpawnEnemy();
                nb_spawned++;
                ResetSpawnTime();
            }
            else if (nb_spawned == max_spawned)
            {
                if (typeQueue.Count > 0)
                    Reconfigure();
                else
                {
                    CheckWaveStatus();
                }
            }
        }
        else
        {
            GameController.WaveCompleted(waveReward);
            Destroy(gameObject);
            return;
        }

        // 많은 적 소환 Test
        //if (m_input.OnKeyPressed(KeyCode.M))
        //    SpawnEnemy();
    }

    void SpawnEnemy()
    {
        Waypoints wp = FindObjectOfType<Waypoints>();
        if (wp != null)
        {
            Vector3 startPos = wp.GetStartingPoint().transform.position;
            GameObject enemy = Instantiate(m_enemy, startPos, Quaternion.identity) as GameObject;
            enemy.GetComponent<EnemyController>().SetSpeed(10f);
            enemy.transform.parent = transform;
            spawnedEnemies.Add(enemy);
        }
    }

    // spawner 제거
    void CleanUpEnemyList()
    {
        spawnedEnemies.RemoveAll(item => item == null);
    }

    void timer()
    {
        spawnTime -= Time.deltaTime;
    }

    void ResetSpawnTime()
    {
        spawnTime = spawnTimeReset;
    }

    // 다음 적 재설정
    void Reconfigure()
    {
        m_enemy = typeQueue.Dequeue();
        max_spawned = nbQueue.Dequeue();
        nb_spawned = 0;
        waveActive = true;
    }

    void CheckWaveStatus()
    {
        spawnedEnemies.RemoveAll(item => item == null);
        if (spawnedEnemies.Count == 0)
        {
            waveActive = false;
        }
    }

    public void ActivateWave()
    {
        Reconfigure();
    }

    public void AddToSpawnQueue(GameObject enemyType, int nbOfSpawn)
    {
        typeQueue.Enqueue(enemyType);
        nbQueue.Enqueue(nbOfSpawn);
    }

    public void ResetWave()
    {
        Debug.Log("ResetWave");
        CleanUpEnemyList();
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            KillEnemy(spawnedEnemies[i]);
        }
        CleanUpEnemyList();
    }

    public void SetWaveReward(int rewardAmount)
    {
        waveReward = rewardAmount;
    }

    public void ReachedEnd()
    {
        // EnemyController script?....Test
        GameController.EndReached(1);
    }

    public void KillEnemy(GameObject obj)
    {
        EnemyController enemyController = obj.GetComponent<EnemyController>();
        if (enemyController != null)
            enemyController.Kill(); // Make Go Boom!
        else
            Destroy(obj);
    }
}
