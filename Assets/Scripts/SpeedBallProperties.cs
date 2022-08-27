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

public enum BallOwner
{
    None=1,
    Opponent = 2,
    Team = 3,
    Me = 4
}

public enum TeammateAvailable
{
    Found = 1,
    None = 2
}

public enum ShootOrPass
{
    Shoot = 1,
    DoNotShoot = 2,
    Pass = 3
}

public enum RotationObjective
{
    Ball = 1,
    FormationSpot = 2,
    Goal = 3,
    Teammate = 4
}

public static class PlayerRewards
{
    public static float REWARD_GETTING_BALL = 0.1f;
    public static float REWARD_TACKLE = 0.1f;
    public static float REWARD_SHOOTING = 0.1f;

    public static float REWARD_LOSSING_BALL = -0.01f;
    public static float REWARD_HITTING_WALL = -0.01f;
}

public static class PlayerProperties
{
    public static string ANIM_TACKLE = "Tackle";
    public static string ANIM_STEP_BACK = "StepBack";
    public static string ANIM_SHOT = "Shot";
    public static string ANIM_MOVE = "Speed";

    public static float SHOOTING_FORCE = 15f;
    public static float SHOOTING_HEIGHT = 0.5f;
    public static float MAX_SHOOTING_DISTANCE = 10f;

    public static float SPEED_DAMP_TIME = 0.2f;
    public static float TURN_SMOOTHING = 7f;

    public static float COLLIDER_MAX_TIMEOFF = 0.5f;

    public static float MIN_STATE_CHANGE_TIME = 0.25f;
    public static float MAX_STATE_CHANGE_TIME = 2.0f;

    public static float STAMINA_DIVIDER = 64.0f;
    public static float STAMINA_MIN = 0.5f;
    public static float STAMINA_MAX = 1.0f;

    public static float MAX_DISTANCE_FORMATION_POSITION = 3.0f;
    public static float MIN_DISTANCE_FORMATION_POSITION = 1.0f;

    public static float MAX_DISTANCE_BALL_THRESHOLD = 4f;
    public static float MIN_DISTANCE_BALL_THRESHOLD = 1.5f;
    public static float MAX_BALL_SPEED_TOLERANCE = 6f;
    public static float MAX_BALL_LOST_DISTANCE = -2.5f;

    public static float RUN_SPEED = 1.0f;
    public static float SPRINT_SPEED = 2.0f;
    public static float JOG_SPEED = 0.5f;
    public static float WALK_SPEED = 0.25f;
    public static float IDLE_SPEED = 0.0f;

}
public enum AIType
{
    FUZZY = 0,
    POCA = 1,
    STATE_MACHINE =2
}

public static class EnvironmentProperties
{
    public static string WALLS_TAG = "Walls";
    public static string BALL_TAG = "Ball";
    public static string GOAL_TEAM1_POS_TAG = "Goal1";
    public static string GOAL_TEAM1_TAG = "GoalTeam1";
    public static string GOAL_TEAM2_TAG = "GoalTeam2";
    public static string GOAL_TEAM2_POS_TAG = "Goal2";

    public static float FIELD_SIZE = 30.0f;

}



