using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : SpaceObject
{

    bool isLaunch;
    Vector2 LaunchForce;

    private void Awake()
    {
        if (Mass == 0)
        {
            Debug.LogWarning("Rocket mass is not setted setting to 1.");
            Mass = 1;
        }

        float drag = 0;
        SetObject((SpaceObject)this, Mass, drag);
    }

    private void LateUpdate()
    {
        if (transform.position.y > 100f || transform.position.x > 40f || transform.position.x < -40f)
        {
            gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (isLaunch)
        {
            rb.AddForce(LaunchForce, ForceMode.Impulse);
            isLaunch = false;
        }
    }

    public void LaunchRocket(Vector2 force)
    {
        LaunchForce = force;
        isLaunch = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        this.gameObject.SetActive(false);
    }

   
}
