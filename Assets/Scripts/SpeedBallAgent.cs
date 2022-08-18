using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public enum Team
{
    Team1 = 0,
    Team2 = 1
}

public class SpeedBallAgent : Agent
{

    public Team team;
    public enum Position
    { 
        Striker,
        Goalie,
        Generic
    };
    
    public Position position;
    [HideInInspector]
    public Vector3 initialPos;
    [HideInInspector]
    public float rotSign;

    [HideInInspector]
    public Rigidbody agentRb;
    [HideInInspector]
    public Animator animator;
    
    public Ball ball;
    public Transform fakeCamTransform;

    BehaviorParameters _behaviorParameters;

    //private float m_BallTouch;
    public Transform goalTeam;
    public Transform goalOpponent;

    private SpeedBallEnvController envController;
    private float _existential;
    private float _profileMultiplier;
    
    public const float REWARD_GETTING_BALL =  0.1f;
    public const float REWARD_LOSSING_BALL = -0.01f;
    public const float REWARD_HITTING_WALL = -0.01f;
    public const float REWARD_TACKLE = 0.1f;
    public const float REWARD_SHOOTING = 0.1f;

    public string ANIM_TACKLE    = "Tackle";
    public string ANIM_STEP_BACK = "StepBack";
    public string ANIM_SHOT      = "Shot";
    public string ANIM_MOVE      = "Speed";
    public float SHOOTING_FORCE  = 15f;

    public float SPEED_DAMP_TIME = 0.2f;
    public float TURN_SMOOTHING = 7f;
    public override void Initialize()
    {
        animator = GetComponent<Animator>();
        agentRb = GetComponent<Rigidbody>();
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
        initialPos = transform.position;
        
        envController = GetComponentInParent<SpeedBallEnvController>();
        if (envController != null)
        {
            _existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            _existential = 1f / MaxStep;
        }

        _existential = _existential / 10;
        if (position == Position.Striker)
        {
            _profileMultiplier = 60f;
        }
        else if (position == Position.Generic)
        {
            _profileMultiplier = 30f;
        }
        else 
        {
            _profileMultiplier = 10f;
        }
        
        _behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (_behaviorParameters.TeamId == (int)Team.Team1)
        {
            team = Team.Team1;
            rotSign = 1f;
            goalTeam = GameObject.FindGameObjectWithTag("Goal1").transform;
            goalOpponent = GameObject.FindGameObjectWithTag("Goal2").transform;
        }
        else
        {
            team = Team.Team2;
            rotSign = -1f;
            goalTeam = GameObject.FindGameObjectWithTag("Goal2").transform;
            goalOpponent = GameObject.FindGameObjectWithTag("Goal1").transform;
        }

    }
    public override void OnEpisodeBegin()
    {
        //transform.position = initialPos;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        //base.CollectObservations(sensor);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(ball.transform.localPosition);
        sensor.AddObservation(Vector3.Dot(agentRb.velocity, agentRb.transform.forward));
        sensor.AddObservation(Vector3.Dot(agentRb.velocity, agentRb.transform.right));
        sensor.AddObservation(goalTeam.localPosition);
        //sensor.AddObservation(m_GoalTeam.localPosition);
        //sensor.AddObservation(m_GoalOpponent.localPosition);

    }
    public override void OnActionReceived(ActionBuffers actions)
    {

        if (!ball.Owner || ball.Owner.team != team)
        {
            if (position == Position.Goalie)
            {
                // Existential penalty for Goalies.
                AddReward(-_existential * _profileMultiplier);
            }
            else if (position == Position.Generic)
            {
                // Existential penalty for Generic
                AddReward(-_existential * _profileMultiplier);
            }
            else if (position == Position.Striker)
            {
                // Existential penalty for Strikers
                AddReward(-_existential * _profileMultiplier);
            }
        }


        //base.OnActionReceived(actions);
        //float Horizontal = actions.ContinuousActions[0];
        //float Vertical = actions.ContinuousActions[1];

        int horizontalAxis = actions.DiscreteActions[0];
        int horizontal = 0;
        switch (horizontalAxis)
        {
            case 1:
                horizontal = -1;
                break;
            case 2:
                horizontal = 1;
                break;
        }
        int verticalAxis = actions.DiscreteActions[1];
        int vertical = 0;
        switch (verticalAxis)
        {
            case 1:
                vertical = -1;
                break;
            case 2:
                vertical = 1;
                break;
        }

        if (horizontal != 0f || vertical != 0f)
        {
            //Create a new vector based on camera forward
            Vector3 cameraFwd = fakeCamTransform.transform.forward;
            //kill y and normalize it
            cameraFwd.y = 0;
            cameraFwd.Normalize();
            cameraFwd *= vertical;
            //Get Camera's right
            Vector3 cameraR = fakeCamTransform.transform.right;
            cameraR.y = 0;
            cameraR.Normalize();
            cameraR *= horizontal;

            Vector3 targetDir = cameraFwd + cameraR;
            //Create rotation based on this new vector assuming that up is the global y axis.
            Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
            //Create a rotation tahat is an increment closer to our targetRot from player's rotation.
            Quaternion newRot = Quaternion.Lerp(agentRb.rotation, targetRot, TURN_SMOOTHING * Time.fixedDeltaTime);
            //Aply rotation
            agentRb.MoveRotation(newRot);


            if (ball.Owner == this)
            {
                animator.SetFloat(ANIM_MOVE, Mathf.Clamp(0.75f * (horizontal * horizontal) + (vertical * vertical) * 0.75f, 0f, 0.75f), SPEED_DAMP_TIME, Time.fixedDeltaTime);
            }
            else
            {
                animator.SetFloat(ANIM_MOVE, horizontal * horizontal + vertical * vertical, SPEED_DAMP_TIME, Time.fixedDeltaTime);
            }
        }
        else
        {
            animator.SetFloat(ANIM_MOVE, 0.0f, SPEED_DAMP_TIME, Time.fixedDeltaTime);
        }

        if (ball.Owner == this && actions.DiscreteActions[2] == 1)
        {
            ball.Owner = null;
            ball.m_Rigidbody.velocity = new Vector3(transform.forward.x * SHOOTING_FORCE, 0.5f, transform.forward.z * SHOOTING_FORCE);
            animator.SetBool(ANIM_SHOT, true);
            AddReward(REWARD_SHOOTING);
            
        }
       
        
    }
       
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //base.Heuristic(actionsOut);
        //ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        //continuousActions[0] = Input.GetAxisRaw("Horizontal");
        //continuousActions[1] = Input.GetAxisRaw("Vertical");
        //continuousActions[2] = Input.GetKey(KeyCode.Mouse0) ? 1 : 0; 
        //var discreteActionsOut = actionsOut.DiscreteActions;
        
        //var horizontalAxis = Input.GetAxisRaw("Horizontal");
        //if (horizontalAxis > 0)
        //{
        //    discreteActionsOut[0] = 2;
        //}
        //else if (horizontalAxis < 0)
        //{
        //    discreteActionsOut[0] = 1;
        //}
        //else 
        //{
        //    discreteActionsOut[0] = 0;
        //}

        //var verticalAxis = Input.GetAxisRaw("Vertical");
        //if (verticalAxis > 0)
        //{
        //    discreteActionsOut[1] = 2;
        //}
        //else if (verticalAxis < 0)
        //{
        //    discreteActionsOut[1] = 1;
        //}
        //else
        //{
        //    discreteActionsOut[1] = 0;
        //}

        //discreteActionsOut[2] = Input.GetKey(KeyCode.Mouse0) ? 1 : 0;

    }

    public void OnShot()
    {
        animator.SetBool(ANIM_SHOT, false);
    }

    public void OnStepBack()
    {
        animator.SetBool(ANIM_STEP_BACK, false);
    }
    public void OnTackle()
    {
        animator.SetBool(ANIM_TACKLE, false);
    }

    public void OnBallLost()
    {
        ball.Owner = null;
        ball.m_Rigidbody.velocity = new Vector3(transform.forward.x * -2f, 7.5f, transform.forward.z * -2f);
        
    }

    public void ResetAnimator()
    {
        animator.SetBool(ANIM_STEP_BACK, false);
        animator.SetFloat(ANIM_MOVE, 0);
        animator.SetBool(ANIM_SHOT, false);
        animator.SetBool(ANIM_TACKLE, false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            if (_ball.Owner == null)
            {
                _ball.m_Rigidbody.rotation = Quaternion.identity;
                _ball.Owner = this;
                AddReward(REWARD_GETTING_BALL);
            }
        }
        
        if (collision.gameObject.tag == "Walls")
        {
            animator.SetBool(ANIM_STEP_BACK, true);
            if (ball.Owner == this)
            {
                OnBallLost();
                AddReward(REWARD_LOSSING_BALL);
            }
            AddReward(REWARD_HITTING_WALL);
        }
        if (collision.gameObject.tag == "GoalTeam2" || collision.gameObject.tag == "GoalTeam1")
        {
            animator.SetBool(ANIM_STEP_BACK, true);
            AddReward(REWARD_HITTING_WALL);
        }


        if (collision.gameObject.TryGetComponent<SpeedBallAgent>(out SpeedBallAgent player))
        {
            if (player.team != team )
            {
                if (ball.Owner == this)
                {
                    OnBallLost();
                    animator.SetBool(ANIM_STEP_BACK, true);
                    AddReward(REWARD_LOSSING_BALL);
                    player.animator.SetBool(ANIM_TACKLE, true);
                    player.transform.LookAt(new Vector3(transform.position.x, 0f, transform.position.z));
                    player.AddReward(REWARD_TACKLE);
                }

                if (ball.Owner == player)
                {
                    player.OnBallLost();
                    animator.SetBool(ANIM_TACKLE, true);
                    transform.LookAt(new Vector3(player.transform.position.x, 0f, player.transform.position.z));
                    AddReward(REWARD_GETTING_BALL);
                    player.animator.SetBool(ANIM_STEP_BACK, true);
                    player.AddReward(REWARD_LOSSING_BALL);
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
            //animator.SetBool(ANIM_STEP_BACK, true);
            //if (ball.Owner == this)
            //{
            //    OnBallLost();
            //    //AddReward(REWARD_LOSSING_BALL);
            //}
            //AddReward(REWARD_HITTING_WALL);
        //}
    }
}
