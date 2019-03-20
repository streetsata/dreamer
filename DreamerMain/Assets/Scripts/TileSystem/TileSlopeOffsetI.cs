[System.Serializable]
public struct TileSlopeOffsetI
{
    public int freeLeft, freeRight, freeDown, freeUp, collidingLeft, collidingRight, collidingBottom, collidingTop;

    public TileSlopeOffsetI(int _freeLeft, int _freeRight, int _freeDown, int _freeUp, int _collidingLeft, int _collidingRight, int _collidingBottom, int _collidingTop)
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

    public TileSlopeOffsetI(TileSlopeOffsetSB other)
    {
        freeLeft = other.freeLeft;
        freeRight = other.freeRight;
        freeDown = other.freeDown;
        freeUp = other.freeUp;

        collidingLeft = other.collidingLeft;
        collidingRight = other.collidingRight;
        collidingBottom = other.collidingBottom;
        collidingTop = other.collidingTop;
    }
}
