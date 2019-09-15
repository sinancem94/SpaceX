using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceShip : SpaceObject
{
    bool isLaunch; //Eğer true olursa gemiyi ateşler ve hemen false a setlenir. Game Engine kontrol ediyor.
    Vector2 LaunchForce;

    bool isWorking;//Eğer true olursa gemiyi ateşler. false ise gemiyi durdurur. Game Engine kontrol ediyor. 
    float EngineTorque;
    float torqueCoeff;

    public float maxVelocity;
    public float stabilizeCoeff;

    ParticleSystem EngineBurst;
    ParticleSystem.MainModule EngineBurstMain;

    void Start()
    {
        if(Mass == 0f)
        {
            Debug.LogWarning("Ship mass is not setted setting to 1.");
            Mass = 1;
        }

        float drag = 0.4f;

        if (GameCache.gCache.GetGameStyle() == GameCache.GameStyle.DRAGnRELEASE)
            maxVelocity = 25f;
        else if (GameCache.gCache.GetGameStyle() == GameCache.GameStyle.PUSH)
            maxVelocity = 8f;


        SetObject((SpaceObject)this,Mass, drag);
        GameEngine.Engine.SetShip(this);

        EngineBurst = GetComponentInChildren<ParticleSystem>();
        EngineBurstMain = EngineBurst.main;
        EngineBurstMain.startSpeed = 0f;

        stabilizeCoeff = 0.004f;
        torqueCoeff = 0.08f;

        EngineTorque = 0f;
    }

    // Update is called once per frame
    void Update()
    {
         ////////////////////////////////
        //Debug denemeleri için duruyor. Kullanmak için yorumu kaldır sadece

       /* float vert = Input.GetAxis("Vertical");
        float hort = Input.GetAxis("Horizontal");

        Vector3 force = new Vector3(hort *10f, vert * 10f, 0f);

        rb.AddForce(force,ForceMode.Acceleration);

        if (Input.GetKey("space"))
        {
            rb.AddForce(Vector3.up * 20f);
        }*/
        /////////////
    }

    private void FixedUpdate()
    {
        if(isLaunch && GameCache.gCache.GetGameStyle() == GameCache.GameStyle.DRAGnRELEASE)
        {
            rb.AddForce(LaunchForce,ForceMode.Impulse);
            isLaunch = false;
        }
        else if(isWorking && GameCache.gCache.GetGameStyle() == GameCache.GameStyle.PUSH)
        {
            rb.AddForce(LaunchForce, ForceMode.Force);

        }

        StabilizeDrag(stabilizeCoeff);

        if(rb.velocity.magnitude > 0f)
            RotateObject(rb.velocity);
    }

    public void SetShipEngine(Vector2 iForce)
    {
        if(!isWorking)
        {
            isWorking = true;
            EngineTorque = 0f;
        }

        if(EngineTorque < 1f)
        {
            EngineTorque += torqueCoeff;
        }

        SetEngineFlame(iForce, EngineTorque);

        iForce *= EngineTorque;

        iForce.x *= transform.right.x;
        iForce.y *= transform.up.y;

        LaunchForce = iForce;
        
    }

    void SetEngineFlame(Vector2 input,float torque)
    {
        //Normalize burst pos. Since input force is multiplied by gameSpeed and maxVelocity of ship divide those values to get 1 based input.
        float burstPos = -1 * input.x * 0.3f / maxVelocity / GameEngine.Engine.GameSpeed;

        EngineBurst.transform.localPosition = new Vector2(burstPos, 0f);
        EngineBurstMain.startSpeed = torque;
    }

    public void StopShipEngine()
    {
        EngineTorque = 0f;
        EngineBurstMain.startSpeed = EngineTorque;
        isWorking = false;
    }

    public void LaunchShip(Vector2 force)
    {
        rb.AddForce(rb.velocity * -1, ForceMode.Impulse);//Before launching the ship make velocity 0 instantly.
        LaunchForce = force;
        isLaunch = true;
    }

   
}
