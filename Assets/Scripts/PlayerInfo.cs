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
    public SpeedBallEnvController envController;
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
    public Vector2 coverageRange = new Vector2(14f, 8f);
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
        envController = GetComponentInParent<SpeedBallEnvController>();
        
        switch (playerRole)
        {
            case PlayerRole.Goalie:
                coverageRange = new Vector2(14f, 8f);
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
            agentFuzzy = gameObject.AddComponent<SpeedBallFuzzyAI>();
            Destroy(gameObject.GetComponent<DecisionRequester>());
            Destroy(gameObject.GetComponent<SpeedBallAgent>());
            Destroy(gameObject.GetComponent<RayPerceptionSensorComponent3D>());
            Destroy(gameObject.GetComponent<BehaviorParameters>());

        }
        else 
        {
            agent = GetComponent<SpeedBallAgent>();
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

    public void Shoot()
    {
        ball.owner = null;
        ball.rigidBody.velocity = new Vector3(transform.forward.x * PlayerProperties.SHOOTING_FORCE,
                                                        PlayerProperties.SHOOTING_HEIGHT,
                                                        transform.forward.z * PlayerProperties.SHOOTING_FORCE);

        animator.SetBool(PlayerProperties.ANIM_SHOT, true);
        //StartCoroutine(ShootInSeconds(0.15f));
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

    public Transform GetBestCandidate()
    {
        Transform _candidate = null;
        float bestCandidateDistance = 10000.0f;

        foreach (var item in envController.agentsList)
        {
            if (item.team != team && item != this) continue;

            float distance = (transform.position - item.transform.position).magnitude;
            if (distance < bestCandidateDistance)
            {
                _candidate = item.transform;
                bestCandidateDistance = distance;
            }
        }
        return _candidate;
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

    IEnumerator ShootInSeconds(float waitTime)
    {
        float endTime = Time.time + waitTime;
        while (Time.time < endTime)
        {
            yield return null;
        }
        ball.owner = null;
        ball.rigidBody.velocity = new Vector3(transform.forward.x * PlayerProperties.SHOOTING_FORCE,
                                                        PlayerProperties.SHOOTING_HEIGHT,
                                                        transform.forward.z * PlayerProperties.SHOOTING_FORCE);

        animator.SetBool(PlayerProperties.ANIM_SHOT, true);
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
                else 
                {
                    agentFuzzy.changeStateTime = Random.Range(PlayerProperties.MIN_STATE_CHANGE_TIME, PlayerProperties.MAX_STATE_CHANGE_TIME);
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
    }

}
