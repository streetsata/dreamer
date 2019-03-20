using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PhysicsBody : MonoBehaviour
{
    public TilePhysicsSettingsData physicsSettings;
    [Tooltip("opportunity to push other objects")]
    public bool isKinematic = false;
    public bool isTriger = false;
    [Header("Body size")]
    public float halfSizeY = 20.0f;
    public float halfSizeX = 6.0f;
    public float bBoxPositionOffsetY = 0.0f;
    [Space]
    public UnityEvent OnСrushed;

    [HideInInspector] public BoundingBox bBox; // The BBox for collision queries.

    [HideInInspector] public Vector2 speedForse; // The current speed in pixels/second
    [HideInInspector] public Vector2 oldSpeedForse; // The previous speed in pixels/second
    [HideInInspector] public float AntiGravityForce = 0;

    [HideInInspector] public Vector2 previousPosition;
    [HideInInspector] public Vector2 position;
    [HideInInspector] public Vector2 reminder;

    [HideInInspector] public PhysicsBody mountParent = null;
    [HideInInspector] public bool isMount = false;
    private float mountY;

    [HideInInspector] public bool ignoresOneWay = false;
    [HideInInspector] public bool sticksToSlope = true;
    protected int slopeWallHeight = 4; // for slopes. How many pixels can bBox go to a tile
    protected PositionState posState;
    protected Map map;

    [NonSerialized] public List<Vector3Int> areas = new List<Vector3Int>(); // possible collision area
    [NonSerialized] public List<int> idsInAreas = new List<int>(); // area id
    [NonSerialized] public List<CollisionData> allCollidingObjects = new List<CollisionData>();

    #region Scale
    private Vector2 scale;
    public Vector2 Scale
    {
        set
        {
            scale = value;
            bBox.Scale = new Vector2(Mathf.Abs(value.x), Mathf.Abs(value.y));
        }
        get { return scale; }
    }
    public float ScaleX
    {
        set
        {
            scale.x = value;
            bBox.Scale.x = Mathf.Abs(value);
        }
        get { return scale.x; }
    }
    public float ScaleY
    {
        set
        {
            scale.y = value;
            bBox.Scale.y = Mathf.Abs(value);
        }
        get { return scale.y; }
    }
    #endregion
    #region BBoxOffset
    private Vector2 bBoxOffset;
    public Vector2 BBoxOffset
    {
        set { bBoxOffset = value; }
        get { return new Vector2(bBoxOffset.x * scale.x, bBoxOffset.y * scale.y); }
    }
    public float BBoxOffsetX
    {
        set { bBoxOffset.x = value; }
        get { return bBoxOffset.x * scale.x; }
    }
    public float BBoxOffsetY
    {
        set { bBoxOffset.y = value; }
        get { return bBoxOffset.y * scale.y; }
    }
    #endregion

    public delegate void OnEnter(List<CollisionData> collisionDatas);
    public delegate void OnStay(List<CollisionData> collisionDatas);
    public delegate void OnExit(List<CollisionData> collisionDatas);
    public OnEnter onEnter;
    public OnStay onStay;
    public OnExit onExit;
    protected int onEnterCount = 0;
    protected int onExitCount = 0;
    protected bool onEnterHelper = false;

    protected void Awake()
    {
        map = GameObject.FindGameObjectWithTag("Map").GetComponent<Map>();
        Scale = transform.localScale;
        position = transform.position;
        bBox.HalfSize = new Vector2(halfSizeX, halfSizeY);
        BBoxOffsetY = bBox.HalfSizeY + bBoxPositionOffsetY;
    }

    protected virtual void FixedUpdate()
    {
        if (map != null) CommonUpdate();
    }
    public virtual void CommonUpdate()
    {
        CollisionLogic();
        UpdatePhysicsFinaly();

        Gravity();
        /*if (!isTriger)*/ UpdatePhysics();        
        map.UpdateAreas(this);
        allCollidingObjects.Clear();
    }

    #region Slope
    public void CollidesWithTiles(ref Vector2 position, ref Vector2 topRight, ref Vector2 bottomLeft, ref PositionState state)
    {
        Vector2 pos = position, tr = topRight, bl = bottomLeft;
        CollidesWithTileTop(ref position, ref topRight, ref bottomLeft, ref state);
        CollidesWithTileBottom(ref position, ref topRight, ref bottomLeft, ref state);
        CollidesWithTileLeft(ref pos, ref tr, ref bl, ref state);
        CollidesWithTileRight(ref pos, ref tr, ref bl, ref state);
    }

    public bool CollidesWithTileLeft(ref Vector2 position, ref Vector2 topRight, ref Vector2 bottomLeft, ref PositionState state, bool move = false)
    {
        Vector3Int topRightTile = map.GetMapTileAtPoint(new Vector2(topRight.x - 0.5f, topRight.y - 0.5f));
        Vector3Int bottomLeftTile = map.GetMapTileAtPoint(new Vector2(bottomLeft.x - 0.5f, bottomLeft.y + 0.5f));
        float slopeOffset = 0.0f, oldSlopeOffset = 0.0f;
        bool wasOneWay = false, isOneWay;
        TileType slopeCollisionType = TileType.Empty;

        for (int y = bottomLeftTile.y; y <= topRightTile.y; ++y)
        {
            var tileCollisionType = map.GetCollisionType(bottomLeftTile.x, y);
            isOneWay = TileSlopes.IsOneWay(tileCollisionType);

            if (isOneWay && (!move || ignoresOneWay || state.tmpIgnoresOneWay || y != bottomLeftTile.y))
                continue;

            switch (tileCollisionType)
            {
                default://slope
                    Vector2 tileCenter = map.GetMapTilePosition(bottomLeftTile.x, y);
                    oldSlopeOffset = slopeOffset;

                    var offset = TileSlopes.GetOffset(tileCenter, bottomLeft.x - 0.5f, topRight.x - 0.5f, bottomLeft.y + 0.5f, topRight.y - 0.5f, tileCollisionType);
                    slopeOffset = Mathf.Abs(offset.freeUp) < Mathf.Abs(offset.freeDown) ? offset.freeUp : offset.freeDown;

                    if (!isOneWay && (Mathf.Abs(slopeOffset) >= slopeWallHeight || (slopeOffset < 0 && state.pushesBottomTile) || (slopeOffset > 0 && state.pushesTopTile)))
                    {
                        state.pushesLeftTile = true;
                        state.leftTile = new Vector3Int(bottomLeftTile.x, y, 1);
                        return true;
                    }
                    else if (Mathf.Abs(slopeOffset) > Mathf.Abs(oldSlopeOffset))
                    {
                        wasOneWay = isOneWay;
                        slopeCollisionType = tileCollisionType;
                        state.leftTile = new Vector3Int(bottomLeftTile.x, y, 1);
                    }
                    else
                        slopeOffset = oldSlopeOffset;

                    break;
                case TileType.Empty:
                    break;
            }
        }

        if (slopeCollisionType != TileType.Empty && slopeOffset != 0)
        {
            if (slopeOffset > 0 && slopeOffset < slopeWallHeight)
            {
                Vector2 pos = position, tr = topRight, bl = bottomLeft;
                pos.y += slopeOffset - Mathf.Sign(slopeOffset);
                tr.y += slopeOffset - Mathf.Sign(slopeOffset);
                bl.y += slopeOffset - Mathf.Sign(slopeOffset);
                PositionState s = new PositionState();

                if (CollidesWithTileTop(ref pos, ref tr, ref bl, ref s))
                {
                    state.pushesLeftTile = true;
                    return true;
                }
                else if (move)
                {
                    position.y += slopeOffset;
                    bottomLeft.y += slopeOffset;
                    topRight.y += slopeOffset;
                    state.pushesBottomTile = true;
                    state.onOneWay = wasOneWay;
                }
            }
            else if (slopeOffset < 0 && slopeOffset > -slopeWallHeight)
            {
                Vector2 pos = position, tr = topRight, bl = bottomLeft;
                pos.y += slopeOffset - Mathf.Sign(slopeOffset);
                tr.y += slopeOffset - Mathf.Sign(slopeOffset);
                bl.y += slopeOffset - Mathf.Sign(slopeOffset);
                PositionState s = new PositionState();

                if (CollidesWithTileBottom(ref pos, ref tr, ref bl, ref s))
                {
                    state.pushesLeftTile = true;
                    return true;
                }
                else if (move)
                {
                    position.y += slopeOffset;
                    bottomLeft.y += slopeOffset;
                    topRight.y += slopeOffset;
                    state.pushesTopTile = true;
                    state.onOneWay = wasOneWay;
                }
            }
        }

        if (sticksToSlope && state.pushedBottomTile && move)
        {
            var nextX = map.GetMapTileXAtPoint(topRight.x - 1.5f);
            var bottomY = map.GetMapTileYAtPoint(bottomLeft.y + 1.0f) - 1;

            var prevPos = map.GetMapTilePosition(new Vector3Int(topRightTile.x, bottomLeftTile.y, 1));
            var nextPos = map.GetMapTilePosition(new Vector3Int(nextX, bottomY, 1));

            var prevCollisionType = map.GetCollisionType(new Vector3Int(topRightTile.x, bottomLeftTile.y, 1));
            var nextCollisionType = map.GetCollisionType(new Vector3Int(nextX, bottomY, 1));

            int x1 = (int)Mathf.Clamp((topRight.x - 1.0f - (prevPos.x - Map.tileSize / 2)), 0.0f, 15.0f);
            int x2 = (int)Mathf.Clamp((topRight.x - 1.5f - (nextPos.x - Map.tileSize / 2)), 0.0f, 15.0f);

            int slopeHeight = TileSlopes.GetSlopeHeightFromBottom(x1, prevCollisionType);
            int nextSlopeHeight = TileSlopes.GetSlopeHeightFromBottom(x2, nextCollisionType);

            var offset = slopeHeight + Map.tileSize - nextSlopeHeight;

            if (offset < slopeWallHeight && offset > 0)
            {
                Vector2 pos = position, tr = topRight, bl = bottomLeft;
                pos.y -= offset - Mathf.Sign(offset);
                tr.y -= offset - Mathf.Sign(offset);
                bl.y -= offset - Mathf.Sign(offset);
                bl.x -= 1.0f;
                tr.x -= 1.0f;
                PositionState s = new PositionState();

                if (!CollidesWithTileBottom(ref pos, ref tr, ref bl, ref s))
                {
                    position.y -= offset;
                    bottomLeft.y -= offset;
                    topRight.y -= offset;
                    state.pushesBottomTile = true;
                    state.onOneWay = wasOneWay;
                }
            }
        }

        return false;
    }

    public bool CollidesWithTileRight(ref Vector2 position, ref Vector2 topRight, ref Vector2 bottomLeft, ref PositionState state, bool move = false)
    {
        Vector3Int topRightTile = map.GetMapTileAtPoint(new Vector2(topRight.x + 0.5f, topRight.y - 0.5f));
        Vector3Int bottomLeftTile = map.GetMapTileAtPoint(new Vector2(bottomLeft.x + 0.5f, bottomLeft.y + 0.5f));
        float slopeOffset = 0.0f, oldSlopeOffset = 0.0f;
        bool wasOneWay = false, isOneWay;
        TileType slopeCollisionType = TileType.Empty;

        for (int y = bottomLeftTile.y; y <= topRightTile.y; ++y)
        {
            var tileCollisionType = map.GetCollisionType(topRightTile.x, y);
            isOneWay = TileSlopes.IsOneWay(tileCollisionType);

            if (isOneWay && (!move || ignoresOneWay || state.tmpIgnoresOneWay || y != bottomLeftTile.y))
                continue;

            switch (tileCollisionType)
            {
                default://slope
                    Vector2 tileCenter = map.GetMapTilePosition(topRightTile.x, y);
                    oldSlopeOffset = slopeOffset;

                    var offset = TileSlopes.GetOffset(tileCenter, bottomLeft.x + 0.5f, topRight.x + 0.5f, bottomLeft.y + 0.5f, topRight.y - 0.5f, tileCollisionType);
                    slopeOffset = Mathf.Abs(offset.freeUp) < Mathf.Abs(offset.freeDown) ? offset.freeUp : offset.freeDown;

                    if (!isOneWay && (Mathf.Abs(slopeOffset) >= slopeWallHeight || (slopeOffset < 0 && state.pushesBottomTile) || (slopeOffset > 0 && state.pushesTopTile)))
                    {
                        state.pushesRightTile = true;
                        state.rightTile = new Vector3Int(topRightTile.x, y, 1);
                        return true;
                    }
                    else if (Mathf.Abs(slopeOffset) > Mathf.Abs(oldSlopeOffset))
                    {
                        wasOneWay = isOneWay;
                        slopeCollisionType = tileCollisionType;
                        state.rightTile = new Vector3Int(topRightTile.x, y, 1);
                    }
                    else
                        slopeOffset = oldSlopeOffset;

                    break;
                case TileType.Empty:
                    break;
            }
        }

        if (slopeCollisionType != TileType.Empty && slopeOffset != 0)
        {
            if (slopeOffset > 0 && slopeOffset < slopeWallHeight)
            {
                Vector2 pos = position, tr = topRight, bl = bottomLeft;
                pos.y += slopeOffset - Mathf.Sign(slopeOffset);
                tr.y += slopeOffset - Mathf.Sign(slopeOffset);
                bl.y += slopeOffset - Mathf.Sign(slopeOffset);
                PositionState s = new PositionState();

                if (CollidesWithTileTop(ref pos, ref tr, ref bl, ref s))
                {
                    state.pushesRightTile = true;
                    return true;
                }
                else if (move)
                {
                    position.y += slopeOffset;
                    bottomLeft.y += slopeOffset;
                    topRight.y += slopeOffset;
                    state.pushesBottomTile = true;
                    state.onOneWay = wasOneWay;
                }
            }
            else if (slopeOffset < 0 && slopeOffset > -slopeWallHeight)
            {
                Vector2 pos = position, tr = topRight, bl = bottomLeft;
                pos.y += slopeOffset - Mathf.Sign(slopeOffset);
                tr.y += slopeOffset - Mathf.Sign(slopeOffset);
                bl.y += slopeOffset - Mathf.Sign(slopeOffset);
                PositionState s = new PositionState();

                if (CollidesWithTileBottom(ref pos, ref tr, ref bl, ref s))
                {
                    state.pushesRightTile = true;
                    return true;
                }
                else if (move)
                {
                    position.y += slopeOffset;
                    bottomLeft.y += slopeOffset;
                    topRight.y += slopeOffset;
                    state.pushesTopTile = true;
                    state.onOneWay = wasOneWay;
                }
            }
        }

        if (sticksToSlope && state.pushedBottomTile && move)
        {
            var nextX = map.GetMapTileXAtPoint(bottomLeft.x + 1.0f);
            var bottomY = map.GetMapTileYAtPoint(bottomLeft.y + 1.0f) - 1;

            var prevPos = map.GetMapTilePosition(new Vector3Int(bottomLeftTile.x, bottomLeftTile.y, 1));
            var nextPos = map.GetMapTilePosition(new Vector3Int(nextX, bottomY, 1));

            var prevCollisionType = map.GetCollisionType(new Vector3Int(bottomLeftTile.x, bottomLeftTile.y, 1));
            var nextCollisionType = map.GetCollisionType(new Vector3Int(nextX, bottomY, 1));

            int x1 = (int)Mathf.Clamp((bottomLeft.x - (prevPos.x - Map.tileSize / 2)), 0.0f, 15.0f);
            int x2 = (int)Mathf.Clamp((bottomLeft.x + 1.0f - (nextPos.x - Map.tileSize / 2)), 0.0f, 15.0f);

            int slopeHeight = TileSlopes.GetSlopeHeightFromBottom(x1, prevCollisionType);
            int nextSlopeHeight = TileSlopes.GetSlopeHeightFromBottom(x2, nextCollisionType);

            var offset = slopeHeight + Map.tileSize - nextSlopeHeight;

            if (offset < slopeWallHeight && offset > 0)
            {
                Vector2 pos = position, tr = topRight, bl = bottomLeft;
                pos.y -= offset - Mathf.Sign(offset);
                tr.y -= offset - Mathf.Sign(offset);
                bl.y -= offset - Mathf.Sign(offset);
                bl.x += 1.0f;
                tr.x += 1.0f;
                PositionState s = new PositionState();

                if (!CollidesWithTileBottom(ref pos, ref tr, ref bl, ref s))
                {
                    position.y -= offset;
                    bottomLeft.y -= offset;
                    topRight.y -= offset;
                    state.pushesBottomTile = true;
                    state.onOneWay = wasOneWay;
                }
            }
        }

        return false;
    }

    public bool CollidesWithTileTop(ref Vector2 position, ref Vector2 topRight, ref Vector2 bottomLeft, ref PositionState state)
    {
        Vector3Int topRightTile = map.GetMapTileAtPoint(new Vector2(topRight.x - 0.5f, topRight.y + 0.5f));
        Vector3Int bottomleftTile = map.GetMapTileAtPoint(new Vector2(bottomLeft.x + 0.5f, bottomLeft.y + 0.5f));
        int freeDown = int.MaxValue;
        int slopeX = -1;

        for (int x = bottomleftTile.x; x <= topRightTile.x; ++x)
        {
            var tileCollisionType = map.GetCollisionType(x, topRightTile.y);

            if (TileSlopes.IsOneWay(tileCollisionType))
                continue;

            switch (tileCollisionType)
            {
                default://slope

                    Vector2 tileCenter = map.GetMapTilePosition(x, topRightTile.y);
                    TileSlopeOffsetI sf = TileSlopes.GetOffset(tileCenter, bottomLeft.x + 0.5f, topRight.x - 0.5f, bottomLeft.y + 0.5f, topRight.y + 0.5f, tileCollisionType);
                    sf.freeDown += 1;
                    sf.collidingTop += 1;

                    if (sf.freeDown < freeDown && sf.freeDown <= 0 && sf.freeDown == sf.collidingTop)
                    {
                        freeDown = sf.freeDown;
                        slopeX = x;
                    }

                    break;
                case TileType.Empty:
                    break;
                case TileType.Full:
                    state.pushesTopTile = true;
                    state.topTile = new Vector3Int(x, topRightTile.y, 1);
                    return true;
            }
        }

        if (slopeX != -1)
        {
            state.pushesTopTile = true;
            state.topTile = new Vector3Int(slopeX, topRightTile.y, 1);
            position.y += freeDown;
            topRight.y += freeDown;
            bottomLeft.y += freeDown;
            return true;
        }

        return false;
    }

    public bool CollidesWithTileBottom(ref Vector2 position, ref Vector2 topRight, ref Vector2 bottomLeft, ref PositionState state)
    {
        Vector3Int topRightTile = map.GetMapTileAtPoint(new Vector2(topRight.x - 0.5f, topRight.y - 0.5f));
        Vector3Int bottomleftTile = map.GetMapTileAtPoint(new Vector2(bottomLeft.x + 0.5f, bottomLeft.y - 0.5f));
        int collidingBottom = int.MinValue;
        int slopeX = -1;
        bool wasOneWay = false, isOneWay;
        

        for (int x = bottomleftTile.x; x <= topRightTile.x; ++x)
        {
            var tileCollisionType = map.GetCollisionType(x, bottomleftTile.y);

            isOneWay = TileSlopes.IsOneWay(tileCollisionType);

            if ((ignoresOneWay || state.tmpIgnoresOneWay) && isOneWay)
                continue;

            switch (tileCollisionType)
            {
                default://slope

                    Vector2 tileCenter = map.GetMapTilePosition(x, bottomleftTile.y);

                    TileSlopeOffsetI sf = TileSlopes.GetOffset(tileCenter, bottomLeft.x + 0.5f, topRight.x - 0.5f, bottomLeft.y - 0.5f, topRight.y - 0.5f, tileCollisionType);
                    sf.freeUp -= 1;
                    sf.collidingBottom -= 1;

                    if (((sf.freeUp >= 0 && sf.collidingBottom == sf.freeUp)
                            || (sticksToSlope && state.pushedBottom && sf.freeUp - sf.collidingBottom < slopeWallHeight && sf.freeUp >= sf.collidingBottom))
                        && sf.collidingBottom >= collidingBottom
                        && !(isOneWay && Mathf.Abs(sf.collidingBottom) >= slopeWallHeight))
                    {
                        wasOneWay = isOneWay;
                        collidingBottom = sf.collidingBottom;
                        slopeX = x;
                    }

                    break;
                case TileType.Empty:
                    break;
                case TileType.Full:
                    state.onOneWay = false;
                    state.pushesBottomTile = true;
                    state.bottomTile = new Vector3Int(x, bottomleftTile.y, 1);
                    state.tmpIgnoresOneWay = false;
                    return true;
            }
        }

        if (slopeX != -1)
        {
            state.onOneWay = wasOneWay;
            state.oneWayY = bottomleftTile.y;
            state.pushesBottomTile = true;
            state.bottomTile = new Vector3Int(slopeX, bottomleftTile.y, 1);
            position.y += collidingBottom;
            topRight.y += collidingBottom;
            bottomLeft.y += collidingBottom;
            return true;
        }

        return false;
    }
    #endregion
    #region Move
    private void MoveX(ref Vector2 position, ref bool foundObstacleX, float offset, float step, ref Vector2 topRight, ref Vector2 bottomLeft, ref PositionState state)
    {
        while (!foundObstacleX && offset != 0.0f)
        {
            offset -= step;

            if (step > 0.0f)
                foundObstacleX = CollidesWithTileRight(ref position, ref topRight, ref bottomLeft, ref state, true);
            else
                foundObstacleX = CollidesWithTileLeft(ref position, ref topRight, ref bottomLeft, ref state, true);

            if (!foundObstacleX)
            {
                position.x += step;
                topRight.x += step;
                bottomLeft.x += step;

                CollidesWithTileTop(ref position, ref topRight, ref bottomLeft, ref state);
                CollidesWithTileBottom(ref position, ref topRight, ref bottomLeft, ref state);
            }
        }
    }

    private void MoveY(ref Vector2 position, ref bool foundObstacleY, float offset, float step, ref Vector2 topRight, ref Vector2 bottomLeft, ref PositionState state)
    {
        while (!foundObstacleY && offset != 0.0f)
        {
            offset -= step;

            if (step > 0.0f)
                foundObstacleY = CollidesWithTileTop(ref position, ref topRight, ref bottomLeft, ref state);
            else
                foundObstacleY = CollidesWithTileBottom(ref position, ref topRight, ref bottomLeft, ref state);

            if (!foundObstacleY)
            {
                position.y += step;
                topRight.y += step;
                bottomLeft.y += step;

                CollidesWithTileLeft(ref position, ref topRight, ref bottomLeft, ref state);
                CollidesWithTileRight(ref position, ref topRight, ref bottomLeft, ref state);
            }
        }
    }

    public void Move(Vector2 offset, Vector2 speed, ref Vector2 position, ref Vector2 reminder, BoundingBox bBox, ref PositionState state)
    {
        reminder += offset;

        Vector2 topRight = bBox.Max();
        Vector2 bottomLeft = bBox.Min();

        bool foundObstacleX = false, foundObstacleY = false;

        var step = new Vector2(Mathf.Sign(offset.x), Mathf.Sign(offset.y));
        var move = new Vector2(Mathf.Round(reminder.x), Mathf.Round(reminder.y));
        reminder -= move;

        if (move.x == 0.0f && move.y == 0.0f)
            return;
        else if (move.x != 0.0f && move.y == 0.0f)
        {
            MoveX(ref position, ref foundObstacleX, move.x, step.x, ref topRight, ref bottomLeft, ref state);

            if (step.x > 0.0f)
                state.pushesLeftTile = CollidesWithTileLeft(ref position, ref topRight, ref bottomLeft, ref state);
            else
                state.pushesRightTile = CollidesWithTileRight(ref position, ref topRight, ref bottomLeft, ref state);
        }
        else if (move.y != 0.0f && move.x == 0.0f)
        {
            MoveY(ref position, ref foundObstacleY, move.y, step.y, ref topRight, ref bottomLeft, ref state);

            if (step.y > 0.0f)
                state.pushesBottomTile = CollidesWithTileBottom(ref position, ref topRight, ref bottomLeft, ref state);
            else
                state.pushesTopTile = CollidesWithTileTop(ref position, ref topRight, ref bottomLeft, ref state);

            if (!ignoresOneWay && state.tmpIgnoresOneWay && map.GetMapTileYAtPoint(bottomLeft.y - 0.5f) != state.oneWayY)
                state.tmpIgnoresOneWay = false;
        }
        else
        {
            float speedRatio = Mathf.Abs(speed.y) / Mathf.Abs(speed.x);
            float vertAccum = 0.0f;

            while (!foundObstacleX && !foundObstacleY && (move.x != 0.0f || move.y != 0.0f))
            {
                vertAccum += Mathf.Sign(step.y) * speedRatio;

                MoveX(ref position, ref foundObstacleX, step.x, step.x, ref topRight, ref bottomLeft, ref state);
                move.x -= step.x;

                while (!foundObstacleY && move.y != 0.0f && (Mathf.Abs(vertAccum) >= 1.0f || move.x == 0.0f))
                {
                    move.y -= step.y;
                    vertAccum -= step.y;

                    MoveY(ref position, ref foundObstacleX, step.y, step.y, ref topRight, ref bottomLeft, ref state);
                }
            }

            if (step.x > 0.0f)
                state.pushesLeftTile = CollidesWithTileLeft(ref position, ref topRight, ref bottomLeft, ref state);
            else
                state.pushesRightTile = CollidesWithTileRight(ref position, ref topRight, ref bottomLeft, ref state);

            if (step.y > 0.0f)
                state.pushesBottomTile = CollidesWithTileBottom(ref position, ref topRight, ref bottomLeft, ref state);
            else
                state.pushesTopTile = CollidesWithTileTop(ref position, ref topRight, ref bottomLeft, ref state);

            if (!ignoresOneWay && state.tmpIgnoresOneWay && map.GetMapTileYAtPoint(bottomLeft.y - 0.5f) != state.oneWayY)
                state.tmpIgnoresOneWay = false;
        }
    }
    #endregion
    #region UpdatePhysics
    public void UpdatePhysics()
    {
        //assign the previous state of onGround, atCeiling, pushesRightWall, pushesLeftWall
        //before those get recalculated for this frame
        posState.pushedBottom = posState.pushesBottom;
        posState.pushedRight = posState.pushesRight;
        posState.pushedLeft = posState.pushesLeft;
        posState.pushedTop = posState.pushesTop;

        posState.pushedBottomTile = posState.pushesBottomTile;
        posState.pushedLeftTile = posState.pushesLeftTile;
        posState.pushedRightTile = posState.pushesRightTile;
        posState.pushedTopTile = posState.pushesTopTile;

        posState.pushesBottomTile = posState.pushesBottom;
        posState.pushesTopTile = posState.pushesTop;
        posState.pushesRightTile = posState.pushesRight;
        posState.pushesLeftTile = posState.pushesLeft;

        posState.pushesBottomTile = posState.pushesLeftTile = posState.pushesRightTile = posState.pushesTopTile =
        posState.pushesBottomObject = posState.pushesLeftObject = posState.pushesRightObject = posState.pushesTopObject = false;

        Vector2 topRight = bBox.Max();
        Vector2 bottomLeft = bBox.Min();

        CollidesWithTiles(ref position, ref topRight, ref bottomLeft, ref posState);

        if (!posState.pushesRightTile)
        {
            CollidesWithTiles(ref position, ref topRight, ref bottomLeft, ref posState);
        }
        //save the speed to oldSpeedForse vector
        oldSpeedForse = speedForse;

        if (posState.pushesBottomTile)
            speedForse.y = Mathf.Max(0.0f, speedForse.y);
        if (posState.pushesTopTile)
            speedForse.y = Mathf.Min(0.0f, speedForse.y);
        if (posState.pushesLeftTile)
            speedForse.x = Mathf.Max(0.0f, speedForse.x);
        if (posState.pushesRightTile)
            speedForse.x = Mathf.Min(0.0f, speedForse.x);

        //save the position to the previousPosition vector
        previousPosition = position;

        Vector2 newPosition = position + speedForse * Time.deltaTime;

        Vector2 offset = newPosition - position;

        if (offset != Vector2.zero)
            Move(offset, speedForse, ref position, ref reminder, bBox, ref posState);

        if (mountParent != null)
            if (!HasCollisionDataFor(mountParent) || !mountParent.isMount)
                mountParent = null;

        //update the bBox
        bBox.Center = position;

        posState.pushesBottom = posState.pushesBottomTile;
        posState.pushesRight = posState.pushesRightTile;
        posState.pushesLeft = posState.pushesLeftTile;
        posState.pushesTop = posState.pushesTopTile;
    }
    private void UpdatePhysicsResponse()
    {
        if (isKinematic || isTriger)
            return;

        posState.pushedBottomObject = posState.pushesBottomObject;
        posState.pushedRightObject = posState.pushesRightObject;
        posState.pushedLeftObject = posState.pushesLeftObject;
        posState.pushedTopObject = posState.pushesTopObject;

        posState.pushesBottomObject = false;
        posState.pushesRightObject = false;
        posState.pushesLeftObject = false;
        posState.pushesTopObject = false;

        Vector2 offsetSum = Vector2.zero;

        for (int i = 0; i < allCollidingObjects.Count; ++i)
        {
            var other = allCollidingObjects[i].other;
            var data = allCollidingObjects[i];
            var overlap = data.overlap - offsetSum;

            if (overlap.x == 0.0f)
            {
                if (other.bBox.Center.x > bBox.Center.x)
                {
                    posState.pushesRightObject = true;
                    speedForse.x = Mathf.Min(speedForse.x, 0.0f);
                }
                else
                {
                    posState.pushesLeftObject = true;
                    speedForse.x = Mathf.Max(speedForse.x, 0.0f);
                }
                continue;
            }
            else if (overlap.y == 0.0f)
            {
                if (other.bBox.Center.y > bBox.Center.y)
                {
                    posState.pushesTopObject = true;
                    speedForse.y = Mathf.Min(speedForse.y, 0.0f);
                }
                else
                {
                    if (mountParent == null && other.isMount) mountParent = other;
                    posState.pushesBottomObject = true;
                    speedForse.y = Mathf.Max(speedForse.y, 0.0f);
                }
                continue;
            }

            Vector2 absSpeed1 = new Vector2(Mathf.Abs(data.pos1.x - data.oldPos1.x), Mathf.Abs(data.pos1.y - data.oldPos1.y));
            Vector2 absSpeed2 = new Vector2(Mathf.Abs(data.pos2.x - data.oldPos2.x), Mathf.Abs(data.pos2.y - data.oldPos2.y));
            Vector2 speedSum = absSpeed1 + absSpeed2;

            float speedRatioX, speedRatioY;

            if (other.isKinematic || !other.isTriger)
                speedRatioX = speedRatioY = 1.0f;
            else
            {
                if (speedSum.x == 0.0f && speedSum.y == 0.0f)
                {
                    speedRatioX = speedRatioY = 0.5f;
                }
                else if (speedSum.x == 0.0f)
                {
                    speedRatioX = 0.5f;
                    speedRatioY = absSpeed1.y / speedSum.y;
                }
                else if (speedSum.y == 0.0f)
                {
                    speedRatioX = absSpeed1.x / speedSum.x;
                    speedRatioY = 0.5f;
                }
                else
                {
                    speedRatioX = absSpeed1.x / speedSum.x;
                    speedRatioY = absSpeed1.y / speedSum.y;
                }
            }

            float offsetX = overlap.x * speedRatioX;
            float offsetY = overlap.y * speedRatioY;

            bool overlappedLastFrameX = Mathf.Abs(data.oldPos1.x - data.oldPos2.x) < bBox.HalfSizeX + other.bBox.HalfSizeX;
            bool overlappedLastFrameY = Mathf.Abs(data.oldPos1.y - data.oldPos2.y) < bBox.HalfSizeY + other.bBox.HalfSizeY;

            if ((!overlappedLastFrameX && overlappedLastFrameY)
                || (!overlappedLastFrameX && !overlappedLastFrameY && Mathf.Abs(overlap.x) <= Mathf.Abs(overlap.y)))
            {
                position.x += offsetX;
                offsetSum.x += offsetX;

                if (overlap.x < 0.0f)
                {
                    if (other.isKinematic && other.isMount && posState.pushesLeftTile)
                        OnСrushed.Invoke();
                    posState.pushesRightObject = true;
                    speedForse.x = Mathf.Min(speedForse.x, 0.0f);
                }
                else
                {
                    if (other.isKinematic && other.isMount && posState.pushesRightTile)
                        OnСrushed.Invoke();
                    posState.pushesLeftObject = true;
                    speedForse.x = Mathf.Max(speedForse.x, 0.0f);
                }
            }
            else
            {
                position.y += offsetY;
                offsetSum.y += offsetY;

                if (overlap.y < 0.0f)
                {
                    if (other.isKinematic && other.isMount && posState.pushesBottomTile)
                        OnСrushed.Invoke();
                    posState.pushesTopObject = true;
                    speedForse.y = Mathf.Min(speedForse.y, 0.0f);
                }
                else
                {
                    if (other.isKinematic && other.isMount && posState.pushesTopTile)
                        OnСrushed.Invoke();
                    if (mountParent == null && other.isMount) mountParent = other;
                    posState.pushesBottomObject = true;
                    speedForse.y = Mathf.Max(speedForse.y, 0.0f);
                }
            }
        }
    }
    public void UpdatePhysicsFinaly()
    {
        UpdatePhysicsResponse();

        posState.pushesBottom = posState.pushesBottomTile || posState.pushesBottomObject;
        posState.pushesRight = posState.pushesRightTile || posState.pushesRightObject;
        posState.pushesLeft = posState.pushesLeftTile || posState.pushesLeftObject;
        posState.pushesTop = posState.pushesTopTile || posState.pushesTopObject;

        //update the bBox
        bBox.Center = position + BBoxOffset;

        mountY = mountParent != null ? mountParent.previousPosition.y - mountParent.position.y : 0;
        if (mountY < 0) mountY = 0;
        else if (mountY > 0) mountY = halfSizeY / 10;
        float mountX = mountParent != null ? mountParent.previousPosition.x - mountParent.position.x : 0;
        Vector2 mount = new Vector2(mountX, mountY);
        position -= mount;

        //apply the changes to the transform
        transform.position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), transform.position.z);
        transform.localScale = new Vector3(ScaleX, transform.localScale.y, transform.localScale.z);
    }
    #endregion

    protected void Gravity()
    {
        speedForse.y += physicsSettings.GravityForce * Time.deltaTime + AntiGravityForce;
        speedForse.y = Mathf.Max(speedForse.y, physicsSettings.MaxFallingSpeed);
    }

    public void CollisionLogic()
    {
        if (allCollidingObjects.Count < onExitCount)
        {
            if (onExit != null) onExit.Invoke(allCollidingObjects);
            onExitCount = allCollidingObjects.Count;
        }

        if (allCollidingObjects.Count > 0 && onEnterHelper)
        {
            onExitCount = onEnterCount = allCollidingObjects.Count;
            if(onEnter != null) onEnter.Invoke(allCollidingObjects);
            onEnterHelper = false;
        }
        else if (allCollidingObjects.Count != onEnterCount)
            onEnterHelper = true;

        if (allCollidingObjects.Count > 0 && onStay != null) onStay.Invoke(allCollidingObjects);
    }

    public bool HasCollisionDataFor(PhysicsBody other) // Have we defined collision between two objects? If yes - return false
    {
        for (int i = 0; i < allCollidingObjects.Count; ++i)
            if (allCollidingObjects[i].other == other)
                return false;
        return true;
    }

    Vector2 RoundVector(Vector2 v)
    {
        return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawPhysicsBodyGizmos();
    }
    protected void DrawPhysicsBodyGizmos() // Draws the bBox and ceiling, ground and wall sensors
    {
        Vector3 bBoxPos;
        Gizmos.color = Color.yellow;
        if (EditorApplication.isPlaying) // calculate the position of the bBox's center and draw the bBox rectangle
        {
            bBoxPos = transform.position + (Vector3)BBoxOffset;
            Gizmos.DrawWireCube(bBoxPos, bBox.HalfSize * 2.0f);
        }
        else
        {
            bBoxPos = transform.position + new Vector3(0, (halfSizeY) + bBoxPositionOffsetY);
            Gizmos.DrawWireCube(bBoxPos, new Vector3(halfSizeX, halfSizeY) * 2.0f);
        }

        //draw the ground checking sensor
        Vector2 bottomLeft = bBoxPos - new Vector3(bBox.HalfSizeX, bBox.HalfSizeY, 0.0f) - Vector3.up + Vector3.right;
        var bottomRight = new Vector2(bottomLeft.x + bBox.HalfSizeX * 2.0f - 2.0f, bottomLeft.y);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(bottomLeft, bottomRight);

        //draw the ceiling checking sensor
        Vector2 topRight = bBoxPos + new Vector3(bBox.HalfSizeX, bBox.HalfSizeY, 0.0f) + Vector3.up - Vector3.right;
        var topLeft = new Vector2(topRight.x - bBox.HalfSizeX * 2.0f + 2.0f, topRight.y);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(topLeft, topRight);

        //draw left wall checking sensor
        bottomLeft = bBoxPos - new Vector3(bBox.HalfSizeX, bBox.HalfSizeY, 0.0f) - Vector3.right;
        topLeft = bottomLeft;
        topLeft.y += bBox.HalfSizeY * 2.0f;

        Gizmos.DrawLine(topLeft, bottomLeft);

        //draw right wall checking sensor
        bottomRight = bBoxPos + new Vector3(bBox.HalfSizeX, -bBox.HalfSizeY, 0.0f) + Vector3.right;
        topRight = bottomRight;
        topRight.y += bBox.HalfSizeY * 2.0f;

        Gizmos.DrawLine(topRight, bottomRight);
    }
#endif

    [Serializable]
    public struct PositionState // need to keep track of character state
    {
        public bool pushesRight;
        public bool pushesLeft;
        public bool pushesBottom;
        public bool pushesTop;

        public bool pushedTop;
        public bool pushedBottom;
        public bool pushedRight;
        public bool pushedLeft;

        public bool pushedLeftObject;
        public bool pushedRightObject;
        public bool pushedBottomObject;
        public bool pushedTopObject;

        public bool pushesLeftObject;
        public bool pushesRightObject;
        public bool pushesBottomObject;
        public bool pushesTopObject;

        public bool pushedLeftTile;
        public bool pushedRightTile;
        public bool pushedBottomTile;
        public bool pushedTopTile;

        public bool pushesLeftTile;
        public bool pushesRightTile;
        public bool pushesBottomTile;
        public bool pushesTopTile;

        public bool onOneWay;
        public bool tmpIgnoresOneWay;
        public int oneWayY;

        public Vector3Int leftTile;
        public Vector3Int rightTile;
        public Vector3Int topTile;
        public Vector3Int bottomTile;

        public void Reset()
        {
            leftTile = rightTile = topTile = bottomTile = new Vector3Int(-1, -1, 1);
            oneWayY = -1;

            pushesRight = false;
            pushesLeft = false;
            pushesBottom = false;
            pushesTop = false;

            pushedTop = false;
            pushedBottom = false;
            pushedRight = false;
            pushedLeft = false;

            pushedLeftObject = false;
            pushedRightObject = false;
            pushedBottomObject = false;
            pushedTopObject = false;

            pushesLeftObject = false;
            pushesRightObject = false;
            pushesBottomObject = false;
            pushesTopObject = false;

            pushedLeftTile = false;
            pushedRightTile = false;
            pushedBottomTile = false;
            pushedTopTile = false;

            pushesLeftTile = false;
            pushesRightTile = false;
            pushesBottomTile = false;
            pushesTopTile = false;

            onOneWay = false;
        }
    }
}
