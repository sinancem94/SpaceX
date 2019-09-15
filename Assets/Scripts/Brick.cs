using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : SpaceObject
{
   
    void Start()
    {
        float mass = 0.1f;
        float drag = 0.1f;
        SetObject(this, mass, drag);
    }


    private void OnCollisionEnter(Collision collision)
    {
       
    }
}
