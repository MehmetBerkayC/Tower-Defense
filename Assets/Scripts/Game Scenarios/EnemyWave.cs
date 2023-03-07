using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyWave : ScriptableObject
{
    [SerializeField]
    EnemySpawnSequence[] spawnSequences = {
        new EnemySpawnSequence()
    };
}
