using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

public enum MoveStatus
{
    UP,
    MOVE,
    DOWN,
    END,
}
public class PieceManager : PlayerView
{
    Board board;
    public Board Board { get => board; set => board = value; }

    public Side Side;

    bool isNextTurnWin = false;

    public void SetWinOnNextTurnIfAlive() => isNextTurnWin = true;

    const string PIECE_TAG = "Piece";
    const string TILE_TAG = "Tile";

    const string READY_TEXT = "Ready!";
    const string SET_READY_TEXT = "Set Ready";

    [SerializeField] Button readyButton;
    [SerializeField] Button restartButton;
    PlayerControl playerControl;
    Piece selectedPiece;

    [SerializeField] Piece yourFlag;
    [SerializeField] Piece[] myPieces;

    Vector3? currentTargetPosition;
    float moveSpeed = 7f;
    float moveElevation = 1f;

    MoveStatus? moveStatus;
    public Player You;
    bool isReady = false;
    bool hasGameStarted = false;
    bool hasGameEnded = false;

    public void SetGameHasEnded() => hasGameEnded = true;

    [SerializeField] GameObject[] objectsToDisableOnReady;

    bool isYourTurn = false;
    public bool IsYourTurn { get => isYourTurn; set => isYourTurn = value; }

    public void StartGame()
    {
        readyButton.gameObject.SetActive(false);
        board.ResetBoardColor();
        foreach (Piece piece in myPieces)
        {
            if (piece.TargetTile == null)
            {
                piece.IsDead = true;
                piece.gameObject.SetActive(false);
            }
        }
        hasGameStarted = true;
    }

    private void Awake()
    {
        playerControl = new PlayerControl();
    }

    public override void OnEnable()
    {
        playerControl.Enable();
    }

    public override void OnDisable()
    {
        playerControl.Disable();
    }

    public void EnablePlayerToReady()
    {
        PV.RPC(nameof(RPC_EnablePlayerToReady), RpcTarget.All);
    }

    [PunRPC]
    public void RPC_EnablePlayerToReady()
    {
        Debug.Log("Ready Available!");
        if (!PV.IsMine) return;
        foreach (GameObject obj in toEnableIfReady)
        {
            obj.SetActive(true);
        }
    }


    void Start()
    {
        InitializePlayer();
    }

    public void InitializePlayer()
    {
        board = FindObjectOfType<Board>();
        if (board == null)
        {
            enabled = false;
            return;
        }

        You = PhotonNetwork.LocalPlayer;
        Side = board.OnPlayerJoins();


        if (!PV.IsMine)
        {
            foreach (GameObject obj in toDisableIfNotMine)
            {
                obj.SetActive(false);
            }
            foreach (Piece piece in myPieces)
            {
                piece.SetPiece(moveSpeed, moveElevation);
                piece.SetIsNotMine();
            }

        }
        else
        {
            if (Side == Side.TOP)
                board.RotatePlayerCamera(180f);

            board.SetBoardPlayerManager(this);
            if (Side != Side.ANY)
            {
                board.GetAvailablePositionsFromSide(Side);
            }
        }

        playerControl.BoardControl.Select.performed += _ => OnSelectSomething();
        playerControl.BoardControl.Escape.performed += _ => RoomManager.Instance.OnToggleMenu();

        int index = 0;

        foreach (Piece piece in myPieces)
        {
            piece.SetPiece(moveSpeed, moveElevation);
            piece.ID = index;
            index++;
            piece.PV = PV;
            if (!PV.IsMine) return;
            piece.IsFriendly = true;
            piece.PM = this;
        }
    }

    void OnSelectSomething()
    {
        if (!PV.IsMine) return;
        if (!IsYourTurn && hasGameStarted) return;
        if (isReady && !hasGameStarted) return;
        if (selectedPiece != null && selectedPiece.IsMoving) return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();

            if (interactable == null) return;
            switch (interactable.tag)
            {
                case PIECE_TAG:
                    Piece _piece = interactable as Piece;
                    OnSelectPiece(_piece);
                    break;

                case TILE_TAG:
                    Tile _tile = interactable as Tile;
                    OnSelectTile(_tile);
                    break;

                default:
                    break;
            }
        }
    }

    void OnSelectPiece(Piece piece)
    {
        if (!PV.IsMine) return;
        if (!piece.IsFriendly)
        {

            return;
        }

        piece.OnClick(selectedPiece);

        if (piece.IsSelected && selectedPiece == null)
        {
            selectedPiece = piece;
        }
        else if (piece.IsSelected && selectedPiece != null)
        {
            selectedPiece.Unselect();
            selectedPiece = piece;
        }
        else
        {
            selectedPiece = null;
            if (hasGameStarted)
                board.ResetBoardColor();
        }

        if (hasGameStarted)
        {
            if (selectedPiece != null && selectedPiece.TargetTile != null)
            {
                board.GetAvailablePositionsFromPosition(selectedPiece.TargetTile);
            }
        }

    }

    void OnSelectTile(Tile tile)
    {
        if (!PV.IsMine) return;
        if (selectedPiece == null)
        {
            if (tile.Piece)
            {
                OnSelectPiece(tile.Piece);
            }
        }
        else if (selectedPiece != null)
        {
            if (tile.Piece == null)
            {
                if (selectedPiece.TargetTile != null)
                {
                    selectedPiece.MoveTo(tile, true);
                    if (hasGameStarted)
                        EndTurn(selectedPiece, tile);

                }
                else
                {
                    currentTargetPosition = tile.transform.position;
                    selectedPiece.MoveTo(tile, true);
                    moveStatus = MoveStatus.DOWN;
                    if (hasGameStarted)
                        EndTurn(selectedPiece, tile);
                }
                return;
            }
            else
            {
                if (tile.Piece.IsFriendly)
                {
                    OnSelectPiece(tile.Piece);
                }
                else
                {
                    selectedPiece.MoveTo(tile, true, true);
                    if (hasGameStarted)
                        EndTurn(selectedPiece, tile);
                }
            }
        }

        foreach (Piece piece in myPieces)
        {
            if (piece.TargetTile != null)
            {
                readyButton.interactable = true;
                break;
            }
            readyButton.interactable = false;
        }
    }

    void GetPieceNewMovePosition()
    {

        if (moveStatus == MoveStatus.END)
        {
            currentTargetPosition = null;
            moveStatus = null;
            if (hasGameStarted)
            {
                board.ResetBoardColor();
            }
            if (selectedPiece != null)
            {
                selectedPiece.Unselect();
                selectedPiece = null;
            }
            return;
        }
        if (moveStatus == null)
        {
            moveStatus = MoveStatus.UP;
        }

        switch (moveStatus)
        {
            case MoveStatus.UP:

                currentTargetPosition = selectedPiece.transform.position + (moveElevation * Vector3.up);
                moveStatus = MoveStatus.MOVE;

                break;
            case MoveStatus.MOVE:

                if (selectedPiece.TargetTile == null) return;
                currentTargetPosition = selectedPiece.TargetTile.transform.position + (moveElevation * Vector3.up);
                moveStatus = MoveStatus.DOWN;

                break;
            case MoveStatus.DOWN:

                if (selectedPiece.TargetTile == null) return;
                currentTargetPosition = selectedPiece.TargetTile.transform.position;
                moveStatus = MoveStatus.END;
                break;
            default:
                moveStatus = null;
                break;
        }
    }

    public void OnPressReady()
    {
        if (!PV.IsMine) return;
        isReady = true;

        if (isReady && selectedPiece != null)
        {
            selectedPiece.Unselect();
            selectedPiece = null;
        }

        foreach (GameObject obj in objectsToDisableOnReady)
        {
            obj.SetActive(false);
        }

        SendPositions();
    }

    public void SendPositions()
    {
        Vector2[] coordinateList = new Vector2[myPieces.Length];
        for (int c = 0; c < myPieces.Length; c++)
        {
            Vector2 coordinate = myPieces[c].TargetTile.GetCoordinateVector2();
            if (myPieces[c].IsDead)
            {
                coordinate.x = -1;
                coordinate.y = -1;
            }
            coordinateList[c] = coordinate;
        }



        PV.RPC(nameof(RPC_SendPositions), RpcTarget.All, coordinateList);
        board.StartGame();
    }

    public void StartTurn(string userID)
    {
        hasGameStarted = true;
        IsYourTurn = You.UserId == userID;
        if (doWinTheGame && IsYourTurn)
        {
            WinByCapture();
        }
        PV.RPC(nameof(RPC_StartTurn), RpcTarget.All, userID);
    }

    [PunRPC]
    public void RPC_StartTurn(string userID)
    {
        if (!PV.IsMine) return;
        hasGameStarted = true;
        IsYourTurn = You.UserId == userID;
    }

    public void EndTurn(Piece piece, Tile tile)
    {
        if (selectedPiece != null)
        {
            selectedPiece.Unselect();
            selectedPiece = null;
        }
        MovePiece(piece, tile);
        board.ResetBoardColor();
        board.EndTurn(You.UserId);
    }

    public void OnRandomizePiecePosition()
    {
        if (!PV.IsMine) return;
        Piece[] availablePieces = myPieces.Where(piece => !piece.IsMoving).ToArray();
        board.ResetTilesPieceReference();
        if (availablePieces.Length != myPieces.Length) return;
        List<Tile> tileList = board.GetRandomTiles(myPieces.Length, Side);
        if (tileList == null) return;
        int index = 0;
        Shuffle(tileList);
        foreach (Tile tile in tileList)
        {
            tile.Piece = null;
        }
        foreach (Piece piece in myPieces)
        {
            Tile tile = tileList[index];
            index++;
            piece.MoveTo(tile, false);
        }
    }

    public void Shuffle<T>(List<T> list)
    {
        System.Random random = new System.Random();

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    [PunRPC]
    public void RPC_SendPositions(Vector2[] coordinateList, PhotonMessageInfo info)
    {
        if (PV.IsMine) return;
        for (int c = 0; c < myPieces.Length; c++)
        {
            Vector2 coordinate = coordinateList[c];
            if (coordinate.x < 0 || coordinate.y < 0)
            {

            }
            else
            {
                Tile tile = board.GetTileFromCoordinate(coordinate);
                myPieces[c].MoveTo(tile, false);
            }
        }
    }

    public void MovePiece(Piece piece, Tile tile)
    {
        int pieceIndex = 0;
        for (int c = 0; c < myPieces.Length; c++)
        {
            if (piece.ID == myPieces[c].ID)
            {
                pieceIndex = c;
                break;
            }
        }

        Vector2 coordinate = tile.GetCoordinateVector2();


        PV.RPC(nameof(RPC_MovePiece), RpcTarget.All, pieceIndex, coordinate);
    }

    bool doWinTheGame = false;

    [PunRPC]
    public void RPC_MovePiece(int pieceIndex, Vector2 coordinate, PhotonMessageInfo info)
    {
        if (PV.IsMine)
        {
            if (isNextTurnWin && !doWinTheGame)
            {
                doWinTheGame = true;
            }
            return;
        }
        Piece piece = myPieces[pieceIndex];
        Tile tile = Board.GetTileFromCoordinate(coordinate);

        piece.MoveTo(tile, true);

    }

    void WinByCapture()
    {
        if (isNextTurnWin && !yourFlag.IsDead)
        {
            if (Side == Side.TOP)
            {
                EndWithTopVictory();
            }
            else if (Side == Side.BOTTOM)
            {
                EndWithBottomVictory();
            }
        }
    }

    public static PieceManager Find(Player player)
    {
        return FindObjectsOfType<PieceManager>().SingleOrDefault(x => x.PV.Owner != player);
    }

    public void EndWithTopVictory()
    {
        if (!PV.IsMine) return;
        board.EndGameWithTopSideVictory();
    }

    public void EndWithBottomVictory()
    {
        if (!PV.IsMine) return;
        board.EndGameWithBottomSideVictory();
    }

    public void EndWithDraw()
    {
        if (!PV.IsMine) return;
        board.EndGameWithDraw();
    }

    void TEST()
    {
        OnRandomizePiecePosition();
        Hashtable hash = new Hashtable();
        hash.Add("kills", 0);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

}
