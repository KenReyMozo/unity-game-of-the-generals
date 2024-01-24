using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class PieceTester : MonoBehaviour
{
    [SerializeField] TMP_Dropdown piece1Dropdown;
    [SerializeField] TMP_Dropdown piece2Dropdown;

    List<string> options;


    void Start()
    {
        piece1Dropdown.onValueChanged.AddListener(onChangeDropdown1);
        piece2Dropdown.onValueChanged.AddListener(onChangeDropdown2);

        string[] positionValues = Enum.GetNames(typeof(Position));
        options = new List<string>(positionValues);

        piece1Dropdown.AddOptions(options);
        piece2Dropdown.AddOptions(options);

        SetupPosition();
    }

    public void onChangeDropdown1(int index)
    {

    }   

    public void onChangeDropdown2(int index)
    {

    }

    private void OnDisable()
    {
        piece1Dropdown.onValueChanged.RemoveListener(onChangeDropdown1);
        piece2Dropdown.onValueChanged.RemoveListener(onChangeDropdown2);
    }

    private Dictionary<Position, Dictionary<Position, int>> pieceMatrix;

    void SetupPosition()
    {
        pieceMatrix = new Dictionary<Position, Dictionary<Position, int>>();
        foreach (Position pos in System.Enum.GetValues(typeof(Position)))
        {
            AddPositionMatrix(pos);
        }
    }

    void AddPositionMatrix(Position piecePosition)
    {
        foreach (Position pos in System.Enum.GetValues(typeof(Position)))
        {
            switch (piecePosition)
            {
                case Position.FLAG:
                    if (pos == Position.FLAG)
                        AddValue(piecePosition, pos, 1);
                    else
                        AddValue(piecePosition, pos, -1);
                    break;
                case Position.SPY:
                    if (pos == Position.PRIVATE)
                        AddValue(piecePosition, pos, -1);
                    else if (piecePosition == pos)
                        AddValue(piecePosition, pos, 0);
                    else
                        AddValue(piecePosition, pos, 1);
                    break;
                case Position.PRIVATE:
                    if (pos == Position.SPY)
                        AddValue(piecePosition, pos, 1);
                    else if (piecePosition == pos)
                        AddValue(piecePosition, pos, 0);
                    else if (piecePosition == Position.FLAG)
                        AddValue(piecePosition, pos, 1);
                    else
                        AddValue(piecePosition, pos, -1);
                    break;
                default:
                    if (piecePosition < pos)
                        AddValue(piecePosition, pos, 1);
                    else if (piecePosition > pos)
                        AddValue(piecePosition, pos, -1);
                    else
                        AddValue(piecePosition, pos, 0);
                    break;
            }
        }
    }

    void AddValue(Position offense, Position defense, int value)
    {
        if (!pieceMatrix.ContainsKey(offense))
        {
            pieceMatrix[offense] = new Dictionary<Position, int>();
        }

        pieceMatrix[offense][defense] = value;
    }

    public void TestFight()
    {
        Position one, two;

        string name1 = options[piece1Dropdown.value];
        string name2 = options[piece2Dropdown.value];

        Enum.TryParse(name1, out one);
        Enum.TryParse(name2, out two);

        if(one != null && two != null)
        {
            int value = pieceMatrix[one][two];
            Debug.Log(name1 + " vs. " + name2 + " = " + value);
        }
    }
}

