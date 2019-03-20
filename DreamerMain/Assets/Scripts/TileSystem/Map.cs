using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Map : MonoBehaviour
{
    public const int tileSize = 128;

    public TileType[,] tilesCollisionMap;
    public Vector3 mapPosition;

    // map size
    public int width = 80;
    public int height = 60;

    [HideInInspector] public List<PhysicsBody>[,] objectsInArea;
    [HideInInspector] public int horizontalAreasCount;
    [HideInInspector] public int verticalAreasCount;

    [Header("Area size in tile")] // separation of areas for optimal collision checking
    [SerializeField] private int gridAreaWidth = 16;
    [SerializeField] private int gridAreaHeight = 16;

    // update areas for check collision - UpdateAreas
    private static List<Vector3Int> overlappingAreas = new List<Vector3Int>(4); // max 4 areas

    private void Awake()
    {
        TileSlopes.Init();
        Init();
    }
    public void Init()
    {
        mapPosition = transform.position;
        tilesCollisionMap = new TileType[width, height];

        horizontalAreasCount = Mathf.CeilToInt(width / (float)gridAreaWidth);
        verticalAreasCount = Mathf.CeilToInt(height / (float)gridAreaHeight);

        objectsInArea = new List<PhysicsBody>[horizontalAreasCount, verticalAreasCount];

        for (var y = 0; y < verticalAreasCount; ++y)
            for (var x = 0; x < horizontalAreasCount; ++x)
                objectsInArea[x, y] = new List<PhysicsBody>();
    }

    public void LateUpdate()
    {
        CheckCollisions();
    }
    public void CheckCollisions()
    {
        Vector2 overlap;

        for (int y = 0; y < verticalAreasCount; ++y)
        {
            for (int x = 0; x < horizontalAreasCount; ++x)
            {
                var objectsInArea = this.objectsInArea[x, y];
                for (int i = 0; i < objectsInArea.Count - 1; ++i)
                {
                    var obj1 = objectsInArea[i];
                    for (int j = i + 1; j < objectsInArea.Count; ++j)
                    {
                        var obj2 = objectsInArea[j];

                        if (obj1.bBox.OverlapsSigned(obj2.bBox, out overlap) && obj1.HasCollisionDataFor(obj2))
                        {
                            obj1.allCollidingObjects.Add(new CollisionData(obj2, overlap, obj1.speedForse, obj2.speedForse, obj1.previousPosition, obj2.previousPosition, obj1.position, obj2.position));
                            obj2.allCollidingObjects.Add(new CollisionData(obj1, -overlap, obj2.speedForse, obj1.speedForse, obj2.previousPosition, obj1.previousPosition, obj2.position, obj1.position));
                        }
                    }
                }
            }
        }
    }

    #region add and remove area
    public void RemoveObjectFromArea(Vector3Int areaIndex, int objIndexInArea, PhysicsBody obj)
    {
        var area = objectsInArea[areaIndex.x, areaIndex.y];
        // swap the last item with the one we are deleting
        var tmp = area[area.Count - 1];
        area[area.Count - 1] = obj;
        if (objIndexInArea < area.Count) area[objIndexInArea] = tmp;

        var tmpIds = tmp.idsInAreas;
        var tmpAreas = tmp.areas;
        for (int i = 0; i < tmpAreas.Count; ++i)
        {
            if (tmpAreas[i] == areaIndex)
            {
                tmpIds[i] = objIndexInArea;
                break;
            }
        }
        // remove last object
        area.RemoveAt(area.Count - 1);
    }

    public void AddObjectToArea(Vector3Int areaIndex, PhysicsBody obj)
    {
        var area = objectsInArea[areaIndex.x, areaIndex.y];
        // save object index in area
        obj.areas.Add(areaIndex);
        obj.idsInAreas.Add(area.Count);
        // add to list
        area.Add(obj);
    }
    #endregion

    public void UpdateAreas(PhysicsBody obj)
    {
        // get area in 4 poin bBox
        var topLeft = GetMapTileAtPoint(obj.bBox.Center + new Vector2(-obj.bBox.HalfSize.x, obj.bBox.HalfSizeY));
        var topRight = GetMapTileAtPoint(obj.bBox.Center + obj.bBox.HalfSize);
        var bottomLeft = GetMapTileAtPoint(obj.bBox.Center - obj.bBox.HalfSize);
        var bottomRight = new Vector3Int();

        topLeft.x /= gridAreaWidth;
        topLeft.y /= gridAreaHeight;

        topRight.x /= gridAreaWidth;
        topRight.y /= gridAreaHeight;

        bottomLeft.x /= gridAreaWidth;
        bottomLeft.y /= gridAreaHeight;

        bottomRight.x = topRight.x;
        bottomRight.y = bottomLeft.y;

        // see how many different areas we have
        if (topLeft.x == topRight.x && topLeft.y == bottomLeft.y)
            overlappingAreas.Add(topLeft);
        else if (topLeft.x == topRight.x)
        {
            overlappingAreas.Add(topLeft);
            overlappingAreas.Add(bottomLeft);
        }
        else if (topLeft.y == bottomLeft.y)
        {
            overlappingAreas.Add(topLeft);
            overlappingAreas.Add(topRight);
        }
        else
        {
            overlappingAreas.Add(topLeft);
            overlappingAreas.Add(bottomLeft);
            overlappingAreas.Add(topRight);
            overlappingAreas.Add(bottomRight);
        }

        var areas = obj.areas;
        var ids = obj.idsInAreas;

        for (int i = 0; i < areas.Count; ++i)
        {
            if (!overlappingAreas.Contains(areas[i]))
            {
                RemoveObjectFromArea(areas[i], ids[i], obj);
                areas.RemoveAt(i);
                ids.RemoveAt(i);
                --i;
            }
        }

        for (var i = 0; i < overlappingAreas.Count; ++i)
        {
            var area = overlappingAreas[i];
            if (!areas.Contains(area))
                AddObjectToArea(area, obj);
        }

        overlappingAreas.Clear();
    }

    #region tile definition
    public bool IsObstacle(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return true;
        return (tilesCollisionMap[x, y] != TileType.Empty);
    }
    public bool IsEmpty(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return false;
        return (tilesCollisionMap[x, y] == TileType.Empty);
    }
    #endregion

    #region Check collision whis tile
    public int GetMapTileYAtPoint(float y)
    {
        return (int)((y - mapPosition.y + tileSize / 2.0f) / (float)(tileSize));
    }

    public int GetMapTileXAtPoint(float x)
    {
        return (int)((x - mapPosition.x + tileSize / 2.0f) / (float)(tileSize));
    }

    public Vector3Int GetMapTileAtPoint(Vector2 point)
    {
        return new Vector3Int((int)((point.x - mapPosition.x + tileSize / 2.0f) / (float)tileSize), (int)((point.y - mapPosition.y + tileSize / 2.0f) / (float)tileSize), 1);
    }

    public Vector2 GetMapTilePosition(int tileIndexX, int tileIndexY)
    {
        return new Vector2((tileIndexX * tileSize) + mapPosition.x, (tileIndexY * tileSize) + mapPosition.y);
    }

    public Vector2 GetMapTilePosition(Vector3Int tileCoords)
    {
        return new Vector2((tileCoords.x * tileSize) + mapPosition.x, (tileCoords.y * tileSize) + mapPosition.y);
    }

    public TileType GetCollisionType(Vector3Int pos)
    {
        if (pos.x <= -1 || pos.x >= width || pos.y <= -1 || pos.y >= height)
            return TileType.Empty;
        return tilesCollisionMap[pos.x, pos.y];
    }
    public TileType GetCollisionType(int x, int y)
    {
        if (x <= -1 || x >= width || y <= -1 || y >= height)
            return TileType.Empty;
        return tilesCollisionMap[x, y];
    }
    #endregion

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(width * 0.5f * tileSize, height * 0.5f * tileSize), new Vector3(width * tileSize, height * tileSize));
    }
#endif
}