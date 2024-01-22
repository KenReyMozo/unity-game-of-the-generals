using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField] TextMeshProUGUI winnerPlayerName;
    bool isPlayer1Ready, isPlayer2Ready;

    [SerializeField] Color availablePositionColor;
    [SerializeField] Color unavailablePositionColor;
    [SerializeField] Color friendlyPositionColor;
    [SerializeField] Color enemyPositionColor;

    [SerializeField] Transform cameraHolder;

    [SerializeField] GameObject[] objectsToEnableOnGameEnd;

    public int GetBottomSideWinIndex() => 0;
    public int GetTopSideWinIndex() => boardY-1;

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
        {
            MakePlayerEnableReady();
            return Side.ANY;
        }
        else if(playerCount == 2 && (player1 != null && player2 != null))
        {
            isPlayer1Ready = false;
            isPlayer2Ready = false;
            MakePlayerEnableReady();
            if (PhotonNetwork.LocalPlayer == player1)
                return Side.BOTTOM;
            if (PhotonNetwork.LocalPlayer == player2)
                return Side.TOP;
        }


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
        {
            MakePlayerEnableReady();
            return Side.TOP;
        }
        return Side.ANY;
    }

    void MakePlayerEnableReady()
    {
        PieceManager[] pieceManagers = FindObjectsOfType<PieceManager>();
        foreach (PieceManager pieceManager in pieceManagers)
        {
            pieceManager.EnablePlayerToReady();
        }
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
        tile.SetAsAvailable();
        if (tile.Piece == null)
        {
            tile.SetColor(availablePositionColor);
        }
        else if (tile.Piece != null && tile.Piece.IsFriendly)
        {
            tile.SetColor(friendlyPositionColor);
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
                Tile tile = tiles[c, c1];
                if (tile.Piece != null && tile.Piece.IsFriendly)
                {
                    tile.SetColor(friendlyPositionColor);
                    tile.SetAsAvailable();
                }
                else
                {
                    tile.SetColor(unavailablePositionColor);
                    tile.SetAsUnavailable();
                }
            }
        }
    }

    public void GetAvailablePositionsFromSide(Side side)
    {
        if(side == Side.TOP)
        {
            int maxY = Mathf.FloorToInt(boardY/2) - 2;
            for (int c = 0; c < boardX; c++)
            {
                for(int c1 = 0; c1 < boardY; c1++)
                {
                    if(c1 <= maxY)
                    {
                        tiles[c, c1].SetColor(availablePositionColor);
                        tiles[c, c1].SetAsAvailable();
                    }
                    else
                    {
                        tiles[c, c1].SetColor(unavailablePositionColor);
                        tiles[c, c1].SetAsUnavailable();
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
                        tiles[c, c1].SetAsAvailable();
                    }
                    else
                    {
                        tiles[c, c1].SetColor(unavailablePositionColor);
                        tiles[c, c1].SetAsUnavailable();
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

    public void ResetTilesPieceReference()
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

    public void RotatePlayerCamera()
    {
        RotateObjectOnYAxis(cameraHolder, 180f);
    }

    public void RotatePlayerCamera(float degrees)
    {
        RotateObjectOnYAxis(cameraHolder, degrees);
    }

    void RotateObjectOnYAxis(Transform transformToRotate, float angle)
    {
        Vector3 currentRotation = transformToRotate.rotation.eulerAngles;

        currentRotation.y = angle;

        transformToRotate.rotation = Quaternion.Euler(currentRotation);
    }

    public void EndGame()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(SceneConstants.LOBBY_SCENE);
    }

    public void EndGameWithTopSideVictory()
    {
        OnEndGame();
        winnerPlayerName.text = "Winner: " + player2.NickName + " !!!";
        PV.RPC(nameof(RPC_OnGameEndWithTopSideVictory), RpcTarget.All);
    }
    public void EndGameWithBottomSideVictory()
    {
        OnEndGame();
        winnerPlayerName.text = "Winner: " + player1.NickName + " !!!";
        PV.RPC(nameof(RPC_OnGameEndWithBottomSideVictory), RpcTarget.All);
    }

    public void EndGameWithDraw()
    {
        OnEndGame();
        PV.RPC(nameof(RPC_OnGameEndWithBottomSideVictory), RpcTarget.All);
    }

    [PunRPC]
    public void RPC_OnGameEndWithTopSideVictory()
    {
        if (!PV.IsMine) return;
        OnEndGame();
        winnerPlayerName.text = "Winner: "+player2.NickName + " !!!";
    }   
    
    [PunRPC]
    public void RPC_OnGameEndWithBottomSideVictory()
    {
        if (!PV.IsMine) return;
        OnEndGame();
        winnerPlayerName.text = "Winner: "+player1.NickName+" !!!";

    }    
    [PunRPC]

    public void RPC_OnGameEndWithDraw()
    {
        if (!PV.IsMine) return;
        OnEndGame();
        winnerPlayerName.text = "DRAW !!!";
    }


    void OnEndGame()
    {
        foreach (GameObject obj in objectsToEnableOnGameEnd)
        {
            obj.SetActive(true);
        }
        PieceManager[] pieceManagers = FindObjectsOfType<PieceManager>();
        foreach(PieceManager pieceManager in pieceManagers)
        {
            pieceManager.SetGameHasEnded();
        }
    }
}
