using HIVE.Commons.Flatbuffers.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// EntityHandler stores and manages all entites within the scene.
/// </summary>
public class EntityHandler : MonoBehaviour
{
    public static event Action<EntityHandler> OnEntityUpdateEvent;

    private ReticleController reticleController;
    private ulong thisHeadsetId;
    private Dictionary<ulong, GameObject> entities;

    [Header("Entity Prefabs")]
    [SerializeField] private GameObject headsetPrefab;
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private GameObject geometryPrefab;

    [Header("Geometry Parent")]
    [SerializeField] private Transform geometryParent;

    private readonly float entityTimerSeconds = 1f;
    private float entityTimerElapsed = 0f;

    /// <summary>
    /// Initializes the entities dictionary and gets the ReticleController instance.
    /// </summary>
    private void Awake()
    {
        entities = new Dictionary<ulong, GameObject>();
        reticleController = GetComponent<ReticleController>();
    }

    /// <summary>
    /// Checks the entity timer and check for entity timeouts if the timer has ended.
    /// </summary>
    private void Update()
    {
        // if timerElapsed surpasses timer check for timeouts
        if(entityTimerElapsed > entityTimerSeconds)
        {
            entityTimerElapsed = 0f;
            CheckEntityTimeouts();
        }

        // increment timer
        entityTimerElapsed += Time.deltaTime;
    }


    /// <summary>
    /// Iterate over the entities dictionary and check the last time each entity had their transform updated.
    /// If it has been more / less than the timeout time then it sets the entity's visuals to inactive / active respectively.
    /// </summary>
    private void CheckEntityTimeouts()
    {
        // iterate over entities
        foreach (KeyValuePair<ulong, GameObject> entityObject in entities.ToList())
        {
            // get entity script
            Entity entity = entityObject.Value.GetComponent<Entity>();

            // check if its a geom this headset owns, if it is we don't want to remove it so continue
            if (entity is Geometry)
            {
                Geometry geometry = entity as Geometry;
                if (geometry.OwnerId == thisHeadsetId) continue;
            }

            // check the time since last update, if greater than timeout, disable entity
            // if entity is inactive and last update is smaller than timeout, activate entity
            float timeSinceLastEntityUpdate = entity.TimeSinceLastUpdate;
            if (entity.IsVisualsActive && Time.realtimeSinceStartup - timeSinceLastEntityUpdate > entityTimerSeconds) SetEntityActive(entity, false);
            else if(!entity.IsVisualsActive && Time.realtimeSinceStartup - timeSinceLastEntityUpdate < entityTimerSeconds) SetEntityActive(entity, true);
        }
    }

    /// <summary>
    /// Sets the visuals of the given entity and invokes the OnEntityUpdateEvent.
    /// </summary>
    /// <param name="entity">The entity to toggle the visuals of.</param>
    /// <param name="active">Determines whether to turn the visuals on or off.</param>
    private void SetEntityActive(Entity entity, bool active)
    {
        entity.SetVisualsActive(active);
        OnEntityUpdateEvent?.Invoke(this);
    }

    /// <summary>
    /// Adds the new Entity instance to the dictionary and invoke the OnEntityUpdateEvent.
    /// </summary>
    /// <param name="newEntity">Entity instance to add.</param>
    /// <param name="entityId">Unique identifier of the Entity instance.</param>
    private void UpdateEntities(GameObject newEntity, ulong entityId)
    {
        entities[entityId] = newEntity;
        OnEntityUpdateEvent?.Invoke(this);
    }

    /// <summary>
    /// Instantiates a new robot prefab GameObject and stores its Robot instance in the dictionary.
    /// </summary>
    /// <param name="robotData">The Flatbuffer message containing the static robot data.</param>
    private void InstantiateRobot(HIVE.Commons.Flatbuffers.Generated.Robot robotData)
    {
        // instantiate a new robot and assign static data
        GameObject newRobot = Instantiate(robotPrefab, transform.position, Quaternion.identity, transform);
        newRobot.GetComponent<Robot>().UpdateStaticData(robotData.Id, robotData.Name, robotData.Subscription, robotData.Rate, robotData.BoundingBox, robotData.Colour);

        // add robot to entities and assign its name
        UpdateEntities(newRobot, robotData.Id);
        newRobot.name = robotData.Name;
    }

    /// <summary>
    /// Instantiates a new headset prefab GameObject and stores its Headset instance in the dictionary.
    /// </summary>
    /// <param name="headsetData">The Flatbuffer message containing the static headset data.</param>
    private void InstantiateHeadset(HIVE.Commons.Flatbuffers.Generated.Headset headsetData)
    {
        // instantiate a new headset and assign static data
        GameObject newHeadset = Instantiate(headsetPrefab, transform.position, Quaternion.identity, transform);
        newHeadset.GetComponent<Headset>().UpdateStaticData(headsetData.Id, headsetData.Name, headsetData.Subscription, headsetData.Rate, headsetData.BoundingBox);

        // add headset to entities and assign its name
        UpdateEntities(newHeadset, headsetData.Id);
        newHeadset.name = headsetData.Name;
    }

    /// <summary>
    /// Instantiates a new geometry prefab GameObject and stores its Geometry instance in the dictionary.
    /// </summary>
    /// <param name="geometry">The Flatbuffer message containing the static geometry data.</param>
    private void InstantiateGeometry(HIVE.Commons.Flatbuffers.Generated.Geometry geometry)
    {
        // check if the geometry has a boundingBox
        // if not return as geometry needs boundingBox to be initilaized
        if (!geometry.BoundingBox.HasValue)
        {
            Debug.LogWarning($"Geometry {geometry.Name} : {geometry.Id} does not have a bounding box");
            return;
        }

        // instantiate a new geometry and assign static data
        GameObject newGeometry;
        if (geometryParent != null) newGeometry = Instantiate(geometryPrefab, transform.position, Quaternion.identity, geometryParent.transform);
        else newGeometry = Instantiate(geometryPrefab, transform.position, Quaternion.identity, transform);

        newGeometry.GetComponent<Geometry>().UpdateStaticData(geometry.Id, geometry.Name, geometry.BoundingBox.Value, geometry.OwnerId, geometry.Exclusive);

        // add geometry to entities and assign its name
        UpdateEntities(newGeometry, geometry.Id);
        newGeometry.name = geometry.Name;
    }

    /// <summary>
    /// Iterates over all geometries in the geometry parent Transform and adds them to the entities dictionary.
    /// </summary>
    public void AddGeometriesToList()
    {
        if (geometryParent == null) return;

        foreach(Geometry geometry in geometryParent.GetComponentsInChildren<Geometry>())
        {
            entities.Add(geometry.EntityID, geometry.gameObject);
        }
    }

    /// <summary>
    /// Takes in a new Flatbuffer entity message to be handled.
    /// </summary>
    /// <param name="entity">The Flatbuffer entity message.</param>
    public void SetEntityData(HIVE.Commons.Flatbuffers.Generated.Entity entity)
    {
        HandleEntityData(entity);
    }

    /// <summary>
    /// Takes in a new list of Flatbuffer entity messages to be handled.
    /// </summary>
    /// <param name="entities">The Flatbuffer entity messages.</param>
    public void SetEntityData(List<HIVE.Commons.Flatbuffers.Generated.Entity> entities)
    {
        foreach (HIVE.Commons.Flatbuffers.Generated.Entity entity in entities.ToArray())
        {
            HandleEntityData(entity);
        }
    }

    /// <summary>
    /// Gets the entities dictionary.
    /// </summary>
    /// <returns>Returns the entites dictionary.</returns>
    public Dictionary<ulong, GameObject> GetEntities()
    {
        return entities;
    }

    /// <summary>
    /// Switches over the EntityUnion of the given Flatbuffer entity message and calls the relvant method to handle it.
    /// </summary>
    /// <param name="entity">The Flatbuffer entity message.</param>
    private void HandleEntityData(HIVE.Commons.Flatbuffers.Generated.Entity entity)
    {
        // switch over the entity type and call the relevant handle method
        switch (entity.EntityType)
        {
            case EntityUnion.Headset: HandleHeadsetData(entity.Entity_AsHeadset()); return;
            case EntityUnion.Robot: HandleRobotData(entity.Entity_AsRobot()); return;
            case EntityUnion.Node: UpdateNodeData(entity.Entity_AsNode()); return;
            case EntityUnion.Geometry: HandleGeometryData(entity.Entity_AsGeometry()); return;
            case EntityUnion.Command: HandleCommandData(entity.Entity_AsCommand()); return;
            default: return;
        }
    }

    /// <summary>
    /// Takes in a Flatbuffer headset message and processes the data.
    /// Instantiates a new headset if it is not found in the entities dictionary, updates the transform if it is.
    /// </summary>
    /// <param name="headsetData">The Flatbuffer headset message.</param>
    private void HandleHeadsetData(HIVE.Commons.Flatbuffers.Generated.Headset headsetData)
    {
        // don't want to spawn our own headset so return if the names match
        if (HeadsetNames.IsThisHeadset(headsetData.Id)) return;
        
        // look for headset in entites dictionary
        if (entities.TryGetValue(headsetData.Id, out GameObject headset))
        {
            // if found update static data
            headset.GetComponent<Headset>().UpdateStaticData
            (
                headsetData.Id,
                headsetData.Name,
                headsetData.Subscription,
                headsetData.Rate,
                headsetData.BoundingBox.Value
            );
        }
        else
        {
            // if not found instantiate new headset
            InstantiateHeadset(headsetData);
        }
    }

    /// <summary>
    /// Takes in a Flatbuffer robot message and processes the data.
    /// Instantiates a new robot if it is not found in the entities dictionary, updates the transform if it is.
    /// </summary>
    /// <param name="robotData">The Flatbuffer robot message.</param>
    private void HandleRobotData(HIVE.Commons.Flatbuffers.Generated.Robot robotData)
    {
        // look for robot in entities dictionary
        if (entities.TryGetValue(robotData.Id, out GameObject robot))
        {
            if (robot.TryGetComponent(out Robot robotEntity))
            {
                // if found update static data
                robotEntity.UpdateStaticData
                (   
                    robotData.Id,
                    robotData.Name,
                    robotData.Subscription,
                    robotData.Rate,
                    robotData.BoundingBox.Value,
                    robotData.Colour
                );
            }
        }
        else
        {
            // if not found instantiate new robot
            InstantiateRobot(robotData);
        }
    }

    /// <summary>
    /// Takes in a Flatbuffer geometry message and processes the data.
    /// Instantiates a new geometry if it is not found in the entities dictionary, updates the transform if it is.
    /// </summary>
    /// <param name="geometryData">The Flatbuffer geometry message.</param>
    private void HandleGeometryData(HIVE.Commons.Flatbuffers.Generated.Geometry geometryData)
    {
        // look for geometry in entities dictionary
        if (entities.TryGetValue(geometryData.Id, out GameObject geometry))
        {
            // if geometry update static data
            geometry.GetComponent<Geometry>().UpdateStaticData
            (
                geometryData.Id,
                geometryData.Name,
                geometryData.BoundingBox.Value,
                geometryData.OwnerId,
                geometryData.Exclusive
            );
        }
        else
        {
            // if not found instantiate new geometry
            InstantiateGeometry(geometryData);
        }
    }

    /// <summary>
    /// Switches over the command type and calls the relevant command handle method.
    /// </summary>
    /// <param name="commandData">The command message to be handled.</param>
    private void HandleCommandData(Command commandData)
    {
        // switch over command types and call relevant handle method
        switch (commandData.CommandType)
        {
            case CommandUnion.MoveTo: HandleMoveToCommand(commandData.Command_AsMoveTo()); return;
            default: return;
        }
    }

    /// <summary>
    /// Takes in a move to command and instantiates a new reticle using its data.
    /// </summary>
    /// <param name="moveToCommand">The MoveTo instance to instantiate a reticle with.</param>
    private void HandleMoveToCommand(MoveTo moveToCommand)
    {
        Vector3 position = new Vector3(moveToCommand.Destination.Value.X, moveToCommand.Destination.Value.Y, moveToCommand.Destination.Value.Z);
        reticleController.InstantiateReticle(position, moveToCommand.Id);
    }

    /// <summary>
    /// Takes in node data an updates an entity from the entities dictionary if it exists.
    /// </summary>
    /// <param name="nodeData">The node data to update the entity with.</param>
    private void UpdateNodeData(Node nodeData)
    {
        // return if entity is not found, nothing to update
        if (!entities.ContainsKey(nodeData.Id)) return;

        // store values from nodeData in variables
        Vector3 position = new Vector3(nodeData.Position.Value.X, nodeData.Position.Value.Y, nodeData.Position.Value.Z);
        Quaternion rotation = new Quaternion();
        rotation.Set(nodeData.Rotation.Value.X, nodeData.Rotation.Value.Y, nodeData.Rotation.Value.Z, nodeData.Rotation.Value.W);
        Vector3 velocity = new Vector3(nodeData.Velocity.Value.X, nodeData.Velocity.Value.Y, nodeData.Velocity.Value.Z);

        // get the entity component
        Entity entity = entities[nodeData.Id].GetComponent<Entity>();

        // check if it is a geometry
        // if it is owned by this headset don't update, physics will update its position
        if (entity is Geometry)
        {
            Geometry geometry = entity as Geometry;
            if (geometry.OwnerId == thisHeadsetId) return;
        }

        // update the entities transform
        entity.UpdateTransform(position, rotation, velocity, nodeData.Error);
    }

    /// <summary>
    /// Sets the unique headset identifier to the given value.
    /// </summary>
    /// <param name="headsetID">Qunique headset identifier.</param>
    public void SetHeadsetID(ulong headsetID)
    {
        thisHeadsetId = headsetID;
    }
}