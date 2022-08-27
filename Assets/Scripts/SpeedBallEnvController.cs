using Unity.MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedBallEnvController : MonoBehaviour
{
    public AIType trainerType;
    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;
    
    //List of Agents On Platform
    public List<PlayerInfo> agentsList = new List<PlayerInfo>();
    public SimpleMultiAgentGroup team1AgentGroup;
    public SimpleMultiAgentGroup team2AgentGroup;
    public GameObject vfxGoal;
    public TextMeshProUGUI stepsCountText;
    public TextMeshPro team1GoalCountText;
    public TextMeshPro team2GoalCountText;
    public int team1GoalCount = 0;
    public int team2GoalCount = 0;
    public Ball ball;

    private Vector3 _ballStartingPos;
    private int _resetTimer = 0;
    private List<SpeedBallAgent> _team1 = new List<SpeedBallAgent>();
    private List<SpeedBallAgent> _team2 = new List<SpeedBallAgent>();

    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
        _ballStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);

        foreach (var item in agentsList)
        {
                if (item.team == PlayerTeam.Team1)
                {
                    if (item.AI == AIType.POCA)
                    {
                        if (team1AgentGroup == null) team1AgentGroup = new SimpleMultiAgentGroup();
                        team1AgentGroup.RegisterAgent(item.agent);
                    }
                    _team1.Add(item.agent);
                }
                else
                {
                    if (item.AI == AIType.POCA)
                    {
                        if (team2AgentGroup == null) team2AgentGroup = new SimpleMultiAgentGroup();
                        team2AgentGroup.RegisterAgent(item.agent);
                    }
                    _team2.Add(item.agent);
                }
        }
        ResetScene();
    }
    private void FixedUpdate()
    {

        _resetTimer += 1;
        if (_resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            if (trainerType == AIType.POCA)
            {
                if (team1AgentGroup != null) team1AgentGroup.GroupEpisodeInterrupted();
                if (team2AgentGroup != null) team2AgentGroup.GroupEpisodeInterrupted();
            }

            ResetScene();
        }

        if (stepsCountText)
        {
            stepsCountText.SetText(_resetTimer.ToString());
        }
        if (team1GoalCountText)
        {
            team1GoalCountText.SetText(team1GoalCount.ToString());
        }
        if (team2GoalCountText)
        {
            team2GoalCountText.SetText(team2GoalCount.ToString());
        }
    }

    public void ResetBall()
    {
        var randomPosX = Random.Range(-5f, 5f);

        ball.owner = null;
        ball.transform.position = _ballStartingPos + new Vector3(randomPosX, 0f, 0f);
        ball.rigidBody.velocity = Vector3.zero;
        ball.rigidBody.angularVelocity = Vector3.zero;
        ball.gameObject.SetActive(true);

    }

    public void GoalTouched(PlayerTeam scoredTeam)
    {
        Instantiate(vfxGoal, ball.transform.position, Quaternion.identity);
        if (scoredTeam == PlayerTeam.Team1)
        {
            team1GoalCount += 1;
            if (trainerType == AIType.POCA)
            {
                if (team1AgentGroup != null)
                {
                    team1AgentGroup.SetGroupReward(1f);
                    team1AgentGroup.EndGroupEpisode();
                }
                if (team2AgentGroup != null)
                {
                    team2AgentGroup.SetGroupReward(-1f);
                    team2AgentGroup.EndGroupEpisode();
                }
            }
        }
        else
        {
            team2GoalCount += 1;
            if (trainerType == AIType.POCA)
            {
                if (team1AgentGroup != null)
                {
                    team1AgentGroup.SetGroupReward(-1f);
                    team1AgentGroup.EndGroupEpisode();
                }
                if (team2AgentGroup != null)
                {
                    team2AgentGroup.SetGroupReward(1f);
                    team2AgentGroup.EndGroupEpisode();
                }
            }
            
        }
        //Invoke("ResetAgents", 0.5f);
        Invoke("ResetScene", 0.5f);
        
    }

    public void ResetAgents()
    {
        //Reset Agents
        foreach (var item in agentsList)
        {
            var randomPosX = Random.Range(-1f, 1f);
            var newStartPos = item.initialPos + new Vector3(randomPosX, 0f, 0f);
            item.transform.SetPositionAndRotation(newStartPos, item.initialRot);

            item.rigidBody.velocity = Vector3.zero;
            item.rigidBody.angularVelocity = Vector3.zero;
            item.ResetAnimator();
        }

        //Reset Ball
        ResetBall();
    }
    public void ResetScene()
    {
        //Reset Timer
        _resetTimer = 0;
        team1GoalCount = 0;
        team2GoalCount = 0;
        //Reset Agents
        ResetAgents();

    }
}
