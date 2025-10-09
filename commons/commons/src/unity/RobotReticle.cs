using UnityEngine;

/// <summary>
/// RobotReticle is used to control and manage the robot reticle GameObject.
/// </summary>
public class RobotReticle : MonoBehaviour
{
    private Robot selectedRobot;

    private float timeToLive = 10f;
    private float lifeTimeElapsed = 0f;

    /// <summary>
    /// Stores the given Robot instance as the selectedRobot.
    /// </summary>
    /// <param name="selectedRobot">SelectedRobot is stored as this RobotReticle instance's selectedRobot.</param>
    public void InitializeReticle(Robot selectedRobot)
    {
        this.selectedRobot = selectedRobot;
    }

    /// <summary>
    /// Stores the given lifetime float as this RobotReticle instance's time to live.
    /// </summary>
    /// <param name="lifetime">Determines how long this RobotReticle instance will live for.</param>
    public void InitializeReticle(float lifetime)
    {
        timeToLive = lifetime;
    }

    /// <summary>
    /// Sets the given Robot targetRobot and returns true if this RobotReticle instance has a selectedRobot.
    /// </summary>
    /// <param name="targetRobot">Sets the target robot to check the distance from.</param>
    /// <returns></returns>
    public bool GetTargetRobot(out Robot targetRobot)
    {
        targetRobot = selectedRobot;
        return targetRobot != null;
    }

    /// <summary>
    /// Checks the distance from the selected robot if there is one. Checks the time to live float and checks if the selected robot is active. 
    /// If the Robot is close enough, or the timer runs out or the selected robot is inactive, destory this RobotReticle instance.
    /// </summary>
    private void Update()
    {
        // get the distance from the robot and destroy the reticle if its close enough
        if(selectedRobot != null)
        {
            float distance = Vector3.Distance(transform.position, selectedRobot.transform.position);
            if (distance < 0.15f) Destroy(gameObject);
        }

        // if the robot object is not active, destroy the reticle
        if (!selectedRobot.isActiveAndEnabled) Destroy(gameObject);

        // check timer and destroy if max lifetime is reached
        if (lifeTimeElapsed > timeToLive) Destroy(gameObject);

        // increment elapsed timer
        lifeTimeElapsed += Time.deltaTime;
    }
}
