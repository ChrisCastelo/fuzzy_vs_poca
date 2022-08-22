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
        if (!playerInfo.ball.owner || playerInfo.ball.owner.team != playerInfo.team)
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


            if (playerInfo.ball.owner == this.playerInfo)
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

        if (playerInfo.ball.owner == this.playerInfo && actions.DiscreteActions[2] == 1)
        {
            playerInfo.ball.owner = null;
            playerInfo.ball.rigidBody.velocity = new Vector3(transform.forward.x * PlayerProperties.SHOOTING_FORCE, 0.5f, transform.forward.z * PlayerProperties.SHOOTING_FORCE);
            playerInfo.animator.SetBool(PlayerProperties.ANIM_SHOT, true);
            AddReward(PlayerRewards.REWARD_SHOOTING);
        }
    }
       
    public override void Heuristic(in ActionBuffers actionsOut)
    {
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
}
