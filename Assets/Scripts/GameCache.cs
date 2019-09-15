using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCache
{
    public static GameCache gCache;

    public GameCache()
    {
        gCache = this;

        SetCache();
    }

    public GamePlayData gamePlayData;

    public struct GamePlayData
    {
        public  GameStyle gravGameStyle;
        public  float gameSpeed;
    }

    public enum GameStyle
    {
        DRAGnRELEASE,
        PUSH
    }

    void SetCache()
    {
        gamePlayData.gravGameStyle = GameStyle.PUSH;
        gamePlayData.gameSpeed = 1.0f;
    }


    public GameStyle GetGameStyle()
    {
        return gamePlayData.gravGameStyle;
    }
}
