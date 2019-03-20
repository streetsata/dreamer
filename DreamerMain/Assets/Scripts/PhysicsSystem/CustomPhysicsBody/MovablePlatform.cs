using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovablePlatform : PhysicsBody
{
    public float Speed = 100f;

	void Start ()
    {
        slopeWallHeight = 0;
        isKinematic = true;
        speedForse.x = Speed;
        isMount = true;
    }

    public override void CommonUpdate()
    {
        CollisionLogic();
        UpdatePhysicsFinaly();

        if (posState.pushesRightTile && !posState.pushesBottomTile)
            speedForse.y = -Speed;
        else if (posState.pushesBottomTile && !posState.pushesLeftTile)
            speedForse.x = -Speed;
        else if (posState.pushesLeftTile && !posState.pushesTopTile)
            speedForse.y = Speed;
        else if (posState.pushesTopTile && !posState.pushesRightTile)
            speedForse.x = Speed;

        UpdatePhysics();

        map.UpdateAreas(this);
        allCollidingObjects.Clear();
    }
}
