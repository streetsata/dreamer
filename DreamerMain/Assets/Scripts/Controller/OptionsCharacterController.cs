using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsCharacterController : MonoBehaviour
{
    [Header("Parameter")]
    [SerializeField] private float strenght;
    [SerializeField] private float agility;
    [SerializeField] private float intelligence;
    [SerializeField] private float armour;
    [SerializeField] private float startDamage;
    [SerializeField] private float magicDamage;
    [SerializeField] private float mainDamage;
    [SerializeField] private float healthPoint;

    public float HealthPoint { get { return healthPoint; }  set { healthPoint = value; } }

    [SerializeField] private float factorDamage;
    [SerializeField] private float effectiveHealthPoint;
    [SerializeField] private float magicResist;
    [SerializeField] private float effectiveMagicHealtPoint;
    [SerializeField] private int level;
    [SerializeField] private float growthStrenght;
    [SerializeField] private float growthAgiglity;
    [SerializeField] private float growthIntelligence;
}
