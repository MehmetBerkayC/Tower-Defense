using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField]
    Transform ground = default;

    [SerializeField] 
    GameTile tilePrefab = default;

    Vector2Int size;

    GameTile[] tiles;

    Queue<GameTile> searchFrontier = new Queue<GameTile>();

    GameTileContentFactory contentFactory;

    bool showPaths;

    public bool ShowPaths
    {
        get => showPaths;
        set
        {
            showPaths = value;
            if (showPaths)
            {
                foreach (GameTile tile in tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                foreach (GameTile tile in tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }


    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
    {
        this.size = size;
        this.contentFactory = contentFactory;
        ground.localScale = new Vector3(size.x, size.y, 1f);

        Vector2 offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);

        // Board Size
        tiles = new GameTile[size.x * size.y];

        // Filling the board
        for(int i = 0, y = 0; y < size.y; y++) // Columns
        {
            for(int x = 0; x < size.x; x++ , i++) // Rows
            {
                GameTile tile = tiles[i] = Instantiate(tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);

                if (x > 0)
                {
                    // need the tile made before this tile
                    GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
                }
                if (y > 0)
                {
                    // need to subtract a whole row to get to the same position
                    GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
                }

                // if x is even make tile alternative, check link for AND explanation
                // https://catlikecoding.com/unity/tutorials/tower-defense/the-board/#:~:text=What%20does%20(x%20%26%201)%20%3D%3D%200%20do%3F
                tile.IsAlternative = (x & 1) == 0; // Could've used %(mod)
                
                if((y & 1) == 0) // if y is even
                {
                    // revert changes
                    tile.IsAlternative = !tile.IsAlternative;
                } // This will make a checkerboard design

                // Assign every tile as empty
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }

        // Make an exit and find a path
        ToggleDestination(tiles[tiles.Length / 2]);
    }

    public void ToggleDestination(GameTile tile)
    {
        // Adding a destination can never result in a valid board state, but removing a destination can.
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            if (!FindPaths()) // When findpaths returns false - invalid board - no destination
            {
                tile.Content = contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if(tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }

    }

    public void ToggleWall(GameTile tile)
    {
        // Walls will only get switched with empty tiles 
        if(tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if(tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Wall);
            if (!FindPaths()) // Don't allow walls on destination tiles
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
    }

    private bool FindPaths()
    {
        // Each tile's pathing
        foreach (GameTile tile in tiles)
        {
            // Make it an Exit tile 
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                // Add to Search Frontiers 
                searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }
        
        // If there isn't any destination tiles
        if(searchFrontier.Count == 0)
        {
            return false;
        }

        // Breadth-First Search
        while(searchFrontier.Count > 0)
        {
            GameTile tile = searchFrontier.Dequeue();
            if(tile != null)
            {
                if (tile.IsAlternative)
                {
                    searchFrontier.Enqueue(tile.GrowPathToNorth());
                    searchFrontier.Enqueue(tile.GrowPathToSouth());
                    searchFrontier.Enqueue(tile.GrowPathToEast());
                    searchFrontier.Enqueue(tile.GrowPathToWest());
                }
                else
                {
                    searchFrontier.Enqueue(tile.GrowPathToEast());
                    searchFrontier.Enqueue(tile.GrowPathToWest());
                    searchFrontier.Enqueue(tile.GrowPathToNorth());
                    searchFrontier.Enqueue(tile.GrowPathToSouth());
                }
            }
        }

        // Check for any invalid path
        foreach (GameTile tile in tiles)
        {
            if (!tile.HasPath)
            {
                return false;
            }
        }

        if (showPaths)
        {
            // Display pathing
            foreach (GameTile tile in tiles)
            {
                tile.ShowPath();
            }
        }
      

        return true;
    }

    // Getting the tile player clicked
    public GameTile GetTile(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            int x = (int)(hit.point.x + size.x * 0.5f);
            int y = (int)(hit.point.z + size.y * 0.5f);
            if (x >= 0 && x < size.x && y >= 0 && y < size.y)
            {
                return tiles[x + y * size.x];
            }
        }

        return null;
    }

}
