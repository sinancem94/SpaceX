using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickGenerator : MonoBehaviour
{
    List<Vector2> BrickPositions;

    GameObject Brick;
    Transform BrickParent;

    int Row;
    int Column;
    int brickCount;

    void Start()
    {
        BrickParent = new GameObject("Bricks").transform;
        Brick = (GameObject)Resources.Load("Prefabs/Brick");

        Vector2 StartPosition = new Vector2(-10,40);

        float xPosIncrease = Brick.transform.localScale.x + 0.1f;
        float yPosIncrease = Brick.transform.localScale.y + 0.1f;

        Row = 5;
        Column = 10;
        brickCount = Row * Column;

        CreateBricks(Row, Column, StartPosition, xPosIncrease, yPosIncrease);


        Vector2 startposTwo = new Vector2(-15f, 10f);

        int row2 = 2;
        int column2 = 5;

        CreateBricks(row2, column2, startposTwo, xPosIncrease, yPosIncrease);
    }


    void CreateBricks(int row,int column, Vector2 startPos , float xInc, float yInc)
    {
        Vector2 tmpPos = startPos;

        for(int r = 0;r<row;r++)
        {
            for(int c = 0; c < column;c++ )
            {
                Instantiate(Brick, tmpPos, Quaternion.Euler(Vector3.zero),BrickParent);
                tmpPos.x += xInc;
            }
            tmpPos.x = startPos.x;
            tmpPos.y += yInc;
        }
    }
}
