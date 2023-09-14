using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    [Tooltip("The distance at which an enemy can detect the player")]
    public float detectionSphereRadius;
    [Tooltip("The distance at which an enemy can attack the player (Should be less than detection range)")]
    public float attackSphereRadius;
    public bool isStationary;
}
