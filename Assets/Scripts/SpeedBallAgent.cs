using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class SpeedBallAgent : Agent
{
    public PlayerInfo playerInfo;
    
    private float _existential;

    public override void Initialize()
    {
        playerInfo = GetComponent<PlayerInfo>();

        
        if (playerInfo.envController != null)
        {
            _existential = 1f / playerInfo.envController.MaxEnvironmentSteps;
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
        if (playerInfo.ball.owner && playerInfo.ball.owner.team != playerInfo.team)
        {
            AddReward(-_existential);
        }
        if (playerInfo.agentFuzzy) return;

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
            Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
            Quaternion newRot = Quaternion.Lerp(playerInfo.rigidBody.rotation, targetRot, PlayerProperties.TURN_SMOOTHING * Time.fixedDeltaTime);
            playerInfo.rigidBody.MoveRotation(newRot);


            if (playerInfo.ball.owner == this.playerInfo)
            {
                playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.RUN_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.fixedDeltaTime);
            }
            else
            {
                playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, PlayerProperties.SPRINT_SPEED, PlayerProperties.SPEED_DAMP_TIME, Time.fixedDeltaTime);
            }
        }
        else
        {
            playerInfo.animator.SetFloat(PlayerProperties.ANIM_MOVE, 0.0f, PlayerProperties.SPEED_DAMP_TIME, Time.fixedDeltaTime);
        }

        if (playerInfo.ball.owner == this.playerInfo && actions.DiscreteActions[2] == 1)
        {
            playerInfo.Shoot();
        }
    }
       
    public override void Heuristic(in ActionBuffers actionsOut)
    {

        var discreteActionsOut = actionsOut.DiscreteActions;

        //var horizontalAxis = Input.GetAxisRaw("Horizontal");
        var horizontalAxis = playerInfo.agentFuzzy.horizontal;
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

        //var verticalAxis = Input.GetAxisRaw("Vertical");
        var verticalAxis = playerInfo.agentFuzzy.vertical;
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

        //discreteActionsOut[2] = Input.GetKey(KeyCode.Mouse0) ? 1 : 0;
        discreteActionsOut[2] = playerInfo.isShooting ? 1 : 0;
    }
}
