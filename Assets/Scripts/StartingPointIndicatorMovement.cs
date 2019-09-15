using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingPointIndicatorMovement : MonoBehaviour
{
    Vector3 offset;

    private void OnEnable()
    {
        if(GameEngine.Engine.spaceShip)
            offset = transform.position - GameEngine.Engine.spaceShip.transform.position;
    }

    void LateUpdate()
    {
        transform.position = GameEngine.Engine.spaceShip.transform.position + offset;
    }
}
