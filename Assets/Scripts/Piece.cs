using UnityEngine;
using TMPro;
using Photon.Pun;
public class Piece : Interactable
{
    float textSizeSelected = 8f;
    float textSizeDefault = 6f;

    [HideInInspector] public PhotonView PV;
    public PieceManager PM;

    Animator animator;

    int id;
    public int ID { get => id; set => id = value; }

    [SerializeField] TextMeshPro pieceNameText;
    [SerializeField] Position position;
    public Position Position { get => position; private set => position = value; }


    Vector3? currentTargetPosition;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float moveElevation = 1f;

    [SerializeField] GameObject[] objectsToDisableIfNotMine;

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

    bool winOnNextTurnIfAlive = false;
    public void SetWinOnNextTurnIfAlive() => winOnNextTurnIfAlive = true;

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

    public void SetIsNotMine()
    {
        pieceNameText.gameObject.SetActive(false);
        CapsuleCollider collider = GetComponentInChildren<CapsuleCollider>();
        if(collider != null)
        {
            collider.enabled = false;
        }
        transform.rotation = new Quaternion(0f, -180f, 0f, 0f);

        foreach (GameObject obj in objectsToDisableIfNotMine)
        {
            obj.SetActive(false);
        }
    }

    public void Fight(Piece pieceToFight, Tile tile)
    {
        bool killYourPiece = false, killEnemyPiece = false;

        int state = PM.Board.PieceMatrix[Position][pieceToFight.Position];

        if(state > 0)
        {
            killEnemyPiece = true;
        }
        else if(state < 0)
        {
            killYourPiece = true;
        }
        else
        {
            killEnemyPiece = true;
            killYourPiece = true;
        }


        if(killYourPiece && killEnemyPiece)
        {
            FXManager.Instance.GetLoseFX(transform.position);
            FXManager.Instance.GetLoseFX(pieceToFight.transform.position);
            tile.Piece = null;
            TargetTile = tile;
            Die();
            pieceToFight.Die();
        }
        else if (killYourPiece)
        {
            FXManager.Instance.GetLoseFX(transform.position);
            FXManager.Instance.GetWinFX(pieceToFight.transform.position);
            tile.Piece = pieceToFight;
            TargetTile = null;
            Die();
            if (Position == Position.FLAG)
            {
                if (PM.Side == Side.TOP)
                    PM.EndWithBottomVictory();
                else if (PM.Side == Side.BOTTOM)
                    PM.EndWithTopVictory();
            }
        }
        else if (killEnemyPiece)
        {
            FXManager.Instance.GetWinFX(transform.position);
            FXManager.Instance.GetLoseFX(pieceToFight.transform.position);
            tile.Piece = this;
            TargetTile = tile;
            pieceToFight.Die();
            if (pieceToFight.Position == Position.FLAG)
            {
                if (PM.Side == Side.TOP)
                    PM.EndWithTopVictory();
                else if (PM.Side == Side.BOTTOM)
                    PM.EndWithBottomVictory();
            }
        }
    }

    public void Die()
    {
        IsDead = true;
        gameObject.SetActive(false);
    }

    public void MoveTo(Tile tile, bool isSingleMove)
    {
        if (IsMoving) return;
        if (TargetTile != null && isSingleMove)
        {
            TargetTile.Piece = null;
        }
        if (tile.Piece != null)
        {
            if (tile.Piece.IsFriendly)
            {
                Fight(tile.Piece, tile);
            }
            else
            {
                tile.Piece = this;
                TargetTile = tile;
            }
        }
        else
        {
            tile.Piece = this;
            TargetTile = tile;
        }

        CheckIfFlagInWinPosition(tile);
        targetTilePosition = tile.transform.position;
        GetPieceNewMovePosition();
    }

    void CheckIfFlagInWinPosition(Tile tile)
    {
        if (Position != Position.FLAG) return;
        if (IsDead) return;

        if (PM.Side == Side.TOP)
        {
            int winIndex = PM.Board.GetTopSideWinIndex();
            if(tile.Y == winIndex)
            {
                PM.SetWinOnNextTurnIfAlive();
            }
        }
        else if (PM.Side == Side.BOTTOM)
        {
            int winIndex = PM.Board.GetBottomSideWinIndex();
            if (tile.Y == winIndex)
            {
                PM.SetWinOnNextTurnIfAlive();
            }
        }
    }

    public void MoveTo(Tile tile, bool isSingleMove, bool overrideIsEnemy)
    {
        if (IsMoving) return;
        if (TargetTile != null && isSingleMove)
        {
            TargetTile.Piece = null;
        }
        if (tile.Piece != null)
        {
            if (tile.Piece.IsFriendly || overrideIsEnemy)
            {
                Fight(tile.Piece, tile);
            }
            else
            {
                tile.Piece = this;
                TargetTile = tile;
            }
        }
        else
        {
            tile.Piece = this;
            TargetTile = tile;
        }

        targetTilePosition = tile.transform.position;
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
        animator.SetBool("isSelected", true);
    }
    void OnUnselect()
    {
        pieceNameText.color = colorDefault;
        pieceNameText.fontSize = textSizeDefault;
        animator.SetBool("isSelected", false);
    }

    private void Awake()
    {
        colorSelected = new(1f, 0f, 0f);
        colorDefault = new(0.113f, 0.113f, 0.113f);
        _t = transform;
    }
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
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
