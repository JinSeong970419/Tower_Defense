using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeV2 : MonoBehaviour
{

    [SerializeField] public float m_height = 10F;
    [SerializeField] string m_type = "terrain";
    protected int row;
    protected int col;
    protected bool isTowerPlacable = false;
    protected bool isPlayablePerimeter = false;
    protected bool isEnvPlacable = true;
    public bool towerPlaced = false;
    GameObject towerObject = null;
    GameObject envObject = null;

    public void SetCoordinate(Vector2Int coord)
    {
        row = coord.y;
        col = coord.x;
    }

    public Vector2Int GetCoordinate()
    {
        return new Vector2Int(col, row);
    }


    public void SetIsTowerPlacable(bool isTowerPlacable)
    {
        this.isTowerPlacable = isTowerPlacable;
    }

    public bool GetIsTowerPlacable()
    {
        return isTowerPlacable;
    }

    public void SetTowerObject(GameObject tower)
    {
        towerObject = tower;
    }
    public GameObject GetTowerObject()
    {
        return towerObject;
    }

    public void ReplaceTower()
    {
        if(towerObject!= null)
        {
            Destroy(towerObject);
            towerObject = null;
        }
    }

    public void RemoveTower()
    {
        if(towerObject != null)
        {
            Destroy(towerObject);
            towerObject = null;
            towerPlaced = false;
        }
    }

    public void SetEnvObject(GameObject env)
    {
        envObject = env;
    }
    public GameObject GetEnvObject()
    {
        return envObject;
    }

    public void DeleteEnvObject()
    {
        if(envObject != null)
        {
            Destroy(envObject);
        }
    }



    public void SetIsEnvPlacable(bool val)
    {
        isEnvPlacable = val;
    }
    public bool GetIsEnvPlacable()
    {
        return isEnvPlacable;
    }

    public void SetIsPlayablePerimeter(bool val)
    {
        isPlayablePerimeter = val;
    }
    public bool GetPlayablePerimeter()
    {
        return isPlayablePerimeter;
    }

    public void SetNodeType(string type)
    {
        m_type = type;
    }

    public string GetNodeType()
    {
        return m_type;
    }

    public bool GetIsPath()
    {
        return m_type == "start" || m_type == "path" || m_type == "end";
    }


    public Vector3 GetEnvOffset()
    {
        return new Vector3(0, m_height, 0);
    }

    public void SetIsHighlighted()
    {

    }


    // DEPRECATED
    public void SetOriginalColor(Color color)
    {
        
    }

}