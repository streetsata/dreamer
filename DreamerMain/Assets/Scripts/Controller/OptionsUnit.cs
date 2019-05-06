using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsUnit : MonoBehaviour
{
    [Header("Parameter")]
    [SerializeField] protected float strenght;
    [SerializeField] protected float agility;
    [SerializeField] protected float inteligence;
    [SerializeField] protected float armour;
    [SerializeField] protected float startDamage;
    [SerializeField] protected float magicDamage;
    [SerializeField] protected float mainDamage;
    [SerializeField] protected float healthPoint;
    [SerializeField] protected float factorDamage;
    [SerializeField] protected float effectiveHealthPoint;
    [SerializeField] protected float magicResist;
    [SerializeField] protected float effectiveMagicHealthPoint;
    [SerializeField] protected int level;
    [SerializeField] protected float growthStrenght;
    [SerializeField] protected float growthAgility;
    [SerializeField] protected float growthIntelligence;

    public virtual void TakeDamage()
    {

    }


    public virtual void OnCollisionEnter()
    {
    }
}
