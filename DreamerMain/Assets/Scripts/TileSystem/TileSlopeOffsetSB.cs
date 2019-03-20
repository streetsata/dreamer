[System.Serializable]
public struct TileSlopeOffsetSB
{
    public sbyte freeLeft, freeRight, freeDown, freeUp, collidingLeft, collidingRight, collidingBottom, collidingTop;

    public TileSlopeOffsetSB(sbyte _freeLeft, sbyte _freeRight, sbyte _freeDown, sbyte _freeUp, sbyte _collidingLeft, sbyte _collidingRight, sbyte _collidingBottom, sbyte _collidingTop)
    {
        freeLeft = _freeLeft;
        freeRight = _freeRight;
        freeDown = _freeDown;
        freeUp = _freeUp;

        collidingLeft = _collidingLeft;
        collidingRight = _collidingRight;
        collidingBottom = _collidingBottom;
        collidingTop = _collidingTop;
    }
}
