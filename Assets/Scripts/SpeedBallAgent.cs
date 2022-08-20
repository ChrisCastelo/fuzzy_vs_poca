using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;



public class SpeedBallAgent : Agent
{
    public PlayerInfo playerInfo;

    #region Private Fields

    private SpeedBallEnvController _envController;
    private float _existential;

    #endregion Private Fields
    
    public override void Initialize()
    {
        playerInfo = GetComponent<PlayerInfo>();

        _envController = GetComponentInParent<SpeedBallEnvController>();
        if (_envController != null)
        {
            _existential = 1f / _envController.MaxEnvironmentSteps;
        }
        else
        {
            _existential = 1f / MaxStep;
        }

    }
    public override void OnEpisodeBegin(){}
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(playerInfo.ball.transform.localPosition);
        sensor.AddObservation(Vector3.Dot(playerInfo.rigidBody.velocity, playerInfo.rigidBody.transform.forward));
        sensor.AddObservation(Vector3.Dot(playerInfo.rigidBody.velocity, playerInfo.rigidBody.transform.right));
        sensor.AddObservation(playerInfo.goalTeam.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (playerInfo.AI == AIType.FUZZY)  return;
        
        if (!playerInfo.ball.Owner || playerInfo.ball.Owner.team != playerInfo.team)
        {
            // Existential penalty for Goalies.
            AddReward(-_existential);
        }

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
            Vector3 cameraFwd = Camera.main.transform.forward;
            //kill y and normalize it
            cameraFwd.y = 0;
            cameraFwd.Normalize();
            cameraFwd *= vertical;
            //Get Camera's right
            Vector3 cameraR = Camera.main.transform.right;
            cameraR.y = 0;
            cameraR.Normalize();
            cameraR *= horizontal;

            Vector3 targetDir = cameraFwd + cameraR;
            //Create rotation based on this new vector assuming that up is the global y axis.
            Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
            //Create a rotation tahat is an increment closer to our targetRot from player's rotation.
            Quaternion newRot = Quaternion.Lerp(playerInfo.rigidBody.rotation, targetRot, PlayerProperties.TURN_SMOOTHING * Time.fixedDeltaTime);
            //Aply rotation
            playerInfo.rigidBody.MoveRotation(newRot);


            if (playerInfo.ball.Owner == this)
            {
                playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, Mathf.Clamp(0.75f * (horizontal * horizontal) + (vertical * vertical) * 0.75f, 0f, 0.75f), PlayerProperties.SPEED_DAMP_TIME, Time.fixedDeltaTime);
            }
            else
            {
                playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, horizontal * horizontal + vertical * vertical, PlayerProperties.SPEED_DAMP_TIME, Time.fixedDeltaTime);
            }
        }
        else
        {
            playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, 0.0f, PlayerProperties.SPEED_DAMP_TIME, Time.fixedDeltaTime);
        }

        if (playerInfo.ball.Owner == this && actions.DiscreteActions[2] == 1)
        {
            playerInfo.ball.Owner = null;
            playerInfo.ball.m_Rigidbody.velocity = new Vector3(transform.forward.x * PlayerProperties.SHOOTING_FORCE, 0.5f, transform.forward.z * PlayerProperties.SHOOTING_FORCE);
            playerInfo.animator.SetBool(PlayerProperties.ANIM_SHOT, true);
            AddReward(PlayerRewards.REWARD_SHOOTING);
        }
    }
       
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        if (playerInfo.AI == AIType.FUZZY) return;

        var discreteActionsOut = actionsOut.DiscreteActions;

        var horizontalAxis = Input.GetAxisRaw("Horizontal");
        if (horizontalAxis > 0)
        {
            discreteActionsOut[0] = 2;
        }
        else if (horizontalAxis < 0)
        {
            discreteActionsOut[0] = 1;
        }
        else
        {
            discreteActionsOut[0] = 0;
        }

        var verticalAxis = Input.GetAxisRaw("Vertical");
        if (verticalAxis > 0)
        {
            discreteActionsOut[1] = 2;
        }
        else if (verticalAxis < 0)
        {
            discreteActionsOut[1] = 1;
        }
        else
        {
            discreteActionsOut[1] = 0;
        }

        discreteActionsOut[2] = Input.GetKey(KeyCode.Mouse0) ? 1 : 0;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Ball>(out Ball _ball))
        {
            if (_ball.Owner == null)
            {
                _ball.m_Rigidbody.rotation = Quaternion.identity;
                _ball.Owner = this.playerInfo;
                AddReward(PlayerRewards.REWARD_GETTING_BALL);
            }
        }
        
        if (collision.gameObject.tag == "Walls")
        {
            playerInfo.animator.SetBool(PlayerProperties.ANIM_STEP_BACK, true);
            if (playerInfo.ball.Owner == this)
            {
                playerInfo.OnBallLost();
                AddReward(PlayerRewards.REWARD_LOSSING_BALL);
            }
            AddReward(PlayerRewards.REWARD_HITTING_WALL);
        }

        if (collision.gameObject.tag == "GoalTeam2" || collision.gameObject.tag == "GoalTeam1")
        {
            playerInfo.animator.SetBool(PlayerProperties.ANIM_STEP_BACK, true);
            AddReward(PlayerRewards.REWARD_HITTING_WALL);
        }

        if (collision.gameObject.TryGetComponent<PlayerInfo>(out PlayerInfo otherplayerInfo))
        {
            if (otherplayerInfo.team != playerInfo.team )
            {
                if (playerInfo.ball.Owner == this)
                {
                    playerInfo.OnBallLost();
                    playerInfo.animator.SetBool(PlayerProperties.ANIM_STEP_BACK, true);
                    AddReward(PlayerRewards.REWARD_LOSSING_BALL);
                    otherplayerInfo.animator.SetBool(PlayerProperties.ANIM_TACKLE, true);
                    otherplayerInfo.transform.LookAt(new Vector3(transform.position.x, 0f, transform.position.z));
                    otherplayerInfo.agent.AddReward(PlayerRewards.REWARD_TACKLE);
                }

                if (playerInfo.ball.Owner == otherplayerInfo)
                {
                    otherplayerInfo.OnBallLost();
                    playerInfo.animator.SetBool(PlayerProperties.ANIM_TACKLE, true);
                    transform.LookAt(new Vector3(otherplayerInfo.transform.position.x, 0f, otherplayerInfo.transform.position.z));
                    AddReward(PlayerRewards.REWARD_GETTING_BALL);
                    otherplayerInfo.animator.SetBool(PlayerProperties.ANIM_STEP_BACK, true);
                    otherplayerInfo.agent.AddReward(PlayerRewards.REWARD_LOSSING_BALL);
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
        //if (playerInfo.ball.Owner == this)
        //{
        //    OnBallLost();
        //    //AddReward(PlayerProperties.REWARD_LOSSING_BALL);
        //}
        //AddReward(PlayerProperties.REWARD_HITTING_WALL);
        //}
    }
}
