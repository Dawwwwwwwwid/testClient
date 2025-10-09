using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ReticleController stores all reticle GameObjects and provides methods to manage them.
/// </summary>
public class ReticleController : MonoBehaviour
{
    [Header("Robot Reticle Prefab")]
    [SerializeField] private GameObject robotReticlePrefab;
    private List<GameObject> reticles;

    /// <summary>
    /// Initializes the reticles GameObject List.
    /// </summary>
    private void Awake()
    {
        reticles = new List<GameObject>();
    }

    /// <summary>
    /// Instantiates the robotReticlePrefab with a given position and lifeTime. Adds the new instance to the reticles GameObject List.
    /// </summary>
    /// <param name="position">Postion to instantiate the reticle GameObject at.</param>
    /// <param name="lifeTime">Life time to assign to the RobotReticle instance.</param>
    public void InstantiateReticle(Vector3 position, float lifeTime)
    {
        GameObject reticle = Instantiate(robotReticlePrefab, position, Quaternion.Euler(90f, 0f, 0f));
        reticle.GetComponent<RobotReticle>().InitializeReticle(lifeTime);
        reticles.Add(reticle);
    }

    /// <summary>
    /// Instantiates the robotReticlePrefab with a given position and a given targetRobot. Adds the new instance to the reticles GameObject List.
    /// </summary>
    /// <param name="position">Position to instantiate the reticle GameObject at.</param>
    /// <param name="targetRobot">TargetRobot used to determine when to destroy the new RobotReticle instance</param>
    public void InstantiateReticle(Vector3 position, Robot targetRobot)
    {
        RemovePreviousReticle(targetRobot);
        GameObject reticle = Instantiate(robotReticlePrefab, position, Quaternion.Euler(90f, 0f, 0f));
        reticle.GetComponent<RobotReticle>().InitializeReticle(targetRobot);
        reticles.Add(reticle);
    }

    /// <summary>
    /// Uses a given Robot instance and iterates over the reticles array to see if any of them are being targetted by the given Robot instance.
    /// If so, remove that reticle from the List and destroy its GameObject.
    /// </summary>
    /// <param name="targetRobot">TargetRobot to check against the reticles GameObject array</param>
    private void RemovePreviousReticle(Robot targetRobot)
    {
        // return if no reticles are currently stored
        if (reticles.Count == 0) return;

        // iterate over all reticles
        foreach(GameObject reticle in reticles.ToArray())
        {
            // check if the reticle is waiting for a certain robot
            if(reticle == null) continue;
            if (!reticle.TryGetComponent(out RobotReticle robotReticle)) continue;
            if(robotReticle.GetTargetRobot(out Robot robot))
            {
                // if this reticles robot is the same as the target robot, destroy the reticle
                if(robot != targetRobot) continue;
                reticles.Remove(reticle);
                Destroy(reticle);
                return;
            }
        }
    }
}
