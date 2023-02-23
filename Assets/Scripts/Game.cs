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
        board.Initialize(boardSize);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
        
    }

    private void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if(tile != null)
        {
            tile.Content = tileContentFactory.Get(GameTileContentType.Destination);
        }
    }
}
