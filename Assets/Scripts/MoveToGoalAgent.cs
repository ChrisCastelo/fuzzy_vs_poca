using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    [SerializeField] private Transform fakeCam;

    private Animator  m_Animator;
    private Rigidbody m_Rigidbody;

    public override void OnEpisodeBegin()
    {
        //base.OnEpisodeBegin();

        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();

        transform.localPosition = new Vector3(Random.Range(-3.5f,3.5f),0, Random.Range(-3.5f,3.5f));
        targetTransform.localPosition = new Vector3(Random.Range(-3.5f, 3.5f), 0.5f, Random.Range(-3.5f, 3.5f));
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        //base.CollectObservations(sensor);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        //base.OnActionReceived(actions);
        float Horizontal = actions.ContinuousActions[0];
        float Vertical = actions.ContinuousActions[1];
        
        float speedDampTime = 0.2f;
        float turnSmoothing = 7f;

        if (Horizontal != 0f || Vertical != 0f)
        {
            //Create a new vector based on camera forward
            Vector3 cameraFwd = fakeCam.transform.forward;
            //kill y and normalize it
            cameraFwd.y = 0;
            cameraFwd.Normalize();
            cameraFwd *= Vertical;
            //Get Camera's right
            Vector3 cameraR = fakeCam.transform.right;
            cameraR.y = 0;
            cameraR.Normalize();
            cameraR *= Horizontal;

            Vector3 targetDir = cameraFwd + cameraR;
            //Create rotation based on this new vector assuming that up is the global y axis.
            Quaternion targetRot = Quaternion.LookRotation(targetDir, Vector3.up);
            //Create a rotation tahat is an increment closer to our targetRot from player's rotation.
            Quaternion newRot = Quaternion.Lerp(m_Rigidbody.rotation, targetRot, turnSmoothing * Time.fixedDeltaTime);
            //Aply rotation
            m_Rigidbody.MoveRotation(newRot);
            m_Animator.SetFloat("Speed", Horizontal * Horizontal + Vertical * Vertical, speedDampTime, Time.fixedDeltaTime);
        }
        else
        {
            m_Animator.SetFloat("Speed", 0.0f, speedDampTime, Time.fixedDeltaTime);
        }

        //transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed ;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //base.Heuristic(actionsOut);
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(+1f);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
        if (collision.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }
}
