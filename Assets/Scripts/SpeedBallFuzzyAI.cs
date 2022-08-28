using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using AForge.Fuzzy;
public enum PlayerState
{
    IDLE,
    MOVING,
    PASSING,
    SHOOTING,
    TACKLING
}

public class SpeedBallFuzzyAI : MonoBehaviour
{
    #region Public Fields
    public PlayerState playerState;
    public PlayerInfo playerInfo;
    
    public float changeStateTime;
    public float stamina = 64.0f;
    public float vertical;
    public float horizontal;

    public float newSpeed;
    public float newRotation;
    public float shootPass;
    public Vector3 rotObjective;
    #endregion Public Fields


    #region Private Fields
    private float _velocity;
    private Vector3 _formationPos;
    private float _distanceFormationPos;
    private Vector3 _ballOnGround;
    private float _distanceBall;
    private float _distanceGoal;
    private float _ballSpeed;
    private BallOwner _ballOwner;
    private Quaternion _lookOnLook;
    private Transform _candidate;
    private InferenceSystem IS;
    #endregion Private Fields


    void Start()
    {
        changeStateTime = UnityEngine.Random.Range(0.0f, 1.0f);
        playerState = PlayerState.IDLE;
        playerInfo = transform.GetComponent<PlayerInfo>();
        _formationPos = playerInfo.initialPos;
        _lookOnLook = Quaternion.LookRotation(_ballOnGround - transform.position);
        _ballOwner = BallOwner.None;
        //Fuzzy Database --------------
        Database fuzzyDB = FuzzyLogic.FuzzyDataBase();
        //Fuzzy Database
        ///////////////////////////////////////////////////////////

        //Inference system --------------
        IS = new InferenceSystem(fuzzyDB, new CentroidDefuzzifier(1000));
        //Inference system --------------
        ///////////////////////////////////////////////////////////

        //Fuzzy Rules Dictionary --------------
        string path = Path.GetDirectoryName(Application.dataPath) + "\\" + playerInfo.playerRole.ToString() + "RulesDictionary.txt";
        if (System.IO.File.Exists(path))
        {
            string[] lines = System.IO.File.ReadAllLines(path);
            foreach (var line in lines)
            {
                string[] _rule = line.Split(',');
                try
                {
                    IS.NewRule(_rule[0], _rule[1]);
                }
                catch
                {
                    Debug.Log("Something went wrong with rule: " + line);
                    Debug.Log("Defaulting to base rule book");
                    IS = new InferenceSystem(fuzzyDB, new CentroidDefuzzifier(1000));
                    FuzzyLogic.FuzzyRulesDictionary(IS);
                    break;
                }
            }
        }
        else
        {
            FuzzyLogic.FuzzyRulesDictionary(IS);
        }
        //Fuzzy Rules Dictionary --------------
        ///////////////////////////////////////////////////////////



    }



    void Update()
    {
        changeStateTime -= Time.deltaTime;
        _formationPos.z = Mathf.Clamp(playerInfo.ball.transform.position.z, playerInfo.coverageRange.x, playerInfo.coverageRange.y);
        _formationPos.y = 0f;
        _distanceGoal = (transform.position - playerInfo.goalTeam.position).magnitude;
        _distanceFormationPos = (transform.position - _formationPos).magnitude;
        _ballOnGround = playerInfo.ball.transform.localPosition;
        _ballOnGround.y = 0f;
        _distanceBall = (transform.position - playerInfo.ball.transform.position).magnitude;
        _ballSpeed = playerInfo.ball.rigidBody.velocity.magnitude;
        _velocity = playerInfo.rigidBody.velocity.magnitude;
        _candidate = playerInfo.GetBestCandidate();
        //Keep rotation towards objective
        transform.rotation = Quaternion.Slerp(transform.rotation, _lookOnLook, Time.deltaTime * PlayerProperties.TURN_SMOOTHING);

        ///////////////////////////////////////////////
        //IMITATION TRAINNING RESERVED ------ 
        if (_velocity > 0)
        {
            horizontal = transform.forward.normalized.z;
            vertical = transform.right.normalized.z;
        }
        else
        {
            vertical = 0;
            horizontal = 0;
        }
        //IMITATION TRAINNING  ------
        ///////////////////////////////////////////////

        switch (playerInfo.AI)
        {
            case AIType.FUZZY:

                if (!playerInfo.ball.owner)
                {
                    _ballOwner = BallOwner.None;
                }
                else if (playerInfo.ball.owner.team != playerInfo.team)
                {
                    _ballOwner = BallOwner.Opponent;
                }
                else if (playerInfo.ball.owner.team == playerInfo.team && playerInfo.ball.owner != this.playerInfo)
                {
                    _ballOwner = BallOwner.Team;
                }
                else
                {
                    _ballOwner = BallOwner.Me;
                }

                ////////////////////////////////////////////////
                ///FUZZY LOGIC -------------------------------

                IS.SetInput("BallOwner", (float)_ballOwner);
                IS.SetInput("StrategyDistance", _distanceFormationPos);
                IS.SetInput("BallDistance", _distanceBall);
                IS.SetInput("GoalDistance", _distanceGoal);
                if (_candidate != null)
                {
                    IS.SetInput("TeammateAvailable", (float)TeammateAvailable.Found);
                }
                else
                {
                    IS.SetInput("TeammateAvailable", (float)TeammateAvailable.None);
                }


                newSpeed = IS.Evaluate("Speed");
                newRotation = IS.Evaluate("Rotation");
                shootPass = IS.Evaluate("ShootOrPass");

                ///FUZZY LOGIC -------------------------------
                ////////////////////////////////////////////////
                switch ((int)newRotation)
                {
                    case 1:
                        rotObjective = _ballOnGround;
                        break;
                    case 2:
                        rotObjective = _formationPos;
                        break;
                    case 3:
                        rotObjective = playerInfo.goalTeam.position;
                        break;
                    case 4:
                        
                        if (_candidate != null)
                        {
                            rotObjective = new Vector3(_candidate.position.x, 0.0f, _candidate.position.z);
                            transform.LookAt(rotObjective);
                        }
                        break;
                }
                _lookOnLook = Quaternion.LookRotation(rotObjective - transform.position);
                playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, newSpeed, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);

                switch ((int)shootPass)
                {
                    case 1:
                        playerInfo.Shoot();
                        break;
                    case 3:
                        playerInfo.Shoot();
                        break;
                }

                break;

            case AIType.STATE_MACHINE:
                
                switch (playerState)
                {
                    case PlayerState.IDLE:

                        playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, 0.0f, PlayerProperties.SPEED_DAMP_TIME, Time.fixedDeltaTime);

                        if (changeStateTime < 0)
                        {
                            playerState = PlayerState.MOVING;
                            changeStateTime = UnityEngine.Random.Range(0.0f, 2.0f);
                        }
                        break;

                    case PlayerState.MOVING:

                        if (!playerInfo.ball.owner || playerInfo.ball.owner.team != playerInfo.team)
                        {

                            if (_distanceBall > PlayerProperties.MAX_DISTANCE_BALL_THRESHOLD)
                            {
                                if (_distanceFormationPos > PlayerProperties.MAX_DISTANCE_FORMATION_POSITION)
                                {
                                    _lookOnLook = Quaternion.LookRotation(_formationPos - transform.position);
                                    playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.SPRINT_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
                                }
                                else if (_distanceFormationPos < PlayerProperties.MIN_DISTANCE_FORMATION_POSITION)
                                {
                                    _lookOnLook = Quaternion.LookRotation(_ballOnGround - transform.position);
                                    playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.IDLE_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
                                }
                                else
                                {
                                    _lookOnLook = Quaternion.LookRotation(_formationPos - transform.position);
                                    playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.JOG_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
                                }
                            }
                            else
                            {
                                _lookOnLook = Quaternion.LookRotation(_ballOnGround - transform.position);
                                if (_distanceBall < PlayerProperties.MIN_DISTANCE_BALL_THRESHOLD)
                                {
                                    transform.LookAt(_ballOnGround);
                                }
                                playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.RUN_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
                            }

                        }
                        else
                        {

                            if (_distanceFormationPos < PlayerProperties.MAX_DISTANCE_FORMATION_POSITION * 2)
                            {
                                _lookOnLook = Quaternion.LookRotation(playerInfo.goalTeam.position - transform.position);
                                playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.RUN_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
                            }
                            else
                            {
                                _lookOnLook = Quaternion.LookRotation(playerInfo.goalTeam.position - transform.position);
                                playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.IDLE_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
                            }

                            if (changeStateTime < 0 && playerInfo.ball.owner == this.playerInfo)
                            {

                                if (_distanceGoal < PlayerProperties.MAX_SHOOTING_DISTANCE)
                                {
                                    playerInfo.Shoot();
                                }
                                else
                                {
                                    if (_candidate != null)
                                    {
                                        transform.LookAt(new Vector3(_candidate.position.x, 0.0f, _candidate.position.z));
                                        playerInfo.Shoot();
                                    }
                                }

                            }
                        }
                        break;

                }
                break;
        }
    }
}
