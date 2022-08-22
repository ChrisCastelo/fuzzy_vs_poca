using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public PlayerInfo owner;
    [HideInInspector]
    public Rigidbody rigidBody;
    [HideInInspector]
    public Collider col;

    public SpeedBallEnvController envController;

    // Start is called before the first frame update
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        envController = GetComponentInParent<SpeedBallEnvController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (owner)
        {
            Vector3 handPos = owner.animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 handFwd = owner.transform.forward;
            transform.position = handPos + handFwd * 0.35f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == EnvironmentProperties.GOAL_TEAM1_TAG || 
            collision.gameObject.tag == EnvironmentProperties.GOAL_TEAM1_POS_TAG)
        {
            envController.GoalTouched(PlayerTeam.Team1);
            gameObject.SetActive(false);
        }
        if (collision.gameObject.tag == EnvironmentProperties.GOAL_TEAM2_TAG ||
            collision.gameObject.tag == EnvironmentProperties.GOAL_TEAM2_POS_TAG)
        {
            envController.GoalTouched(PlayerTeam.Team2);
            gameObject.SetActive(false);
        }
    }
}
