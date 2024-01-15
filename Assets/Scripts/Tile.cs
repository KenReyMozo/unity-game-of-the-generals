using UnityEngine;

public class Tile : Interactable
{
    Piece piece;
    public Piece Piece { get => piece; set => piece = value; }
    [SerializeField] MeshRenderer meshRenderer;

    public bool IsOccupied => piece != null;

    int index_X;
    int index_Y;

    public int X { get => index_X; private set => index_X = value; }
    public int Y { get => index_Y; private set => index_Y = value; }

    public override Interactable OnClick()
    {
        if (IsOccupied) return null;
        if (Piece.IsFriendly) return Piece;
        return this;
    }

    public void SetupTile(Vector3 position, Vector3 scale, int index_X, int index_Y)
    {
        transform.position = position;
        transform.localScale = scale;
        X = index_X;
        Y = index_Y;
    }

    public void SetColor(Color color)
    {
        meshRenderer.material.color = color;
    }

    public Vector2Int GetCoordinateVector2()
    {
        Vector2Int coordinate = new Vector2Int(X, Y);
        return coordinate;
    }
}
