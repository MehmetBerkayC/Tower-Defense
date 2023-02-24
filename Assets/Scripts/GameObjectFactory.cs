using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameObjectFactory : ScriptableObject
{
    Scene scene;

    protected T CreateGameObjectInstance<T> (T prefab) where T : MonoBehaviour
    {
        // While scene isn't loaded
        if (!scene.isLoaded)
        {
            // Check if on editor playmode
            if (Application.isEditor)
            {
                // Check if scene is already loaded/open
                scene = SceneManager.GetSceneByName(name);
                if (!scene.isLoaded) // if not create it
                {
                    scene = SceneManager.CreateScene(name);
                }
            }
            else // in build application, so create the scene
            {
                scene = SceneManager.CreateScene(name);
            }
        }

        T instance = Instantiate(prefab);
        SceneManager.MoveGameObjectToScene(instance.gameObject, scene);
        return instance;
    }
}

