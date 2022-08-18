﻿using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class WaveController : MonoBehaviour
{
	private int level = 1;

    [SerializeField]
    GameObject spawner; //spawner prefab

    //GUI
    [SerializeField]
    TextMeshProUGUI guiWaveText;

    List<GameObject> spawnerList = new List<GameObject>();

    [SerializeField] GameObject m_type1_enemy;
    [SerializeField] GameObject m_type2_enemy;
    [SerializeField] GameObject m_type3_enemy;
    [SerializeField] GameObject m_type4_enemy;
    [SerializeField] GameObject m_type5_enemy;
    [SerializeField] GameObject m_type6_enemy;
    [SerializeField] GameObject m_type7_enemy;
    [SerializeField] GameObject m_type8_enemy;
    [SerializeField] GameObject m_type9_enemy;


    [SerializeField] float initialWaitTime = 20.0f;
    [SerializeField] float inBetweenLevelWaitTime;

    const float inBetweenTimeReset = 25.0f;

    void Start()
    {
        level = 1; // 1Lv 부터 시작
        inBetweenLevelWaitTime = inBetweenTimeReset;
        guiWaveText.text = level.ToString();
    }

    void Update()
    {
        // 초기 타이머
        if (level == 1)
        {
            InitialTimer();
            if (initialWaitTime <= 0.0f)
            {
                SpawnWave(WaveTemplate.FetchWave(level));
                LevelUp();
            }
        }
        // Other levels
        else
        {
            Timer();
            if (inBetweenLevelWaitTime <= 0.0f)
            {
                CleanUpSpawnerList();
                SpawnWave(WaveTemplate.FetchWave(level));
                inBetweenLevelWaitTime = inBetweenTimeReset;
                LevelUp();
            }
        }
    }

    // Spawn 관련
    void SpawnWave(Dictionary<string, int> wave)
	{
        int waveReward = RevenueTemplate.revenueDictWaves[level];
        GameObject newSpawner = Instantiate(spawner, transform) as GameObject;
        EnemySpawnerController esc = newSpawner.GetComponent<EnemySpawnerController>();
        foreach (KeyValuePair<string, int> item in wave)
        {
            GameObject enemyType = SelectEnemyType(item.Key);
            esc.AddToSpawnQueue(enemyType, item.Value);
        }
        esc.ActivateWave();
        esc.SetWaveReward(waveReward);
        spawnerList.Add(newSpawner);
	}

    // 문자열 기반으로 Enemy Prefab 선택
    // 추 후 변경
    GameObject SelectEnemyType(string enemyType)
    {
        switch (enemyType)
        {
            case "normalSmall":
                return m_type1_enemy;
            case "normal":
                return m_type2_enemy;
            case "fireSmall":
                return m_type3_enemy;
            case "fire":
                return m_type4_enemy;
            case "natureSmall":
                return m_type5_enemy;
            case "nature":
                return m_type6_enemy;
            case "iceSmall":
                return m_type7_enemy;
            case "ice":
                return m_type8_enemy;
            case "arcaneSmall":
                return m_type9_enemy;
            default:
                return m_type1_enemy;
        }
    }

    // 파괴된 스포너 제거
    void CleanUpSpawnerList()
    {
        spawnerList.RemoveAll(item => item == null);
    }

    // Wave 관련
    void LevelUp()
    {
        if (level < 51)
            level++;
        else
            level = 51;
        guiWaveText.text = (level - 1).ToString();
    }

    public void ResetAll()
    {
        level = 1;
        guiWaveText.text = level.ToString();
        CleanUpSpawnerList();
        for (int i = 0; i < spawnerList.Count; i++)
        {
            spawnerList[i].GetComponent<EnemySpawnerController>().ResetWave();
            Destroy(spawnerList[i]);
        }
        CleanUpSpawnerList();
        inBetweenLevelWaitTime = initialWaitTime;
    }

    //Timers
    void InitialTimer()
    {
        initialWaitTime -= Time.deltaTime;
    }
    void Timer()
    {
        inBetweenLevelWaitTime -= Time.deltaTime;
    }
}
