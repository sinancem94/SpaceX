using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    List<Rocket> Rockets;
    GameObject rocket;

    int rocketCount;
    public Rocket RocketToLaunch;

    public float maxVelocity;

    void Start()
    {
        rocket = (GameObject)Resources.Load("Prefabs/Rocket");
        Rockets = new List<Rocket>();
        maxVelocity = 50f;

        rocketCount = 10;

        for (int i = 0; i < rocketCount; i++)
        {
            GameObject rckt = Instantiate(rocket, this.transform);
            rckt.SetActive(false);

            Rockets.Add(rckt.GetComponent<Rocket>());
        }

        RocketToLaunch = GetRocket();

        GameEngine.Engine.SetLauncher(this);
    }



    public void LaunchRocket(Vector2 launchForce)
    {
        //activate and launch rocket
        RocketToLaunch.gameObject.SetActive(true);
        RocketToLaunch.transform.position += Vector3.up;

        RocketToLaunch.LaunchRocket(launchForce);

        //Change rocket to next one
        RocketToLaunch = GetRocket();
    }

    private Rocket GetRocket()
    {
        foreach(Rocket r in Rockets)
        {
            if(!r.gameObject.activeSelf)
            {
                return r;
            }
        }
        return null;
    }

}
