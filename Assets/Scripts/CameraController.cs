using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ContentSizeMapping;

public class CameraController : MonoBehaviour
{
    Vector3 offset;

    public float distanceWithScene;

    void Start()
    {
        if(Mathf.Approximately(distanceWithScene,0f))
        {
            distanceWithScene = 25f;
        }
        
        offset = new Vector3(0f, distanceWithScene * 0.55f , -distanceWithScene);
    }


    void Update()
    {
        transform.position = GameEngine.Engine.spaceShip.transform.position + offset;
    }
}
