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
   
    public float stamina = 64.0f;

    private Vector3 _formationPos;
    public float _distanceFormationPos;
    private Vector3 _ballOnGround;
    public float _distanceBall;
   
    public float _ballSpeed;
    private float _changeStateTime;
    private Quaternion _lookOnLook;
    GameObject sphere;
    // Start is called before the first frame update
    void Start()
    {
        _changeStateTime = Random.Range(0.0f, 1.0f);
        playerState = PlayerState.IDLE;
        playerInfo = transform.GetComponent<PlayerInfo>();
        _formationPos = playerInfo.initialPos;
        _lookOnLook = Quaternion.LookRotation(_ballOnGround - transform.position);
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        Destroy(sphere.GetComponent<Collider>()) ;
    }

    // Update is called once per frame

    void Update()
    {
        _formationPos.z = Mathf.Clamp(playerInfo.ball.transform.position.z, playerInfo.coverageRange.x, playerInfo.coverageRange.y);
        _formationPos.y = 0f;

        _distanceFormationPos = (transform.position - _formationPos).magnitude;
        _ballOnGround = playerInfo.ball.transform.localPosition;
        _ballOnGround.y = 0f;
        _distanceBall = (transform.position - playerInfo.ball.transform.position).magnitude;
        
        _ballSpeed = playerInfo.ball.rigidBody.velocity.magnitude;
        //DEBUG ONLY
        sphere.transform.position = _formationPos;

        transform.rotation = Quaternion.Slerp(transform.rotation, _lookOnLook, Time.deltaTime * PlayerProperties.TURN_SMOOTHING);

        switch (playerState)
        {
            case PlayerState.IDLE:

                playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, 0.0f, PlayerProperties.SPEED_DAMP_TIME, Time.fixedDeltaTime);
                _changeStateTime -= Time.deltaTime;

                if (_changeStateTime < 0 )
                {
                    playerState = PlayerState.MOVING;
                    _changeStateTime = Random.Range(0.0f, 2.0f);
                }
                break;

            case PlayerState.MOVING:

                if (!playerInfo.ball.owner || playerInfo.ball.owner.team != playerInfo.team)
                {
                    if (_distanceBall > PlayerProperties.MAX_DISTANCE_BALL_THRESHOLD)
                    {
                        if (_distanceFormationPos > PlayerProperties.MAX_DISTANCE_FORMATION_POSITION)
                        {
                            //transform.LookAt(_formationPos);
                            _lookOnLook = Quaternion.LookRotation(_formationPos - transform.position);
                            playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.SPRINT_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
                        }
                        else if (_distanceFormationPos < PlayerProperties.MIN_DISTANCE_FORMATION_POSITION)
                        {
                            //transform.LookAt(_ballOnGround);
                            _lookOnLook = Quaternion.LookRotation(_ballOnGround - transform.position);
                            playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.IDLE_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
                        }
                        else
                        {
                            _lookOnLook = Quaternion.LookRotation(_formationPos - transform.position);
                            playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.JOG_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
                        }
                    }
                    else //if (_ballSpeed < PlayerProperties.MAX_BALL_SPEED_TOLERANCE)
                    {
                        _lookOnLook = Quaternion.LookRotation(_ballOnGround - transform.position);
                        playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.SPRINT_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.deltaTime);
                    }
                }
                //else if (playerInfo.ball.owner.team != playerInfo.team) //Defending
                //{
                //    if (_distanceBall < PlayerProperties.MAX_DISTANCE_BALL_THRESHOLD)
                //    {
                //        //transform.LookAt(_ballOnGround);
                //        Quaternion lookOnLook = Quaternion.LookRotation(_ballOnGround - transform.position);
                //        transform.rotation = Quaternion.Slerp(transform.rotation, lookOnLook, Time.deltaTime*5f);
                //        playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.SPRINT_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.fixedDeltaTime);
                //    }
                //}
                else if (playerInfo.ball.owner.team == playerInfo.team)
                { 
                
                
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
