using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerTeam
{
    Team1 = 0,
    Team2 = 1
}

public enum PlayerRole
{
    Striker,
    Goalie,
    Generic
}

public static class PlayerRewards
{
    public static float REWARD_GETTING_BALL = 0.1f;
    public static float REWARD_LOSSING_BALL = -0.01f;
    public static float REWARD_HITTING_WALL = -0.01f;
    public static float REWARD_TACKLE = 0.1f;
    public static float REWARD_SHOOTING = 0.1f;
}

public static class PlayerProperties
{
    public static string ANIM_TACKLE = "Tackle";
    public static string ANIM_STEP_BACK = "StepBack";
    public static string ANIM_SHOT = "Shot";
    public static string ANIM_MOVE = "Speed";
    public static float SHOOTING_FORCE = 15f;

    public static float SPEED_DAMP_TIME = 0.2f;
    public static float TURN_SMOOTHING = 7f;
}

public enum AIType
{
    FUZZY = 0,
    POCA = 1,
    PPO = 2
}



