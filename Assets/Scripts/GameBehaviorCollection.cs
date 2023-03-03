using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameBehaviorCollection
{
    List<GameBehavior> behaviors = new List<GameBehavior>();

    public void Add(GameBehavior behavior)
    {
        behaviors.Add(behavior);
    }

    public void GameUpdate()
    {
        for(int i = 0; i < behaviors.Count; i++)
        {
            // if this enemy fails to update -destroyed/lost/inactive-
            if (!behaviors[i].GameUpdate())
            {
                // move it to the end of the list and remove,
                // go 1 step back because the enemies[i] has a new entry
                // and needs to get checked again
                int lastIndex = behaviors.Count - 1;
                behaviors[i] = behaviors[lastIndex];
                behaviors.RemoveAt(lastIndex);
                i -= 1;
            }
        }
    }
}
