using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
public class PlayerInfo : MonoBehaviour
{
    public AIType AI;
    public PlayerTeam team;
    public PlayerRole playerRole;
    [HideInInspector]
    public SpeedBallAgent agent;
    [HideInInspector]
    public SpeedBallFuzzyAI agentFuzzy;
    [HideInInspector]
    public Vector3 initialPos;
    [HideInInspector]
    public Quaternion initialRot;
    [HideInInspector]
    public Rigidbody rigidBody;
    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public Ball ball;
    [HideInInspector]
    public Transform goalTeam;
    [HideInInspector]
    public Transform goalOpponent;
    [HideInInspector]
    public Vector2 coverageRange = new Vector2(13f, 5f);
    [HideInInspector]
    public Collider col;
    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        initialPos = transform.position;
        initialRot = transform.rotation;
        ball = GameObject.FindGameObjectWithTag(EnvironmentProperties.BALL_TAG).GetComponent<Ball>();
        if (AI == AIType.POCA)
        {
            agent = GetComponent<SpeedBallAgent>();
        }
        else
        {
            agentFuzzy = GetComponent<SpeedBallFuzzyAI>();
        }

        switch (playerRole)
        {
            case PlayerRole.Goalie:
                coverageRange = new Vector2(13f, 5f);
                break;
            case PlayerRole.Generic:
                coverageRange = new Vector2(10f, -5f);
                break;
            case PlayerRole.Striker:
                coverageRange = new Vector2(7f, -12f);
                break;
        }

        if (team == PlayerTeam.Team1)
        {
            goalTeam = GameObject.FindGameObjectWithTag(EnvironmentProperties.GOAL_TEAM1_POS_TAG).transform;
            goalOpponent = GameObject.FindGameObjectWithTag(EnvironmentProperties.GOAL_TEAM2_POS_TAG).transform;
        }
        else
        {
            goalTeam = GameObject.FindGameObjectWithTag(EnvironmentProperties.GOAL_TEAM2_POS_TAG).transform;
            goalOpponent = GameObject.FindGameObjectWithTag(EnvironmentProperties.GOAL_TEAM1_POS_TAG).transform;
            coverageRange = new Vector2(-1*coverageRange.x, -1*coverageRange.y);
        }

        if (AI == AIType.FUZZY)
        {
            gameObject.AddComponent<SpeedBallFuzzyAI>();
            Destroy(gameObject.GetComponent<DecisionRequester>());
            Destroy(gameObject.GetComponent<SpeedBallAgent>());
            Destroy(gameObject.GetComponent<RayPerceptionSensorComponent3D>());
            Destroy(gameObject.GetComponent<BehaviorParameters>());
           
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetAnimator()
    {
        animator.SetBool(PlayerProperties.ANIM_STEP_BACK, false);
        animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.IDLE_SPEED);
        animator.SetBool(PlayerProperties.ANIM_SHOT, false);
        animator.SetBool(PlayerProperties.ANIM_TACKLE, false);
    }
    public void OnShot()
    {
        animator.SetBool(PlayerProperties.ANIM_SHOT, false);
    }

    public void OnStepBack()
    {
        animator.SetBool(PlayerProperties.ANIM_STEP_BACK, false);
    }
    public void OnTackle()
    {
        animator.SetBool(PlayerProperties.ANIM_TACKLE, false);
    }

    public void OnBallLost()
    {
        ball.owner = null;
        StartCoroutine(DeactivateColliderForSeconds(PlayerProperties.COLLIDER_MAX_TIMEOFF, ball.col));
        ball.rigidBody.velocity = new Vector3(transform.forward.x * PlayerProperties.MAX_BALL_LOST_DISTANCE, 
                                              PlayerProperties.MAX_BALL_SPEED_TOLERANCE, transform.forward.z * 
                                              PlayerProperties.MAX_BALL_LOST_DISTANCE);
    }

    IEnumerator DeactivateColliderForSeconds(float waitTime, Collider _col)
    {
        float endTime = Time.time + waitTime; 
        while (Time.time < endTime)
        {
            _col.enabled = false; 
            yield return null;
        }
        _col.enabled = true;
        yield break;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            if (_ball.owner == null)
            {
                _ball.rigidBody.rotation = Quaternion.identity;
                _ball.owner = this;
                
                if (AI == AIType.POCA)
                {
                    agent.AddReward(PlayerRewards.REWARD_GETTING_BALL);
                }
            }
        }

        if (collision.gameObject.tag == EnvironmentProperties.WALLS_TAG)
        {
            animator.SetBool(PlayerProperties.ANIM_STEP_BACK, true);
            
            if (ball.owner == this)
            {
                OnBallLost();
                
                if (AI == AIType.POCA)
                {
                    agent.AddReward(PlayerRewards.REWARD_LOSSING_BALL);
                }
            }
            
            if (AI == AIType.POCA)
            {
                agent.AddReward(PlayerRewards.REWARD_HITTING_WALL);
            }
        }

        if (collision.gameObject.tag == EnvironmentProperties.GOAL_TEAM1_TAG || collision.gameObject.tag == EnvironmentProperties.GOAL_TEAM2_TAG)
        {
            animator.SetBool(PlayerProperties.ANIM_STEP_BACK, true);
            
            if (AI == AIType.POCA)
            {
                agent.AddReward(PlayerRewards.REWARD_HITTING_WALL);
            }
        }

        if (collision.gameObject.TryGetComponent<PlayerInfo>(out PlayerInfo _otherplayerInfo))
        {
            if (_otherplayerInfo.team != team)
            {
                if (ball.owner == this)
                {
                    OnBallLost();
                    animator.SetBool(PlayerProperties.ANIM_STEP_BACK, true);

                    if (AI == AIType.POCA)
                    {
                        agent.AddReward(PlayerRewards.REWARD_LOSSING_BALL);
                    }
                    else 
                    { 
                    
                    }

                    _otherplayerInfo.animator.SetBool(PlayerProperties.ANIM_TACKLE, true);
                    _otherplayerInfo.transform.LookAt(new Vector3(transform.position.x, 0f, transform.position.z));
                    
                    if (_otherplayerInfo.AI == AIType.POCA)
                    {
                        _otherplayerInfo.agent.AddReward(PlayerRewards.REWARD_TACKLE);
                    }
                }

                if (ball.owner == _otherplayerInfo)
                {
                    _otherplayerInfo.OnBallLost();
                    animator.SetBool(PlayerProperties.ANIM_TACKLE, true);
                    transform.LookAt(new Vector3(_otherplayerInfo.transform.position.x, 0f, _otherplayerInfo.transform.position.z));
                    
                    if (AI == AIType.POCA)
                    {
                        agent.AddReward(PlayerRewards.REWARD_GETTING_BALL);
                    }
                    
                    _otherplayerInfo.animator.SetBool(PlayerProperties.ANIM_STEP_BACK, true);

                    if (_otherplayerInfo.AI == AIType.POCA)
                    {
                        _otherplayerInfo.agent.AddReward(PlayerRewards.REWARD_LOSSING_BALL);
                    }
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            _ball.GetComponent<Rigidbody>().rotation = Quaternion.identity;
        }

        //if (collision.gameObject.tag == "Walls" || collision.gameObject.tag == "GoalTeam2" || collision.gameObject.tag == "GoalTeam1")
        //{
        //animator.SetBool(PlayerProperties.ANIM_STEP_BACK, true);
        //if (playerInfo.ball.owner == this)
        //{
        //    OnBallLost();
        //    //AddReward(PlayerProperties.REWARD_LOSSING_BALL);
        //}
        //AddReward(PlayerProperties.REWARD_HITTING_WALL);
        //}
    }

}
