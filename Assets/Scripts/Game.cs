using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField, Range(0,100)]
    int startingPlayerHealth = 10;

    int playerHealth;

    [SerializeField]
    Vector2Int boardSize = new Vector2Int(11, 11);

    [SerializeField]
    GameBoard board = default;

    [SerializeField] GameTileContentFactory tileContentFactory = default;
    [SerializeField] WarFactory warFactory = default;

    GameBehaviorCollection enemies = new GameBehaviorCollection();
    GameBehaviorCollection nonEnemies = new GameBehaviorCollection();

    private TowerType selectedTowerType;

    [SerializeField] GameScenario scenario = default;
    GameScenario.State activeScenario;

    [SerializeField, Range(1f, 10f)]
    float playSpeed = 1f;

    const float pausedTimeScale = 0f;

    static Game instance;
    
    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    private void OnEnable()
    {
        instance = this;    
    }

    private void Awake()
    {
        playerHealth = startingPlayerHealth;
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
        activeScenario = scenario.Begin();
    }

    private void OnValidate()
    {
        // At the least 2x2 board is available
        if (boardSize.x < 2)
        {
            boardSize.x = 2;
        }
        if (boardSize.y < 2)
        {
            boardSize.y = 2;
        }
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
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            selectedTowerType = TowerType.Mortar;
        }

        // Pause Game
        if (Input.GetKeyDown(KeyCode.Space))
        { 
            // if already paused continue, else pause
            Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
        }else if(Time.timeScale > pausedTimeScale)
        {
            Time.timeScale = playSpeed;
        }

        // New Game
        if (Input.GetKeyDown(KeyCode.B))
        {
            BeginNewGame();
        }

        if(playerHealth <= 0 && startingPlayerHealth > 0)
        {
            Debug.Log("Defeat!");
            BeginNewGame();
        }

        if (!activeScenario.Progress() && enemies.IsEmpty) // returns false if scenarios complete
        {
            Debug.Log("Victory!");
            BeginNewGame();
            activeScenario.Progress();
        }

        enemies.GameUpdate();
        Physics.SyncTransforms();
        board.GameUpdate();
        nonEnemies.GameUpdate();
    }

    private void BeginNewGame()
    {
        playerHealth = startingPlayerHealth;
        enemies.Clear();
        nonEnemies.Clear();
        board.Clear();
        activeScenario = scenario.Begin();
    }

    public static void EnemyReachedDestination()
    {
        instance.playerHealth -= 1;
    }

    public static Explosion SpawnExplosion()
    {
        Explosion explosion = instance.warFactory.Explosion;
        instance.nonEnemies.Add(explosion);
        return explosion;
    }

    public static Shell SpawnShell()
    {
        Shell shell = instance.warFactory.Shell;
        instance.nonEnemies.Add(shell);
        return shell;
    }

    public static void SpawnEnemy(EnemyFactory[] factories, EnemyType type)
    {
        GameTile spawnPoint = instance.board.GetSpawnPoint(UnityEngine.Random.Range(0, instance.board.SpawnPointCount));
        Enemy enemy = factories[UnityEngine.Random.Range(0, factories.Length)].Get(type);
            
        enemy.SpawnOn(spawnPoint);
        instance.enemies.Add(enemy);
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
