using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsEnemyController : MonoBehaviour
{
    [SerializeField] private float damage;
    public float Damage { get { return damage; } private set { } }
}
