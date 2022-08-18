using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSpawning : MonoBehaviour
{
    [SerializeField]
    GameObject tower;

    [SerializeField]
    LayerMask nodeMask;

    [SerializeField]
    TextMeshProUGUI guiStatusText;

    [SerializeField] bool spawnable = false;

    void Update()
    {
        if (!IsPaused() && spawnable)
        {
            SpawnThatTower();
        }
    }

    // 하드코딩.... 추 후 변경하기
    public void SpawnThatTower()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Test UI
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeMask))
            {
                NodeV2 node = hit.transform.parent.gameObject.GetComponent<NodeV2>();
                
                // 타워 배치여부 확인
                if (!IsThereATower(node) && node.GetIsTowerPlacable())
                {
                    Vector3 position = node.transform.position + node.GetEnvOffset();
                    node.DeleteEnvObject();
                    GameObject temp = Instantiate(tower, position, Quaternion.identity) as GameObject;
                    node.SetTowerObject(temp);

                    // 노드를 타워가 배치된 것으로 변경
                    node.towerPlaced = true;

                    temp.transform.parent = hit.transform;

                    spawnable = false;
                }
                else
                {
                    guiStatusText.text = "여기에 타워를 설치할 수 없습니다.";
                }
            }
        }
    }

    public void AssignTower(GameObject tower_to_place) { tower = tower_to_place; }

    bool IsThereATower(NodeV2 node) { return node.towerPlaced && node.GetIsTowerPlacable(); }

    // GUI 왼쪽하단 버튼
    public void SetSpawnable() { spawnable = true; }

    public bool GetSpawnAble() { return spawnable; }
    
    // 일시 정지
    PauseController pauseController = null;

    PauseController GetPauseController()
    {
        if (pauseController == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("PauseController");
            if (obj != null)
                pauseController = obj.GetComponent<PauseController>();
        }
        return pauseController;
    }

    bool IsPaused()
    {
        bool isPaused = false;
        PauseController pc = GetPauseController();
        if (pc != null)
            isPaused = pc.IsPaused();
        return isPaused;
    }
}