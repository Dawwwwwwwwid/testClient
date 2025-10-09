using HIVE.Commons.Flatbuffers.Generated;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Headset extends the Entity class and it represents a Headset in the system.
/// </summary>
public class Headset : Entity
{
    /// <summary>
    /// Updates the static data of the Headset to the given values.
    /// </summary>
    /// <param name="entityID">The ID of the Headset.</param>
    /// <param name="entityName">The name of the Headset.</param>
    /// <param name="subscription">The subscription of the Headset.</param>
    /// <param name="subscriptionRate">Rate of subscription.</param>
    /// <param name="boundingBox">Headset's bounding box.</param>
    public void UpdateStaticData(ulong entityID, string entityName, ushort subscription, SubscriptionRate subscriptionRate, BoundingBox? boundingBox)
    {
        // re-assign the properties
        EntityID = entityID;
        EntityName = entityName;
        Subscription = subscription;
        SubscriptionRate = subscriptionRate;
        transform.GetChild(0).name = entityName;

        // initialize the boundingBox if it has a value
        if (boundingBox != null && boundingBox.HasValue) InitializeBoundingBox(boundingBox.Value);
    }

    /// <summary>
    /// Initializes the bounding box using the given Flatbuffer bounding box.
    /// </summary>
    /// <param name="boundingBox">Flatbuffer bounding box.</param>
    public override void InitializeBoundingBox(BoundingBox boundingBox)
    {
        // get the values from the boundingBox
        Vec3 centre = boundingBox.Centre.Value;
        Vec3 dimensions = boundingBox.Dimensions.Value;

        // return if not ellipsoid, headset can only have capsule based bounding box
        if (!boundingBox.Ellipsoid) return;

        // try get capsuleCollider from first child, add if not found
        if (!transform.GetChild(0).TryGetComponent(out CapsuleCollider capsuleCollider))
        {
            capsuleCollider = transform.GetChild(0).AddComponent<CapsuleCollider>();
        }

        // set the capsuleCollider variables
        capsuleCollider.isTrigger = false;
        capsuleCollider.center = new Vector3(centre.X, centre.Y, centre.Z);
        capsuleCollider.height = dimensions.Y;
        capsuleCollider.radius = dimensions.X;
    }

    /// <summary>
    /// Toggles the visibility of the Headset visuals and toggles the collider based on the given value.
    /// </summary>
    /// <param name="active">Determines whether to toggle the visuals / collider on or off.</param>
    public override void SetVisualsActive(bool active)
    {
        base.SetVisualsActive(active);
        Transform child = transform.GetChild(0);
        if (child.TryGetComponent(out MeshRenderer meshRenderer)) meshRenderer.enabled = active;

        // check for bounding box and hide
        // bounding box can be constructed with either collider so must check for all
        if (child.TryGetComponent(out BoxCollider boxCollider)) boxCollider.enabled = active;
        if (child.TryGetComponent(out CapsuleCollider capCollider)) capCollider.enabled = active;
        if (child.TryGetComponent(out SphereCollider sphereCollider)) sphereCollider.enabled = active;
    }
}