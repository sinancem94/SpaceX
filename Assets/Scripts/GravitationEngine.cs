using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Space 
{
    public class GForceCalculation
    {
        // Newton’s law of gravitational attraction in Vector form

        //                                             UnitVector 
        //        planetMass * objectMass         planetPos - objectPos
        //  G = --------------------------- *  --------------------------- 
        //        ||planetPos - objectPos||     ||planetPos - objectPos|| 

        //  ||planetPos - objectPos|| = Distance between two vector
        //  UnitVector = unit vector from planetPositin to objectPosition

        public static Vector2 CalculateForceOnObject(float G, Vector2 objectPos,Vector2 planetPos, float objectMass, float planetMass)
        {
            float distance = Mathf.Abs(Vector2.Distance(planetPos, objectPos));
            //Calculate unit vector from obje1 to obje2
            Vector2 unitVector = (planetPos - objectPos) / distance;
            Vector2 force = G * ((planetMass * objectMass) / distance) * unitVector;

            return force;
        }
    }

    enum GravityEvents 
    {
        EnteredOrbit,
        ExitedOrbit
    }

    //Owns a planet list and the ship. When the ship enters a planets orbit that planet sends engine a message and engine starts applying force to the ship.
    public class GravitationEngine 
    {
        const float G = 5f; //UniversalGravitionalConstant

        Dictionary<int, Planet> Planets;
        int planetCount;
        
        public int gravityLayer = LayerMask.NameToLayer("Planet");

        Queue EnteredEvents;
        Queue ExitedEvents;

        public GravitationEngine()
        {
            Planets = new Dictionary<int, Planet>();
            planetCount = 0;
            
            EnteredEvents = new Queue();
            ExitedEvents = new Queue();
        }

        public Vector2 CalculateGravityOnObject(SpaceObject sObject, int planetKey)
        {
            try
            {
                float pMass = Planets[planetKey].Mass;
                Vector2 pPos = Planets[planetKey].Position;

                float oMass = sObject.Mass;
                Vector2 objectPos = sObject.transform.position;

                return GForceCalculation.CalculateForceOnObject(G,objectPos, pPos, oMass, pMass);
            }
            catch(KeyNotFoundException)
            {
                Debug.LogError("Given planet key not defined. Key is : " + planetKey);
                return Vector2.zero;
            }
        }

        public Vector2 CalculateGravityOnObject(Vector2 sObjectPos, float mass, int planetKey)
        {
            try
            {
                float pMass = Planets[planetKey].Mass;
                Vector2 pPos = Planets[planetKey].Position;

                float oMass = mass;
                Vector2 objectPos = sObjectPos;

                return GForceCalculation.CalculateForceOnObject(G,objectPos, pPos, oMass, pMass);
            }
            catch(KeyNotFoundException)
            {
                Debug.LogError("Given planet key not defined. Key is : " + planetKey);
                return Vector2.zero;
            }
        }

        public int isOnGravitySpace(Vector2 pos, float objectScale = 0f) //returns planet index of which applies gravity on object. If object is on freespace returns 0
        {
            foreach(var p in Planets)
            {
                if(Vector2.Distance(pos,(Vector2)p.Value.Position) <= p.Value.GravityRadius + objectScale) // + 0.5f for space ships scale
                {
                    return p.Key;
                }
            }

            return 0;
        }

        public bool isCollidedWithPlanet(Vector2 pos, int planetIndex)
        {
            if(Vector2.Distance(pos, Planets[planetIndex].Position) <= Planets[planetIndex].PlanetRadius)
            {
                return true;
            }
            return false;
        }

        #region PlanetMessages

        public int AddPlanet(Planet planet)
        {
            planetCount++;
            Planets.Add(planetCount, planet);
            return planetCount;
        }

        #endregion




        #region trash
        /// <summary>
        /// old (buggy) functions for gravity not used at the moment
        /// </summary>
        /// <returns>The object entered orbit.</returns>
        /// <param name="planetIndex">Planet ındex.</param>


        void ObjectEnteredPlanetOrbit(int planetNo, SpaceObject sObject)
        {
            GameEngine.Engine.ObjectEnteredOrbit(planetNo, sObject);
        }

        void ObjectExitedPlanetOrbit(int planetNo, SpaceObject sObject)
        {
            GameEngine.Engine.ObjectExitedOrbit(planetNo, sObject);
        }

        public void Entered(int planetNo)
        {
            EnteredEvents.Enqueue(planetNo);
        }

        public void Exited(int planetNo)
        {
            ExitedEvents.Enqueue(planetNo);
        }

        public IEnumerator IEnumObjectEnteredPlanet(int planetNo)
        {
            SpaceObject sObject = null;

            while (sObject == null)
            {
                sObject = GameEngine.Engine.FindObjectEnteredOrbit(planetNo);
                yield return null;
            }
            ObjectEnteredPlanetOrbit(planetNo, sObject);
        }

        public IEnumerator IEnumObjectExitedPlanet(int planetNo)
        {
            SpaceObject sObject = null;

            while (sObject == null)
            {
                sObject = GameEngine.Engine.FindObjectExitedOrbit(planetNo);
                yield return null;
            }
            ObjectExitedPlanetOrbit(planetNo, sObject);
        }

        public void CalculateEnteredExited() //to be calculated at each update
        {
            int enteredPlanet = 0;
            SpaceObject enteredObject = null;

            if (EnteredEvents.Count > 0)
            {
                enteredPlanet = (int)EnteredEvents.Peek();
                enteredObject = GameEngine.Engine.FindObjectEnteredOrbit(enteredPlanet);
            }

            if (enteredObject != null)
            {
                ObjectEnteredPlanetOrbit(enteredPlanet, enteredObject);
                EnteredEvents.Dequeue();
            }

            int exitedPlanet = 0;
            SpaceObject exitedObject = null;

            if (ExitedEvents.Count > 0)
            {
                exitedPlanet = (int)ExitedEvents.Peek();
                exitedObject = GameEngine.Engine.FindObjectExitedOrbit(exitedPlanet);
            }

            if (exitedObject != null)
            {
                ObjectExitedPlanetOrbit(exitedPlanet, exitedObject);
                ExitedEvents.Dequeue();
            }

        }

        public bool isOnGravitySpaceOfPlanetSAFE(Vector2 pos, int planetIndex) // returns true if given object position inside gravity space of given planet. Difference is this ads 0.5f to the distance
        {
            if (Vector2.Distance(pos, Planets[planetIndex].Position) <= Planets[planetIndex].GravityRadius + 0.5f) // + 0.5f for space ships scale
            {
                return true;
            }
            return false;
        }

        public bool isOnGravitySpaceOfPlanet(Vector2 pos, int planetIndex) // returns true if given object position inside gravity space of given planet
        {
            if (Vector2.Distance(pos, Planets[planetIndex].Position) <= Planets[planetIndex].GravityRadius)
            {
                return true;
            }
            return false;
        }


        //Not using part ends
        #endregion
    }
}
