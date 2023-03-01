using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField]
    GameBoard board = default;

    [SerializeField] GameTileContentFactory tileContentFactory = default;
    [SerializeField] EnemyFactory enemyFactory = default;

    [SerializeField, Range(0.1f, 10f)] float spawnSpeed = 1f;
    float spawnProgress;

    EnemyCollection enemies = new EnemyCollection();

    private TowerType selectedTowerType;

    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    private void OnValidate()
    {
        // At the least 2x2 board is available
        if(boardSize.x < 2)
        {
            boardSize.x = 2;
        }    
        if(boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

    private void Awake()
    {
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Select Tile Action
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }else if (Input.GetMouseButtonDown(1))
        {
            HandleAlternativeTouch();
        }

        // Display Pathing
        if (Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }

        // Display Grid
        if (Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }

        // Tower Selection
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedTowerType = TowerType.Laser;
        }else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedTowerType = TowerType.Mortar;
        }

        spawnProgress += Time.deltaTime * spawnSpeed;
        while (spawnProgress >= 1f)
        {
            spawnProgress -= 1;
            SpawnEnemy();
        }

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
    }

    private void SpawnEnemy()
    {
        GameTile spawnPoint = board.GetSpawnPoint(UnityEngine.Random.Range(0, board.SpawnPointCount));
        Enemy enemy = enemyFactory.Get();
        enemy.SpawnOn(spawnPoint);
        enemies.Add(enemy);
    }

    private void HandleAlternativeTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if(tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleDestination(tile);
            }
            else 
            { 
                board.ToggleSpawnPoint(tile);
            }
        }
    }

    private void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if(tile != null)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleTower(tile, selectedTowerType);
            }
            else
            {
                board.ToggleWall(tile);
            }
        }
    }
}
