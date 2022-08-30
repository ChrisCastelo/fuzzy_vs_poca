# Fuzzy and MA-POCA: AIs that play games
BSc Computer Science – Machine Learning and Artificial Intelligence
<br>CM3070: Computer Science Final Project
<br>By Chris Castelo

## Instructions Fuzzy Logic
Users can opt to iterate on this project and modify Fuzzy Logic behaviour by editing the rules for each existing player role:
-	[Generic Player Rules](https://github.com/ChrisCastelo/fuzzy_vs_poca/blob/main/GenericRulesDictionary.txt)
-	[Goalie Player Rules](https://github.com/ChrisCastelo/fuzzy_vs_poca/blob/main/GoalieRulesDictionary.txt)
-	[Striker Player Rules](https://github.com/ChrisCastelo/fuzzy_vs_poca/blob/main/StrikerRulesDictionary.txt)

An example of this rule is as follows:

<code>Rule 2,IF BallOwner IS None AND BallDistance IS Near THEN Speed IS Sprint</code>

This type of rule will apply logic to the Fuzzy Logic AI which will decided accordingly and output of speed.

### Supported Inputs:
<br>
<code>BallOwner IS None</code>
<code>BallOwner IS Opponent</code>
<code>BallOwner IS Team</code>
<code>BallOwner IS Me</code>
<br>
<code>BallDistance IS Near</code>
<code>BallDistance IS Medium</code>
<code>BallDistance IS Far</code>
<br>
<code>GoalDistance IS Near</code>
<code>GoalDistance IS Medium</code>
<code>GoalDistance IS Far</code>
<br>
<code>StrategyDistance IS Far</code>
<code>StrategyDistance IS Medium</code>
<code>StrategyDistance IS Near</code>
<br>
<code>TeammateAvailable IS None</code>
<code>TeammateAvailable IS Found</code>

### Supported Outputs:
<code>Rotation IS TowardsGoal</code>
<code>Rotation IS TowardsBall</code>
<code>Rotation IS TowardsFormationSpot</code>
<br>
<code>Speed IS Idle</code>
<code>Speed IS Walk</code>
<code>Speed IS Jog</code>
<code>Speed IS Run</code>
<code>Speed IS Sprint</code>
<br>
<code>ShootOrPass IS Pass</code>
<code>ShootOrPass IS Shoot</code>
<code>ShootOrPass IS DoNotShoot</code>

### Supported Operators:
<code>IF</code>
<code>THEN</code>
<code>AND</code>
<code>OR</code>
<code>IS</code>


## Instructions MA-POCA

Users can also iterate in our neuro network based AI MA-POCA by modifying the hyperparameters of it:[Hyperparameters](https://github.com/ChrisCastelo/fuzzy_vs_poca/blob/main/config/SpeedBall.yaml)

In order to train a new brain or resume training we first need to setup our virtual environment by following these steps:

-Python version used: 3.79 https://www.python.org/downloads/release/python-379/
-Navigate to project root (Unity project root) e.g.: <code> cd C:\fuzzy_vs_poca\ </code>

Then create a virtual environment:

1.	Create a new environment with <code>python -m venv venv</code>
2.	Activate the environment with <code>venv\Scripts\activate</code>
3.	Upgrade to the latest pip version using <code>pip install --upgrade pip</code>
4.	Install pytorch 
<code>pip install torch==1.7.0 -f https://download.pytorch.org/whl/torch_stable.html</code>
5.	Now install ML-Agents with <code>pip install mlagents</code>
6.	To verify everything installed correctly type the help command of mlagents, that is <code>mlagents-learn –help</code>. Now we are ready to resume training.

To resume training or restart a new one type the following commands:

<code>mlagents-learn config/SpeedBall_ImitationLearning.yaml --run-id=RunIdExample --resume</code> 
<br>
<code>mlagents-learn config/SpeedBall.yaml --run-id=RunIdExample</code>

### Evaluation - Viewing the results
We can see the tensorboard of the results of the training by typing:
<code>tensorboard --logdir results --port 6006</code> 
and then going to <code>http://localhost:6006/</code>
