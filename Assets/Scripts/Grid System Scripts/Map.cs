using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    [SerializeField] public GameObject m_nodeParent;
    [SerializeField] public GameObject m_mapObject;
    [SerializeField] public int m_mapSeed = 652697955;
    [SerializeField] public bool m_useRandomMap;

    // Ȯ�� ����
    [SerializeField] float m_exitDirectionWeight = 0.0f; // �ⱸ �켱 ����
    [SerializeField] float m_sameDirectionWeight = 5.0f; // ���� ���� ���� -> ���� ���� ��ΰ� �ܼ�����
    [SerializeField] float m_windyWeight = 5.0f; // ���� ����
    [SerializeField] float m_randomDirectionWeight = 3.0f; // ������ Ȯ�� �ʷ�(����ġ)

    // Default �� ũ��
    const int m_mapSizeX = 16;
    const int m_mapSizeY = 16;

    // ��� Reference
    Node[,] m_nodeMap;
    List<PathSegment> m_pathSegments;

    MapTemplate m_currentMapTemplate;
    public InputField m_userSeedField;

    void Start()
    {
        // �� ��� ��ġ ��_ �ʱ�ȭ
        InitNodes();

        if (m_useRandomMap)
        {
            // ������ ���ø� ����
            m_currentMapTemplate = GenerateRandomMapTemplate();
            m_mapSeed = m_currentMapTemplate.GetMapSeed();
        }
        else
        {
            m_currentMapTemplate = GenerateMapTemplateFromSeed(m_mapSeed);

            // ���Ͽ��� ���� ���ø� �ε��� ���_ �̿ϼ�
            // m_currentMapTemplate = LoadMapTemplateFromFile("defaultMap.json");
        }
        ApplyTemplateToMap(m_currentMapTemplate);
    }

    public void ResetMap() { Start(); }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { GenerateMap(); }
    }

    public List<PathSegment> GetPathSegments() { return m_pathSegments; }

    public Node GetNode(int row, int col) { return m_nodeMap[row, col]; }

    /**
     * �ʱ�ȭ
     * ȭ�鿡 ��带 �ҷ�����
     */
    void InitNodes()
    {
        Node[] nodes = m_nodeParent.GetComponentsInChildren<Node>();

        // Debug.Log("Node count: " + nodes.Length);

        // ��� �� ����
        m_nodeMap = new Node[m_mapSizeX, m_mapSizeY];
        for (int i = 0; i < nodes.Length; ++i)
        {
            int row = (int)Mathf.Floor(i / m_mapSizeX);
            int col = i % m_mapSizeX;
            m_nodeMap[row, col] = nodes[i];
        }
    }

    /**
     * ������ ���ø� ����
     * ���ø��� �ݿ��ϵ��� ��带 ����
     */
    public void ApplyTemplateToMap(MapTemplate mapTemplate)
    {
        Color pathStartColor = Color.cyan;
        Color pathEndColor = Color.red;

        Color pathColor = new Color(49.0f / 255.0f, 27.0f / 255.0f, 5.0f / 255.0f, 1.0f); // brown;
        Color terrainColor = new Color(22.0f / 255.0f, 117.0f / 255.0f, 22.0f / 255.0f, 1.0f); // green;
        Color chosenColor = terrainColor;

        // ���׸�Ʈ ��� �ε�
        m_pathSegments = mapTemplate.GetPathSegments();

        int row;
        int col;

        for (row = 0; row < m_mapSizeX; ++row)
            for (col = 0; col < m_mapSizeY; ++col)
                if (m_nodeMap[row, col])
                    m_nodeMap[row, col].SetOriginalColor(terrainColor);

        /*
        // ������ �۾�_ �̱���
        
        int globalPathTileCount = 0;
        int segmentLargestIndex = m_pathSegments.Count - 1;
        Debug.Log("segmetn count "+ (segmentLargestIndex+1));
        for (int s = 0; s <= segmentLargestIndex; ++s)
        {
            bool isLastSegment = s == segmentLargestIndex;

            PathSegment segment = m_pathSegments[s];

            int deltaX = segment.end.x - segment.start.x;
            int deltaY = segment.end.y - segment.start.y;

            int directionX = deltaX == 0 ? 0 : deltaX / Mathf.Abs(deltaX);
            int directionY = deltaY == 0 ? 0 : deltaY / Mathf.Abs(deltaY);

            int dy = 0;
            int dx = 0;
            bool isLastIteration = false;
            bool isDone = false;
            while (!isDone)
            {
                if (isLastIteration)
                    isDone = true;
                row = segment.start.y + dy;
                col = segment.start.x + dx;

                bool isFirstTile = globalPathTileCount == 0;
                bool isLastTile = isLastSegment && isLastIteration;
                if (isFirstTile)
                    chosenColor = pathStartColor;
                else if(isLastTile)
                    chosenColor = pathEndColor;
                else
                    chosenColor = pathColor;

                m_nodeMap[row, col].SetOriginalColor(chosenColor);

                ++globalPathTileCount;

                if (dx != deltaX)
                    dx += directionX;
                else if (dy != deltaY)
                    dy += directionY;

                isLastIteration = (dx == deltaX && dy == deltaY);
            }
        }
        */

        //*
        // ���� ���� �о����
        NodeTemplate template;

        for (row = 0; row < m_mapSizeY; ++row)
            for (col = 0; col < m_mapSizeX; ++col)
            {
                if (m_nodeMap[row, col])
                {
                    template = mapTemplate.GetNode(row, col);

                    chosenColor = terrainColor;
                    if (template.type == "path")
                        chosenColor = pathColor;

                    if (template.type == "start")
                        chosenColor = pathStartColor;

                    if (template.type == "end")
                        chosenColor = pathEndColor;

                    if (terrainColor != chosenColor)
                    {
                        m_nodeMap[row, col].SetOriginalColor(chosenColor);
                        m_nodeMap[row, col].SetNodeType(template.type);
                    }
                }
            }
         //*/
    }

    /**
     * ������ ���� ����
     */
    MapTemplate GenerateRandomMapTemplate()
    {
        MapGenerator mapGenerator = new MapGenerator(m_mapSizeX, m_mapSizeY);

        // �� Seed ����
        mapGenerator.SetSeed(Random.Range(0, System.Int32.MaxValue));

        // ���� ������ ����
        mapGenerator.SetExitDirectionWeight(m_exitDirectionWeight);
        mapGenerator.SetSameDirectionWeight(m_sameDirectionWeight);
        mapGenerator.SetWindyWeight(m_windyWeight);
        mapGenerator.SetRandomDirectionWeight(m_randomDirectionWeight);

        MapTemplate randomMap = mapGenerator.GenerateRandomMap();

        //Debug.Log("Map Seed: " + randomMap.GetMapSeed());
        return randomMap;
    }

    MapTemplate GenerateMapTemplateFromSeed(int seed)
    {
        MapGenerator mapGenerator = new MapGenerator(m_mapSizeX, m_mapSizeY);
        mapGenerator.SetSeed(seed);
        return mapGenerator.GenerateRandomMap();
    }

    MapTemplate LoadMapTemplateFromFile(string fileName)
    {
        string mapFile = "Assets/Data/" + fileName;
        string dataAsJson = File.ReadAllText(mapFile);
        return new MapTemplate(dataAsJson);
    }


    /**
     * ���ο� ���� ����
     */
    public void GenerateMap()
    {
        Destroy(m_mapObject);
        ResetMap();
    }

    public void GenerateMapFromSeed()
    {
        int result;
        string userInput = m_userSeedField.text;
        if (userInput != "")
        {
            try
            {
                result = System.Int32.Parse(userInput);
                Destroy(m_mapObject);
                InitNodes();
                m_currentMapTemplate = GenerateMapTemplateFromSeed(result);
                ApplyTemplateToMap(m_currentMapTemplate);
                ResetMap();
            }
            catch (System.FormatException)
            {
                System.Console.WriteLine($"�� Ȯ�� '{userInput}'");
            }
        }

    }
}