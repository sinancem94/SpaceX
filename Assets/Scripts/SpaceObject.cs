using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Uzayda hareket eden objeler için Base Class. Gravity gibi force lar bu class a uygulanıyor. 
public class SpaceObject : MonoBehaviour
{

    public float Mass;
    public Rigidbody rb;

    //drag of the object. When gravity force applied decrease drog until 0. When it's not increase it slowly until 1. (for know could change it)
    public float rbDrag;

    public bool GravityApplied;
    public int PlanetAppliesGravity;

    public int objectNo;

    public void SetObject(SpaceObject sObje,float mass, float drag)
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("No rigidbody assigned to object. Setting a default rigidbody with no gravity.");
            rb = this.gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }

        GravityApplied = false;
        Mass = mass;
        rbDrag = drag;

        rb.mass = Mass;
        rb.drag = rbDrag;

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        objectNo = GameEngine.Engine.AddSpaceObject(sObje);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == GameEngine.Engine.gravitation.gravityLayer)
        {
            Debug.Log("Object(" + objectNo + ") Entered Trigger");
            GravityApplied = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == GameEngine.Engine.gravitation.gravityLayer)
        {
            Debug.Log("Object(" + objectNo + ") Exited Trigger");
            GravityApplied = false;
        }
    }


    private void OnDisable()
    {
        transform.localPosition = Vector3.zero;
        rb.velocity = Vector3.zero;

        if(GravityApplied)
        {
            GameEngine.Engine.RemoveObjectFromGravityList(this);
            GravityApplied = false;
        }
    }

    //When objects entered gravity space reduces their drag. They will rotate around planet easily
    protected void StabilizeDrag(float stabilizeCoeff)
    {
        if(GravityApplied && rb.drag > 0)
        {
            if(Mathf.Approximately(rb.drag,0f))
            {
                rb.drag = 0f;
            }
            else
            {
                rb.drag -= stabilizeCoeff;
            }
        }
        else if(!GravityApplied && rb.drag < rbDrag)
        {
            rb.drag = rbDrag;
        }
    }

    //Rotate objects according to their velocity
    public void RotateObject(Vector2 velocity)
    {
        Vector2 v = rb.velocity;
        float angle = Mathf.Atan2(v.y,v.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

}