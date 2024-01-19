using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;

public enum Side
{
    TOP,
    BOTTOM,
    ANY,
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

    Player player1, player2;
    PieceManager playerManager;
    [SerializeField] TextMeshProUGUI player1NameText;
    [SerializeField] TextMeshProUGUI player2NameText;
    [SerializeField] TextMeshProUGUI currentPlayerTurnName;
    bool isPlayer1Ready, isPlayer2Ready;

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

    public void SetBoardPlayerManager(PieceManager manager)
    {
        playerManager = manager;
    }

    public Side OnPlayerJoins()
    {
        int playerCount = PhotonNetwork.PlayerList.Length;
        if(playerCount > 2)
            return Side.ANY;

        int index = 0;

        foreach(Player player in PhotonNetwork.PlayerList)
        {
         
            if (index == 0)
            {
                player1 = player;
                player1NameText.text = player1.NickName;
            }
            if(index == 1)
            {
                player2 = player;
                player2NameText.text = player2.NickName;
            }
            index++;

        }
        if (playerCount == 1)
            return Side.BOTTOM;
        if (playerCount == 2)
            return Side.TOP;
        return Side.ANY;
    }

    public void StartGame()
    {
        PV.RPC(nameof(RPC_OnPlayerReady), RpcTarget.All);
    }

    [PunRPC]
    public void RPC_OnPlayerReady(PhotonMessageInfo info)
    {
        if(info.Sender == player1)
        {
            isPlayer1Ready = true;
        }
        if(info.Sender == player2)
        {
            isPlayer2Ready = true;
        }

        if (isPlayer1Ready && isPlayer2Ready)
        {
            currentPlayerTurnName.text = player1.NickName;
            playerManager.StartTurn(player1.UserId);
        }

    }

    public void EndTurn(string userID)
    {
        if(userID == player1.UserId)
        {
            PV.RPC(nameof(RPC_OnPlayerEndTurn), RpcTarget.All);
        }
        if (userID == player2.UserId)
        {
            PV.RPC(nameof(RPC_OnPlayerEndTurn), RpcTarget.All);
        }
    }

    [PunRPC]
    public void RPC_OnPlayerEndTurn(PhotonMessageInfo info)
    {
        if (info.Sender == player1)
        {
            currentPlayerTurnName.text = player2.NickName;
            playerManager.StartTurn(player2.UserId);
        }
        if (info.Sender == player2)
        {
            currentPlayerTurnName.text = player1.NickName;
            playerManager.StartTurn(player1.UserId);
        }
    }

    public void MovePiece()
    {
        PV.RPC(nameof(RPC_MovePiece), RpcTarget.All);
    }

    [PunRPC]
    public void RPC_MovePiece()
    {

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

                if (c1 < maxTopY)
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

    public void ResetTilesPieveReference()
    {
        foreach (Tile tile in tiles)
        {
            tile.Piece = null;
        }
    }

    public void SetTilePieceReference(Vector2Int coordinate, Piece piece)
    {
        tiles[coordinate.x, coordinate.y].Piece = piece;
    }

    public Tile GetTileFromCoordinate(Vector2Int coordinate)
    {
        return tiles[coordinate.x, coordinate.y];
    }
    public Tile GetTileFromCoordinate(Vector2 coordinate)
    {
        int X = Mathf.FloorToInt(coordinate.x);
        int Y = Mathf.FloorToInt(coordinate.y);
        return tiles[X, Y];
    }
}
