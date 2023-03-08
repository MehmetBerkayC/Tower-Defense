using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnSequence 
{
    [SerializeField]
    EnemyFactory[] factories = default;

    [SerializeField]
    EnemyType type = EnemyType.Medium;

    [SerializeField, Range(1, 100)]
    int amount = 1;

    [SerializeField, Range(0.1f, 10f)]
    float cooldown = 1f;

    public State Begin() => new State(this);

    //To make the state survive hot reloads in the editor it needs to be serializable.
    // Downside of this approach is that we need to create a new state object each time a sequence is started
    // avoid memory allocations by making it as a struct instead of a class
    [System.Serializable] 
    public struct State // Nested
    {
        EnemySpawnSequence sequence;

        int count;
        float cooldown;

        // Constructor
        public State(EnemySpawnSequence sequence)
        {
            this.sequence = sequence;
            count = 0;
            cooldown = sequence.cooldown;
        }

        public float Progress(float deltaTime)
        {
            cooldown += deltaTime;
            while(cooldown >= sequence.cooldown)
            {
                cooldown -= sequence.cooldown;
                if(count >= sequence.amount)
                {
                    return cooldown;
                }
                count += 1;
                Game.SpawnEnemy(sequence.factories, sequence.type);
            }
            return -1f;
        }

    }
}
