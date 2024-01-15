using UnityEngine;
using Unity.Collections;
using Photon.Pun;
using System.Collections.Generic;

public enum Side
{
    TOP,
    BOTTOM,
}

public class Board : MonoBehaviourPunCallbacks
{
    [SerializeField] PhotonView PV;
  
    [Range(8, 12)]
    [SerializeField] int boardX = 9;
    [Range(8, 12)]
    [SerializeField] int boardY = 8;

    [SerializeField] float tileGap = 0.5f;
    [SerializeField] float tileSize = 1f;

    [SerializeField] GameObject tilePrefab;
    Tile[,] tiles;
    List<Tile> tileListTop;
    List<Tile> tileListBottom;
    

    [SerializeField] Color availablePositionColor;
    [SerializeField] Color unavailablePositionColor;
    [SerializeField] Color enemyPositionColor;

    private void OnDrawGizmos()
    {
        Vector3 _tileSize = new(tileSize, 0.5f, tileSize);
        Vector3 initialPosition = transform.position;
        Gizmos.color = Color.black;

        if (!Application.isPlaying)
        {
            for (int c = 0; c < boardX; c++)
            {
                Vector3 newInitialPosition = initialPosition + ((c * tileGap) * Vector3.right);

                for (int c1 = 0; c1 < boardY; c1++)
                {
                    Vector3 newPosition = newInitialPosition + ((c1 * tileGap) * -Vector3.forward);
                    Gizmos.DrawCube(newPosition, _tileSize);
                }
            }
        }
    }

    void Start()
    {
        InitializeBoard();
    }

    void InitializeBoard()
    {
        tileListTop = new List<Tile>();
        tileListBottom = new List<Tile>();
        tiles = new Tile[boardX, boardY];

        Vector3 _tileSize = new(tileSize, 0.5f, tileSize);
        Vector3 initialPosition = transform.position;


        int maxTopY = Mathf.FloorToInt(boardY / 2) - 1;
        int minBottomY = Mathf.FloorToInt(boardY / 2) + 1;

        for (int c = 0; c < boardX; c++)
        {
            Vector3 newInitialPosition = initialPosition + ((c * tileGap) * Vector3.right);

            for (int c1 = 0; c1 < boardY; c1++)
            {
                Vector3 newPosition = newInitialPosition + ((c1 * tileGap) * -Vector3.forward);

                GameObject _tile = Instantiate(tilePrefab, transform);


                tiles[c, c1] = _tile.GetComponent<Tile>();
                tiles[c, c1].SetupTile(newPosition, _tileSize, c, c1);
                tiles[c, c1].SetColor(unavailablePositionColor);

                if (c1 <= maxTopY)
                    tileListTop.Add(tiles[c, c1]);
                else if(c1 >= minBottomY)
                    tileListBottom.Add(tiles[c, c1]);
            }
        }
    }


     public void GetAvailablePositionsFromPosition(Tile selectedTile)
    {
        ResetBoardColor();
        int x = selectedTile.X;
        int y = selectedTile.Y;

        int topY = y - 1;
        int bottomY = y + 1;
        int leftX = x - 1;
        int rightX = x + 1;

        if(topY >= 0)
        {
            Tile tile = tiles[x, topY];
            ProcessTilePosition(tile);
        }
        if(bottomY <= (boardY - 1))
        {
            Tile tile = tiles[x, bottomY];
            ProcessTilePosition(tile);
        }

        if (leftX >= 0)
        {
            Tile tile = tiles[leftX, y];
            ProcessTilePosition(tile);
        }
        if (rightX <= (boardX - 1))
        {
            Tile tile = tiles[rightX, y];
            ProcessTilePosition(tile);
        }
    }

    private void ProcessTilePosition(Tile tile)
    {
        if(tile.Piece == null)
        {
            tile.SetColor(availablePositionColor);
        }
        else if(tile.Piece != null && !tile.Piece.IsFriendly)
        {
            tile.SetColor(enemyPositionColor);
        }
    }

    public void ResetBoardColor()
    {
        for (int c = 0; c < boardX; c++)
        {
            for (int c1 = 0; c1 < boardY; c1++)
            {
                tiles[c, c1].SetColor(unavailablePositionColor);
            }
        }
    }

    public void GetAvailablePositionsFromSide(Side side)
    {
        if(side == Side.TOP)
        {
            int maxY = Mathf.FloorToInt(boardY/2) - 1;
            for (int c = 0; c < boardX; c++)
            {
                for(int c1 = 0; c1 < boardY; c1++)
                {
                    if(c1 <= maxY)
                    {
                        tiles[c, c1].SetColor(availablePositionColor);
                    }
                    else
                    {
                        tiles[c, c1].SetColor(unavailablePositionColor);
                    }
                }
            }
        }
        if(side == Side.BOTTOM)
        {
            int minT = Mathf.FloorToInt(boardY / 2) + 1;
            for (int c = 0; c < boardX; c++)
            {
                for (int c1 = 0; c1 < boardY; c1++)
                {
                    if (c1 >= minT)
                    {
                        tiles[c, c1].SetColor(availablePositionColor);
                    }
                    else
                    {
                        tiles[c, c1].SetColor(unavailablePositionColor);
                    }
                }
            }
        }
    }

    public List<Tile> GetRandomTiles(int count, Side side)
    {

        List<Tile> newTileList = new();
        List<Tile> defaultTileList = new();

        if (side == Side.TOP)
            defaultTileList = tileListTop;
        if (side == Side.BOTTOM)
            defaultTileList = tileListBottom;

        if (defaultTileList.Count < count)
            return null;

        int requiredTilesCount = count;
        int skipsAvailable = defaultTileList.Count - requiredTilesCount;

        for (int c = 0; c < defaultTileList.Count; c++)
        {
            float chances = Random.Range(0f, 1f);
            Tile tile = defaultTileList[c];
            if (chances <= 0.3 || (skipsAvailable <= 0))
            {
                newTileList.Add(tile);
            }
            else
            {
                skipsAvailable--;
            }
        }

        return newTileList;
    }
}
