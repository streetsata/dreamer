using UnityEngine;

public static class TileSlopes
{
    public static readonly sbyte[] empty = new sbyte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public static readonly sbyte[] full = new sbyte[16] { 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16 };

    public static readonly sbyte[] slopeMid1 = new sbyte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 8, 7, 6, 5, 4, 3, 2, 1 };
    public static readonly sbyte[] slope45 = new sbyte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
    public static readonly sbyte[] slope22P1 = new sbyte[16] { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8 };
    public static readonly sbyte[] slope22P2 = new sbyte[16] { 9, 9, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15, 16, 16 };
    public static readonly sbyte[] slope11P1 = new sbyte[16] { 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4 };
    public static readonly sbyte[] slope11P2 = new sbyte[16] { 5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8 };
    public static readonly sbyte[] slope11P3 = new sbyte[16] { 9, 9, 9, 9, 10, 10, 10, 10, 11, 11, 11, 11, 12, 12, 12, 12 };
    public static readonly sbyte[] slope11P4 = new sbyte[16] { 13, 13, 13, 13, 14, 14, 14, 14, 15, 15, 15, 15, 16, 16, 16, 16 };

    public static sbyte[][] slopesHeights;
    public static sbyte[][][] slopesExtended;
    public static sbyte[][][] slopeHeightByPosAndSizeCaches;
    public static TileSlopeOffsetSB[][][][][] slopeOffsets;
    public static sbyte[][] posByHeightCaches;

    public static sbyte[][] Extend(sbyte[] slope)
    {
        sbyte[][] extended = new sbyte[slope.Length][];

        for (int x = 0; x < slope.Length; ++x)
        {
            extended[x] = new sbyte[slope.Length];

            for (int y = 0; y < slope.Length; ++y)
                extended[x][y] = System.Convert.ToSByte(y < slope[x]);
        }

        return extended;
    }

    public static sbyte[] CachePosByHeight(sbyte[] slope)
    {
        var posByHeight = new sbyte[slope.Length];

        for (int i = 0; i < slope.Length; ++i)
        {
            posByHeight[i] = -1;

            for (int j = 0; j < slope.Length; ++j)
            {
                if (slope[j] >= i)
                {
                    posByHeight[i] = slope[j];
                    break;
                }
            }
        }

        return posByHeight;
    }

    public static sbyte[][] CacheSlopeHeightByPosAndLength(sbyte[] slope)
    {
        var maxHeightCache = new sbyte[slope.Length][];

        for (sbyte pos = 0; pos < slope.Length; ++pos)
        {
            maxHeightCache[pos] = new sbyte[slope.Length];
            for (int dist = 0; dist < slope.Length; ++dist)
            {
                sbyte maxHeight = 0;
                for (int k = pos; k <= Mathf.Min(pos + dist, slope.Length - 1); ++k)
                {
                    if (slope[k] > maxHeight)
                        maxHeight = slope[k];
                }
                maxHeightCache[pos][dist] = maxHeight;
            }
        }

        return maxHeightCache;
    }

    public static TileSlopeOffsetSB[][][][] CacheSlopeOffsets(sbyte[][] slopeExtended)
    {
        var offsetCache = new TileSlopeOffsetSB[slopeExtended.Length][][][];

        for (int x = 0; x < slopeExtended.Length; ++x)
        {
            offsetCache[x] = new TileSlopeOffsetSB[slopeExtended.Length][][];

            for (int y = 0; y < slopeExtended.Length; ++y)
            {
                offsetCache[x][y] = new TileSlopeOffsetSB[slopeExtended.Length][];

                for (int w = 0; w < slopeExtended.Length; ++w)
                {
                    offsetCache[x][y][w] = new TileSlopeOffsetSB[slopeExtended.Length];

                    for (int h = 0; h < slopeExtended.Length; ++h)
                    {
                        offsetCache[x][y][w][h] = GetOffset(slopeExtended, (sbyte)x, (sbyte)y, (sbyte)w, (sbyte)h);
                    }
                }
            }
        }

        return offsetCache;
    }

    public static bool Collides(sbyte[][] slopeExtended, sbyte posX, sbyte posY, sbyte w, sbyte h)
    {
        for (int x = posX; x <= posX + w && x < slopeExtended.Length; ++x)
        {
            for (int y = posY; y <= posY + h && y < slopeExtended.Length; ++y)
            {
                if (slopeExtended[x][y] == 1)
                    return true;
            }
        }

        return false;
    }

    public static TileSlopeOffsetSB GetOffset(sbyte[][] slopeExtended, sbyte posX, sbyte posY, sbyte w, sbyte h)
    {
        sbyte freeUp = 0, freeDown = 0, collidingTop = 0, collidingBottom = 0;
        sbyte freeLeft = 0, freeRight = 0, collidingLeft = 0, collidingRight = 0;

        sbyte movH = h;
        while (movH >= 0 && posY + freeUp < slopeExtended.Length && Collides(slopeExtended, posX, (sbyte)(posY + freeUp), w, movH))
        {
            if (posY + freeUp == slopeExtended.Length)
                --movH;
            else
                ++freeUp;
        }

        freeUp += (sbyte)(h - movH);

        movH = h;
        while (movH >= 0 && posY + freeDown >= 0 && Collides(slopeExtended, posX, (sbyte)(posY + freeDown), w, movH))
        {
            if (posY + freeDown == 0)
                --movH;
            else
                --freeDown;
        }

        freeDown -= (sbyte)(h - movH);

        if (freeUp == 0)
        {
            movH = h;
            while (movH >= 0 && posY + collidingTop < slopeExtended.Length && !Collides(slopeExtended, posX, (sbyte)(posY + collidingTop), w, movH))
            {
                if (posY + collidingTop == slopeExtended.Length)
                    --movH;
                else
                    ++collidingTop;
            }

            collidingTop += (sbyte)(h - movH);
            collidingTop -= 1;
        }
        else
            collidingBottom = freeUp;

        if (freeDown == 0)
        {
            movH = h;
            while (movH >= 0 && posY + collidingBottom >= 0 && !Collides(slopeExtended, posX, (sbyte)(posY + collidingBottom), w, movH))
            {
                if (posY + collidingBottom == 0)
                    --movH;
                else
                    --collidingBottom;
            }

            collidingBottom -= (sbyte)(h - movH);
            collidingBottom += 1;
        }
        else
            collidingTop = freeDown;

        //width

        sbyte movW = w;
        while (movW >= 0 && posY + freeRight < slopeExtended.Length && Collides(slopeExtended, (sbyte)(posX + freeRight), posY, movW, h))
        {
            if (posX + freeRight == slopeExtended.Length)
                --movW;
            else
                ++freeRight;
        }

        freeRight += (sbyte)(w - movW);

        movW = w;
        while (movW >= 0 && posX + freeLeft >= 0 && Collides(slopeExtended, (sbyte)(posX + freeLeft), posY, movW, h))
        {
            if (posX + freeLeft == 0)
                --movW;
            else
                --freeLeft;
        }

        freeLeft -= (sbyte)(w - movW);

        if (freeRight == 0)
        {
            movW = w;
            while (movW >= 0 && posX + collidingRight < slopeExtended.Length && !Collides(slopeExtended, (sbyte)(posX + collidingRight), posY, movW, h))
            {
                if (posX + collidingRight == slopeExtended.Length)
                    --movW;
                else
                    ++collidingRight;
            }

            collidingRight += (sbyte)(w - movW);
            collidingRight -= 1;
        }
        else
            collidingLeft = freeRight;

        if (freeLeft == 0)
        {
            movW = w;
            while (movW >= 0 && posX + collidingLeft >= 0 && !Collides(slopeExtended, (sbyte)(posX + collidingLeft), posY, movW, w))
            {
                if (posX + collidingLeft == 0)
                    --movW;
                else
                    --collidingLeft;
            }

            collidingLeft -= (sbyte)(w - movW);
            collidingLeft += 1;
        }
        else
            collidingRight = freeLeft;

        return new TileSlopeOffsetSB(freeLeft, freeRight, freeDown, freeUp, collidingLeft, collidingRight, collidingBottom, collidingTop);
    }

    public static TileSlopeOffsetI GetOffset(Vector2 tileCenter, float leftX, float rightX, float bottomY, float topY, TileType tileCollisionType)
    {
        int posX, posY, sizeX, sizeY;

        float leftTileEdge = tileCenter.x - Map.tileSize / 2;
        float rightTileEdge = leftTileEdge + Map.tileSize;
        float bottomTileEdge = tileCenter.y - Map.tileSize / 2;
        float topTileEdge = bottomTileEdge + Map.tileSize;
        TileSlopeOffsetI offset;

        if (!IsFlipped90(tileCollisionType))
        {
            if (IsFlippedX(tileCollisionType))
            {
                posX = (int)Mathf.Clamp(rightTileEdge - rightX, 0.0f, Map.tileSize - 1);
                sizeX = (int)Mathf.Clamp((rightTileEdge - posX) - leftX, 0.0f, Map.tileSize - 1);
            }
            else
            {
                posX = (int)Mathf.Clamp(leftX - leftTileEdge, 0.0f, Map.tileSize - 1);
                sizeX = (int)Mathf.Clamp(rightX - (leftTileEdge + posX), 0.0f, Map.tileSize - 1);
            }

            if (IsFlippedY(tileCollisionType))
            {
                posY = (int)Mathf.Clamp(topTileEdge - topY, 0.0f, Map.tileSize - 1);
                sizeY = (int)Mathf.Clamp((topTileEdge - posY) - bottomY, 0.0f, Map.tileSize - 1);
            }
            else
            {
                posY = (int)Mathf.Clamp(bottomY - bottomTileEdge, 0.0f, Map.tileSize - 1);
                sizeY = (int)Mathf.Clamp(topY - (bottomTileEdge + posY), 0.0f, Map.tileSize - 1);
            }

            offset = new TileSlopeOffsetI(slopeOffsets[(int)tileCollisionType][posX / 8][posY / 8][sizeX / 8][sizeY / 8]);

            if (IsFlippedY(tileCollisionType))
            {
                int tmp = offset.freeDown;
                offset.freeDown = -offset.freeUp;
                offset.freeUp = -tmp;
                tmp = offset.collidingTop;
                offset.collidingTop = -offset.collidingBottom;
                offset.collidingBottom = -tmp;
            }
        }
        else
        {
            if (IsFlippedY(tileCollisionType))
            {
                posX = (int)Mathf.Clamp(bottomY - bottomTileEdge, 0.0f, Map.tileSize - 1);
                sizeX = (int)Mathf.Clamp(topY - (bottomTileEdge + posX), 0.0f, Map.tileSize - 1);
            }
            else
            {
                posX = (int)Mathf.Clamp(topTileEdge - topY, 0.0f, Map.tileSize - 1);
                sizeX = (int)Mathf.Clamp((topTileEdge - posX) - bottomY, 0.0f, Map.tileSize - 1);
            }

            if (IsFlippedX(tileCollisionType))
            {
                posY = (int)Mathf.Clamp(rightTileEdge - rightX, 0.0f, Map.tileSize - 1);
                sizeY = (int)Mathf.Clamp((rightTileEdge - posY) - leftX, 0.0f, Map.tileSize - 1);
            }
            else
            {
                posY = (int)Mathf.Clamp(leftX - leftTileEdge, 0.0f, Map.tileSize - 1);
                sizeY = (int)Mathf.Clamp(rightX - (leftTileEdge + posY), 0.0f, Map.tileSize - 1);
            }

            offset = new TileSlopeOffsetI(slopeOffsets[(int)tileCollisionType][posX / 8][posY / 8][sizeX / 8][sizeY / 8]);

            if (IsFlippedY(tileCollisionType))
            {
                offset.collidingBottom = offset.collidingLeft;
                offset.freeDown = offset.freeLeft;
                offset.collidingTop = offset.collidingRight;
                offset.freeUp = offset.freeRight;
            }
            else
            {
                offset.collidingBottom = -offset.collidingRight;
                offset.freeDown = -offset.freeRight;
                offset.collidingTop = -offset.collidingLeft;
                offset.freeUp = -offset.freeLeft;
            }
        }

        if (topTileEdge < topY)
        {
            if (offset.freeDown < 0)
                offset.freeDown -= (int)(topY - topTileEdge);
            offset.collidingTop = offset.freeDown;
        }
        if (bottomTileEdge > bottomY)
        {
            if (offset.freeUp > 0)
                offset.freeUp += (int)bottomTileEdge - (int)bottomY;
            offset.collidingBottom = offset.freeUp;
        }

        return offset;
    }

    public static int GetSlopeHeightFromBottom(int x, TileType type)
    {
        switch (type)
        {
            case TileType.Empty:
                return 0;
            case TileType.Full:
            case TileType.OneWayFull:
                return Map.tileSize;
        }

        if (IsFlippedX(type))
            x = Map.tileSize - 1 - x;

        if (!IsFlipped90(type))
        {
            var offset = new TileSlopeOffsetI(slopeOffsets[(int)type][x / 8][0][0][(Map.tileSize - 1) / 8]);
            return IsFlippedY(type) ? -offset.collidingTop : offset.collidingBottom;
        }
        else
        {
            var offset = new TileSlopeOffsetI(slopeOffsets[(int)type][0][x / 8][(Map.tileSize - 1) / 8][0]);
            return IsFlippedY(type) ? offset.collidingLeft : -offset.collidingRight;
        }
    }

    public static bool IsOneWay(TileType type)
    {
        return ((int)type >= (int)TileType.OneWayStart && (int)type < (int)TileType.OneWayEnd);
    }

    public static bool IsFlipped90(TileType type)
    {
        switch (type)
        {
            case TileType.Slope45F90X:
            case TileType.Slope45F90:
            case TileType.SlopeMid1F90XY:
            case TileType.SlopeMid1F90Y:
                return true;
        }
        return false;
    }

    public static bool IsFlippedX(TileType type)
    {
        switch (type)
        {
            case TileType.OneWaySlope11P1FX:
            case TileType.OneWaySlope11P2FX:
            case TileType.OneWaySlope11P3FX:
            case TileType.OneWaySlope11P4FX:
            case TileType.OneWaySlope22P1FX:
            case TileType.OneWaySlope22P2FX:
            case TileType.OneWaySlope45FX:
            case TileType.OneWaySlopeMid1FX:
            case TileType.Slope11P1FX:
            case TileType.Slope11P2FX:
            case TileType.Slope11P3FX:
            case TileType.Slope11P4FX:
            case TileType.Slope22P1FX:
            case TileType.Slope22P2FX:
            case TileType.Slope45F90X:
            case TileType.SlopeMid1F90XY:
                return true;
        }
        return false;
    }

    public static bool IsFlippedY(TileType type)
    {
        if (type == TileType.SlopeMid1F90XY || type == TileType.SlopeMid1F90Y) return true;
        return false;
    }

    public static void Init()
    {
        slopesHeights = new sbyte[(int)TileType.Count][];
        slopesExtended = new sbyte[(int)TileType.Count][][];
        slopeHeightByPosAndSizeCaches = new sbyte[(int)TileType.Count][][];
        posByHeightCaches = new sbyte[(int)TileType.Count][];
        slopeOffsets = new TileSlopeOffsetSB[(int)TileType.Count][][][][];

        for (int i = 0; i < (int)TileType.Count; ++i)
        {
            switch ((TileType)i)
            {
                case TileType.Empty:
                    slopesHeights[i] = empty;
                    slopesExtended[i] = Extend(slopesHeights[i]);
                    posByHeightCaches[i] = CachePosByHeight(slopesHeights[i]);
                    slopeHeightByPosAndSizeCaches[i] = CacheSlopeHeightByPosAndLength(slopesHeights[i]);
                    slopeOffsets[i] = CacheSlopeOffsets(slopesExtended[i]);
                    break;

                case TileType.Full:
                    slopesHeights[i] = full;
                    slopesExtended[i] = Extend(slopesHeights[i]);
                    posByHeightCaches[i] = CachePosByHeight(slopesHeights[i]);
                    slopeHeightByPosAndSizeCaches[i] = CacheSlopeHeightByPosAndLength(slopesHeights[i]);
                    slopeOffsets[i] = CacheSlopeOffsets(slopesExtended[i]);
                    break;
                case TileType.OneWayFull:
                    slopesHeights[i] = slopesHeights[(int)TileType.Full];
                    slopesExtended[i] = slopesExtended[(int)TileType.Full];
                    posByHeightCaches[i] = posByHeightCaches[(int)TileType.Full];
                    slopeHeightByPosAndSizeCaches[i] = slopeHeightByPosAndSizeCaches[(int)TileType.Full];
                    slopeOffsets[i] = slopeOffsets[(int)TileType.Full];
                    break;

                case TileType.Slope45F90:
                    slopesHeights[i] = slope45;
                    slopesExtended[i] = Extend(slopesHeights[i]);
                    posByHeightCaches[i] = CachePosByHeight(slopesHeights[i]);
                    slopeHeightByPosAndSizeCaches[i] = CacheSlopeHeightByPosAndLength(slopesHeights[i]);
                    slopeOffsets[i] = CacheSlopeOffsets(slopesExtended[i]);
                    break;
                case TileType.Slope45F90X:
                case TileType.OneWaySlope45:
                case TileType.OneWaySlope45FX:
                    slopesHeights[i] = slopesHeights[(int)TileType.Slope45F90];
                    slopesExtended[i] = slopesExtended[(int)TileType.Slope45F90];
                    posByHeightCaches[i] = posByHeightCaches[(int)TileType.Slope45F90];
                    slopeHeightByPosAndSizeCaches[i] = slopeHeightByPosAndSizeCaches[(int)TileType.Slope45F90];
                    slopeOffsets[i] = slopeOffsets[(int)TileType.Slope45F90];
                    break;

                case TileType.SlopeMid1:
                    slopesHeights[i] = slopeMid1;
                    slopesExtended[i] = Extend(slopesHeights[i]);
                    posByHeightCaches[i] = CachePosByHeight(slopesHeights[i]);
                    slopeHeightByPosAndSizeCaches[i] = CacheSlopeHeightByPosAndLength(slopesHeights[i]);
                    slopeOffsets[i] = CacheSlopeOffsets(slopesExtended[i]);
                    break;
                case TileType.SlopeMid1F90XY:
                case TileType.SlopeMid1F90Y:
                case TileType.OneWaySlopeMid1FX:
                    slopesHeights[i] = slopesHeights[(int)TileType.SlopeMid1];
                    slopesExtended[i] = slopesExtended[(int)TileType.SlopeMid1];
                    posByHeightCaches[i] = posByHeightCaches[(int)TileType.SlopeMid1];
                    slopeHeightByPosAndSizeCaches[i] = slopeHeightByPosAndSizeCaches[(int)TileType.SlopeMid1];
                    slopeOffsets[i] = slopeOffsets[(int)TileType.SlopeMid1];
                    break;

                case TileType.Slope22P1:
                    slopesHeights[i] = slope22P1;
                    slopesExtended[i] = Extend(slopesHeights[i]);
                    posByHeightCaches[i] = CachePosByHeight(slopesHeights[i]);
                    slopeHeightByPosAndSizeCaches[i] = CacheSlopeHeightByPosAndLength(slopesHeights[i]);
                    slopeOffsets[i] = CacheSlopeOffsets(slopesExtended[i]);
                    break;
                case TileType.Slope22P1FX:
                case TileType.OneWaySlope22P1:
                case TileType.OneWaySlope22P1FX:
                    slopesHeights[i] = slopesHeights[(int)TileType.Slope22P1];
                    slopesExtended[i] = slopesExtended[(int)TileType.Slope22P1];
                    posByHeightCaches[i] = posByHeightCaches[(int)TileType.Slope22P1];
                    slopeHeightByPosAndSizeCaches[i] = slopeHeightByPosAndSizeCaches[(int)TileType.Slope22P1];
                    slopeOffsets[i] = slopeOffsets[(int)TileType.Slope22P1];
                    break;

                case TileType.Slope22P2:
                    slopesHeights[i] = slope22P2;
                    slopesExtended[i] = Extend(slopesHeights[i]);
                    posByHeightCaches[i] = CachePosByHeight(slopesHeights[i]);
                    slopeHeightByPosAndSizeCaches[i] = CacheSlopeHeightByPosAndLength(slopesHeights[i]);
                    slopeOffsets[i] = CacheSlopeOffsets(slopesExtended[i]);
                    break;
                case TileType.Slope22P2FX:
                case TileType.OneWaySlope22P2:
                case TileType.OneWaySlope22P2FX:
                    slopesHeights[i] = slopesHeights[(int)TileType.Slope22P2];
                    slopesExtended[i] = slopesExtended[(int)TileType.Slope22P2];
                    posByHeightCaches[i] = posByHeightCaches[(int)TileType.Slope22P2];
                    slopeHeightByPosAndSizeCaches[i] = slopeHeightByPosAndSizeCaches[(int)TileType.Slope22P2];
                    slopeOffsets[i] = slopeOffsets[(int)TileType.Slope22P2];
                    break;

                case TileType.Slope11P1:
                    slopesHeights[i] = slope11P1;
                    slopesExtended[i] = Extend(slopesHeights[i]);
                    posByHeightCaches[i] = CachePosByHeight(slopesHeights[i]);
                    slopeHeightByPosAndSizeCaches[i] = CacheSlopeHeightByPosAndLength(slopesHeights[i]);
                    slopeOffsets[i] = CacheSlopeOffsets(slopesExtended[i]);
                    break;
                case TileType.Slope11P1FX:
                case TileType.OneWaySlope11P1:
                case TileType.OneWaySlope11P1FX:
                    slopesHeights[i] = slopesHeights[(int)TileType.Slope11P1];
                    slopesExtended[i] = slopesExtended[(int)TileType.Slope11P1];
                    posByHeightCaches[i] = posByHeightCaches[(int)TileType.Slope11P1];
                    slopeHeightByPosAndSizeCaches[i] = slopeHeightByPosAndSizeCaches[(int)TileType.Slope11P1];
                    slopeOffsets[i] = slopeOffsets[(int)TileType.Slope11P1];
                    break;

                case TileType.Slope11P2:
                    slopesHeights[i] = slope11P2;
                    slopesExtended[i] = Extend(slopesHeights[i]);
                    posByHeightCaches[i] = CachePosByHeight(slopesHeights[i]);
                    slopeHeightByPosAndSizeCaches[i] = CacheSlopeHeightByPosAndLength(slopesHeights[i]);
                    slopeOffsets[i] = CacheSlopeOffsets(slopesExtended[i]);
                    break;
                case TileType.Slope11P2FX:
                case TileType.OneWaySlope11P2:
                case TileType.OneWaySlope11P2FX:
                    slopesHeights[i] = slopesHeights[(int)TileType.Slope11P2];
                    slopesExtended[i] = slopesExtended[(int)TileType.Slope11P2];
                    posByHeightCaches[i] = posByHeightCaches[(int)TileType.Slope11P2];
                    slopeHeightByPosAndSizeCaches[i] = slopeHeightByPosAndSizeCaches[(int)TileType.Slope11P2];
                    slopeOffsets[i] = slopeOffsets[(int)TileType.Slope11P2];
                    break;

                case TileType.Slope11P3:
                    slopesHeights[i] = slope11P3;
                    slopesExtended[i] = Extend(slopesHeights[i]);
                    posByHeightCaches[i] = CachePosByHeight(slopesHeights[i]);
                    slopeHeightByPosAndSizeCaches[i] = CacheSlopeHeightByPosAndLength(slopesHeights[i]);
                    slopeOffsets[i] = CacheSlopeOffsets(slopesExtended[i]);
                    break;
                case TileType.Slope11P3FX:
                case TileType.OneWaySlope11P3:
                case TileType.OneWaySlope11P3FX:
                    slopesHeights[i] = slopesHeights[(int)TileType.Slope11P3];
                    slopesExtended[i] = slopesExtended[(int)TileType.Slope11P3];
                    posByHeightCaches[i] = posByHeightCaches[(int)TileType.Slope11P3];
                    slopeHeightByPosAndSizeCaches[i] = slopeHeightByPosAndSizeCaches[(int)TileType.Slope11P3];
                    slopeOffsets[i] = slopeOffsets[(int)TileType.Slope11P3];
                    break;

                case TileType.Slope11P4:
                    slopesHeights[i] = slope11P4;
                    slopesExtended[i] = Extend(slopesHeights[i]);
                    posByHeightCaches[i] = CachePosByHeight(slopesHeights[i]);
                    slopeHeightByPosAndSizeCaches[i] = CacheSlopeHeightByPosAndLength(slopesHeights[i]);
                    slopeOffsets[i] = CacheSlopeOffsets(slopesExtended[i]);
                    break;
                case TileType.Slope11P4FX:
                case TileType.OneWaySlope11P4:
                case TileType.OneWaySlope11P4FX:
                    slopesHeights[i] = slopesHeights[(int)TileType.Slope11P4];
                    slopesExtended[i] = slopesExtended[(int)TileType.Slope11P4];
                    posByHeightCaches[i] = posByHeightCaches[(int)TileType.Slope11P4];
                    slopeHeightByPosAndSizeCaches[i] = slopeHeightByPosAndSizeCaches[(int)TileType.Slope11P4];
                    slopeOffsets[i] = slopeOffsets[(int)TileType.Slope11P4];
                    break;
            }
        }
    }
}
