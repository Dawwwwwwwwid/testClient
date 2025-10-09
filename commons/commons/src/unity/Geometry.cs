using HIVE.Commons.Flatbuffers.Generated;
using System;
using UnityEngine;

/// <summary>
/// Geometry extends the Entity class and it represents a Geometry object in the system.
/// </summary>
public class Geometry : Entity
{
    public event Action<Geometry> OnGeometryStaticChange;

    [Header("Base Objects")]
    [SerializeField] private GameObject baseCube;
    [SerializeField] private GameObject baseCapsule;
    [SerializeField] private GameObject baseSphere;

    // colliders
    private CapsuleCollider capCollider;
    private BoxCollider boxCollider;
    private SphereCollider sphereCollider;

    // transform properties
    private Quaternion rotation;
    private Vector3 position;
    private Vector3 velocity;
    private Vector3 centre;
    private Vector3 size;

    // shape bools
    private bool spherical = false;
    private bool ellipsoid = false;

    /// <summary>
    /// Property for the ID of the Geometry owner.
    /// </summary>
    public ulong OwnerId { get; private set; } = 0;

    /// <summary>
    /// Property for the exclusivity of the Geometry ownership.
    /// </summary>
    public bool Exclusive { get; private set; } = false;

    /// <summary>
    /// Gets the dimensions of the bounding box.
    /// </summary>
    /// <returns>Returns the size of the bounding box.</returns>
    public Vector3 GetBoundingBoxDimensions() { return size; }

    /// <summary>
    /// Gets the rotation of the bounding box.
    /// </summary>
    /// <returns>Returns the rotation of the bounding box.</returns>
    public Quaternion GetBoundingBoxRotation() { return rotation; }

    /// <summary>
    /// Gets the centre of the bounding box.
    /// </summary>
    /// <returns>Returns the centre of the bounding box.</returns>
    public Vector3 GetBoundingBoxCentre() { return centre; }

    /// <summary>
    /// Gets the postion of the Geometry.
    /// </summary>
    /// <returns>Returns the current postion of the Geometry.</returns>
    public Vector3 GetCurrentPosition() { return position; }

    /// <summary>
    /// Gets the velocity of the Geometry.
    /// </summary>
    /// <returns>Returns the velocity of the Geometry rigidbody.</returns>
    public Vector3 GetCurrentVelocity() { return velocity; }

    /// <summary>
    /// Checks if the bounding box is ellipsoid.
    /// </summary>
    /// <returns>Returns the Geomtry's ellipsoid flag.</returns>
    public bool IsBoundingBoxEllipsoid() { return ellipsoid; }

    /// <summary>
    /// Initializes the Geometry's owner ID with the given value if its current value is 0.
    /// </summary>
    /// <param name="ownerId">The given owner ID.</param>
    public void InitializeOwnerId(ulong ownerId) { if (OwnerId == 0) OwnerId = ownerId; }

    /// <summary>
    /// Updates the static data of the Geometry with the given values.
    /// Invokes the OnGeometryStaticChange event.
    /// </summary>
    /// <param name="entityID">The ID of the Geoemtry.</param>
    /// <param name="entityName">The name of the Geometry.</param>
    /// <param name="boundingBox">The Geometry bounding box.</param>
    /// <param name="ownerId">ID of the owner of the Geoemtry.</param>
    /// <param name="exclusive">Indicates the exclusivity of the ownership.</param>
    public void UpdateStaticData(ulong entityID, string entityName, BoundingBox boundingBox, ulong ownerId, bool exclusive)
    {
        // re-assign the properties
        EntityID = entityID;
        EntityName = entityName;
        transform.name = entityName;
        ellipsoid = boundingBox.Ellipsoid;
        OwnerId = ownerId;
        Exclusive = exclusive;

        // initialize the bounding box and geometry
        InitializeBoundingBox(boundingBox);
        spherical = boundingBox.Dimensions.Value.X == boundingBox.Dimensions.Value.Y && boundingBox.Dimensions.Value.Z == boundingBox.Dimensions.Value.Y;
        InitializeGeometry(boundingBox);

        OnGeometryStaticChange?.Invoke(this);
    }

    /// <summary>
    /// Toggles the visibility of the Geometry and toggles the bounding box collider.
    /// </summary>
    /// <param name="active">Determines whether to toggle the visuals / bounding box on or off.</param>
    public override void SetVisualsActive(bool active)
    {
        // call the base method
        base.SetVisualsActive(active);

        // set the collider
        if (boxCollider != null) boxCollider.enabled = active;
        else if (capCollider != null) capCollider.enabled = active;
        else if (sphereCollider != null) sphereCollider.enabled = active;

        // set the object renderer
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = active;
    }

    /// <summary>
    /// Sets some variables to default values and tries to set ellipsoid flag.
    /// </summary>
    private void Awake()
    {
        // set variables to default values
        EntityName = transform.name;
        Subscription = 0;
        SubscriptionRate = SubscriptionRate.None;

        // check for colliders to set ellipsoid bool
        if (TryGetComponent(out boxCollider)) ellipsoid = false;
        if (TryGetComponent(out capCollider)) ellipsoid = true;
        if (TryGetComponent(out sphereCollider)) ellipsoid = true;
    }

    /// <summary>
    /// Stores the transform and collider information in relevant variables.
    /// </summary>
    private void Update()
    {
        // store transform values
        rotation = transform.rotation;
        position = transform.position;
        if (TryGetComponent(out Rigidbody rigidbody)) velocity = rigidbody.velocity;

        // store collider dependant transform values
        if (boxCollider != null)
        {
            centre = boxCollider.center;
            size = transform.localScale;
        }
        else if (capCollider != null)
        {
            centre = capCollider.center;
            size = new Vector3(capCollider.radius, capCollider.height, transform.localScale.z);
        }
        else if (sphereCollider != null)
        {
            centre = sphereCollider.center;
            size = transform.localScale;
        }
    }

    /// <summary>
    /// Initializes the Geometry visuals using the given bounding box.
    /// </summary>
    /// <param name="boundingBox">Bounding box to initialize the Geometry.</param>
    private void InitializeGeometry(BoundingBox boundingBox)
    {
        Vec3 dimensions = boundingBox.Dimensions.Value;
        Vec4 rotation = boundingBox.Rotation.Value;

        Quaternion quaternionOrientation = new Quaternion();
        quaternionOrientation.Set(rotation.X, rotation.Y, rotation.Z, rotation.W);

        if (spherical && ellipsoid)
        {
            if (transform.GetComponentInChildren<MeshRenderer>() == null) Instantiate(baseSphere, transform.position, transform.rotation, transform);
            transform.localScale = new Vector3(dimensions.X, dimensions.Y, dimensions.Z);
            sphereCollider = GetComponent<SphereCollider>();
        }
        else if (ellipsoid)
        {
            if (transform.GetComponentInChildren<MeshRenderer>() == null) Instantiate(baseCapsule, transform.position, transform.rotation, transform);
            transform.localScale = new Vector3(dimensions.Z, dimensions.Z, dimensions.Z);
            capCollider = GetComponent<CapsuleCollider>();
        }
        else
        {
            if (transform.GetComponentInChildren<MeshRenderer>() == null) Instantiate(baseCube, transform.position, transform.rotation, transform);
            transform.localScale = new Vector3(dimensions.X, dimensions.Y, dimensions.Z);
            boxCollider = GetComponent<BoxCollider>();
        }

        transform.localRotation = quaternionOrientation;
    }
}
