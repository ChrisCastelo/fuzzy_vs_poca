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

public class SpeedBallFuzzyAI : MonoBehaviour
{

    public PlayerState playerState;
    public SpeedBallAgent player;
    float restingTime;
    // Start is called before the first frame update
    void Start()
    {
        restingTime = Random.Range(2.0f, 5.0f);
        playerState = PlayerState.IDLE;
        player = transform.GetComponent<SpeedBallAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (playerState)
        {
            case PlayerState.IDLE:
                player.animator.SetFloat(player.ANIM_MOVE, 0.0f, player.SPEED_DAMP_TIME, Time.fixedDeltaTime);
                restingTime -= Time.deltaTime;
                
                if (restingTime < 0 && Vector3.Distance(player.ball.transform.position, transform.position) > 5)
                {
                    playerState = PlayerState.MOVING;
                    restingTime = Random.Range(2.0f, 6.0f);
                }
                break;

            case PlayerState.MOVING:
                if (Vector3.Distance(player.ball.transform.position, transform.position) > 3)
                {
                    MoveTowards(player.ball.transform.position);
                }
                else
                {
                    playerState = PlayerState.IDLE;
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

        void MoveTowards(Vector3 target)
        {
            Vector3 targetDir = target;
            //Create rotation based on this new vector assuming that up is the global y axis.
            Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
            //Create a rotation tahat is an increment closer to our targetRot from player's rotation.
            Quaternion newRot = Quaternion.Lerp(player.agentRb.rotation, targetRot, player.TURN_SMOOTHING * Time.fixedDeltaTime);
            //Aply rotation
            player.agentRb.MoveRotation(newRot);


            if (player.ball.Owner == this)
            {
                player.animator.SetFloat(player.ANIM_MOVE, Mathf.Clamp(0.75f * 2 * 0.75f, 0f, 0.75f), player.SPEED_DAMP_TIME, Time.fixedDeltaTime);
            }
            else
            {
                player.animator.SetFloat(player.ANIM_MOVE, 2, player.SPEED_DAMP_TIME, Time.fixedDeltaTime);
            }
        }
    }
}
