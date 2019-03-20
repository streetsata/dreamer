using UnityEngine;
using UnityEngine.Events;

public class MovableCharacter : PhysicsBody
{
    [SerializeField] private float
        jumpForse = 460.0f,
        minJumpForse = 200.0f,
        runSpeed = 260.0f,
        walkSpeed = 160.0f;

    [Space][SerializeField] private CharacterEvent characterEvent;
    [SerializeField] private CharacterStateEvent stateEvent;

    [HideInInspector] public bool IsSwim = false;
    [HideInInspector] public bool NotRunning = false;
    [HideInInspector] public float SpeedDebuff = 0;
    [HideInInspector] public float runBoost = 0f;

    [HideInInspector] public MovingState currentState = MovingState.Stand;
    private MovingState oldState = MovingState.Stand;

    private Vector3Int ledgeTile; // captured position (GrabLedge)
    private const float grabLedgeStartY = 0.0f;
    private const float grabLedgeEndY = 2.0f;

    private int cannotGoLeftFrames = 0;
    private int cannotGoRightFrames = 0;

    private float walkTimer = 0.0f; // to delay the event walk

    protected int framesFromJumpStart = 0; // count frames for which character can still jump once in air
    protected int jumpFramesThreshold = 10;

    private bool sit = true;

    protected bool[] inputs; 
    protected bool[] prevInputs;

    public void Init(bool[] inputs, bool[] prevInputs)
    {
        this.inputs = inputs;
        this.prevInputs = prevInputs;
    }

    public override void CommonUpdate()
    {
        CollisionLogic();
        UpdatePhysicsFinaly();

        if (inputs[(int)KeyInput.Run] && !NotRunning)
        {
            if (runBoost <= runSpeed - walkSpeed) runBoost++;
            else runBoost = runSpeed - walkSpeed;
        }
        else runBoost = 0f;

        switch (currentState)
        {
            #region MovingState.Stand
            case MovingState.Stand:
                walkTimer = characterEvent.delayLoopTimer;
                speedForse = Vector2.zero; // stop move

                if (!posState.pushesBottom)
                {
                    ChangeCurrentState(MovingState.Jump);
                    break;
                }

                if (KeyState(KeyInput.GoRight) != KeyState(KeyInput.GoLeft)) // if left or right key is pressed, but not both
                {
                    ChangeCurrentState(MovingState.Walk);
                    break;
                }
                else if (KeyState(KeyInput.Jump))
                {
                    ScaleReturner();
                    speedForse.y = jumpForse;
                    ChangeCurrentState(MovingState.Jump);
                    break;
                }

                Sit(MovingState.Stand);

                break;
            #endregion
            #region MovingState.Walk
            case MovingState.Walk:
                #region delay loop logic
                walkTimer += Time.deltaTime;
                if (walkTimer > characterEvent.delayLoopTimer)
                {
                    walkTimer = 0.0f;
                    characterEvent.OnWalkLoop.Invoke();
                }
                #endregion

                LeftRightMove();

                if (KeyState(KeyInput.Jump)) 
                {
                    speedForse.y = jumpForse;
                    ScaleReturner();
                    ChangeCurrentState(MovingState.Jump);
                    break;
                }
                else if (!posState.pushesBottom) //if there's no tile to walk on, fall
                {
                    ScaleReturner();
                    ChangeCurrentState(MovingState.Jump);
                    break;
                }

                if (KeyState(KeyInput.GoDown))
                    posState.tmpIgnoresOneWay = true;

                if (speedForse.x == 0.0f) ChangeCurrentState(MovingState.Stand);

                break;
            #endregion
            #region MovingState.Jump
            case MovingState.Jump:

                #region logic which character can still jump once in air
                ++framesFromJumpStart;
                if (framesFromJumpStart <= jumpFramesThreshold)
                {
                    if (posState.pushesTop || speedForse.y > 0.0f)
                        framesFromJumpStart = jumpFramesThreshold + 1;
                    else if (KeyState(KeyInput.Jump))
                    {
                        ScaleReturner();
                        speedForse.y = jumpForse;
                    }
                        
                }
                #endregion

                walkTimer = characterEvent.delayLoopTimer; // update delay loop logic

                Gravity();

                if (!KeyState(KeyInput.Jump) && speedForse.y > 0.0f)
                    speedForse.y = Mathf.Min(speedForse.y, minJumpForse);

                LeftRightMove();

                if (posState.pushesBottom) //if we hit the ground
                {
                    if (KeyState(KeyInput.GoRight) == KeyState(KeyInput.GoLeft)) //if there's no movement change state to standing
                    {
                        ChangeCurrentState(MovingState.Stand);
                        speedForse = Vector2.zero;
                        characterEvent.OnHitWall.Invoke(); // на удаление
                    }
                    else	//either go right or go left are pressed so we change the state to walk
                    {
                        ChangeCurrentState(MovingState.Walk);
                        speedForse.y = 0.0f;
                        characterEvent.OnHitWall.Invoke(); // на удаление
                    }
                }

                if (cannotGoLeftFrames > 0)
                {
                    --cannotGoLeftFrames;
                    inputs[(int)KeyInput.GoLeft] = false;
                }
                if (cannotGoRightFrames > 0)
                {
                    --cannotGoRightFrames;
                    inputs[(int)KeyInput.GoRight] = false;
                }

                #region Grab Ledge
                if (speedForse.y <= 0.0f && !posState.pushesTop && ((posState.pushesRight && inputs[(int)KeyInput.GoRight]) || (posState.pushesLeft && inputs[(int)KeyInput.GoLeft])))
                {
                    Vector2 bBoxCornerOffset;

                    if (posState.pushesRight && inputs[(int)KeyInput.GoRight])
                        bBoxCornerOffset = bBox.HalfSize;
                    else
                        bBoxCornerOffset = new Vector2(-bBox.HalfSizeX - 1.0f, bBox.HalfSizeY);

                    int tileX, topY, bottomY;
                    tileX = map.GetMapTileXAtPoint(bBox.Center.x + bBoxCornerOffset.x);

                    if ((posState.pushedLeft && posState.pushesLeft) || (posState.pushedRight && posState.pushesRight))
                    {
                        topY = map.GetMapTileYAtPoint(previousPosition.y + BBoxOffsetY + bBoxCornerOffset.y - grabLedgeStartY);
                        bottomY = map.GetMapTileYAtPoint(bBox.Center.y + bBoxCornerOffset.y - grabLedgeEndY);
                    }
                    else
                    {
                        topY = map.GetMapTileYAtPoint(bBox.Center.y + bBoxCornerOffset.y - grabLedgeStartY);
                        bottomY = map.GetMapTileYAtPoint(bBox.Center.y + bBoxCornerOffset.y - grabLedgeEndY);
                    }

                    for (int y = topY; y >= bottomY; --y)
                    {
                        if (!map.IsObstacle(tileX, y) && map.IsObstacle(tileX, y - 1))
                        {
                            // calculate the corresponding angle
                            var tileCorner = map.GetMapTilePosition(tileX, y - 1);
                            tileCorner.x -= Mathf.Sign(bBoxCornerOffset.x) * Map.tileSize / 2;
                            tileCorner.y += Map.tileSize / 2;

                            if (y > bottomY || ((bBox.Center.y + bBoxCornerOffset.y) - tileCorner.y <= grabLedgeEndY && tileCorner.y - (bBox.Center.y + bBoxCornerOffset.y) >= grabLedgeStartY))
                            {
                                // save tile that we hold so that we can later check to see if we can hold it
                                ledgeTile = new Vector3Int(tileX, y - 1, 0);
                                // calculate position
                                position.y = tileCorner.y - bBoxCornerOffset.y - BBoxOffsetY - grabLedgeStartY - 4.0f;
                                speedForse = Vector2.zero;
                                // grab!
                                ChangeCurrentState(MovingState.GrabLedge);
                                break;
                            }
                        }
                    }
                }
                #endregion

                if (KeyState(KeyInput.GoDown)) // logic jumping from the platform (if is possible)
                    posState.tmpIgnoresOneWay = true;
                else if (sit && !KeyState(KeyInput.GoDown)) ScaleReturner();

                break;
            #endregion
            #region MovingState.GrabLedge
            case MovingState.GrabLedge:
                bool ledgeOnLeft = ledgeTile.x * Map.tileSize < position.x;
                bool ledgeOnRight = !ledgeOnLeft;

                // if button is input down, then we jump
                if (inputs[(int)KeyInput.GoDown] || (inputs[(int)KeyInput.GoLeft] && ledgeOnRight) || (inputs[(int)KeyInput.GoRight] && ledgeOnLeft))
                {
                    if (ledgeOnLeft) cannotGoLeftFrames = 3;
                    else cannotGoRightFrames = 3;
                    stateEvent.OnJumpAfterGrabLedge.Invoke();
                    ChangeCurrentState(MovingState.Jump);
                }
                else if (inputs[(int)KeyInput.Jump])
                {
                    ScaleReturner();
                    speedForse.y = jumpForse;
                    stateEvent.OnJumpAfterGrabLedge.Invoke();
                    ChangeCurrentState(MovingState.Jump);
                }

                if (!map.IsObstacle(ledgeTile.x, ledgeTile.y))
                {
                    stateEvent.OnJumpAfterGrabLedge.Invoke();
                    ChangeCurrentState(MovingState.Jump);
                }

                break;
                #endregion
        }

        UpdatePhysics();

        if (posState.pushedBottom && !posState.pushesBottom) // if we are not with the ground - reset the frames from which you can jump
            framesFromJumpStart = 0;

        if (posState.pushesBottom && !posState.pushedBottom) // if we are faced with ground
            characterEvent.OnHitWall.Invoke();

        for (byte b = 0; b < (byte)KeyInput.Count; ++b)
            prevInputs[b] = inputs[b]; // update past input on current

        map.UpdateAreas(this);
        allCollidingObjects.Clear();
    }

    private void ChangeCurrentState(MovingState state)
    {
        switch (oldState)
        {
            case MovingState.Stand:
                switch (state)
                {
                    case MovingState.Walk:
                        if (sit) stateEvent.OnSitDownRunState.Invoke();
                        else stateEvent.OnWalkAfterStand.Invoke();
                        break;
                    case MovingState.Jump:
                        if (!IsSwim) stateEvent.OnJumpAfterStand.Invoke();
                        else stateEvent.OnSwimAfterStand.Invoke();
                        break;
                }
                break;
            case MovingState.Walk:
                switch (state)
                {
                    case MovingState.Stand:
                        if (sit) stateEvent.OnSitDownState.Invoke();
                        else stateEvent.OnStandAfterWalk.Invoke();
                        break;
                    case MovingState.Jump:
                        if (!IsSwim) stateEvent.OnJumpAfterWalk.Invoke();
                        else stateEvent.OnSwimAfterWalk.Invoke();
                        break;
                }
                break;
            case MovingState.Jump:
                switch (state)
                {
                    case MovingState.Stand:
                        stateEvent.OnStandAfterJump.Invoke();
                        break;
                    case MovingState.Walk:
                        if (sit) stateEvent.OnSitDownRunState.Invoke();
                        else stateEvent.OnWalkAfterJump.Invoke();
                        break;
                }
                break;
        }
        if(state == MovingState.GrabLedge) stateEvent.OnGrabLedgeAfterJump.Invoke();

        oldState = currentState;
        currentState = state;
    }

    protected void LeftRightMove()
    {
        if (KeyState(KeyInput.GoRight) == KeyState(KeyInput.GoLeft))
            speedForse.x = 0.0f;
        else if (KeyState(KeyInput.GoRight))
        {
            if (posState.pushesRight) speedForse.x = 0.0f;
            else speedForse.x = walkSpeed + runBoost - SpeedDebuff;
            ScaleX = -Mathf.Abs(ScaleX);
        }
        else if (KeyState(KeyInput.GoLeft))
        {
            if (posState.pushesLeft) speedForse.x = 0.0f;
            else speedForse.x = -walkSpeed - runBoost + SpeedDebuff;
            ScaleX = Mathf.Abs(ScaleX);
        }
    }

    private void Sit(MovingState currentState)
    {
        if (KeyState(KeyInput.GoDown) && !sit)
        {
            stateEvent.OnSitDownState.Invoke();
            posState.tmpIgnoresOneWay = true;
            if (!posState.pushesTopObject) ScaleY = 0.5f;
            sit = true;
        }
        else
        {
            ChangeCurrentState(currentState);
            if (!KeyState(KeyInput.GoDown)) ScaleReturner();
            sit = false;
        }
    }
    private void ScaleReturner()
    {
        if (ScaleY != 1) position += new Vector2(0, Mathf.Abs(position.y - (position.y - halfSizeY * ScaleY)) + (posState.pushesTopObject ? halfSizeY : 0));
        ScaleY = 1f;
    }

    protected bool KeyState(KeyInput key)
    {
        return (inputs[(int)key]);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawPhysicsBodyGizmos();

        Vector3 bBoxCentr = transform.position + new Vector3(BBoxOffsetX, BBoxOffsetY, 0.0f);

        // draw grab line
        float dir = ScaleX == 0.0f ? 1.0f : Mathf.Sign(ScaleX);

        Gizmos.color = Color.blue;
        Vector2 halfSize = bBox.HalfSize;
        var grabVectorTopLeft = new Vector2(bBoxCentr.x, bBoxCentr.y)
            + new Vector2(-(halfSize.x + 1.0f) * dir, halfSize.y);
        grabVectorTopLeft.y -= grabLedgeStartY;

        var grabVectorBottomLeft = new Vector2(bBoxCentr.x, bBoxCentr.y)
            + new Vector2(-(halfSize.x + 1.0f) * dir, halfSize.y);
        grabVectorBottomLeft.y -= grabLedgeEndY;

        var grabVectorTopRight = grabVectorTopLeft + Vector2.right * (halfSize.x + 1.0f) * 2.0f * dir;
        var grabVectorBottomRight = grabVectorBottomLeft + Vector2.right * (halfSize.x + 1.0f) * 2.0f * dir;

        Gizmos.DrawLine(grabVectorTopLeft, grabVectorBottomLeft);
        Gizmos.DrawLine(grabVectorTopRight, grabVectorBottomRight);
    }
#endif

    [System.Serializable]
    public enum MovingState
    {
        Stand,
        Walk,
        Jump,
        GrabLedge
    };
}

[System.Serializable]
public class CharacterEvent
{
    public UnityEvent OnHitWall;
    [Space] public float delayLoopTimer = 0.25f;
    public UnityEvent OnWalkLoop; 
}
[System.Serializable]
public class CharacterStateEvent
{
    public UnityEvent OnWalkAfterStand;
    public UnityEvent OnJumpAfterStand;
    public UnityEvent OnStandAfterWalk;
    public UnityEvent OnJumpAfterWalk;
    public UnityEvent OnStandAfterJump;
    public UnityEvent OnWalkAfterJump;
    public UnityEvent OnGrabLedgeAfterJump;
    public UnityEvent OnJumpAfterGrabLedge;
    public UnityEvent OnSitDownState;
    public UnityEvent OnSitDownRunState;

    public UnityEvent OnSwimAfterStand;
    public UnityEvent OnSwimAfterWalk;
}

// Key input enumeration for input sending.
public enum KeyInput
{
    GoLeft = 0,
    GoRight,
    GoDown,
    Jump,
    Run,
    Count
}