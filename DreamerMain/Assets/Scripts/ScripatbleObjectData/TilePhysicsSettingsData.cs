using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TilePhysicsSettings", menuName = "ScriptableObjects/TilePhysicsSettings", order = 3000)]
public class TilePhysicsSettingsData : ScriptableObject
{
    public float GravityForce = -1030.0f;
    public float MaxFallingSpeed = -900.0f;
}
