using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    int m_mapSizeX;
    int m_mapSizeY;
    int m_pathLength = 1000;
    int m_mapBorder = 1;
    int m_endScene = 0;
    int m_bufferEnd = 4;
    int m_randomSeed = 42;

    float m_exitDirectionWeight = 0.0f; // 출구 진행 우선 순위
    float m_previousDirectionWeight = 5.0f; // 방향 변경 횟수 -> simpler path
    float m_windyWeight = 5.0f; // 맵 수평으로
    float m_randomDirectionWeight = 3.0f; // 무작위 filler 가중치

    public MapGenerator(int rows, int cols)
    {
        m_randomSeed = Random.Range(0, System.Int32.MaxValue);
        Random.InitState(m_randomSeed);
        m_mapSizeX = rows;
        m_mapSizeY = cols;
    }

    public void SetSeed(int seed)
    {
        m_randomSeed = seed;
        Random.InitState(m_randomSeed);
    }

    public void SetExitDirectionWeight(float val)
    {
        if (val >= 0)
            m_exitDirectionWeight = val;
    }

    public void SetSameDirectionWeight(float val)
    {
        if (val >= 0)
            m_previousDirectionWeight = val;
    }

    public void SetWindyWeight(float val)
    {
        if (val >= 0)
            m_windyWeight = val;
    }

    public void SetRandomDirectionWeight(float val)
    {
        if (val >= 0)
            m_randomDirectionWeight = val;
    }


    List<Vector2Int> GenerateRandomPath()
    {
        int endGameBuffer = 2 + m_bufferEnd + m_endScene;

        // 확률
        float exitDirectionWeight       = m_exitDirectionWeight;       
        float previousDirectionWeight   = m_previousDirectionWeight;   
        float windyWeight               = m_windyWeight;                
        float randomDirectionWeight     = m_randomDirectionWeight;      

        float oddsTotal = 0;
        SortedDictionary<string, Vector2> Odds = new SortedDictionary<string, Vector2>();
        Odds.Add("exitDirection",       new Vector2(oddsTotal, oddsTotal += exitDirectionWeight));
        Odds.Add("previousDirection",   new Vector2(oddsTotal, oddsTotal += previousDirectionWeight));
        Odds.Add("windy",               new Vector2(oddsTotal, oddsTotal += windyWeight));
        Odds.Add("randomDirection",     new Vector2(oddsTotal, oddsTotal += randomDirectionWeight));

        Vector2Int start = new Vector2Int(
            BetterRandomRange(m_mapBorder + m_bufferEnd, m_mapSizeX - 1 - m_mapBorder - m_bufferEnd),
            0
        );
        Vector2Int end = new Vector2Int(
            BetterRandomRange(m_mapBorder + m_bufferEnd, m_mapSizeX - 1 - m_mapBorder - m_bufferEnd),
            m_mapSizeY - 1 - m_bufferEnd
        );
        
        // 경로 설정
        bool[,] pathExistsOnNode = new bool[m_mapSizeX, m_mapSizeY];
        for (int x=0; x < m_mapSizeX; ++x)
            for (int y=0; y < m_mapSizeY; ++y)
                pathExistsOnNode[x, y] = false;

        // Initialize path
        List<Vector2Int> pathNodes = new List<Vector2Int>();

        // 출발점 추가
        Vector2Int next = start;
        Vector2Int current = next;
        pathNodes.Add(current);
        pathExistsOnNode[current.x, current.y] = true;
        

        Vector2Int previousDirection = new Vector2Int(0, 1);
        Vector2Int moveDirection = new Vector2Int(0, 1);

        while (current != end && pathNodes.Count < m_pathLength)
        {
            if (current.y + endGameBuffer >= end.y)
            {
                if (current.y + endGameBuffer-2 >= end.y) {
                    moveDirection = AlignWithExitDirection(current, end);
                }
                else
                {
                    // 이전 경로 사이 공간 주기
                    moveDirection = ApproachExitDirection(current, end);
                }
            }
            else
            {
                // 가끔 길을 헤맴...
                List<Vector2Int> possibleDirections = GetAvailableDirections(current, end, ref pathExistsOnNode);

                // 방향 분석
                bool previousDirectionPossible = false;
                bool towardExitDirectionPossible = false;
                bool horizontalDirectionsPossible = false;
                List<Vector2Int> horizontalDirections = new List<Vector2Int>();

                for (int i = 0; i < possibleDirections.Count; ++i)
                {
                    // 출구 이동 가능 확인
                    if (possibleDirections[i] == moveDirection)
                        towardExitDirectionPossible = true;

                    // 방향 유지 확인
                    if (possibleDirections[i] == previousDirection)
                        previousDirectionPossible = true;

                    // 수평 움직임 확인
                    if (possibleDirections[i].y == 0)
                    {
                        horizontalDirectionsPossible = true;
                        horizontalDirections.Add(possibleDirections[i]);
                    }
                }

                // 무작위 방향으로 진행 방향 결정
                bool isDirectionApplied = false;
                string directionMode = "exitDirection"; // 기본 목표 출구
                float randomNumber = Random.Range(0.0f, oddsTotal);
                foreach (KeyValuePair<string, Vector2> entry in Odds)
                {
                    if (entry.Value.x <= randomNumber && randomNumber <= entry.Value.y)
                    {
                        directionMode = entry.Key;
                        break;
                    }
                }

                // 방향 선택
                if (horizontalDirectionsPossible && directionMode == "windy")
                {
                    isDirectionApplied = true;
                    moveDirection = horizontalDirections[BetterRandomRange(0, horizontalDirections.Count - 1)];
                }
                // 출구 이동
                if (towardExitDirectionPossible && directionMode == "exitDirection")
                {
                    isDirectionApplied = true;
                    moveDirection = ApproachExitDirection(current, end);
                }

                // 현재 방향 유지
                if (previousDirectionPossible && directionMode == "previousDirection")
                {
                    isDirectionApplied = true;
                    moveDirection = previousDirection;
                }

                if (!isDirectionApplied)
                {
                    if (possibleDirections.Count > 0)
                        moveDirection = possibleDirections[BetterRandomRange(0, possibleDirections.Count - 1)];
                    else
                    {
                        //Debug.Log("나갈 가능성 없음");
                        break;
                    }
                }
            }
            
            // Set tile
            next = current + moveDirection;
            current = next;
            pathNodes.Add(current);
            pathExistsOnNode[current.x, current.y] = true;
            previousDirection = moveDirection;

        }
        return pathNodes;
    }


    int BetterRandomRange(int min, int max)
    {
        return Mathf.RoundToInt(Random.Range(min * 100.0f, max * 100.0f) / 100.0f);
    }

    List<Vector2Int> GetAvailableDirections(Vector2Int current, Vector2Int end, ref bool[,] pathExistsOnNode)
    {
        List<Vector2Int> possibleDirections = new List<Vector2Int>();
        possibleDirections.Add(Vector2Int.right);
        possibleDirections.Add(Vector2Int.left);
        possibleDirections.Add(Vector2Int.up);

        List<Vector2Int> availableDirections = new List<Vector2Int>();

        bool isNotAlreadyPath;
        bool isTooCloseToAnother;
        bool isPossibleToReachEnd = true; // @TODO
        Vector2Int possiblePosition;

        for (int i = 0; i < possibleDirections.Count; ++i)
        {
            possiblePosition = current + possibleDirections[i];
            if (IsPositionInBounds(possiblePosition))
            {
                // 주변 경로 확인
                isTooCloseToAnother = CheckIfTooCloseToPath(current, possibleDirections[i], ref pathExistsOnNode);
                if (!isTooCloseToAnother && isPossibleToReachEnd) {
                    availableDirections.Add(possibleDirections[i]);
                }
            }
        }

        return availableDirections;
    }

    bool IsPositionInBounds(Vector2Int pos)
    {
        return pos.x >= m_mapBorder && pos.x < m_mapSizeX-m_mapBorder
           &&  pos.y >= m_mapBorder && pos.y < m_mapSizeY-m_mapBorder;
    }


    bool CheckIfTooCloseToPath(Vector2Int current, Vector2Int direction, ref bool[,] pathExistsOnNode)
    {
        Vector2Int possiblePosition = current + direction;
        if (pathExistsOnNode[possiblePosition.x, possiblePosition.y])
            return true;

        Vector2Int temp;
        Vector2Int vVector = new Vector2Int(0, 1);
        Vector2Int hVector = new Vector2Int(1, 0);
        List<Vector2Int> checkPositions = new List<Vector2Int>();

        // 이동 방향 양쪽 타일 확인
        if (direction.y == 0) { 
            // 수평 이동
            temp = possiblePosition + vVector;
            if(IsPositionInBounds(temp))
                checkPositions.Add(temp);

            temp = possiblePosition - vVector;
            if (IsPositionInBounds(temp))
                checkPositions.Add(temp);
        }
        else
        {
            temp = possiblePosition + hVector;
            if (IsPositionInBounds(temp))
                checkPositions.Add(temp);

            temp = possiblePosition - hVector;
            if (IsPositionInBounds(temp))
                checkPositions.Add(temp);
        }
        for (int i = 0; i < checkPositions.Count; ++i)
        {
            if (pathExistsOnNode[checkPositions[i].x, checkPositions[i].y])
                return true;
        }
        return false;
    }

    
    // 출구쪽 이동 정렬
    Vector2Int ApproachExitDirection(Vector2Int current, Vector2Int end)
    {
        Vector2Int exitDirection = new Vector2Int(0, 0);
        if(end.y > current.y)
            exitDirection.y = 1;
        else if (end.y < current.y)
            exitDirection.y = -1;
        else if(end.x > current.x)
            exitDirection.x = 1;
        else if (end.x < current.x)
            exitDirection.x = -1;
        return exitDirection;
    }


    Vector2Int AlignWithExitDirection(Vector2Int current, Vector2Int end)
    {
        Vector2Int exitDirection = new Vector2Int(0, 0);
        if (end.x > current.x)
            exitDirection.x = 1;
        else if (end.x < current.x)
            exitDirection.x = -1;
        else if (end.y > current.y)
            exitDirection.y = 1;
        else if (end.y < current.y)
            exitDirection.y = -1;
        return exitDirection;
    }


    // 이동 목록 반환
    List<Vector2Int> MovesTowardsExit(Vector2Int current, Vector2Int end)
    {
        List<Vector2Int> exitMoves = new List<Vector2Int>();
        if (end.y > current.y)
            exitMoves.Add(new Vector2Int(0, 1));
        if (end.y < current.y)
            exitMoves.Add(new Vector2Int(0, -1));
        if (end.x > current.x)
            exitMoves.Add(new Vector2Int(1, 0));
        if (end.x < current.x)
            exitMoves.Add(new Vector2Int(-1, 0));
        return exitMoves;
    }

    bool IsMoveSideways(Vector2Int direction) { return direction.x != 0; }

    public MapTemplate GenerateRandomMap()
    {
        List<Vector2Int> pathList = GenerateRandomPath();

        // Initialize map
        MapTemplate generatedMap = new MapTemplate(m_mapSizeY, m_mapSizeX);

        // seed 가져오기
        generatedMap.SetMapSeed(m_randomSeed);
        ApplyPathToMap(pathList, generatedMap);
        return generatedMap;
    }

    void ApplyPathToMap(List<Vector2Int> pathList, MapTemplate template)
    {
        // 경로 세그먼트 빌드
        List<PathSegment> pathSegments = new List<PathSegment>();
        Vector2Int previousCoord = new Vector2Int(0, 0);
        PathSegment currentSegment = new PathSegment(previousCoord);
        Vector2Int previousDirection = new Vector2Int(0, 0);
        Vector2Int currentDirection = new Vector2Int(0, 0);     // used to see if we still going the same direction


        string currentAxis = "x";
        string previousAxis = currentAxis;


        List<MapTemplate.WallCoord> leftWall = new List<MapTemplate.WallCoord>();
        List<MapTemplate.WallCoord> rightWall = new List<MapTemplate.WallCoord>();

        NodeTemplate.Wall currentLeftWall = NodeTemplate.Wall.N;
        NodeTemplate.Wall currentRightWall = NodeTemplate.Wall.S;

        NodeTemplate.Wall previousLeftWall = currentLeftWall;
        NodeTemplate.Wall previousRightWall = currentRightWall;


        // 기본 방향 단축
        NodeTemplate.Wall N = NodeTemplate.Wall.N;
        NodeTemplate.Wall E = NodeTemplate.Wall.E;
        NodeTemplate.Wall S = NodeTemplate.Wall.S;
        NodeTemplate.Wall W = NodeTemplate.Wall.W;

        // 지도에 경로 확인
        var arr = pathList.ToArray();
        NodeTemplate node;
        int segmentC = 1;

        bool isLastTile = false;
        bool isFirstTile = false;
        bool isSecondTile = false;
        bool updateWallSides = false;
        for (int i = 0; i < arr.Length; ++i)
        {
            updateWallSides = false;
            isLastTile = i == arr.Length - 1;
            isFirstTile = i == 0;
            isSecondTile = i == 1;

            Vector2Int currentCoord = arr[i];

            // 노드 할당
            node = template.GetNode(currentCoord.y, currentCoord.x);
            node.isTowerPlacable = false;
            if (i == 0)
                node.type = "start";
            else if (i == arr.Length - 1)
            {
                node.type = "end";
                for(int p=-2; p<3; ++p)
                {
                    for(int j=-2; j<4; ++j)
                    {
                        Vector2Int tempCoord = currentCoord + new Vector2Int(p, j);
                        NodeTemplate temp = template.GetNode(tempCoord.y, tempCoord.x);

                        if(temp != null)
                        {
                            temp.isEnvPlacable = false;
                            temp.isTowerPlacable = false;
                        }
                    }
                }

            }
            else { node.type = "path"; }
                
            // 첫 번째 노드 확인
            if (isFirstTile)
            {
                currentSegment.start = currentCoord;
            } else {
                // 두 번째 노드
                if (isSecondTile)
                {
                    // 이동 설정
                    currentDirection = currentCoord - previousCoord;
                    previousDirection = currentDirection;

                    currentSegment.end = currentCoord;
                    currentAxis = previousCoord.x != currentCoord.x ? "y" : "x";
                    currentSegment.SetAxis(currentAxis);

                    // Track wall
                    leftWall.Add(new MapTemplate.WallCoord(previousLeftWall, previousCoord));
                    rightWall.Add(new MapTemplate.WallCoord(previousRightWall, previousCoord));
                }
                else
                {
                    // 같은 방향 확인
                    currentDirection = currentCoord - previousCoord;
                    if (currentDirection == previousDirection)
                    {
                        // 증분 세그먼트
                        currentSegment.end = currentCoord;

                        // Track wall
                        leftWall.Add(new MapTemplate.WallCoord(previousLeftWall, previousCoord));
                        rightWall.Add(new MapTemplate.WallCoord(previousRightWall, previousCoord));
                    }
                    // 방향 번경
                    else
                    {
                        ++segmentC;
                        // Save segment
                        pathSegments.Add(currentSegment);
                        // 이전 위치에서 새 세그먼트 시작
                        currentSegment = new PathSegment(previousCoord, currentCoord);

                        currentAxis = previousCoord.x != currentCoord.x ? "y" : "x";
                        currentSegment.SetAxis(currentAxis);

                        updateWallSides = true;

                    }
                }
            }

            if (isLastTile)
            {
                pathSegments.Add(currentSegment);
                updateWallSides = true;
            }

            // 시작 끝 설정
            if (isFirstTile || isLastTile)
            {
                NodeTemplate currentNodeTemplate = template.GetNode(currentCoord.y, currentCoord.x);
                // Vertical
                currentNodeTemplate.hasWall[(int)NodeTemplate.Wall.N] = true;
                currentNodeTemplate.hasWall[(int)NodeTemplate.Wall.S] = true;

                // 경로 끝 기둥제외
                if (isFirstTile)
                {
                    currentNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NE] = true;
                    currentNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.SE] = true;
                }
            }

            if (previousDirection == currentDirection)
            {
                NodeTemplate previousNodeTemplate = template.GetNode(previousCoord.y, previousCoord.x);
                if (previousDirection.y == 0)
                {
                    // Vertical
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.E] = true;
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.W] = true;

                    if (!isLastTile)
                    {
                        previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NE] = true;
                        previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NW] = true;
                    }
                }
                else
                {
                    // Horizontal
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.N] = true;
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.S] = true;

                    if (!isLastTile)
                    {
                        previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.SE] = true;
                        previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NE] = true;
                    }
                }
            }
            else if (i != 1)
            {
                NodeTemplate previousNodeTemplate = template.GetNode(previousCoord.y, previousCoord.x);

                // Vertically
                if (-1 * previousDirection.x == 1)
                {
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.N] = true;
                    previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NE] = true;
                }
                if (-1 * previousDirection.x == -1)
                {
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.S] = true;
                    previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.SE] = true;
                }

                // Horizontally
                if (-1 * previousDirection.y == 1)
                {
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.W] = true;
                    previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NW] = true;
                }
                if (-1 * previousDirection.y == -1)
                {
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.E] = true;
                    previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NE] = true;
                }

                // Handle Corner
                if (-1 * currentDirection.x == 1)
                {
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.S] = true;
                    previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.SE] = true;
                }
                if (-1 * currentDirection.x == -1)
                {
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.N] = true;
                    previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NE] = true;
                }
                if (-1 * currentDirection.y == 1)
                {
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.E] = true;
                    previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NE] = true;
                }
                if (-1 * currentDirection.y == -1)
                {
                    previousNodeTemplate.hasWall[(int)NodeTemplate.Wall.W] = true;
                    previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NW] = true;
                }

                if (-1 * previousDirection.x == -1 && currentDirection.x == 0) // Down to horizontal
                {
                    previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.SW] = true;
                    previousNodeTemplate.hasPillar[(int)NodeTemplate.Pillar.NE] = true;
                }
            }

            if (updateWallSides)
            {
                if (isLastTile)
                {
                    previousAxis = currentAxis;

                    previousLeftWall = currentLeftWall;
                    previousRightWall = currentRightWall;

                    previousCoord = currentCoord;
                    previousDirection = currentDirection;
                }


                // 벡터 N,E,S,W
                NodeTemplate.Wall previousTowardWall = NodeTemplate.Wall.E;
                NodeTemplate.Wall currentTowardWall = NodeTemplate.Wall.E;
                if (previousDirection.y == 0)
                    previousTowardWall = (previousDirection.x < 0) ? N : S;
                else
                    previousTowardWall = (previousDirection.y < 0) ? W : E;

                if (currentDirection.y == 0)
                    currentTowardWall = (currentDirection.x < 0) ? N : S;
                else
                    currentTowardWall = (currentDirection.y < 0) ? W : E;

                /*
                Hashtable dirMap = new Hashtable();
                dirMap.Add(NodeTemplate.Wall.N, "North");
                dirMap.Add(NodeTemplate.Wall.E, "East");
                dirMap.Add(NodeTemplate.Wall.S, "South");
                dirMap.Add(NodeTemplate.Wall.W, "West");

                */

                bool turnedLeft = false;
                // Right -> up
                if (previousTowardWall == E && currentTowardWall == N)
                    turnedLeft = true;

                // Down -> Right
                if (previousTowardWall == S && currentTowardWall == E)
                    turnedLeft = true;

                // Left -> Down
                if (previousTowardWall == W && currentTowardWall == S)
                    turnedLeft = true;

                // UP -> Left
                if (previousTowardWall == N && currentTowardWall == W)
                    turnedLeft = true;



                // Rotate Coordiantes (N, E, S, W = 0, 1, 2, 3) % 4
                if (turnedLeft)
                {
                    // 왼쪽 벽 기준
                    rightWall.Add(new MapTemplate.WallCoord(NodeTemplate.IncrementWall(previousLeftWall, 2), previousCoord)); // Add S wall
                    rightWall.Add(new MapTemplate.WallCoord(NodeTemplate.IncrementWall(previousLeftWall, 1), previousCoord)); // Add E wall
                    currentLeftWall = NodeTemplate.IncrementWall(previousLeftWall, 3);  // W
                    currentRightWall = NodeTemplate.IncrementWall(previousLeftWall, 1); // E
                }
                else
                {
                    // 오른쪽 벽 기준
                    leftWall.Add(new MapTemplate.WallCoord(NodeTemplate.IncrementWall(previousRightWall, 2), previousCoord)); // Add S wall
                    leftWall.Add(new MapTemplate.WallCoord(NodeTemplate.IncrementWall(previousRightWall, 3), previousCoord)); // Add W wall
                    currentLeftWall = NodeTemplate.IncrementWall(previousRightWall, 3); // W
                    currentRightWall = NodeTemplate.IncrementWall(previousRightWall, 1);// E
                }
            }

            previousAxis = currentAxis;

            previousLeftWall = currentLeftWall;
            previousRightWall = currentRightWall;

            previousCoord = currentCoord;
            previousDirection = currentDirection;
        }// end



        // 지도에 매개변수 표시
        for (int row = 0; row < m_mapSizeY; ++row) 
        {
            template.GetNode(row, 0).isPlayablePerimeter = true;
            template.GetNode(row, m_mapSizeX-1).isPlayablePerimeter = true;
        }

        for (int col = 0; col < m_mapSizeX; ++col)
        {
            template.GetNode(0, col).isPlayablePerimeter = true;
            template.GetNode(m_mapSizeY-1, col).isPlayablePerimeter = true;
        }

        for (int i = 0; i < leftWall.Count; ++i)
        {
            if (i % 4 == 0)
            {
                MapTemplate.WallCoord walCoord = leftWall[i];
                NodeTemplate nodeTemplate = template.GetNode(walCoord.coord.y, walCoord.coord.x);

                NodeTemplate.Wall wall = walCoord.wall;
                //nodeTemplate.hasWall[(int)wall] = false;

                nodeTemplate.hasLamp[(int)wall] = true;
            }
        }

        for (int i = 0; i < rightWall.Count; ++i)
        {
            if ((i+2) % 4 == 0)
            {
                MapTemplate.WallCoord walCoord = rightWall[i];
                NodeTemplate nodeTemplate = template.GetNode(walCoord.coord.y, walCoord.coord.x);

                NodeTemplate.Wall wall = walCoord.wall;
                //nodeTemplate.hasWall[(int)wall] = false;

                nodeTemplate.hasLamp[(((int)wall)%4)] = true;
            }
        }

        template.SetLeftWall(leftWall);
        template.SetRightWall(rightWall);
        template.SetPathSegments(pathSegments);
    }
}
