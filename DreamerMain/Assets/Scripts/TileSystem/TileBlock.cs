using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBlock : MonoBehaviour
{
    public Map map;
    public TileType tileType;
    public int x, y;


    void Start ()
    {
        map.tilesCollisionMap[x, y] = tileType;
        Destroy(gameObject);
    }

    public void SetTileBlock(Map map, TileType tileType, int x, int y)
    {
        this.map = map;
        this.tileType = tileType;
        this.x = x;
        this.y = y;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = tileType >= TileType.OneWayStart ? Color.blue : Color.green;
        Vector3[] vector3s = TileFormPoints.GetFormPoint(tileType, new Vector3(x * Map.tileSize + TileFormPoints.half, y * Map.tileSize + TileFormPoints.half));
        if(vector3s != null)
            for (int i = 0; i < vector3s.Length; i++)
                Gizmos.DrawLine(vector3s[i], vector3s[i == vector3s.Length - 1 ? 0 : i + 1]);
    }
}
