using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Classes for arranging Content sizes according to screen
namespace ContentSizeMapping 
{
    public class SizeHandler
    {
        //Arrange platform width convert screen width to world size in x
        public float gameScreenHeight = Camera.main.orthographicSize * 2.0f;
        public float gameScreenWidth = (Camera.main.orthographicSize * 2.0f) * Camera.main.aspect; //gameScreenHeight
        public float defatultGameWidth = 3.5f; //iphone x game width

        protected Vector2 defaultScreenSize = new Vector2(1125f,2436f); //iphone x screen width and screen height
        public Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);
    }

    enum ObjectPlace
    {
        pMiddle,

    }

   public enum ObjectType
    {
        Board,
        Camera,
        Default
    }

    public class ObjectSizeHandler : SizeHandler
    {
        public Vector3 ArrangeObjectSize(ObjectType objectType, Vector3 currentSizeOfObject, float cSize = 1f) 
        {
            Vector3 oldSize = currentSizeOfObject;
            Vector3 newSize = Vector2.one;
            float defaultColumnSize = 3f;
            
            switch(objectType)
            {

                case ObjectType.Board:

                    float columnDiff = defaultColumnSize / cSize;
                    float boardSize = oldSize.x * columnDiff;

                    boardSize = IncreaseSizeLinearly(boardSize);

                    newSize = new Vector3(boardSize, boardSize);



                    //For changing position according to changed scale
                 /*   float posColumnDiff = 0f;
                    if(!Mathf.Approximately(defaultColumnSize,cSize)) // eğer column sayısı 3 den farklıysa
                    {
                        posColumnDiff = (cSize / defaultColumnSize) - (oldSize.x - newSize.x) - (cSize / 2f);

                        Vector3 middlePos = Camera.main.ScreenToWorldPoint((Vector3)(ScreenSize) / 2f);
                        objectToArrange.transform.position = new Vector3(middlePos.x - posColumnDiff, middlePos.y - posColumnDiff, 0f);
                    }
                    else
                    {
                        objectToArrange.transform.position += (Vector3)PosDiffAccordingToChangedSize(oldSize, newSize);
                    }*/

                    break;

                case ObjectType.Camera:

                    float cDiff = defaultColumnSize / cSize;
                    float newCamSize = oldSize.x / cDiff;

                    newCamSize = DecreaseSizeLinearly(newCamSize);

                    newSize = new Vector3(newCamSize, newCamSize);


                    break;
                case ObjectType.Default:
                default:

                    float tmpSize = IncreaseSizeLinearly(oldSize.x);

                    newSize = new Vector2(tmpSize, tmpSize);
                    // objectToArrange.transform.position += (Vector3)PosDiffAccordingToChangedSize(oldSize, newSize); 

                              

                    break;
                

            }

            Debug.Log("Size after changing : " + newSize);

            return newSize;
           // Vector2 diff = Vector2.one - (Vector2)objectToArrange.transform.localScale;
           // objectToArrange.transform.position = diff;//new Vector3(-GetWallPosition() / 4f, -GetGroundPosition() / 4f , 0f);

        }

        float DecreaseSizeLinearly(float oldSize)
        {
            float size = (defatultGameWidth * oldSize) / gameScreenWidth;
            return size;
        }

        float IncreaseSizeLinearly(float oldSize)
        {
            float size = (gameScreenWidth * oldSize) / defatultGameWidth; //doğrusal orantıyla artıyor size
            return size;
        }

        Vector2 PosDiffAccordingToChangedSize(Vector2 oldSize, Vector2 newSize)
        {
            Vector2 diff = new Vector2((oldSize.x - newSize.x), (oldSize.y - newSize.y));
            return diff;
        }

        public float GetWallPosition()
        {
            return gameScreenWidth / 2f;
        }

        public float GetGroundPosition()
        {
            return gameScreenHeight / 2f;
        }


        public float SetDynamicSize(float defaultScreenSize, float defaultContentSize)
        {
            float maxAllowedScreenDiff = defaultScreenSize / 4f;
            float maxAllowedContentDiff = defaultContentSize / 4f;

            float SizeArranger = (Screen.width - defaultScreenSize) / maxAllowedScreenDiff;

            if (Mathf.Abs(SizeArranger) > 1)
                SizeArranger = (SizeArranger > 1) ? 1 : -1;

            return defaultContentSize + (maxAllowedContentDiff * SizeArranger);

        }
    }
    

}
