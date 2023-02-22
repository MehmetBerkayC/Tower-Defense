using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu]
public class GameTileContentFactory : ScriptableObject
{
    Scene contentScene;

    [SerializeField] GameTileContent destinationPrefab = default;
    [SerializeField] GameTileContent emptyPrefab = default;

    public GameTileContent Get(GameTileContentType type)
    {
        switch (type)
        {
            case GameTileContentType.Destination: return Get(destinationPrefab);
            case GameTileContentType.Empty: return Get(emptyPrefab);
        }
        Debug.Assert(false, "Unsupported type: " + type);
        return null;
    }

    GameTileContent Get(GameTileContent prefab)
    {
        GameTileContent instance = Instantiate(prefab);
        instance.OriginFactory = this;
        MoveFactoryScene(instance.gameObject);
        return instance;
    }

    private void MoveFactoryScene(GameObject o)
    {
        if (!contentScene.isLoaded)
        {
            // While in editor-playmode, check if factory scene is already open
            if (Application.isEditor)
            {
                contentScene = SceneManager.GetSceneByName(name);
                if (!contentScene.isLoaded) // if not open, load it
                {
                    contentScene = SceneManager.CreateScene(name);
                }
            }
            else // in builds load the factory scene(won't be open already)
            {
                contentScene = SceneManager.CreateScene(name);
            }
        }
        // Move gameObject to factory scenes
        SceneManager.MoveGameObjectToScene(o, contentScene);
    }

    public void Reclaim(GameTileContent content)
    {
        Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(content.gameObject);
    }
}
