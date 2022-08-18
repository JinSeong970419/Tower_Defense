using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TowerPlacementController : MonoBehaviour
{
    [SerializeField] TowerSpawning towerSpawner;
    [SerializeField] TextMeshProUGUI guiStatusText;

    public void PlaceTower()
    {
        // 이미 구매했는지 확인 및 베이트 타워 구매
        if (!towerSpawner.GetSpawnAble())
        {
            if (GameController.Purchase(CostTemplate.GetCost("tower_base")))
            {
                towerSpawner.SetSpawnable();
            }
            else
            {
                //GUI Text
                guiStatusText.text = "Gold가 부족합니다";
            }
        }
        else { guiStatusText.text = "이미 배치가 되어있습니다."; }
    }
}
