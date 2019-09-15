using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Space;

public class Planet : MonoBehaviour
{
    public Vector3 Position;
    public float Mass;

    public float GravityRadius;
    public float PlanetRadius;

    public int planetNo;
    Transform Orbit;
    float rotationSpd;

    void Start()
    {
        CalculatePlanetScale(Mass);

        Position = transform.position;
        Orbit = this.transform.GetChild(transform.childCount - 1);

        float OrbitRadius = 1f;
        foreach(SphereCollider sc in GetComponents<SphereCollider>())
        {
            if(sc.isTrigger == true)
            {
                OrbitRadius = sc.radius * 2f;
                break;
            }
        }

        Orbit.localScale = new Vector3(OrbitRadius, OrbitRadius, 1f);

        GravityRadius = (OrbitRadius * PlanetRadius); // 0.5f is for ships scale. It should be shipScale /2f 

        rotationSpd = 135f;

        planetNo = GameEngine.Engine.gravitation.AddPlanet(this);
    }

    void CalculatePlanetScale(float mass)
    {
        transform.localScale = new Vector3(mass / 5f, mass / 5f, mass / 5f);
        PlanetRadius = transform.localScale.x / 2f;
    }

    private void LateUpdate()
    {
        Orbit.Rotate(Vector3.forward, Time.deltaTime * rotationSpd);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == GameEngine.Engine.spaceObjectLayer)
        {
            Debug.Log("Planet trigger entered. Planet no : " + planetNo);
            SpaceObject obje = other.GetComponent<SpaceObject>();
            GameEngine.Engine.ObjectEnteredOrbit(planetNo, obje);

            //GameEngine.Engine.gravitation.Entered(planetNo);
            //StartCoroutine(GameEngine.Engine.gravitation.IEnumObjectEnteredPlanet(planetNo));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == GameEngine.Engine.spaceObjectLayer)
        {
            Debug.Log("Planet trigger exited. Planet no : " + planetNo);
            SpaceObject obje = other.GetComponent<SpaceObject>();
            GameEngine.Engine.ObjectExitedOrbit(planetNo, obje);

            //GameEngine.Engine.gravitation.Exited(planetNo);
            //StartCoroutine(GameEngine.Engine.gravitation.IEnumObjectExitedPlanet(planetNo));
        }
    }
}
