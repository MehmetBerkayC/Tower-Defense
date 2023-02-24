using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyCollection
{
    List<Enemy> enemies = new List<Enemy>();

    public void Add(Enemy enemy)
    {
        enemies.Add(enemy);
    }

    public void GameUpdate()
    {
        for(int i = 0; i < enemies.Count; i++)
        {
            // if this enemy fails to update -destroyed/lost/inactive-
            if (!enemies[i].GameUpdate())
            {
                // move it to the end of the list and remove,
                // go 1 step back because the enemies[i] has a new entry
                // and needs to get checked again
                int lastIndex = enemies.Count - 1;
                enemies[i] = enemies[lastIndex];
                enemies.RemoveAt(lastIndex);
                i -= 1;
            }
        }
    }
}
