using Unity.MLAgents;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum AIType
{
    FUZZY = 0,
    POCA = 1,
    PPO = 2
}
public class SpeedBallEnvController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public SpeedBallAgent Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
        [HideInInspector]
        public Animator animator;
    }

    public AIType trainerType;
    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    /// <summary>
    /// The area bounds.
    /// </summary>

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>

    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;
    
    Vector3 m_BallStartingPos;
    int m_ResetTimer = 0;
    //List of Agents On Platform
    public List<PlayerInfo> agentsList = new List<PlayerInfo>();
    
    public SimpleMultiAgentGroup team1AgentGroup;
    public SimpleMultiAgentGroup team2AgentGroup;
    public GameObject vfxGoal;
    [HideInInspector]
    public List<SpeedBallAgent> team1 = new List<SpeedBallAgent>();
    [HideInInspector]
    public List<SpeedBallAgent> team2 = new List<SpeedBallAgent>();
    public TextMeshProUGUI stepsCountText;

    public TextMeshPro team1GoalCountText;
    public TextMeshPro team2GoalCountText;
    public int team1GoalCount = 0;
    public int team2GoalCount = 0;

    void Start()
    {
        // Initialize TeamManager
        if (trainerType == AIType.POCA)
        {
            team1AgentGroup = new SimpleMultiAgentGroup();
            team2AgentGroup = new SimpleMultiAgentGroup();
        }

        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);

        foreach (var item in agentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.agentRb;
            item.animator = item.Agent.animator;
            if (trainerType == AIType.POCA)
            {
                if (item.Agent.team == Team.Team1)
                {
                    team1AgentGroup.RegisterAgent(item.Agent);
                }
                else
                {
                    team2AgentGroup.RegisterAgent(item.Agent);
                }
            }
            else if (trainerType == AIType.PPO)
            {
                if (item.Agent.team == Team.Team1)
                {
                    team1.Add(item.Agent);
                }
                else
                {
                    team2.Add(item.Agent);
                }
            }
            else 
            {
                item.Agent.gameObject.AddComponent<SpeedBallFuzzyAI>();
            }
        }
        ResetScene();
    }

    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            if (trainerType == AIType.POCA)
            {
                team1AgentGroup.GroupEpisodeInterrupted();
                team2AgentGroup.GroupEpisodeInterrupted();
            }
            else
            {
                InterruptTeamEpisode(team1);
                InterruptTeamEpisode(team2);
            }
            ResetScene();
        }
        if (stepsCountText)
        {
            stepsCountText.SetText(m_ResetTimer.ToString());
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

    private SpeedBallAgent[] FindTeam(string tag)
    {
        GameObject[] _teamgo = GameObject.FindGameObjectsWithTag(tag);
        SpeedBallAgent[] _team = new SpeedBallAgent[_teamgo.Length];
        for (int i = 0; i < _teamgo.Length; ++i)
        {
            _team[i] = _teamgo[i].GetComponent<SpeedBallAgent>();
        }
        return _team;
    }

    public void ResetBall()
    {
        var randomPosX = Random.Range(-5f, 5f);

        ball.GetComponent<Ball>().Owner = null;
        ball.transform.position = m_BallStartingPos + new Vector3(randomPosX, 0f, 0f);
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ball.SetActive(true);
    }

    public void GoalTouched(Team scoredTeam)
    {
        Instantiate(vfxGoal, ball.transform.position, Quaternion.identity);
        if (scoredTeam == Team.Team1)
        {
            team1GoalCount += 1;
            if (trainerType == AIType.FUZZY)
            {
                AddTeamReward(team1, 1f);
                AddTeamReward(team2, -1f);
                //FinishTeamEpisode(team1);
                //FinishTeamEpisode(team2);
            }
            else if (trainerType == AIType.PPO)
            {
                team1AgentGroup.SetGroupReward(1f);
                team2AgentGroup.SetGroupReward(-1f);
                //team2AgentGroup.EndGroupEpisode();
                //team1AgentGroup.EndGroupEpisode();
            }
        }
        else
        {
            team2GoalCount += 1;
            if (trainerType == AIType.FUZZY)
            {
                AddTeamReward(team1, -1f);
                AddTeamReward(team2, 1f);
                //FinishTeamEpisode(team1);
                //FinishTeamEpisode(team2);
            }
            else if (trainerType == AIType.PPO)
            {
                team1AgentGroup.SetGroupReward(-1f);
                team2AgentGroup.SetGroupReward(1f);
                //team2AgentGroup.EndGroupEpisode();
                //team1AgentGroup.EndGroupEpisode();
            }
        }
        Invoke("ResetAgents", 0.5f);
        //ResetScene();
    }
    public void AddTeamReward(List<SpeedBallAgent> team, float qt)
    {
        foreach (var agent in team)
        {
            agent.AddReward(qt);
            
        }
    }
    public void SetTeamReward(List<SpeedBallAgent> team, float qt)
    {
        foreach (var agent in team)
        {
            agent.SetReward(qt);
        }
    }

    public void FinishTeamEpisode(List<SpeedBallAgent> team)
    {
        foreach (var agent in team)
        {
            agent.EndEpisode();
        }
    }

    public void InterruptTeamEpisode(List<SpeedBallAgent> team)
    {
        foreach (var agent in team)
        {
            agent.EpisodeInterrupted();
        }
    }

    private void ResetAgents()
    {
        //Reset Agents
        foreach (var item in agentsList)
        {
            var randomPosX = Random.Range(-1f, 1f);
            var newStartPos = item.Agent.initialPos + new Vector3(randomPosX, 0f, 0f);
            var rot = item.Agent.rotSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, item.StartingRot);

            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
            item.Agent.ResetAnimator();
        }

        //Reset Ball
        ResetBall();
    }
    public void ResetScene()
    {
        //Reset Timer
        m_ResetTimer = 0;
        team1GoalCount = 0;
        team2GoalCount = 0;
        //Reset Agents
        ResetAgents();
    }
}
