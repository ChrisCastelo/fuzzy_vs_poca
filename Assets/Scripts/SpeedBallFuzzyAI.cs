using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    IDLE,
    MOVING,
    PASSING,
    SHOOTING,
    TACKLING
}

public enum TeamState
{
    DEFENSE,
    OFFENSE
}

public class SpeedBallFuzzyAI : MonoBehaviour
{

    public PlayerState playerState;
    public PlayerInfo playerInfo;
    public float changeStateTime;
    public float stamina = 64.0f;

    private Vector3 _formationPos;
    public float _distanceFormationPos;
    private Vector3 _ballOnGround;
    public float _distanceBall;
    public float _distanceGoal;
    public float _ballSpeed;
    
    private Quaternion _lookOnLook;
    
    //Debug only ----------
    private GameObject sphere;
    //Debug only ----------
    // Start is called before the first frame update
    void Start()
    {
        changeStateTime = Random.Range(0.0f, 1.0f);
        playerState = PlayerState.IDLE;
        playerInfo = transform.GetComponent<PlayerInfo>();
        _formationPos = playerInfo.initialPos;
        _lookOnLook = Quaternion.LookRotation(_ballOnGround - transform.position);

        //Debug only ----------
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //Debug only ----------
        Destroy(sphere.GetComponent<Collider>()) ;
    }

    // Update is called once per frame

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
        
        //Keep rotation towards objective
        transform.rotation = Quaternion.Slerp(transform.rotation, _lookOnLook, Time.deltaTime * PlayerProperties.TURN_SMOOTHING);

        //DEBUG ONLY ------
        sphere.transform.position = _formationPos;
        //DEBUG ONLY ------

        switch (playerState)
        {
            case PlayerState.IDLE:

                playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, 0.0f, PlayerProperties.SPEED_DAMP_TIME, Time.fixedDeltaTime);

                if (changeStateTime < 0 )
                {
                    playerState = PlayerState.MOVING;
                    changeStateTime = Random.Range(0.0f, 2.0f);
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
                        playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.SPRINT_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
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
                            Transform _candidate = playerInfo.GetBestCandidate();
                            if (_candidate != null)
                            {
                                transform.LookAt(new Vector3(_candidate.position.x, 0.0f, _candidate.position.z));
                                playerInfo.Shoot();
                            }
                            
                        }
                    }

                    
                }









                    break;

            case PlayerState.PASSING:
                
                
                break;

            case PlayerState.SHOOTING:
                
                
                break;

            case PlayerState.TACKLING:
                
                
                break;

            default:
                
                
                break;
        
        }
    }
}
