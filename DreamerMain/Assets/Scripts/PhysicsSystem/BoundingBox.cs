using UnityEngine;

[System.Serializable]
public struct BoundingBox
{
    public Vector2 Scale;
    public Vector2 Center;

    private Vector2 halfSize;
    public Vector2 HalfSize
    {
        set { halfSize = value; }
        get { return new Vector2(Mathf.Round(halfSize.x * Scale.x), Mathf.Round(halfSize.y * Scale.y)); }
    }
    public float HalfSizeX
    {
        set { halfSize.x = value; }
        get { return Mathf.Round(halfSize.x * Scale.x); }
    }
    public float HalfSizeY
    {
        set { halfSize.y = value; }
        get { return Mathf.Round(halfSize.y * Scale.y); }
    }

    public BoundingBox(Vector2 Center, Vector2 halfSize)
    {
        Scale = Vector2.one;
        this.Center = Center;
        this.halfSize = halfSize;
    }

    public Vector2 Max()
    {
        return Center + HalfSize;
    }
    public Vector2 Min()
    {
        return Center - HalfSize;
    }

    #region intersection detection
    public bool Overlaps(BoundingBox other)
    {
        if (Mathf.Abs(Center.x - other.Center.x) > halfSize.x + other.halfSize.x) return false;
        if (Mathf.Abs(Center.y - other.Center.y) > halfSize.y + other.halfSize.y) return false;
        return true;
    }

    public bool OverlapsSigned(BoundingBox other, out Vector2 overlap)
    {
        overlap = Vector2.zero;

        if (HalfSizeX == 0.0f || HalfSizeY == 0.0f || other.HalfSizeX == 0.0f || other.HalfSizeY == 0.0f
            || Mathf.Abs(Center.x - other.Center.x) > HalfSizeX + other.HalfSizeX
            || Mathf.Abs(Center.y - other.Center.y) > HalfSizeY + other.HalfSizeY) return false;

        overlap = new Vector2(Mathf.Sign(Center.x - other.Center.x) * ((other.HalfSizeX + HalfSizeX) - Mathf.Abs(Center.x - other.Center.x)),
            Mathf.Sign(Center.y - other.Center.y) * ((other.HalfSizeY + HalfSizeY) - Mathf.Abs(Center.y - other.Center.y)));

        return true;
    }
    #endregion
}
