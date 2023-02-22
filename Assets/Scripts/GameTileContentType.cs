using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTileContentType : MonoBehaviour
{
    [SerializeField] GameTileContentType type = default;

    public GameTileContentType Type => type;
}
