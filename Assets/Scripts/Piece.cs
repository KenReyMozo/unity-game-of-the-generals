using UnityEngine;
using TMPro;
using Photon.Pun;
public class Piece : Interactable
{
    float textSizeSelected = 10f;
    float textSizeDefault = 6f;

    [HideInInspector] public PhotonView PV;

    [SerializeField] TextMeshPro pieceNameText;
    [SerializeField] Position position;


    Vector3? currentTargetPosition;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float moveElevation = 1f;

    Color colorSelected;
    Color colorDefault;

    Tile currentTile;
    public Tile TargetTile { get => currentTile; private set => currentTile = value; }
    Vector3? targetTilePosition;
    MoveStatus? moveStatus;

    bool isMoving;
    public bool IsMoving { get => isMoving; set => isMoving = value; }

    bool isFriendly = false;
    public bool IsFriendly { get => isFriendly; set => isFriendly = value; }

    bool isDead = false;
    public bool IsDead { get => isDead; set => isDead = value; }
    bool isWaiting = true;
    public bool IsWaiting { get => isWaiting; set => isWaiting = value; }

    bool isSelected = false;
    public bool IsSelected { get => isSelected; private set => isSelected = value; }

    Transform _t;
    public void OnClick(Piece selectedPiece)
    {
        if (IsMoving) return;
        if (IsDead) {
            isSelected = true;
            return;
        }
        if(this == selectedPiece)
        {
            isSelected = false;
            OnUnselect();
        }
        else
        {
            isSelected = true;
            OnSelect();
        }

    }

    public override Interactable OnClick()
    {
        if (IsDead) return null;
        return this;
    }

    public void SetPiece(float speed, float elevation)
    {
        moveSpeed = speed;
        moveElevation = elevation;
        pieceNameText.fontSize = textSizeDefault;
    }

    public void Fight(Piece pieceToFight)
    {

    }

    public void MoveTo(Tile tile, bool isSingleMove)
    {
        if (IsMoving) return;
        if(TargetTile != null && isSingleMove)
        {
            TargetTile.Piece = null;
        }

        targetTilePosition = tile.transform.position;
        tile.Piece = this;
        TargetTile = tile;
        GetPieceNewMovePosition();
    }

    public void Select()
    {
        if (IsMoving) return;
        IsSelected = true;
        OnSelect();
    }
    public void Unselect()
    {
        if (IsMoving) return;
        IsSelected = false;
        OnUnselect();
    }

    void OnSelect()
    {
        pieceNameText.color = colorSelected;
        pieceNameText.fontSize = textSizeSelected;
    }
    void OnUnselect()
    {
        pieceNameText.color = colorDefault;
        pieceNameText.fontSize = textSizeDefault;
    }

    private void Awake()
    {
        colorSelected = new(1f, 0f, 0f);
        colorDefault = new(0.113f, 0.113f, 0.113f);
        _t = transform;
    }
    void Start()
    {

    }

    void Update()
    {
        if (targetTilePosition == null) return;
        bool isWithinDistance = DistanceHelper.IsWithinDistance(_t.position, (Vector3)targetTilePosition);
        IsMoving = !isWithinDistance;
        if (isWithinDistance)
        {
            moveStatus = null;
            currentTargetPosition = null;
            targetTilePosition = null;
            IsMoving = false;
            return;
        }

        if (moveStatus == null) return;
        if (currentTargetPosition == null) return;

        Vector3 currentPiecePosition = _t.position;

        bool isCurrentTargetWithinDistance = DistanceHelper.IsWithinDistance(currentPiecePosition, (Vector3)currentTargetPosition);

        if (isCurrentTargetWithinDistance)
        {
            GetPieceNewMovePosition();
        }

        if (currentTargetPosition == null) return;
        _t.position = Vector3.Lerp(currentPiecePosition, (Vector3)currentTargetPosition, moveSpeed * Time.deltaTime);
    }

    void GetPieceNewMovePosition()
    {
        if(targetTilePosition != null)
        {
            bool isWithinDistance = DistanceHelper.IsWithinDistance(_t.position, (Vector3)targetTilePosition);
            if (isWithinDistance)
            {
                IsMoving = false;
                moveStatus = null;
                return;
            }
            
        }
        if (moveStatus == MoveStatus.END)
        {
            targetTilePosition = null;
            IsMoving = false;
            moveStatus = null;
            return;
        }
        if (moveStatus == null)
        {
            moveStatus = MoveStatus.UP;
        }

        switch (moveStatus)
        {
            case MoveStatus.UP:

                currentTargetPosition = _t.position + (moveElevation * Vector3.up);
                moveStatus = MoveStatus.MOVE;

                break;
            case MoveStatus.MOVE:

                if (TargetTile == null) return;
                currentTargetPosition = TargetTile.transform.position + (moveElevation * Vector3.up);
                moveStatus = MoveStatus.DOWN;

                break;
            case MoveStatus.DOWN:

                if (TargetTile == null) return;
                currentTargetPosition = targetTilePosition;
                moveStatus = MoveStatus.END;
                break;
            default:
                moveStatus = null;
                break;
        }
    }
}
