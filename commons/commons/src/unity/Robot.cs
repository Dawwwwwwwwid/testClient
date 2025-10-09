using HIVE.Commons.Flatbuffers.Generated;
using System;
using UnityEngine;

/// <summary>
/// Robot extends the Entity class and represents a Robot in the system.
/// </summary>
public class Robot : Entity
{
    public event Action<bool> OnRobotVisualsToggle;

    private uint colour;
    private readonly string robotModelName = "Robot Model";

    /// <summary>
    /// Updates the static data of the Robot to the given values.
    /// </summary>
    /// <param name="entityID">The ID of the Robot.</param>
    /// <param name="entityName">The name of the Robot.</param>
    /// <param name="subscription">The subscription of the Robot.</param>
    /// <param name="subscriptionRate">Rate of subscription.</param>
    /// <param name="boundingBox">Robot's bounding box.</param>
    /// <param name="colour">Colour value assigned to this Robot instance.</param>
    public void UpdateStaticData(ulong entityID, string entityName, ushort subscription, SubscriptionRate subscriptionRate, BoundingBox? boundingBox, uint colour)
    {
        // re-assign the properties
        EntityID = entityID;
        EntityName = entityName;
        Subscription = subscription;
        SubscriptionRate = subscriptionRate;
        this.colour = colour;

        // initialize the bounding box if it has a value
        if (boundingBox != null && boundingBox.HasValue) InitializeBoundingBox(boundingBox.Value);
    }

    /// <summary>
    /// Toggles the visibility of the Robot visuals and toggles the collider based on the given value.
    /// Invokes the OnRobotVisualsToggle event.
    /// </summary>
    /// <param name="active">Determines whether to toggle the visuals / collider on or off.</param>
    public override void SetVisualsActive(bool active)
    {
        // call the base method
        base.SetVisualsActive(active);

        // check for colliders and toggle them
        if (TryGetComponent(out BoxCollider boxCollider)) boxCollider.enabled = active;
        if (TryGetComponent(out CapsuleCollider capCollider)) capCollider.enabled = active;
        if (TryGetComponent(out SphereCollider sphereCollider)) sphereCollider.enabled = active;

        // check if there is a robot model and set active
        if (transform.GetChild(0).name == robotModelName) transform.GetChild(0).gameObject.SetActive(active);

        // invoke the visuals toggle event
        OnRobotVisualsToggle?.Invoke(active);
    }
}