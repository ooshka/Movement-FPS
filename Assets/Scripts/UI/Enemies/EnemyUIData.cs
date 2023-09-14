using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyUIData")]
public class EnemyUIData : ScriptableObject
{
    [Header("Meta Data")]
    public new string name;
    public float maxHealth;
}
