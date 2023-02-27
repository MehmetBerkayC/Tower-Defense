using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    [SerializeField]
    Transform arrow = default;

    GameTile north, east, south, west, nextOnPath;
    int distance;

    public Direction PathDirection { get; private set; }

    public bool HasPath => distance != int.MaxValue;

    public bool IsAlternative { get; set; }

    // Edge between 2 tiles
    public Vector3 ExitPoint { get; private set; }

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


    // Pathing 
    public GameTile NextTileOnPath => nextOnPath;
    public GameTile GrowPathToNorth() => GrowPathTo(north, Direction.South);
    public GameTile GrowPathToEast() => GrowPathTo(east, Direction.West);
    public GameTile GrowPathToSouth() => GrowPathTo(south, Direction.North);
    public GameTile GrowPathToWest() => GrowPathTo(west, Direction.East);

    private GameTile GrowPathTo(GameTile neighbor, Direction direction)
    {
        Debug.Assert(HasPath, "No Path!");

        // if tile has pathing, doesn't have a neighbor or neighbor already had pathing
        if(!HasPath || neighbor == null || neighbor.HasPath)
        {
            return null; // do nothing
        }

        neighbor.distance = distance + 1;
        neighbor.nextOnPath = this;

        neighbor.ExitPoint = neighbor.transform.localPosition + direction.GetHalfVector();

        neighbor.PathDirection = direction; // Direction to the next tile

        return neighbor.content.Type != GameTileContentType.Wall ? neighbor : null;
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
        ExitPoint = transform.localPosition;
    }

    public void ShowPath() // Display Pathing by rotating arrows
    {
        if (distance == 0)
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

    public void HidePath()
    {
        arrow.gameObject.SetActive(false);
    }
}
