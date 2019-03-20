using UnityEngine;

public class TileFormPoints
{
    public static float half = Map.tileSize / 2,
                         quarter = half / 2;

    private static Vector3 upRight = new Vector3(half, half),
                           upLeft = new Vector3(-half, half),
                           downRight = new Vector3(half, -half),
                           downLeft = new Vector3(-half, -half);

    public static Vector3[] GetFormPoint(TileType tileType, Vector3 centr)
    {
        switch (tileType)
        {
            #region simple tile
            case TileType.Full:
                return new Vector3[4]
                {
                    centr + upRight,
                    centr + upLeft,
                    centr + downLeft,
                    centr + downRight
                };
            case TileType.SlopeMid1:
                return new Vector3[3]
                {
                    centr + downRight,
                    centr + downLeft,
                    centr
                };
            case TileType.SlopeMid1F90Y:
                return new Vector3[3]
                {
                    centr + upLeft,
                    centr + downLeft,
                    centr
                };
            case TileType.SlopeMid1F90XY:
                return new Vector3[3]
                {
                    centr + upRight,
                    centr + downRight,
                    centr
                };
            case TileType.Slope45F90:
                return new Vector3[3]
                {
                    centr + upLeft,
                    centr + downLeft,
                    centr + downRight
                };
            case TileType.Slope45F90X:
                return new Vector3[3]
                {
                    centr + upRight,
                    centr + downLeft,
                    centr + downRight
                };
            case TileType.Slope22P1:
                return new Vector3[3]
                {
                    centr + new Vector3(half, 0),
                    centr + downLeft,
                    centr + downRight
                };
            case TileType.Slope22P1FX:
                return new Vector3[3]
                {
                    centr + new Vector3(-half, 0),
                    centr + downLeft,
                    centr + downRight
                };
            case TileType.Slope22P2:
                return new Vector3[4]
                {
                    centr + new Vector3(-half, 0),
                    centr + downLeft,
                    centr + downRight,
                    centr + upRight
                };
            case TileType.Slope22P2FX:
                return new Vector3[4]
                {
                    centr + new Vector3(half, 0),
                    centr + downRight,
                    centr + downLeft,
                    centr + upLeft
                };
            case TileType.Slope11P1:
                return new Vector3[3]
                {
                    centr + new Vector3(half, -quarter),
                    centr + downRight,
                    centr + downLeft
                };
            case TileType.Slope11P1FX:
                return new Vector3[3]
                {
                    centr + new Vector3(-half, -quarter),
                    centr + downRight,
                    centr + downLeft
                };
            case TileType.Slope11P2:
                return new Vector3[4]
                {
                    centr + new Vector3(half, 0),
                    centr + downRight,
                    centr + downLeft,
                    centr + new Vector3(-half, -quarter)
                };
            case TileType.Slope11P2FX:
                return new Vector3[4]
                {
                    centr + new Vector3(-half, 0),
                    centr + downLeft,
                    centr + downRight,
                    centr + new Vector3(half, -quarter)
                };
            case TileType.Slope11P3:
                return new Vector3[4]
                {
                    centr + new Vector3(half, quarter),
                    centr + downRight,
                    centr + downLeft,
                    centr + new Vector3(-half, 0)
                };
            case TileType.Slope11P3FX:
                return new Vector3[4]
                {
                    centr + new Vector3(half, 0),
                    centr + downRight,
                    centr + downLeft,
                    centr + new Vector3(-half, quarter)
                };
            case TileType.Slope11P4:
                return new Vector3[4]
                {
                    centr + upRight,
                    centr + downRight,
                    centr + downLeft,
                    centr + new Vector3(-half, quarter)
                };
            case TileType.Slope11P4FX:
                return new Vector3[4]
                {
                    centr + new Vector3(half, quarter),
                    centr + downRight,
                    centr + downLeft,
                    centr + upLeft
                };
            #endregion
            #region OneWay
            case TileType.OneWaySlopeMid1FX:
                return new Vector3[6]
                {
                    centr,
                    centr + downRight,
                    centr + new Vector3(quarter, -half),
                    centr + new Vector3(0, -quarter),
                    centr + new Vector3(-quarter, -half),
                    centr + downLeft
                };
            case TileType.OneWaySlope45:
                return new Vector3[4]
                {
                    centr + upRight,
                    centr + new Vector3(half, quarter),
                    centr + new Vector3(-quarter, -half),
                    centr + downLeft//new Vector3(-half, -quarter)
                };
            case TileType.OneWaySlope45FX:
                return new Vector3[4]
                {
                    centr + upLeft,
                    centr + new Vector3(-half, quarter),
                    centr + new Vector3(quarter, -half),
                    centr + downRight
                };
            case TileType.OneWaySlope22P1:
                return new Vector3[4]
                {
                    centr + new Vector3(half, 0),
                    centr + new Vector3(half, -quarter),
                    centr + new Vector3(-quarter /2, -half),
                    centr + downLeft
                };
            case TileType.OneWaySlope22P1FX:
                return new Vector3[4]
                {
                    centr + new Vector3(-half, 0),
                    centr + new Vector3(-half, -quarter),
                    centr + new Vector3(quarter /2, -half),
                    centr + downRight
                };
            case TileType.OneWaySlope22P2:
                return new Vector3[4]
                {
                    centr + new Vector3(half, quarter),
                    centr + new Vector3(-half, -quarter),
                    centr + new Vector3(-half, 0),
                    centr + upRight
                };
            case TileType.OneWaySlope22P2FX:
                return new Vector3[4]
                {
                    centr + new Vector3(-half, quarter),
                    centr + new Vector3(half, -quarter),
                    centr + new Vector3(half, 0),
                    centr + upLeft
                };
            case TileType.OneWaySlope11P1:
                return new Vector3[3]
                {
                    centr + new Vector3(half, -quarter),
                    centr + downRight,
                    centr + downLeft
                };
            case TileType.OneWaySlope11P1FX:
                return new Vector3[3]
                {
                    centr + new Vector3(-half, -quarter),
                    centr + downRight,
                    centr + downLeft
                };
            case TileType.OneWaySlope11P2:
                return new Vector3[4]
                {
                    centr + new Vector3(half, 0),
                    centr + new Vector3(half, -quarter),
                    centr + downLeft,
                    centr + new Vector3(-half, -quarter)
                };
            case TileType.OneWaySlope11P2FX:
                return new Vector3[4]
                {
                    centr + new Vector3(-half, 0),
                    centr + new Vector3(-half, -quarter),
                    centr + downRight,
                    centr + new Vector3(half, -quarter)
                };
            case TileType.OneWaySlope11P3:
                return new Vector3[4]
                {
                    centr + new Vector3(half, quarter),
                    centr + new Vector3(half, 0),
                    centr + new Vector3(-half, -quarter),
                    centr + new Vector3(-half, 0)
                };
            case TileType.OneWaySlope11P3FX:
                return new Vector3[4]
                {
                    centr + new Vector3(-half, quarter),
                    centr + new Vector3(-half, 0),
                    centr + new Vector3(half, -quarter),
                    centr + new Vector3(half, 0)
                };
            case TileType.OneWaySlope11P4:
                return new Vector3[4]
                {
                    centr + upRight,
                    centr + new Vector3(half, quarter),
                    centr + new Vector3(-half, 0),
                    centr + new Vector3(-half, quarter)
                };
            case TileType.OneWaySlope11P4FX:
                return new Vector3[4]
                {
                    centr + upLeft,
                    centr + new Vector3(-half, quarter),
                    centr + new Vector3(half, 0),
                    centr + new Vector3(half, quarter)
                };
            case TileType.OneWayFull:
                return new Vector3[4]
                {
                    centr + upLeft,
                    centr + upRight,
                    centr + new Vector3(half, quarter),
                    centr + new Vector3(-half, quarter)
                };
                #endregion
        }
        return null;
    }
}
