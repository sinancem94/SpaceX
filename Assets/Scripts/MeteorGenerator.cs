using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorGenerator : MonoBehaviour
{
    List<Vector2> GenerationPositions;

    private List<GameObject> Meteors;
    int metaorCount;

    GameObject MeteorsParent;
    GameObject meteor;

    float timer;
    float sendTime;

    Vector2 randVector; // Used for randomize position and send time

    float maxX; //pozisyonun ne kadar değişceği
    float maxY;

    int counter; //Count how many meteors have sended

    private void Start()
    {
        GenerationPositions = new List<Vector2>();
        GenerationPositions.Add(new Vector2(0f, 70f));

        MeteorsParent = new GameObject("Meteors");
        meteor = (GameObject)Resources.Load("Prefabs/Meteor");
        metaorCount = 20;

        Meteors = new List<GameObject>(metaorCount);

        for(int i = 0; i < metaorCount; i++)
        {
            Meteors.Add(Instantiate(meteor,MeteorsParent.transform));
        }

        maxX = 25f;
        maxY = 10f;

        timer = 0f;
        sendTime = 2f;

        Random.InitState(0);
    }

    GameObject GetMeteor()
    {
        foreach(GameObject m in Meteors) 
        {
            if (m.activeSelf == false)
                return m;
        }

        return null;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if(timer >= sendTime)
        {
            timer = 0f;
            sendTime = Random.Range(1f, 2.5f);

            foreach(Vector2 generatePosition in GenerationPositions)
            {
                GameObject tempMeteor = GetMeteor();

                randVector = new Vector2(Random.Range(-maxX, maxX), Random.Range(-maxY, maxY));

                if (tempMeteor)
                {
                    tempMeteor.transform.position = generatePosition + randVector;
                    tempMeteor.SetActive(true);
                    counter++;

                    //50 den fazla meteor gönderildiyse random seed i sıfırla.
                    if (counter > 50)
                    {
                        Random.InitState(0);
                        counter = 0;
                    }
                }

            }



        }
    }
}
