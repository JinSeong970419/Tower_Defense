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

    // 확률 관련
    [SerializeField] float m_exitDirectionWeight = 0.0f; // 출구 우선 순위
    [SerializeField] float m_sameDirectionWeight = 5.0f; // 방향 변경 관련 -> 적을 수록 경로가 단순해짐
    [SerializeField] float m_windyWeight = 5.0f; // 수평 여부
    [SerializeField] float m_randomDirectionWeight = 3.0f; // 무작위 확률 필러(가중치)

    // Default 맵 크기
    const int m_mapSizeX = 16;
    const int m_mapSizeY = 16;

    // 노드 Reference
    Node[,] m_nodeMap;
    List<PathSegment> m_pathSegments;

    MapTemplate m_currentMapTemplate;
    public InputField m_userSeedField;

    void Start()
    {
        // 각 노드 위치 맵_ 초기화
        InitNodes();

        if (m_useRandomMap)
        {
            // 무작위 템플릿 적용
            m_currentMapTemplate = GenerateRandomMapTemplate();
            m_mapSeed = m_currentMapTemplate.GetMapSeed();
        }
        else
        {
            m_currentMapTemplate = GenerateMapTemplateFromSeed(m_mapSeed);

            // 파일에서 지도 템플릿 로드할 경우_ 미완성
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
     * 초기화
     * 화면에 노드를 불러오기
     */
    void InitNodes()
    {
        Node[] nodes = m_nodeParent.GetComponentsInChildren<Node>();

        // Debug.Log("Node count: " + nodes.Length);

        // 노드 맵 생성
        m_nodeMap = new Node[m_mapSizeX, m_mapSizeY];
        for (int i = 0; i < nodes.Length; ++i)
        {
            int row = (int)Mathf.Floor(i / m_mapSizeX);
            int col = i % m_mapSizeX;
            m_nodeMap[row, col] = nodes[i];
        }
    }

    /**
     * 지도에 템플릿 적용
     * 템플릿을 반영하도록 노드를 수정
     */
    public void ApplyTemplateToMap(MapTemplate mapTemplate)
    {
        Color pathStartColor = Color.cyan;
        Color pathEndColor = Color.red;

        Color pathColor = new Color(49.0f / 255.0f, 27.0f / 255.0f, 5.0f / 255.0f, 1.0f); // brown;
        Color terrainColor = new Color(22.0f / 255.0f, 117.0f / 255.0f, 22.0f / 255.0f, 1.0f); // green;
        Color chosenColor = terrainColor;

        // 세그먼트 경로 로드
        m_pathSegments = mapTemplate.GetPathSegments();

        int row;
        int col;

        for (row = 0; row < m_mapSizeX; ++row)
            for (col = 0; col < m_mapSizeY; ++col)
                if (m_nodeMap[row, col])
                    m_nodeMap[row, col].SetOriginalColor(terrainColor);

        /*
        // 랜더링 작업_ 미구현
        
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
        // 지도 직접 읽어오기
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
     * 임의의 지도 생성
     */
    MapTemplate GenerateRandomMapTemplate()
    {
        MapGenerator mapGenerator = new MapGenerator(m_mapSizeX, m_mapSizeY);

        // 맵 Seed 설정
        mapGenerator.SetSeed(Random.Range(0, System.Int32.MaxValue));

        // 지도 생성값 설정
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
     * 새로운 지도 생성
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
                System.Console.WriteLine($"재 확인 '{userInput}'");
            }
        }

    }
}