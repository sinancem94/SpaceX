using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Space;
using ContentSizeMapping;


public class GameEngine : MonoBehaviour
{
    public GravitationEngine gravitation;
    ObjectSizeHandler objectSizeHandler;
    public SpaceShip spaceShip;
    ShipTrajectory trajectory;
    public RocketLauncher Launcher;
    InputManager inputM;
    GameCache gameCache;

    public Dictionary<int,SpaceObject> SpaceObjects;
    int objectCount;
    public int spaceObjectLayer;

    public List<SpaceObject> gravityAppliedObjects;

    InputManager.State iState = 0; //input state. 0 for idle, 1 for dragging, 2 for released
    public float GameSpeed;
    Vector2 dragVec;

    //When ObjectExitedOrbit could not manage to remove a element from gravityAppliedObjects we catch that object in FixedUpdate and set this value.
    //and after foreach loop try to remove that element again
    int FalselyRemoved;  

    public static GameEngine Engine;

    private void Awake()
    {
        Application.targetFrameRate = 60;

        if (Engine)
            Destroy(this);
        else
            Engine = this;
    }
    
    void Start()
    {
        //First initiliaze cache
        gameCache = new GameCache();

        gravitation = new GravitationEngine();
        trajectory = new ShipTrajectory((int)gameCache.gamePlayData.gravGameStyle);
        objectSizeHandler = new ObjectSizeHandler();
        inputM = new InputManager(objectSizeHandler.ScreenSize.x / 3f);

        SpaceObjects = new Dictionary<int, SpaceObject>();
        gravityAppliedObjects = new List<SpaceObject>();

        

        GameSpeed = 1f;

        spaceObjectLayer = LayerMask.NameToLayer("SpaceObject");
    }

    //When space objects enters start add itself to the SpaceObject List
    public int AddSpaceObject(SpaceObject spaceObject)
    {
        objectCount++;
        SpaceObjects.Add(objectCount, spaceObject);
        return objectCount;
    }

    //When space ship enters start add itself to the SpaceObject List
    public void SetShip(SpaceShip sShip)
    {
        spaceShip = sShip;
    }

    public void SetLauncher(RocketLauncher launcher)
    {
        Launcher = launcher;
    }

    void Update()
    {
        iState = inputM.GetInputState();

        if(gameCache.gamePlayData.gravGameStyle == GameCache.GameStyle.DRAGnRELEASE)
        {
            if (iState == InputManager.State.Dragging)
            {
                Time.timeScale = 0.2f;
                dragVec = CalculateDragVelocity(inputM.GetDragVector());
                trajectory.Dragging(spaceShip.transform.position, dragVec, spaceShip);
            }
            else if (iState == InputManager.State.Release)
            {
                Debug.Log("Ship launched at speed : " + dragVec);
                Time.timeScale = 1f;
                //trajectory.ResetTrajectory();
                spaceShip.LaunchShip(dragVec);
            }

        }
        else if(gameCache.gamePlayData.gravGameStyle == GameCache.GameStyle.PUSH)
        {
            if (iState == InputManager.State.Dragging)
            {
                dragVec = CalculateDragVelocity(inputM.GetDirection());
                spaceShip.SetShipEngine(dragVec);
            }
            else if (iState == InputManager.State.Release)
            {
                //trajectory.ResetTrajectory();
                spaceShip.StopShipEngine();
            }

            trajectory.Dragging(spaceShip.transform.position, spaceShip.rb.velocity, spaceShip);

        }
    }


    private void FixedUpdate()
    {

       // trajectory.Dragging(spaceShip.transform.position, spaceShip.rb.velocity, spaceShip.rb);

        foreach (SpaceObject s in gravityAppliedObjects)
        {
            SpaceObject so = SpaceObjects[s.objectNo];

            if(so.PlanetAppliesGravity == 0)
            {
                Debug.LogWarning("Sometimes ObjectExitedOrbit dont work properly and could not remove index from list. Object no is : " + so.objectNo + ". This causes engine to collapse. So if PlanetAppliesGravity is 0 remove element again.");
                FalselyRemoved = s.objectNo;
                continue;
            }

            Vector2 gforce = gravitation.CalculateGravityOnObject(so, so.PlanetAppliesGravity) * GameSpeed;

            so.rb.AddForce(gforce, ForceMode.Acceleration); //since objects mass are used in gravity calculation we can ignore it here.
        }

        //Remove falselyremoved object from list
        if(FalselyRemoved != 0)
        {
            SpaceObject removedObject = gravityAppliedObjects.Find(obj => obj.objectNo == FalselyRemoved);
            if(removedObject != null)
                 gravityAppliedObjects.Remove(removedObject);

            FalselyRemoved = 0;
        }

    }

    Vector2 CalculateDragVelocity(Vector2 pulledAmount)
    {

        if(gameCache.gamePlayData.gravGameStyle == GameCache.GameStyle.DRAGnRELEASE)
        {
            pulledAmount = pulledAmount / 10f;

            if (pulledAmount.x > spaceShip.maxVelocity)
                pulledAmount.x = spaceShip.maxVelocity;
            else if (pulledAmount.x < -spaceShip.maxVelocity)
                pulledAmount.x = -spaceShip.maxVelocity;

            if (pulledAmount.y > spaceShip.maxVelocity)
                pulledAmount.y = spaceShip.maxVelocity;
            else if (pulledAmount.y < -spaceShip.maxVelocity)
                pulledAmount.y = -spaceShip.maxVelocity;
        }
        else if(gameCache.gamePlayData.gravGameStyle == GameCache.GameStyle.PUSH)
        {
            pulledAmount = pulledAmount * spaceShip.maxVelocity;
        }
        else
        {
            pulledAmount = Vector2.zero;
        }

        return pulledAmount * GameSpeed;
    }

    public void ObjectEnteredOrbit(int planetIndex, SpaceObject sObject)
    {
        if(sObject == null)
        {
            Debug.LogError("Planet gives a message indicates a object entered it's orbit but no object found in planet orbit");
            return;
        }

        gravityAppliedObjects.Add(sObject);
        SpaceObjects[sObject.objectNo].PlanetAppliesGravity = planetIndex;
    }

    public void ObjectExitedOrbit(int planetIndex, SpaceObject sObject)
    {
        SpaceObject actObject = gravityAppliedObjects.Find(obj => obj.objectNo == sObject.objectNo);

        if(actObject != null)
        {
            SpaceObjects[actObject.objectNo].PlanetAppliesGravity = 0;
            gravityAppliedObjects.Remove(actObject);
        }
    }

    public void RemoveObjectFromGravityList(SpaceObject sObject)
    {
        SpaceObject actObject = gravityAppliedObjects.Find(obj => obj.objectNo == sObject.objectNo);
        if(actObject != null)
            gravityAppliedObjects.Remove(actObject);
        
        SpaceObjects[sObject.objectNo].PlanetAppliesGravity = 0;
    }


    #region trash
    /// <summary>
    /// old (buggy) functions for gravity not used at the moment
    /// </summary>
    /// <returns>The object entered orbit.</returns>
    /// <param name="planetIndex">Planet ındex.</param>

    public SpaceObject FindObjectEnteredOrbit(int planetIndex)
    {
        foreach (var so in SpaceObjects)
        {
            bool isExist = gravityAppliedObjects.Contains(so.Value);
            bool isOnThatPlanet = gravitation.isOnGravitySpaceOfPlanetSAFE(so.Value.transform.position, planetIndex);

            if (so.Value.GravityApplied && !isExist && isOnThatPlanet)
            {
                return so.Value;
            }
        }

        return null;
    }

    public SpaceObject FindObjectExitedOrbit(int planetIndex)
    {
        foreach (var so in SpaceObjects)
        {
            bool isExist = gravityAppliedObjects.Contains(so.Value);

            bool isOnThatPlanet = gravitation.isOnGravitySpaceOfPlanet(so.Value.transform.position, planetIndex);

            if (!so.Value.GravityApplied && isExist && !isOnThatPlanet)
            {
                return so.Value;
            }
        }
        return null;
    }

    //Not using part ends
    #endregion
}
