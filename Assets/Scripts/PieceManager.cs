using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PieceManager : MonoBehaviour
{
    Board board;
    enum MoveStatus
    {
        UP,
        MOVE,
        DOWN,
        END,
    }

    const string PIECE_TAG = "Piece";
    const string TILE_TAG = "Tile";

    const string READY_TEXT = "Ready!";
    const string SET_READY_TEXT = "Set Ready";

    [SerializeField] Button readyButton;
    PlayerControl playerControl;
    Piece selectedPiece;

    [SerializeField] Piece[] myPieces;

    Vector3? currentTargetPosition;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float moveElevation = 1f;

    MoveStatus? moveStatus;

    bool isReady = false;
    bool hasGameStarted = false;

    bool isYourTurn = false;
    public bool IsYourTurn { get => isYourTurn; set => isYourTurn = value; }

    public void StartGame()
    {
        readyButton.gameObject.SetActive(false);
        board.ResetBoardColor();
        foreach (Piece piece in myPieces)
        {
            if(piece.TargetTile == null)
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

    private void OnEnable()
    {
        playerControl.Enable();
    }

    private void OnDisable()
    {
        playerControl.Disable();
    }


    void Start()
    {
        playerControl.BoardControl.Select.performed += _ => OnSelectSomething();
        board = FindObjectOfType<Board>();
        if (board == null)
        {
            enabled = false;
            return;
        }
        foreach (Piece piece in myPieces)
        {
            piece.IsFriendly = true;
        }
    }

    void OnSelectSomething()
    {
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
        if (!piece.IsFriendly) return;

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
            if(hasGameStarted)
                board.ResetBoardColor();
        }

        if (hasGameStarted) {
            if (selectedPiece != null && selectedPiece.TargetTile != null)
            {
                board.GetAvailablePositionsFromPosition(selectedPiece.TargetTile);
            }
        }
        else
        {
            if (selectedPiece != null && selectedPiece.TargetTile == null)
            {
                board.GetAvailablePositionsFromSide(Side.BOTTOM);
            }
        }

    }

    void OnSelectTile(Tile tile)
    {
        if (selectedPiece == null)
        {
            if (tile.Piece)
            {
                OnSelectPiece(tile.Piece);
            }
        }
        else if (selectedPiece != null)
        {
            if(tile.Piece == null)
            {
                if(selectedPiece.TargetTile != null)
                {
                    GetPieceNewMovePosition();
                    selectedPiece.MoveTo(tile);
                }
                else
                {
                    currentTargetPosition = tile.transform.position;
                    selectedPiece.MoveTo(tile);
                    moveStatus = MoveStatus.DOWN;
                }
                return;
            }
            else
            {
                OnSelectPiece(tile.Piece);
            }
        }

        foreach (Piece piece in myPieces)
        {
            if(piece.TargetTile != null)
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
            if(selectedPiece != null)
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

    void Update()
    {
        if (selectedPiece == null) return;
        if (moveStatus == null) return;
        if (currentTargetPosition == null) return;

        Vector3 currentPiecePosition = selectedPiece.transform.position;

        bool isWithinDistance = DistanceHelper.IsWithinDistance(currentPiecePosition, (Vector3)currentTargetPosition);

        if (isWithinDistance)
        {
            GetPieceNewMovePosition();
        }

        if (currentTargetPosition == null) return;
        selectedPiece.transform.position = Vector3.Lerp(currentPiecePosition, (Vector3)currentTargetPosition, moveSpeed * Time.deltaTime);
    }

    public void OnPressReady()
    {
        isReady = !isReady;

        if (isReady && selectedPiece != null)
        {
            selectedPiece.Unselect();
            selectedPiece = null;
        }
        TextMeshProUGUI buttonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (isReady)
        {
            board.ResetBoardColor();
            buttonText.text = READY_TEXT;
        }
        else
        {
            buttonText.text = SET_READY_TEXT;
        }
    }

}
