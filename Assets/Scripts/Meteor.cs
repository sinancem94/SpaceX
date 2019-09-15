using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : SpaceObject
{

    Vector2 meteorForce;

    private void OnEnable()
    {
        if(rb)
        {
            rb.velocity = Vector3.zero;
            meteorForce =  ( (Vector2.down * Random.Range(9f,11f)) + (Vector2.right * Random.Range(-1f,1f)) ) * GameEngine.Engine.GameSpeed;
            rb.AddForce(meteorForce, ForceMode.VelocityChange);
        }

    }

    void Start()
    {
        int mass = 1;
        float drag = 0.1f;
        SetObject(this, mass, drag);
        this.gameObject.SetActive(false); //will be set active by meteor generator
    }

    private void Update()
    {
        if(this.transform.position.y < -70f)
            this.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (GravityApplied && Mathf.Approximately(rb.drag,0.05f))
        {
            rb.drag = 0f;
        }
        else if(!GravityApplied)
        {
            rb.AddForce(Vector2.down * Random.Range(10f, 20f) * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == GameEngine.Engine.spaceObjectLayer && collision.gameObject.tag == "Rocket")
        {
            this.gameObject.SetActive(false);
        }
    }
}

