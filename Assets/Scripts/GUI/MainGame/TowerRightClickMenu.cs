using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerRightClickMenu : MonoBehaviour
{
    [SerializeField] GameObject rightClickMenu;

    //GUI Menu Dropdown
    private Dropdown m_Dropdown;
    private GameObject selected_tower;

    //GUI Status Text
    [SerializeField] TextMeshProUGUI guiStatusText;


    [SerializeField] GameObject base_tower;
    [SerializeField] GameObject fire_tower;
    [SerializeField] GameObject ice_tower;
    [SerializeField] GameObject nature_tower;

    // 메뉴 불러올 때 충돌 방지
    [SerializeField] LayerMask towerSphereCollider;

    [SerializeField] Wallet playerWallet;

    void Start()
    {
        m_Dropdown = rightClickMenu.GetComponentInChildren<Dropdown>();

        m_Dropdown.onValueChanged.AddListener(delegate
        {
            ChangeTowerColor(m_Dropdown, selected_tower);
        });
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~towerSphereCollider))
            {
                if (hit.transform.gameObject.CompareTag("Tower") || hit.transform.gameObject.CompareTag("FireTower") || hit.transform.gameObject.CompareTag("IceTower") || hit.transform.gameObject.CompareTag("NatureTower"))
                {

                    //Attach selected_tower to tower that will potentially change
                    //Needed to change to parent GameObject
                    selected_tower = hit.transform.parent.gameObject;

                    if (!rightClickMenu.activeSelf)
                    {
                        rightClickMenu.transform.position = Input.mousePosition;
                        rightClickMenu.SetActive(true);
                    }

                }
                else { selected_tower = null; }
            }
        }
    }

    void ChangeTowerColor(Dropdown change, GameObject selected_tower)
    {
        if (change.value == 1)
        {
            // 지갑 잔고 확인 및 타워 우클릭
            // 하드 코딩...
            if (GameController.Purchase(CostTemplate.GetCost("tower_fire")))
            {
                ChangeTower(selected_tower, "fire");
            }
            else
            {
                guiStatusText.text = "Fire 업그레이드할 돈이 부족합니다.";
            }
        }
        else if (change.value == 2)
        {
            if (GameController.Purchase(CostTemplate.GetCost("tower_ice")))
            {
                ChangeTower(selected_tower, "ice");

            }
            else
            {
                guiStatusText.text = "Ice 업그레이드할 돈이 부족합니다.";
            }
        }
        else if (change.value == 3)
        {
            if (GameController.Purchase(CostTemplate.GetCost("tower_nature")))
            {
                ChangeTower(selected_tower, "nature");
            }
            else
            {
                guiStatusText.text = "Nature 업그레이드할 돈이 부족합니다.";
            }
        }
        else if(change.value == 4)
        {
            //Sell Tower
            SellTower(selected_tower);
            guiStatusText.text = "Tower가 판매되었습니다.";

        }
        else if(change.value == 5)
        {
            // 기본 타워인지 확인
            if (selected_tower.GetComponent<TowerController>().CompareTag("Tower"))
            {
                guiStatusText.text = "타워를 DownGrade 할 수 없습니다.";
            }
            else
            {
                GameController.SellTowerGetMoney(100);
                ChangeTower(selected_tower, "Base");
                guiStatusText.text = "Tower downgraded.";

            }
        }
        else
        {
            // 기본 동작
        }
    }

    public void ChangeTower(GameObject tower, string type)
    {

        NodeV2 node = tower.transform.parent.transform.gameObject.GetComponent<NodeV2>();
        node.RemoveTower();
        Vector3 pos = node.GetTowerObject().transform.position;
        GameObject newTower = null;

        switch (type)
        {
            case "fire":
                newTower = Instantiate(fire_tower, pos, Quaternion.identity) as GameObject;
                break;
            case "ice":
                newTower = Instantiate(ice_tower, pos, Quaternion.identity) as GameObject;
                break;
            case "nature":
                newTower = Instantiate(nature_tower, pos, Quaternion.identity) as GameObject;
                break;
            case "base":
                newTower = Instantiate(base_tower, pos, Quaternion.identity) as GameObject;
                break;
        }

        if (!newTower.Equals(null)) { newTower.transform.parent = node.transform; }
        node.SetTowerObject(newTower);
    }

    public void SellTower(GameObject selectedTower)
    {
        // 이전 배치된 node false로 설정
        selectedTower.transform.parent.transform.gameObject.GetComponent<Node>().towerPlaced = false;
        string towerType = selectedTower.GetComponent<TowerController>().GetTowerType();
        int amountReturned = CostTemplate.cost_dictionary[towerType];
        GameController.SellTowerGetMoney(amountReturned);
        Destroy(selectedTower);
    }

    public void DeselectTower() { selected_tower = null; }
}
