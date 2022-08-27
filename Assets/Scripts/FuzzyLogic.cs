using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AForge.Fuzzy;
//using AI4Unity.Fuzzy;

public class FuzzyLogic : MonoBehaviour
{
    private void Start()
    {
        //Fuzzy Database --------------
        //Database fuzzyDB = FuzzyDataBase();
        //Fuzzy Database
        ///////////////////////////////////////////////////////////

        //Inference engine --------------
        //AForge.Fuzzy.InferenceSystem IS = new AForge.Fuzzy.InferenceSystem(fuzzyDB, new CentroidDefuzzifier(1000));
        //Inference engine --------------
        ///////////////////////////////////////////////////////////

        //Fuzzy Rules Dictionary --------------
        //FuzzyRulesDictionary(IS);
        //Fuzzy Rules Dictionary --------------
        ///////////////////////////////////////////////////////////
        
        //// setting inputs
        //IS.SetInput("BallOwner", 2f);
        //IS.SetInput("BallDistance", 1.2f);
        //IS.SetInput("BallOwner", 4.0f);
        //// getting outputs
        //float newSpeed = IS.Evaluate("Speed");
        //float newRotation = IS.Evaluate("Rotation");
        //float shootPass = IS.Evaluate("ShootOrPass");


        //Debug.Log("Speed: "+ newSpeed);
        //Debug.Log("Rotation: " + newRotation);
        //Debug.Log("ShootOrPass: " + shootPass);

    }

    public static Database FuzzyDataBase()
    {
        //Distance Ball----------------------
        FuzzySet ballNear = new FuzzySet("Near", new TrapezoidalFunction(0.0f, PlayerProperties.MIN_DISTANCE_BALL_THRESHOLD, TrapezoidalFunction.EdgeType.Right));
        FuzzySet ballMedium = new FuzzySet("Medium", new TrapezoidalFunction(0.0f, PlayerProperties.MIN_DISTANCE_BALL_THRESHOLD, PlayerProperties.MIN_DISTANCE_BALL_THRESHOLD, PlayerProperties.MAX_DISTANCE_BALL_THRESHOLD));
        FuzzySet ballFar = new FuzzySet("Far", new TrapezoidalFunction(PlayerProperties.MAX_DISTANCE_BALL_THRESHOLD, EnvironmentProperties.FIELD_SIZE, TrapezoidalFunction.EdgeType.Left));

        LinguisticVariable ballDistance = new LinguisticVariable("BallDistance", 0.0f, EnvironmentProperties.FIELD_SIZE);
        ballDistance.AddLabel(ballNear);
        ballDistance.AddLabel(ballMedium);
        ballDistance.AddLabel(ballFar);
        //Distance Ball----------------------
        ///////////////////////////////////////////////////////////

        //Distance Formation Postion---------
        FuzzySet formationPositionNear = new FuzzySet("Near", new TrapezoidalFunction(0.0f, PlayerProperties.MIN_DISTANCE_FORMATION_POSITION, TrapezoidalFunction.EdgeType.Right));
        FuzzySet formationPositionMedium = new FuzzySet("Medium", new TrapezoidalFunction(0.0f, PlayerProperties.MIN_DISTANCE_FORMATION_POSITION, PlayerProperties.MIN_DISTANCE_FORMATION_POSITION, PlayerProperties.MAX_DISTANCE_FORMATION_POSITION));
        FuzzySet formationPositionFar = new FuzzySet("Far", new TrapezoidalFunction(PlayerProperties.MAX_DISTANCE_FORMATION_POSITION, EnvironmentProperties.FIELD_SIZE, TrapezoidalFunction.EdgeType.Left));

        LinguisticVariable formationPositionDistance = new LinguisticVariable("StrategyDistance", 0.0f, EnvironmentProperties.FIELD_SIZE);
        formationPositionDistance.AddLabel(formationPositionNear);
        formationPositionDistance.AddLabel(formationPositionMedium);
        formationPositionDistance.AddLabel(formationPositionFar);
        //Distance Formation Postion---------
        ///////////////////////////////////////////////////////////

        //Distance Goal ----------------------
        FuzzySet goalPositionNear = new FuzzySet("Near", new TrapezoidalFunction(0.0f, PlayerProperties.MIN_DISTANCE_FORMATION_POSITION, TrapezoidalFunction.EdgeType.Right));
        FuzzySet goalPositionMedium = new FuzzySet("Medium", new TrapezoidalFunction(0.0f, PlayerProperties.MIN_DISTANCE_FORMATION_POSITION, PlayerProperties.MIN_DISTANCE_FORMATION_POSITION, PlayerProperties.MAX_DISTANCE_FORMATION_POSITION));
        FuzzySet goalPositionFar = new FuzzySet("Far", new TrapezoidalFunction(PlayerProperties.MAX_DISTANCE_FORMATION_POSITION, EnvironmentProperties.FIELD_SIZE, TrapezoidalFunction.EdgeType.Left));

        LinguisticVariable goalDistance = new LinguisticVariable("GoalDistance", 0.0f, EnvironmentProperties.FIELD_SIZE);
        goalDistance.AddLabel(goalPositionNear);
        goalDistance.AddLabel(goalPositionMedium);
        goalDistance.AddLabel(goalPositionFar);
        //Distance Goal ----------------------
        ///////////////////////////////////////////////////////////

        //Ball Owner -------------------------
        FuzzySet noOwner = new FuzzySet("None", new TrapezoidalFunction((float)BallOwner.None, (float)BallOwner.None + 0.1f, TrapezoidalFunction.EdgeType.Right));
        FuzzySet opponentOwner = new FuzzySet("Opponent", new TrapezoidalFunction((float)BallOwner.Opponent, (float)BallOwner.Opponent, (float)BallOwner.Opponent, (float)BallOwner.Opponent + 0.1f));
        FuzzySet teamOwner = new FuzzySet("Team", new TrapezoidalFunction((float)BallOwner.Team, (float)BallOwner.Team, (float)BallOwner.Team, (float)BallOwner.Team + 0.1f));
        FuzzySet meOwner = new FuzzySet("Me", new TrapezoidalFunction((float)BallOwner.Me, (float)BallOwner.Me, (float)BallOwner.Me, (float)BallOwner.Me + 0.1f));

        LinguisticVariable ballOwner = new LinguisticVariable("BallOwner", (float)BallOwner.None, (float)BallOwner.Me + 0.1f);
        ballOwner.AddLabel(noOwner);
        ballOwner.AddLabel(opponentOwner);
        ballOwner.AddLabel(teamOwner);
        ballOwner.AddLabel(meOwner);
        //Ball Owner -------------------------
        ///////////////////////////////////////////////////////////


        //TeammateAvailable -------------------------
        FuzzySet bestCandidate = new FuzzySet("Found", new TrapezoidalFunction((float)TeammateAvailable.Found, (float)TeammateAvailable.Found + 0.1f, TrapezoidalFunction.EdgeType.Right));
        FuzzySet noCandidate = new FuzzySet("None", new TrapezoidalFunction((float)TeammateAvailable.None, (float)TeammateAvailable.None, (float)TeammateAvailable.None, (float)TeammateAvailable.None + 0.1f));

        LinguisticVariable teammateAvailable = new LinguisticVariable("TeammateAvailable", (float)TeammateAvailable.Found, (float)TeammateAvailable.None + 0.1f);
        teammateAvailable.AddLabel(bestCandidate);
        teammateAvailable.AddLabel(noCandidate);
        //TeammateAvailable -------------------------
        ///////////////////////////////////////////////////////////


        //Ball ShootOrPass -------------------------
        FuzzySet shoot = new FuzzySet("Shoot", new TrapezoidalFunction((float)ShootOrPass.Shoot, (float)ShootOrPass.Shoot + 0.1f, TrapezoidalFunction.EdgeType.Right));
        FuzzySet doNothing = new FuzzySet("DoNotShoot", new TrapezoidalFunction((float)ShootOrPass.DoNotShoot, (float)ShootOrPass.DoNotShoot, (float)ShootOrPass.DoNotShoot, (float)ShootOrPass.DoNotShoot + 0.1f));
        FuzzySet pass = new FuzzySet("Pass", new TrapezoidalFunction((float)ShootOrPass.Pass, (float)ShootOrPass.Pass, (float)ShootOrPass.Pass, (float)ShootOrPass.Pass + 0.1f));

        LinguisticVariable shootOrPass = new LinguisticVariable("ShootOrPass", (float)ShootOrPass.Shoot, (float)ShootOrPass.Pass + 0.1f);
        shootOrPass.AddLabel(shoot);
        shootOrPass.AddLabel(doNothing);
        shootOrPass.AddLabel(pass);
        //Ball ShootOrPass -------------------------
        ///////////////////////////////////////////////////////////

        //Speed ------------------------------
        FuzzySet idleSpeed = new FuzzySet("Idle", new TrapezoidalFunction(PlayerProperties.IDLE_SPEED + 0.01f, 0.03f, TrapezoidalFunction.EdgeType.Right));
        FuzzySet walkSpeed = new FuzzySet("Walk", new TrapezoidalFunction(PlayerProperties.IDLE_SPEED + 0.01f, 0.03f, 0.15f, PlayerProperties.WALK_SPEED));
        FuzzySet jogSpeed = new FuzzySet("Jog", new TrapezoidalFunction(0.15f, PlayerProperties.WALK_SPEED, PlayerProperties.JOG_SPEED - 0.1f, PlayerProperties.JOG_SPEED));
        FuzzySet runSpeed = new FuzzySet("Run", new TrapezoidalFunction(PlayerProperties.JOG_SPEED - 0.1f, PlayerProperties.JOG_SPEED, PlayerProperties.RUN_SPEED - 0.25f, PlayerProperties.RUN_SPEED));
        FuzzySet sprintSpeed = new FuzzySet("Sprint", new TrapezoidalFunction(PlayerProperties.RUN_SPEED + 0.5f, PlayerProperties.SPRINT_SPEED, TrapezoidalFunction.EdgeType.Left));

        LinguisticVariable playerSpeed = new LinguisticVariable("Speed", PlayerProperties.IDLE_SPEED, PlayerProperties.SPRINT_SPEED);
        playerSpeed.AddLabel(idleSpeed);
        playerSpeed.AddLabel(walkSpeed);
        playerSpeed.AddLabel(jogSpeed);
        playerSpeed.AddLabel(runSpeed);
        playerSpeed.AddLabel(sprintSpeed);
        //Speed -----------------------------
        ///////////////////////////////////////////////////////////

        //Rotate -------------------------
        FuzzySet towardsBall = new FuzzySet("TowardsBall", new TrapezoidalFunction((float)RotationObjective.Ball, (float)RotationObjective.Ball + 0.1f, TrapezoidalFunction.EdgeType.Right));
        FuzzySet towardsFormationSpot = new FuzzySet("TowardsFormationSpot", new TrapezoidalFunction((float)RotationObjective.FormationSpot, (float)RotationObjective.FormationSpot, (float)RotationObjective.FormationSpot, (float)RotationObjective.FormationSpot + 0.1f));
        FuzzySet towardsGoal = new FuzzySet("TowardsGoal", new TrapezoidalFunction((float)RotationObjective.Goal, (float)RotationObjective.Goal, (float)RotationObjective.Goal, (float)RotationObjective.Goal + 0.1f));
        FuzzySet towardsTeammate = new FuzzySet("TowardsTeammate", new TrapezoidalFunction((float)RotationObjective.Teammate, (float)RotationObjective.Teammate, (float)RotationObjective.Teammate, (float)RotationObjective.Teammate + 0.1f));

        LinguisticVariable playerRotation = new LinguisticVariable("Rotation", (float)RotationObjective.Ball, (float)RotationObjective.Teammate + 0.1f);
        playerRotation.AddLabel(towardsBall);
        playerRotation.AddLabel(towardsFormationSpot);
        playerRotation.AddLabel(towardsGoal);
        playerRotation.AddLabel(towardsTeammate);
        //Rotate -------------------------
        ///////////////////////////////////////////////////////////

        //Database ----------------------
        Database fuzzyDB = new Database();
        fuzzyDB.AddVariable(ballDistance);
        fuzzyDB.AddVariable(formationPositionDistance);
        fuzzyDB.AddVariable(goalDistance);
        fuzzyDB.AddVariable(ballOwner);
        fuzzyDB.AddVariable(shootOrPass);
        fuzzyDB.AddVariable(playerSpeed);
        fuzzyDB.AddVariable(playerRotation);
        fuzzyDB.AddVariable(teammateAvailable);
        //Database ----------------------
        ///////////////////////////////////////////////////////////
        return fuzzyDB;
    }

    // the rules (knowledge base) --------------
    public static void FuzzyRulesDictionary(AForge.Fuzzy.InferenceSystem IS)
    {
        // Casses BallOwner is None ---------
        IS.NewRule("Rule 1", "IF BallOwner IS None AND BallDistance IS Near THEN Rotation IS TowardsBall");
        IS.NewRule("Rule 2", "IF BallOwner IS None AND BallDistance IS Near THEN Speed IS Sprint");
        IS.NewRule("Rule 3", "IF BallOwner IS None AND BallDistance IS Medium THEN Rotation IS TowardsBall");
        IS.NewRule("Rule 4", "IF BallOwner IS None AND BallDistance IS Medium THEN Speed IS Run");
        IS.NewRule("Rule 5", "IF BallOwner IS None AND BallDistance IS Far THEN Speed IS Idle");
        IS.NewRule("Rule 6", "IF BallOwner IS None AND BallDistance IS Far THEN Rotation IS TowardsBall");
        // Casses BallOwner is None ---------

        // Casses BallOwner is Opponent ---------
        IS.NewRule("Rule 7", "IF BallOwner IS Opponent AND BallDistance IS Near THEN Rotation IS TowardsBall");
        IS.NewRule("Rule 8", "IF BallOwner IS Opponent AND BallDistance IS Near THEN Speed IS Sprint");
        IS.NewRule("Rule 9", "IF BallOwner IS Opponent AND BallDistance IS Medium THEN Rotation IS TowardsBall");
        IS.NewRule("Rule 10", "IF BallOwner IS Opponent AND BallDistance IS Medium THEN Speed IS Run");
        IS.NewRule("Rule 11", "IF BallOwner IS Opponent AND BallDistance IS Far THEN Speed IS Idle");
        IS.NewRule("Rule 12", "IF BallOwner IS Opponent AND BallDistance IS Far THEN Rotation IS TowardsBall");
        // Casses BallOwner is Opponent ---------

        // Casses BallOwner is Team ---------
        IS.NewRule("Rule 13", "IF BallOwner IS Team THEN Rotation IS TowardsGoal");
        IS.NewRule("Rule 14", "IF BallOwner IS Team AND GoalDistance IS Far THEN Speed IS Jog");
        IS.NewRule("Rule 15", "IF BallOwner IS Team AND GoalDistance IS Medium THEN Speed IS Run");
        IS.NewRule("Rule 16", "IF BallOwner IS Team AND GoalDistance IS Near THEN Speed IS Sprint");
        // Casses BallOwner is Team ---------

        // Casses Strategic Postion ---------
        IS.NewRule("Rule 17", "IF StrategyDistance IS Far THEN Rotation IS TowardsFormationSpot");
        IS.NewRule("Rule 18", "IF StrategyDistance IS Far THEN Speed IS Sprint");
        // Casses Strategic Postion ---------

        // Casses BallOwner is self ---------
        IS.NewRule("Rule 19", "IF BallOwner IS Me THEN Rotation IS TowardsGoal");
        IS.NewRule("Rule 20", "IF BallOwner IS Me THEN Speed IS Run");
        IS.NewRule("Rule 21", "IF BallOwner IS Me AND GoalDistance is Near THEN ShootOrPass IS Shoot");
        IS.NewRule("Rule 22", "IF BallOwner IS Me AND GoalDistance is Medium THEN Rotation IS TowardsGoal");
        IS.NewRule("Rule 23", "IF BallOwner IS Me AND GoalDistance is Medium THEN ShootOrPass IS Shoot");
        IS.NewRule("Rule 24", "IF BallOwner IS Me AND GoalDistance is Far THEN Rotation IS TowardsGoal");
        IS.NewRule("Rule 25", "IF BallOwner IS Me AND GoalDistance is Far THEN ShootOrPass IS DoNotShoot");
        IS.NewRule("Rule 26", "IF BallOwner IS None THEN ShootOrPass IS DoNotShoot");
        IS.NewRule("Rule 27", "IF BallOwner IS Opponent THEN ShootOrPass IS DoNotShoot");
        IS.NewRule("Rule 28", "IF BallOwner IS Team THEN ShootOrPass IS DoNotShoot");
        IS.NewRule("Rule 29", "IF BallOwner IS Team AND BallDistance IS Near THEN Rotation IS TowardsGoal");
        IS.NewRule("Rule 30", "IF BallOwner IS Team AND BallDistance IS Near THEN Speed IS Sprint");
        IS.NewRule("Rule 31", "IF BallOwner IS Me AND TeammateAvailable IS Found THEN Rotation IS TowardsTeammate");
        IS.NewRule("Rule 32", "IF BallOwner IS Me AND TeammateAvailable IS Found THEN ShootOrPass IS Pass");
        IS.NewRule("Rule 33", "IF BallOwner IS Me AND TeammateAvailable IS None THEN Rotation IS TowardsGoal");
        IS.NewRule("Rule 34", "IF BallOwner IS Me AND TeammateAvailable IS None THEN ShootOrPass IS DoNotShoot");
        // Casses BallOwner is self ---------
    }

}
