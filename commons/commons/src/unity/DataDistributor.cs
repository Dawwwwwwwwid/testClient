using Google.FlatBuffers;
using HIVE.Commons.Flatbuffers.Generated;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DataDistributor parses Flatbuffer messages and sends them to an EntityHandler instance.
/// </summary>
public class DataDistributor : MonoBehaviour
{
    [Header("Entity Handler")]
    [SerializeField] private EntityHandler entityHandler;

    // data structures to hold distributable data
    private readonly Queue<HIVE.Commons.Flatbuffers.Generated.Entity> staticDataQueue = new Queue<HIVE.Commons.Flatbuffers.Generated.Entity>();
    private readonly List<HIVE.Commons.Flatbuffers.Generated.Entity> nodeList = new List<HIVE.Commons.Flatbuffers.Generated.Entity>();

    /// <summary>
    /// Calls the DistributeData method if the queues aren't empty.
    /// </summary>
    void Update()
    {
        // distribute data if structures have data
        if (nodeList.Count > 0 || staticDataQueue.Count > 0) DistributeData();
    }

    /// <summary>
    /// Checks the queues and sends the entity data to the EntityHandler instance.
    /// </summary>
    private void DistributeData()
    {
        lock (staticDataQueue)
        {
            // dequeue and send data to entityHandler
            if (staticDataQueue.Count > 0) entityHandler.SetEntityData(staticDataQueue.Dequeue());
        }

        lock (nodeList)
        {
            // send list to entityHandler and clear it to avoid duplicate sends
            if (nodeList.Count > 0) entityHandler.SetEntityData(nodeList);
            nodeList.Clear();
        }
    }


    /// <summary>
    /// Parses the data into Flatbuffer messages and stores the data in the relevant queue.
    /// </summary>
    /// <param name="data">Byte array data to parse</param>
    public void AddData(byte[] data)
    {
        // return if no data is given
        if (data.Length <= 0) return;

        // initialize state from given data
        ByteBuffer byteBuffer = new ByteBuffer(data);
        State state = State.GetRootAsState(byteBuffer);

        // iterate over payloads 
        for(int i = 0; i < state.PayloadLength; i++)
        {
            Payload payload = state.Payload(i).Value;
            HIVE.Commons.Flatbuffers.Generated.Entity entity = payload.GetDataAsEntity().Value;

            // add to relevant data structure based on entity type
            if (entity.EntityType == EntityUnion.Node) lock(nodeList) nodeList.Add(entity);
            else lock(staticDataQueue) staticDataQueue.Enqueue(entity);
        }
    }
}