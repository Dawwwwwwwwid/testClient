using UnityEngine;
using HIVE.Commons.Flatbuffers.Generated;
using Unity.VisualScripting;
using HIVE.Commons.Flatbuffers;

/// <summary>
/// Entity is an abstract class that represents an Entity within the system.
/// </summary>
public abstract class Entity : MonoBehaviour
{
    /// <summary>
    /// Property for unique Entity ID.
    /// </summary>
    public ulong EntityID { get; protected set; }
    /// <summary>
    /// Property for name of Entity
    /// </summary>
    public string EntityName { get; protected set; }
    /// <summary>
    /// Property for Entity subscription(s)
    /// </summary>
    public ushort Subscription { get; protected set; }
    /// <summary>
    /// Property for Entity subscription rate.
    /// </summary>
    public SubscriptionRate SubscriptionRate { get; protected set; }
    /// <summary>
    /// Property for alt tracking quality.
    /// </summary>
    public float TrackingQuality { get; protected set; }
    /// <summary>
    /// Property for velocity.
    /// </summary>
    public Vector3 Velocity { get; protected set; }
    /// <summary>
    /// Property indicating the last time the Entity was updated.
    /// </summary>
    public float TimeSinceLastUpdate { get; protected set; }
    /// <summary>
    /// Property indicating if the visuals are active.
    /// </summary>
    public bool IsVisualsActive { get; protected set; } = true;

    /// <summary>
    /// Sets the EntityID property to the given ID.
    /// </summary>
    /// <param name="entityID"></param>
    public void SetID(ulong entityID) { EntityID = entityID; }

    /// <summary>
    /// Updates the Entity Transform with the given data.
    /// </summary>
    /// <param name="position">Position to set the transform to.</param>
    /// <param name="orientation">Quaternion to set the transform to.</param>
    /// <param name="velocity">Velocity to store.</param>
    /// <param name="error">Error value to store as tracking quality.</param>
    public virtual void UpdateTransform(Vector3 position, Quaternion orientation, Vector3 velocity, float error)
    {
        // update transform and store properties
        transform.SetPositionAndRotation(position, orientation);
        Velocity = velocity;
        TrackingQuality = error;

        // set timestamp
        TimeSinceLastUpdate = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// Stores all information about the Entity in a string array.
    /// </summary>
    /// <returns>Returns a string array of Entity information</returns>
    public virtual string[] GetEntityInfo()
    {
        // output entity info in string array format
        return new string[8]
        {
            $"<b>Entity ID:</b> {EntityID}",
            $"<b>Entity Name:</b> {EntityName}",
            $"<b>Position:</b> <b>X:</b> {transform.position.x:0.00}, <b>Y:</b> {transform.position.y:0.00}, <b>Z:</b> {transform.position.z:0.00}",
            $"<b>Rotation (Euler angles):</b> <b>X:</b> {transform.rotation.eulerAngles.x:0}, <b>Y:</b> {transform.rotation.eulerAngles.y:0}, <b>Z:</b> {transform.rotation.eulerAngles.z:0}",
            $"<b>Velocity:</b> <b>X:</b> {Velocity.x:0.0}, <b>Y:</b> {Velocity.y:0.0}, <b>Z:</b> {Velocity.z:0.0}",
            $"<b>Tracking Quality:</b> {TrackingQuality:0.00}",
            $"<b>Subscription Rate:</b> {GetSubscriptionRateString()}",
            $"<b>Subscription(s):</b> {GetSubscriptionString()}"
        };
    }

    /// <summary>
    /// Sets the visual active flag to the given value.
    /// </summary>
    /// <param name="active">Boolean to set the flag to.</param>
    public virtual void SetVisualsActive(bool active)
    {
        IsVisualsActive = active;
    }

    /// <summary>
    /// Uses the given Flatbuffer bounding box to initialize a relevant collider on the GameObject.
    /// </summary>
    /// <param name="boundingBox">Flatbuffer message containing bounding box information.</param>
    public virtual void InitializeBoundingBox(BoundingBox boundingBox)
    {
        // get values from boundingBox object
        Vec3 centre = boundingBox.Centre.Value;
        Vec3 dimensions = boundingBox.Dimensions.Value;
        Vec4 rotation = boundingBox.Rotation.Value;

        // create quaternion from rotation vector
        Quaternion quaternionOrientation = new Quaternion();
        quaternionOrientation.Set(rotation.X, rotation.Y, rotation.Z, rotation.W);

        if (boundingBox.Dimensions.Value.X == boundingBox.Dimensions.Value.Y && boundingBox.Dimensions.Value.Z == boundingBox.Dimensions.Value.Y && boundingBox.Ellipsoid)
        {
            // add sphereCollider if one isn't found
            if (!TryGetComponent(out SphereCollider sphereCollider))
            {
                sphereCollider = this.AddComponent<SphereCollider>();
            }

            // set sphereCollider variables
            sphereCollider.isTrigger = false;
            sphereCollider.center = new Vector3(centre.X, centre.Y, centre.Z);
            sphereCollider.radius = 0.5f;
            transform.rotation = quaternionOrientation;
        }
        else if (boundingBox.Ellipsoid)
        {
            // add capsuleCollider if one isn't found
            if (!TryGetComponent(out CapsuleCollider capsuleCollider))
            {
                capsuleCollider = this.AddComponent<CapsuleCollider>();
            }

            // set capsuleCollider variables
            capsuleCollider.isTrigger = false;
            capsuleCollider.center = new Vector3(centre.X, centre.Y, centre.Z);
            capsuleCollider.height = dimensions.Y;
            capsuleCollider.radius = dimensions.X;
            transform.rotation = quaternionOrientation;
        }
        else
        {
            // add boxCollider if one isn't found
            if (!TryGetComponent(out BoxCollider boxCollider))
            {
                boxCollider = this.AddComponent<BoxCollider>();
            }

            // set boxCollider variables
            boxCollider.center = new Vector3(centre.X, centre.Y, centre.Z);
            boxCollider.size = Vector3.one;
            boxCollider.transform.rotation = quaternionOrientation;
        }
    }

    /// <summary>
    /// Gets the subscription rate as a string.
    /// </summary>
    /// <returns>Returns the subscription rate as a string.</returns>
    private string GetSubscriptionRateString()
    {
        // switch over the subscriptionrates and return a string
        return SubscriptionRate switch
        {
            SubscriptionRate.None => "None",
            SubscriptionRate.Full => "Full",
            SubscriptionRate.Half => "Half",
            SubscriptionRate.Quarter => "Quarter",
            _ => "None",
        };
    }

    /// <summary>
    /// Gets a subscription string based on the Entity's subscription type.
    /// </summary>
    /// <returns>Returns a string that details the Entity's subscription type.</returns>
    private string GetSubscriptionString()
    {
        // check all subscription types and construct a string
        string subscriptions = "";
        if (FlatbufferSerializer.MatchSubscriptionType(SubscriptionType.None, Subscription)) { return "None"; }
        if (FlatbufferSerializer.MatchSubscriptionType(SubscriptionType.Generic, Subscription)) { subscriptions += "Generic"; }
        if (FlatbufferSerializer.MatchSubscriptionType(SubscriptionType.Robot, Subscription)) { subscriptions += ", Robot"; }
        if (FlatbufferSerializer.MatchSubscriptionType(SubscriptionType.Headset, Subscription)) { subscriptions += ", Headset"; }
        if (FlatbufferSerializer.MatchSubscriptionType(SubscriptionType.Presenter, Subscription)) { subscriptions += ", Presenter"; }
        if (FlatbufferSerializer.MatchSubscriptionType(SubscriptionType.Geometry, Subscription)) { subscriptions += ", Geometry"; }
        if (FlatbufferSerializer.MatchSubscriptionType(SubscriptionType.Own, Subscription)) { subscriptions += ", Own"; }
        return subscriptions;
    }

    /// <summary>
    /// Generates a random ulong ID.
    /// </summary>
    protected void GenerateID()
    {
        EntityID = (ulong)UnityEngine.Random.Range(ulong.MinValue, ulong.MaxValue);
    }
}