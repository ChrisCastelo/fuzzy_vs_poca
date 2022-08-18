using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public SpeedBallAgent Owner;
    [HideInInspector]
    public Rigidbody m_Rigidbody;
    //[HideInInspector]
    public SpeedBallEnvController envController;

    private float m_BallTouchReward;
    
    // Start is called before the first frame update
    void Start()
    {
        m_Rigidbody   = GetComponent<Rigidbody>();
        envController = GetComponentInParent<SpeedBallEnvController>();

        m_BallTouchReward = 1f / envController.MaxEnvironmentSteps;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Owner)
        {
            Vector3 handPos = Owner.animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 handFwd = Owner.transform.forward;
            transform.position = handPos + handFwd * 0.35f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "GoalTeam1")
        {
            envController.GoalTouched(Team.Team1);
            gameObject.SetActive(false);
        }
        if (collision.gameObject.tag == "GoalTeam2")
        {
            envController.GoalTouched(Team.Team2);
            gameObject.SetActive(false);
        }
    }
}
