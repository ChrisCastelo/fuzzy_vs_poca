using UnityEngine;

public class BallOut : MonoBehaviour
{
    SpeedBallEnvController speedBallEnvController;
    private void Start()
    {
        speedBallEnvController = GetComponentInParent<SpeedBallEnvController>();
    }

    void ResetScene()
    {
        if (speedBallEnvController.team1AgentGroup != null)
        {
            speedBallEnvController.team1AgentGroup.GroupEpisodeInterrupted();
        }

        if (speedBallEnvController.team2AgentGroup != null)
        {
            speedBallEnvController.team2AgentGroup.GroupEpisodeInterrupted();
        }
        Debug.Log("Out of bounds, Reseting scene");
        
        Invoke("ResetSceneInSeconds", 0.5f);
    }

    public void ResetSceneInSeconds()
    {
        speedBallEnvController.ResetScene();
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Ball")
        {
            ResetScene();
        }
        if (col.gameObject.tag == "PlayerTeam1" || col.gameObject.tag == "OponentTeam")
        {
            ResetScene();
        }

    }
    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.tag == "Ball")
        {
            ResetScene();
        }
        if (col.gameObject.tag == "Team1" || col.gameObject.tag == "Team2")
        {
            ResetScene();
        }
    }
}