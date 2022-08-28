using UnityEngine;

public class BallOut : MonoBehaviour
{
    SpeedBallEnvController speedBallEnvController;
    private bool _resetCompleted = false;
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
        _resetCompleted = false;
    }

    void OnTriggerEnter(Collider col)
    {
        if (_resetCompleted) return;
        if (col.gameObject.tag == "Ball")
        {
            ResetScene();
        }
        if (col.gameObject.tag == "PlayerTeam1" || col.gameObject.tag == "OponentTeam")
        {
            ResetScene();
        }
        _resetCompleted = true;
    }
    void OnTriggerStay(Collider col)
    {
        if (_resetCompleted) return;
        
        if (col.gameObject.tag == "Ball")
        {
            ResetScene();
        }
        if (col.gameObject.tag == "Team1" || col.gameObject.tag == "Team2")
        {
            ResetScene();
        }
        _resetCompleted = true;
    }
}