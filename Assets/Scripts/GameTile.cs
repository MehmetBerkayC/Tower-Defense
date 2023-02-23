using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    [SerializeField]
    Transform arrow = default;

    GameTile north, east, south, west, nextOnPath;
    int distance;

    public bool HasPath => distance != int.MaxValue;

    public bool IsAlternative { get; set; }

    // Content of the tile
    GameTileContent content;
  
    public GameTileContent Content
    {
        get => content;
        set
        {
            Debug.Assert(value != null, "Null assigned to content!"); // Debug.Assert Displays Msg if condition false
            if(content != null)
            {
                content.Recycle();
            }
            content = value;
            content.transform.localPosition = transform.localPosition;
        }
    }

    // For Arrow Rotations
    static Quaternion
        northRotation = Quaternion.Euler(90f, 0f, 0f),
        eastRotation = Quaternion.Euler(90f, 90f, 0f),
        soutnRotation = Quaternion.Euler(90f, 180f, 0f),
        westRotation = Quaternion.Euler(90f, 270f, 0f);

    public void ShowPath() // Display Pathing by rotating arrows
    {
        if(distance == 0)
        {
            arrow.gameObject.SetActive(false);
            return;
        }

        arrow.gameObject.SetActive(true);
        arrow.localRotation =
            nextOnPath == north ? northRotation :
            nextOnPath == east ? eastRotation :
            nextOnPath == south ? soutnRotation :
            westRotation;
    }

    // Pathing 
    public GameTile GrowPathToNorth() => GrowPathTo(north);
    public GameTile GrowPathToEast() => GrowPathTo(east);
    public GameTile GrowPathToSouth() => GrowPathTo(south);
    public GameTile GrowPathToWest() => GrowPathTo(west);

    private GameTile GrowPathTo(GameTile neighbor)
    {
        Debug.Assert(HasPath, "No Path!");

        // if tile doesn't have a neighbor or already had pathing
        if(neighbor == null || neighbor.HasPath)
        {
            return null;
        }

        neighbor.distance = distance + 1;
        neighbor.nextOnPath = this;

        return neighbor;
    }

    // Defining neighbor tiles
    public static void MakeEastWestNeighbors (GameTile east, GameTile west)
    {
        Debug.Assert(west.east == null && east.west == null, "Redefined neighbors!");
        west.east = east;
        east.west = west;
    }

    public static void MakeNorthSouthNeighbors (GameTile north, GameTile south)
    {
        Debug.Assert(north.south == null && south.north == null, "Redefined neighbors!");
        north.south = south;
        south.north = north;
    }

    public void ClearPath()
    {
        distance = int.MaxValue;
        nextOnPath = null;
    }

    // Tile becomes starting Point for pathfinding search - An exit point
    public void BecomeDestination()
    {
        distance = 0;
        nextOnPath = null;
    }
}
