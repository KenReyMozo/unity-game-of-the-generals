using UnityEngine;
using TMPro;
public class Piece : Interactable
{

    [SerializeField] TextMeshPro pieceNameText;
    [SerializeField] Position position;
    Color colorSelected;
    Color colorDefault;

    Tile currentTile;
    public Tile TargetTile { get => currentTile; private set => currentTile = value; }
    Vector3? targetTilePosition;

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

    public void Fight(Piece pieceToFight)
    {

    }

    public void MoveTo(Tile tile)
    {
        if(TargetTile != null)
            TargetTile.Piece = null;

        targetTilePosition = tile.transform.position;
        tile.Piece = this;
        TargetTile = tile;
    }
    public void Select()
    {
        IsSelected = true;
        OnSelect();
    }
    public void Unselect()
    {
        IsSelected = false;
        OnUnselect();
    }

    void OnSelect()
    {
        pieceNameText.color = colorSelected;
    }
    void OnUnselect()
    {
        pieceNameText.color = colorDefault;
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
            targetTilePosition = null;
            IsMoving = false;
            return;
        }
    }
}
