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

    public void Initialize(Vector2Int size)
    {
        this.size = size;
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
            }
        }

        // Make one tile the exit point and use pathfinding
        FindPaths();
    }

    private void FindPaths()
    {
        // Clear each tile's pathing
        foreach (GameTile tile in tiles)
        {
            tile.ClearPath();
        }

        // Make an Exit tile
        tiles[tiles.Length / 2].BecomeDestination();
        
        // Breadth-First Search
        searchFrontier.Enqueue(tiles[tiles.Length / 2]);

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

        // Display pathing
        foreach (GameTile tile in tiles)
        {
            tile.ShowPath();
        }
    }

    // Getting the tile player clicked
    public GameTile GetTile(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            int x = (int)(hit.point.x + size.x * 0.5f);
            int y = (int)(hit.point.z + size.y * 0.5f);
            if(x >= 0 && x < size.x && y >= 0 && y < size.y)
            {
                return tiles[x + y * size.x];
            }
        }

        return null;
    }

}
