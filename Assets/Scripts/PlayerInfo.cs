using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public AIType AI;
    public PlayerTeam team;
    public PlayerRole playerRole;
    [HideInInspector]
    public SpeedBallAgent agent;
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

    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        initialPos = transform.position;
        initialRot = transform.rotation;
        agent = GetComponent<SpeedBallAgent>();
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();

        if (team == PlayerTeam.Team1)
        {
            
            goalTeam = GameObject.FindGameObjectWithTag("Goal1").transform;
            goalOpponent = GameObject.FindGameObjectWithTag("Goal2").transform;
        }
        else
        {
            goalTeam = GameObject.FindGameObjectWithTag("Goal2").transform;
            goalOpponent = GameObject.FindGameObjectWithTag("Goal1").transform;
        }

        if (AI == AIType.FUZZY)
        {
            gameObject.AddComponent<SpeedBallFuzzyAI>();
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
        animator.SetFloat(PlayerProperties.ANIM_MOVE, 0);
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
        ball.Owner = null;
        ball.m_Rigidbody.velocity = new Vector3(transform.forward.x * -2f, 7.5f, transform.forward.z * -2f);

    }
}
