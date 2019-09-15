using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipTrajectory
{
    private Vector2 LAUNCH_VELOCITY;
    private Vector2 INITIAL_POSITION;

    int dotCount;
    GameObject Trajectories; //Parent object of dots
    GameObject head;

    GameObject dot;
    List<GameObject> dots;
    int StepCount; //step is 0.02 (fixed delta time). The list count equals how much 0.02 seconds had passed.
    float trajectoryTime;

    float StepTime = Time.fixedDeltaTime;
    Vector2 forceOnObject = Vector2.zero;

    int dotDistance; // how many step will be between dots 

    TrajectoryMode trajectoryMode;

    public ShipTrajectory(int gamePlayMode)
    {
        Trajectories = new GameObject("Trajectory");

        SetHead();
        SetDot();

        //dot.transform.parent = Trajectories.transform;
        if (gamePlayMode == (int)GameCache.GameStyle.DRAGnRELEASE)
            trajectoryTime = 5f; // calculate 4 seconds
        else
            trajectoryTime = 2f;

        dotCount = (int)trajectoryTime * 10;

        StepCount = (int)(trajectoryTime / StepTime);

        dotDistance = StepCount / dotCount;

        dots = new List<GameObject>(dotCount);

        for (int count = 0; count < dotCount; count++)
        {
            dots.Add(GameObject.Instantiate(dot, Trajectories.transform));
            dots[count].SetActive(false);
        }

        trajectoryMode = TrajectoryMode.Free;
    }

    void SetDot()
    {
        dot = new GameObject { name = "Dot" };
        dot.transform.parent = Trajectories.transform;

        SpriteRenderer dSprite = dot.AddComponent<SpriteRenderer>();
        dSprite.sprite = head.GetComponent<SpriteRenderer>().sprite;

        dot.gameObject.SetActive(false);
        head.gameObject.SetActive(false);
    }

    void SetHead()
    {
        head = (GameObject)Resources.Load("Prefabs/Trajectory");
        //head.transform.localScale = new Vector3(scale, scale, 1f);
    }

    //Since the force from dragging will be applied via ForceMode.Impluse force will be applied through out the frame
    //applied force in total will be 

    //     
    public void Dragging(Vector2 pos, Vector2 velocity, SpaceShip draggable)
    {
        INITIAL_POSITION = pos;
        LAUNCH_VELOCITY = velocity;
        DrawTrajectory(draggable);
    }


    Vector2 CalculatePosition(float elapsedTime, Vector2 appliedForceOnThatTime, float mass)
    {
        return (appliedForceOnThatTime * mass * (elapsedTime * elapsedTime) * 0.5f) + CalculateVelocity(elapsedTime) + INITIAL_POSITION;
    }

    Vector2 CalculateVelocity(float elapsedTime)
    {
        return (LAUNCH_VELOCITY * elapsedTime);
    }

    public void ResetTrajectory()
    {
        foreach (GameObject d in dots)
        {
            d.SetActive(false);
        }
    }

    enum TrajectoryMode
    {
        Free,
        Gravity,
        AfterGravity // After object exits planet orbit (as predicted by trajectory) 2 more force applied to object in fixedupdate before engine removes object from gravityApplied list
    }

    void DrawTrajectory(SpaceShip draggable)
    {
        float elapsedTime = 0.0f;
        int PlanetAppliedForce = 0;
        Vector2 tmpDotPos = draggable.transform.position;

        bool isGravityApplied = false;

        Vector2 initVelocity = LAUNCH_VELOCITY;
        int PassedDot = 0;

        int afterGravityCounter = 0;
        int tmpGravityPlanet = 0;

        float drag = draggable.rb.drag;

        forceOnObject = Vector2.zero;

        Color dotColor = dot.GetComponent<SpriteRenderer>().material.color;

        ResetTrajectory();

        for (int i = 0; i < StepCount; i++)
        {
            LAUNCH_VELOCITY  = LAUNCH_VELOCITY / (1f + StepTime * drag); // drag on object

            //reset at each cycle for now
            elapsedTime = 0f;
            INITIAL_POSITION = tmpDotPos;

            if (!isGravityApplied)
            {
                elapsedTime += StepTime;
                tmpDotPos = CalculatePosition(elapsedTime, forceOnObject, draggable.Mass);

                PlanetAppliedForce = GameEngine.Engine.gravitation.isOnGravitySpace(tmpDotPos, 1f);

                if (PlanetAppliedForce != 0)
                {
                    isGravityApplied = true;
                    elapsedTime = 0f;
                    INITIAL_POSITION = tmpDotPos;
                }

            }
            else if (isGravityApplied && PlanetAppliedForce != 0)
            {

                elapsedTime += StepTime;

                forceOnObject = GameEngine.Engine.gravitation.CalculateGravityOnObject(tmpDotPos, draggable.Mass, PlanetAppliedForce) * Time.fixedDeltaTime * GameEngine.Engine.GameSpeed;

                tmpDotPos = CalculatePosition(elapsedTime, forceOnObject, draggable.Mass);

                LAUNCH_VELOCITY += forceOnObject;

                if (drag > 0)
                {
                    if(Mathf.Approximately(drag,0f))
                    {
                        drag = 0f;
                    }
                    else
                    {
                        drag -= draggable.stabilizeCoeff;
                    }
                }

                if (GameEngine.Engine.gravitation.isCollidedWithPlanet(tmpDotPos, PlanetAppliedForce))
                {
                    break;
                }

                tmpGravityPlanet = GameEngine.Engine.gravitation.isOnGravitySpace(tmpDotPos, 1f);

                if (tmpGravityPlanet == 0)
                {
                    tmpGravityPlanet = PlanetAppliedForce;
                    PlanetAppliedForce = 0;
                }

            }
            else // if gravity applied force just turned 0 do two more calculations and stop
            {
                elapsedTime += StepTime;
                forceOnObject = GameEngine.Engine.gravitation.CalculateGravityOnObject(tmpDotPos, draggable.Mass, tmpGravityPlanet) * Time.fixedDeltaTime * GameEngine.Engine.GameSpeed;
                tmpDotPos = CalculatePosition(elapsedTime, forceOnObject, draggable.Mass);
                LAUNCH_VELOCITY += forceOnObject;

                afterGravityCounter++;

                if (afterGravityCounter > 2)
                {
                    afterGravityCounter = 0;
                    tmpGravityPlanet = 0;
                    forceOnObject  = Vector2.zero;

                    drag = draggable.rbDrag;
                    isGravityApplied = false;
                }
            }//is gravity applied ends



            if (i % dotDistance == 0 && PassedDot < dots.Count)
            {
                dots[PassedDot].transform.position = tmpDotPos;
                dots[PassedDot].SetActive(true);
              
                if(PassedDot > dots.Count / 3)
                {
                    dotColor.a -= 0.1f;
                    dots[PassedDot].GetComponent<SpriteRenderer>().material.color = dotColor;
                }

                PassedDot++;
            }


        }//for loop ends

    }
}


/*  switch(trajectoryMode)
              {
                  case TrajectoryMode.Free:

                      elapsedTime += StepTime;
                      tmpDotPos = CalculatePosition(elapsedTime, forceOnObject);
                      forceOnObject = Vector2.zero;

                      PlanetAppliedForce = GameEngine.Engine.gravitation.isOnGravitySpace(tmpDotPos, 0.5f);

                      if (PlanetAppliedForce != 0)
                      {
                          afterGravityPlanet = PlanetAppliedForce;
                          trajectoryMode = TrajectoryMode.Gravity;
                          elapsedTime = 0f;
                          INITIAL_POSITION = tmpDotPos;
                      }
                      break;

                  case TrajectoryMode.Gravity:

                      Debug.LogWarning("Inside Gravity and index is : " + PlanetAppliedForce);
                      elapsedTime += StepTime;
                      forceOnObject = GameEngine.Engine.gravitation.CalculateGravityOnObject(tmpDotPos, PlanetAppliedForce) * Time.fixedDeltaTime;
                      tmpDotPos = CalculatePosition(elapsedTime, forceOnObject);
                      Debug.LogWarning("Inside Gravity and index is : " + PlanetAppliedForce);
                      elapsedTime = 0f;
                      INITIAL_POSITION = tmpDotPos;
                      LAUNCH_VELOCITY += forceOnObject;

                      PlanetAppliedForce = GameEngine.Engine.gravitation.isOnGravitySpace(tmpDotPos, 1f);
                      Debug.LogWarning("Gravvv " + PlanetAppliedForce);
                      if (PlanetAppliedForce == 0)
                      {
                          trajectoryMode = TrajectoryMode.AfterGravity;
                      }
                      else if (GameEngine.Engine.gravitation.isCollidedWithPlanet(tmpDotPos, PlanetAppliedForce))
                      {
                          break;
                      }
                      break;

                  case TrajectoryMode.AfterGravity:

                      elapsedTime += StepTime;
                      forceOnObject = GameEngine.Engine.gravitation.CalculateGravityOnObject(tmpDotPos, afterGravityPlanet) * Time.fixedDeltaTime;
                      tmpDotPos = CalculatePosition(elapsedTime, forceOnObject);
                      elapsedTime = 0f;
                      INITIAL_POSITION = tmpDotPos;
                      LAUNCH_VELOCITY += forceOnObject;

                      afterGravityCounter++;

                      if(afterGravityCounter >= 2)
                      {
                          afterGravityCounter = 0;
                          trajectoryMode = TrajectoryMode.Free;
                      }

                      break;
                  default:
                      break;
              }*/


/*forceOnObject = GameEngine.Engine.gravitation.CalculateGravityOnObjectGivenPlanet(d.transform.position,PlanetAppliedForce);

                d.transform.position = CalculatePositionGivenGravity(forceOnObject, d.transform.position);

                Debug.LogWarning("forceOnObject : " + forceOnObject);






    
            if (!isGravityApplied)
            {
                elapsedTime += StepTime;
                tmpDotPos = CalculatePosition(elapsedTime, forceOnObject);
                forceOnObject = Vector2.zero;

                PlanetAppliedForce = GameEngine.Engine.gravitation.isOnGravitySpace(tmpDotPos,0.5f);

                if (PlanetAppliedForce != 0)
                {
                    isGravityApplied = true;
                    elapsedTime = 0f;
                    INITIAL_POSITION = tmpDotPos;
                }

            }
            else
            {

                elapsedTime += StepTime;

                forceOnObject = GameEngine.Engine.gravitation.CalculateGravityOnObject(tmpDotPos, PlanetAppliedForce) * Time.fixedDeltaTime;

                tmpDotPos = CalculatePosition(elapsedTime, forceOnObject);
          
                elapsedTime = 0f;
                INITIAL_POSITION = tmpDotPos;
                LAUNCH_VELOCITY += forceOnObject;

                if(GameEngine.Engine.gravitation.isCollidedWithPlanet(tmpDotPos,PlanetAppliedForce))
                {
                    break;
                }


                PlanetAppliedForce = GameEngine.Engine.gravitation.isOnGravitySpace(tmpDotPos,1f);

                if (PlanetAppliedForce == 0)
                {
                    elapsedTime = 0f;
                    isGravityApplied = false;
                }

            }






    for (int i = 0; i<StepCount;i++)
        {
            if (!isGravityApplied)
            {
                elapsedTime += StepTime;
                tmpDotPos = CalculatePosition(elapsedTime, forceOnObject);

head.transform.position = tmpDotPos;

                if(headCollision.EnteredOrbit)
                {

                    PlanetAppliedForce = GameEngine.Engine.gravitation.isOnGravitySpace(tmpDotPos);

                    if (PlanetAppliedForce != 0)
                    {
                        isGravityApplied = true;
                        elapsedTime = 0f;
                        INITIAL_POSITION = tmpDotPos;
                    }
                }
            }
            else
            {

                elapsedTime += StepTime;

                forceOnObject = GameEngine.Engine.gravitation.CalculateGravityOnObject(tmpDotPos, PlanetAppliedForce) * Time.fixedDeltaTime;

tmpDotPos = CalculatePosition(elapsedTime, forceOnObject);

elapsedTime = 0f;
                INITIAL_POSITION = tmpDotPos;
                LAUNCH_VELOCITY += forceOnObject;

                head.transform.position = tmpDotPos;

                if(GameEngine.Engine.gravitation.isCollidedWithPlanet(tmpDotPos, PlanetAppliedForce))
                {
                    break;
                }

                if (!headCollision.EnteredOrbit)
                {
                    PlanetAppliedForce = GameEngine.Engine.gravitation.isOnGravitySpace(tmpDotPos);

                    if (PlanetAppliedForce == 0)
                    {
                        elapsedTime = 0f;
                        forceOnObject = Vector2.zero;
                        isGravityApplied = false;
                    }
                }
            }

           

            if(i % dotDistance == 0 && PassedDot<dots.Count)
            {
                dots[PassedDot].transform.position = tmpDotPos;
                dots[PassedDot].SetActive(true);
PassedDot++;


            }
        }

*/
