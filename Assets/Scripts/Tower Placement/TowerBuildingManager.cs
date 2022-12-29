using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TowerBuildingManager : MonoBehaviour
{
    [SerializeField] GameObject towerToBuild;
    [SerializeField] GameObject towerSelected;

    [SerializeField] private NodeV2 previousNode = null;
    [SerializeField] NodeV2 currentNode = null;
    [SerializeField] NodeV2 selectedNode = null;

    [SerializeField] LayerMask nodeMask;
    [SerializeField] TextMeshProUGUI guiStatusText;
    [SerializeField] bool spawnable = false;

    //타워 Upgrade Menu
    [SerializeField] TextMeshProUGUI selectedTowerStatus;
    [SerializeField] GameObject menuContainer;
    [SerializeField] GameObject baseUpradeMenu;
    [SerializeField] GameObject elemUpgradeMenu;
    [SerializeField] TowerContainer towerContainer;
    [SerializeField] TextMeshProUGUI nextUpgradeCost;

    private Dictionary<string, GameObject> towers;

    private void Awake()
    {
        towers = towerContainer.GetTowerContainer();
        EventSystem.current.SetSelectedGameObject(null);
    }

    void Update()
    {
        if (!IsPaused())
        {
            // 랜덤
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, nodeMask))
            {
                currentNode = hit.transform.parent.gameObject.GetComponent<NodeV2>();

                if (currentNode == null)
                {
                    if (previousNode != null)
                    {
                        previousNode.GetComponentInChildren<NodeHoverV2>().StopHovering();
                        previousNode = null;
                    }
                }
                else if (previousNode == null)
                {
                    previousNode = currentNode;
                    hit.transform.gameObject.GetComponent<NodeHoverV2>().Hover();
                }
                else if (currentNode.GetInstanceID() != previousNode.GetInstanceID())
                {
                    previousNode.GetComponentInChildren<NodeHoverV2>().StopHovering();
                    hit.transform.gameObject.GetComponent<NodeHoverV2>().Hover();
                    previousNode = currentNode;
                }

                // 선택
                if (Input.GetMouseButtonDown(0))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                        return;

                    selectedNode = currentNode;

                    if (selectedNode.towerPlaced)
                    {
                        towerSelected = selectedNode.GetTowerObject();

                        // 타워 상태 및 업그레이드 된 타워 관련
                        if (towerSelected.GetComponent<TowerController>().GetTowerType() != "base_tower")
                        {
                            selectedTowerStatus.text = towerSelected.GetComponent<TowerController>().formattedName;
                            SetNextPurchaseCost(towerSelected);
                        }

                        if (!towerSelected.GetComponent<Outline>().isActiveAndEnabled)
                        {
                            towerSelected.GetComponent<Outline>().enabled = true;
                            SpawnMenu(towerSelected);
                        }
                        else
                        {
                            towerSelected.GetComponent<Outline>().enabled = false;
                            towerSelected = null;
                            DeSpawnMenu();
                        }
                    }
                }

                // 2. 타워가 생성되지 않았을 경우 타워 생성
                else if (spawnable && selectedNode.GetIsTowerPlacable())
                {
                    Vector3 position = selectedNode.transform.position + selectedNode.GetEnvOffset();
                    selectedNode.DeleteEnvObject();
                    GameObject temp = Instantiate(towerToBuild, position, Quaternion.identity) as GameObject;
                    selectedNode.SetTowerObject(temp);

                    //타워 배치 표시
                    selectedNode.towerPlaced = true;
                    temp.transform.parent = selectedNode.transform;

                    spawnable = false;
                }
            }
        }
        else
        { 
            //Debug.Log(Time.timeScale); 
        }
    }

    public void UpgradeFromBase(int type)
    {
        if (towerSelected != null)
        {
            Vector3 pos = towerSelected.transform.position;
            NodeV2 parentNode = towerSelected.GetComponentInParent<NodeV2>();

            switch (type)
            {
                // 파이어 타워 관련
                case 0:
                    if (GameController.Purchase(CostTemplate.GetCost("tower_fire")))
                    {
                        parentNode.ReplaceTower();
                        GameObject newTower = Instantiate(towers["tower_fire"], pos, Quaternion.identity) as GameObject;
                        parentNode.SetTowerObject(newTower);
                        towerSelected = newTower;
                        newTower.transform.parent = parentNode.transform;

                        SetNextPurchaseCost(towerSelected);


                        DeSpawnMenu();
                        towerSelected.GetComponent<Outline>().enabled = true;
                        selectedTowerStatus.text = towerSelected.GetComponent<TowerController>().formattedName;
                        SpawnMenu(towerSelected);

                    }
                    else
                    {
                        guiStatusText.text = "업그레이드할 돈이 부족합니다.";
                    }
                    break;

                // 아이스 타워 관련
                case 1:
                    if (GameController.Purchase(CostTemplate.GetCost("tower_ice")))
                    {
                        parentNode.ReplaceTower();
                        GameObject newTower = Instantiate(towers["tower_ice"], pos, Quaternion.identity) as GameObject;
                        towerSelected = newTower;
                        parentNode.SetTowerObject(newTower);
                        newTower.transform.parent = parentNode.transform;

                        SetNextPurchaseCost(towerSelected);

                        DeSpawnMenu();
                        towerSelected.GetComponent<Outline>().enabled = true;
                        selectedTowerStatus.text = towerSelected.GetComponent<TowerController>().formattedName;
                        SpawnMenu(towerSelected);


                    }
                    else
                    {
                        guiStatusText.text = "업그레이드할 돈이 부족합니다.";
                    }
                    break;
                
                //nature 타워 관련
                case 2:
                    if (GameController.Purchase(CostTemplate.GetCost("tower_nature")))
                    {
                        parentNode.ReplaceTower();
                        GameObject newTower = Instantiate(towers["tower_nature"], pos, Quaternion.identity) as GameObject;
                        towerSelected = newTower;
                        parentNode.SetTowerObject(newTower);
                        newTower.transform.parent = parentNode.transform;

                        SetNextPurchaseCost(towerSelected);


                        DeSpawnMenu();
                        towerSelected.GetComponent<Outline>().enabled = true;
                        selectedTowerStatus.text = towerSelected.GetComponent<TowerController>().formattedName;
                        SpawnMenu(towerSelected);
                    }
                    else
                    {
                        guiStatusText.text = "업그레이드할 돈이 부족합니다.";
                    }
                    break;
                case 3:
                    SellTower(towerSelected, parentNode);
                    DeSpawnMenu();
                    break;
            }
        }
    }

    // 기존 타워가 존재하는 경우 업그레이드 진행
    public void UpgradeFromElement(int type)
    {
        if (towerSelected != null)
        {
            Vector3 pos = towerSelected.transform.position;
            NodeV2 parentNode = towerSelected.GetComponentInParent<NodeV2>();

            if (type == 0)
            {
                // 파이어 타워
                if (towerSelected.GetComponent<TowerController>().GetTowerType() == "tower_fire")
                {
                    if (GameController.Purchase(CostTemplate.GetCost("tower_fire_2")))
                    {
                        parentNode.ReplaceTower();
                        GameObject newTower = Instantiate(towers["tower_fire_2"], pos, Quaternion.identity) as GameObject;
                        parentNode.SetTowerObject(newTower);
                        towerSelected = newTower;
                        newTower.transform.parent = parentNode.transform;
                        towerSelected.GetComponent<Outline>().enabled = true;
                        nextUpgradeCost.text = CostTemplate.GetCost("tower_fire_3").ToString();
                        selectedTowerStatus.text = towerSelected.GetComponent<TowerController>().formattedName;
                        SpawnMenu(towerSelected);
                        
                    }
                    else
                    {
                        guiStatusText.text = "업그레이드할 돈이 부족합니다.";
                    }
                }
                else if(towerSelected.GetComponent<TowerController>().GetTowerType() == "tower_fire_2")
                {
                    if (GameController.Purchase(CostTemplate.GetCost("tower_fire_3")))
                    {
                        parentNode.ReplaceTower();
                        GameObject newTower = Instantiate(towers["tower_fire_3"], pos, Quaternion.identity) as GameObject;
                        parentNode.SetTowerObject(newTower);
                        towerSelected = newTower;
                        newTower.transform.parent = parentNode.transform;
                        
                        nextUpgradeCost.text = "";
                        towerSelected.GetComponent<Outline>().enabled = true;
                        selectedTowerStatus.text = towerSelected.GetComponent<TowerController>().formattedName;

                        SpawnMenu(towerSelected);
                        
                    }
                    else
                    {
                        guiStatusText.text = "업그레이드할 돈이 부족합니다.";
                    }
                }
                else if(towerSelected.GetComponent<TowerController>().GetTowerType() == "tower_fire_3")
                {
                    guiStatusText.text = "Fire Tower 업그레이드를 더이상 진행할 수 없습니다.";
                }

                else if (towerSelected.GetComponent<TowerController>().GetTowerType() == "tower_ice")
                {
                    if (GameController.Purchase(CostTemplate.GetCost("tower_ice_2")))
                    {
                        parentNode.ReplaceTower();
                        GameObject newTower = Instantiate(towers["tower_ice_2"], pos, Quaternion.identity) as GameObject;
                        parentNode.SetTowerObject(newTower);
                        towerSelected = newTower;
                        newTower.transform.parent = parentNode.transform;
                        //Component Testing Xav
                        towerSelected.GetComponent<Outline>().enabled = true;
                        nextUpgradeCost.text = CostTemplate.GetCost("tower_ice_3").ToString();
                        selectedTowerStatus.text = towerSelected.GetComponent<TowerController>().formattedName;
                        SpawnMenu(towerSelected);
                    }
                    else
                    {
                        guiStatusText.text = "업그레이드할 돈이 부족합니다.";
                    }
                }
                else if (towerSelected.GetComponent<TowerController>().GetTowerType() == "tower_ice_2")
                {
                    if (GameController.Purchase(CostTemplate.GetCost("tower_ice_3")))
                    {
                        parentNode.ReplaceTower();
                        GameObject newTower = Instantiate(towers["tower_ice_3"], pos, Quaternion.identity) as GameObject;
                        parentNode.SetTowerObject(newTower);
                        towerSelected = newTower;
                        newTower.transform.parent = parentNode.transform;

                        towerSelected.GetComponent<Outline>().enabled = true;
                        nextUpgradeCost.text = "";
                        selectedTowerStatus.text = towerSelected.GetComponent<TowerController>().formattedName;
                        SpawnMenu(towerSelected);
                    }
                    else
                    {
                        guiStatusText.text = "업그레이드할 돈이 부족합니다.";
                    }
                }
                else if (towerSelected.GetComponent<TowerController>().GetTowerType() == "tower_ice_3")
                {
                    guiStatusText.text = "ice Tower 업그레이드를 더이상 진행할 수 없습니다.";
                }

                //
                //NATURE TOWER UPGRADES
                //
                else if (towerSelected.GetComponent<TowerController>().GetTowerType() == "tower_nature")
                {
                    if (GameController.Purchase(CostTemplate.GetCost("tower_nature_2")))
                    {
                        parentNode.ReplaceTower();
                        GameObject newTower = Instantiate(towers["tower_nature_2"], pos, Quaternion.identity) as GameObject;
                        parentNode.SetTowerObject(newTower);
                        towerSelected = newTower;
                        newTower.transform.parent = parentNode.transform;

                        towerSelected.GetComponent<Outline>().enabled = true;
                        nextUpgradeCost.text = CostTemplate.GetCost("tower_nature_3").ToString();
                        selectedTowerStatus.text = towerSelected.GetComponent<TowerController>().formattedName;
                        SpawnMenu(towerSelected);
                    }
                    else
                    {
                        guiStatusText.text = "업그레이드할 돈이 부족합니다.";
                    }
                }
                else if (towerSelected.GetComponent<TowerController>().GetTowerType() == "tower_nature_2")
                {
                    if (GameController.Purchase(CostTemplate.GetCost("tower_nature_3")))
                    {
                        parentNode.ReplaceTower();
                        GameObject newTower = Instantiate(towers["tower_nature_3"], pos, Quaternion.identity) as GameObject;
                        parentNode.SetTowerObject(newTower);
                        towerSelected = newTower;
                        newTower.transform.parent = parentNode.transform;
                        towerSelected.GetComponent<Outline>().enabled = true;
                        nextUpgradeCost.text = "";
                        selectedTowerStatus.text = towerSelected.GetComponent<TowerController>().formattedName;
                        SpawnMenu(towerSelected);
                    }
                    else
                    {
                        guiStatusText.text = "업그레이드할 돈이 부족합니다.";
                    }
                }
                else if (towerSelected.GetComponent<TowerController>().GetTowerType() == "tower_nature_3")
                {
                    guiStatusText.text = "Nature Tower 업그레이드를 더이상 진행할 수 없습니다.";
                }
            }

            else if(type == 1) { }
            else if(type == 2)
            {
                // 타워 판매
                SellTower(towerSelected, parentNode);

                towerSelected = null;
                DeSpawnMenu();
            }
        }
    }

    public void DeSpawnMenu()
    {
        if (elemUpgradeMenu.activeSelf)
        {
            elemUpgradeMenu.SetActive(false);
        }

        if (baseUpradeMenu.activeSelf)
        {
            baseUpradeMenu.SetActive(false);
        }

        if (menuContainer.activeSelf)
        {
            menuContainer.SetActive(false);
        }
    }

    // 선택한 타워 유형에 따라 메뉴가 변경됩니다.
    public void SpawnMenu(GameObject tower)
    {
        if (tower.GetComponent<TowerController>().GetTowerType() == "tower_base")
        {
            DeSpawnMenu();
            menuContainer.SetActive(true);
            baseUpradeMenu.SetActive(true);
        }
        else
        {
            DeSpawnMenu();
            menuContainer.SetActive(true);
            elemUpgradeMenu.SetActive(true);
        }        
    }

    public void SetNextPurchaseCost(GameObject tower)
    {
        if(tower != null)
        {
            if (tower.GetComponent<TowerController>().GetTowerType() == "tower_fire")
            {
                nextUpgradeCost.text = CostTemplate.GetCost("tower_fire_2").ToString();
            }
            else if (tower.GetComponent<TowerController>().GetTowerType() == "tower_fire_2")
            {
                nextUpgradeCost.text = CostTemplate.GetCost("tower_fire_3").ToString();
            }
            else if (tower.GetComponent<TowerController>().GetTowerType() == "tower_ice")
            {
                nextUpgradeCost.text = CostTemplate.GetCost("tower_ice_2").ToString();
            }
            else if (tower.GetComponent<TowerController>().GetTowerType() == "tower_ice_2")
            {
                nextUpgradeCost.text = CostTemplate.GetCost("tower_ice_3").ToString();
            }
            
            else if (tower.GetComponent<TowerController>().GetTowerType() == "tower_nature")
            {
                nextUpgradeCost.text = CostTemplate.GetCost("tower_nature_2").ToString();
            }
            else if (tower.GetComponent<TowerController>().GetTowerType() == "tower_nature_2")
            {
                nextUpgradeCost.text = CostTemplate.GetCost("tower_nature_3").ToString();
            }
            else
            {
                nextUpgradeCost.text = "";
            }
        }
    }

    public void SellTower(GameObject selectedTower, NodeV2 parentNode)
    {
        // 판매된 위치 초기화
        parentNode.towerPlaced = false;
        parentNode.SetIsTowerPlacable(true);

        string towerType = selectedTower.GetComponent<TowerController>().GetTowerType();

        int amountReturned = CostTemplate.cost_dictionary[towerType];

        GameController.SellTowerGetMoney(amountReturned);

        parentNode.RemoveTower();
    }

    public bool GetSpawnable() { return spawnable; }

    public void SetSpawnable(bool spawnable) { this.spawnable = spawnable; }

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