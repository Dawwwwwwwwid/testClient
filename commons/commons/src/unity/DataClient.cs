using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
using System.Timers;
using HIVE.Commons.Flatbuffers;
using HIVE.Commons.Math.Vec;

/// <summary>
/// DataClient creates and handles multiple threads that read and write from a TCP connection stream.
/// </summary>
public class DataClient : MonoBehaviour
{
    // network objects
    private TcpClient client;
    private NetworkStream clientNetworkStream;
    private Thread connectionThread;
    private Thread readerThread;

    // data objects
    private readonly object streamLock = new object();
    private readonly Queue<byte[]> dataQueue = new Queue<byte[]>();
    private readonly int magicNumber = 0x23476945;
    private DataDistributor dataDistributor;

    // connection status bools 
    private bool connected = false;
    private bool cancel = false;

    [Header("Connection Configurations")]
    [SerializeField] private int port;
    [SerializeField] private string ipAddress;
    [SerializeField] private float fps = 2;
    [SerializeField] private bool connectAsPresenter = false;
    [SerializeField] private string connectionName = "Presenter";

    // presenter info collection
    private ulong randomId;
    private string[] netStats;
    private int messageCount = 0;
    private static System.Timers.Timer messagesTimer = new System.Timers.Timer();

    /// <summary>
    /// Assigns the DataDistrubutor instance found on this GameObject.
    /// If the DataClient is connected as a presenter, generate a random unique identifier.
    /// </summary>
    private void Awake()
    {
        dataDistributor = GetComponent<DataDistributor>();

        if (connectAsPresenter)
        {
            randomId = (ulong)UnityEngine.Random.Range(ulong.MinValue, ulong.MaxValue);
            netStats = new string[4];
            netStats[1] = $"<b>Presenter ID:</b> {randomId}";
        }
    }

    /// <summary>
    /// Starts the connection thread and calls the Connect method.
    /// </summary>
    private void Start()
    {
        if (connectAsPresenter) Application.targetFrameRate = 240;

        // start the connection thread
        connectionThread = new Thread(Connect);
        connectionThread.IsBackground = true;
        connectionThread.Start();
    }

    /// <summary>
    /// Creates a new TCPClient instance and connects to the data centre using a magic number and static data.
    /// Creates a new thread to read data and then leaves this thread to handle sending data.
    /// </summary>
    private void Connect()
    {
        try
        {
            // connect to the client, if no IP address is given assume localhost
            client = new TcpClient();
            if (ipAddress == string.Empty) client.Connect(IPAddress.Loopback, port);
            else client.Connect(ipAddress, port);

            if(connectAsPresenter) netStats[0] = $"<b>Connected on:</b> {client.Client.LocalEndPoint}";

            // assign the network stream and send magic number to data centre
            clientNetworkStream = client.GetStream();
            SendMagicNumber();

            if(connectAsPresenter) SendStaticData();
            connected = true;

            if (connectAsPresenter) InitializeTimer();
            if (connectAsPresenter) ListenForData(); // runs indefinitely until cancelled
            else
            {
                // start the reader thread
                readerThread = new Thread(ReadData);
                readerThread.IsBackground = true;
                readerThread.Start();
            }
            

            // runs indefinitely until cancelled
            if (!connectAsPresenter) SendData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error connecting to server: {e}");
            if(connectAsPresenter) netStats[0] = $"Failed to connect";
        }
        finally
        {
            // set the connection status bools
            connected = false;
            cancel = true;

            // dispose of the network objects
            lock (streamLock)
            {
                clientNetworkStream?.Dispose();
                client?.Dispose();
            }
        }
    }

    /// <summary>
    /// Creates the presenter static data and writes it to the TCPClient stream.
    /// </summary>
    private void SendStaticData()
    {
        byte[] state = FlatbufferSerializer.CreatePresenterStaticState(randomId, connectionName);

        try
        {
            lock (streamLock) clientNetworkStream.Write(state);
        }
        catch (Exception e)
        {
            Debug.Log($"Error sending static state: {e}");
        }
    }

    /// <summary>
    /// Stores the magic number as bytes and writes it to the TCPClient stream.
    /// </summary>
    private void SendMagicNumber()
    {
        try
        {
            byte[] magicData = BitConverter.GetBytes(magicNumber);
            lock (streamLock) clientNetworkStream.Write(magicData);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error sending magic number: {e}");
        }
    }

    /// <summary>
    /// Runs continually and sends writes any data in the data queue to the TCPClient stream.
    /// Sleeps the loop every iteration to account for fps.
    /// </summary>
    private void SendData()
    {
        while (true)
        {
            // Sleep first, to avoid the continue statement becoming a spin loop
            Thread.Sleep((int)(1000 / fps));
            if (cancel) return;

            lock (dataQueue)
            {
                // if there is anything in the dataQueue, dequeue and write to stream
                if (dataQueue.Count > 0)
                {
                    lock (streamLock) clientNetworkStream.Write(dataQueue.Dequeue());
                }

                // clear queue if it gets too large
                if (dataQueue.Count > 100) dataQueue.Clear();
            }
        }
    }

    /// <summary>
    /// Continually checks if there is data available on the TCPClient stream and reads it if there is.
    /// </summary>
    private void ListenForData()
    {
        while (true)
        {
            if (cancel) return;
            try
            {
                lock (streamLock) if (!clientNetworkStream.DataAvailable) continue;

                ReadInData();
                messageCount++;
            }
            catch
            {
                lock (streamLock) clientNetworkStream?.Dispose();
                cancel = true;
            }
        }
    }

    /// <summary>
    /// Continually checks if there is data available on the TCPClient stream and reads it if there is. 
    /// </summary>
    private void ReadData()
    {
        while (true)
        {
            // Sleep first, to avoid the continue statement becoming a spin loop
            Thread.Sleep((int)(1000 / fps));
            if (cancel) return;
            try
            {
                lock (streamLock)
                {
                    // check the stream for data and read in if available
                    if (clientNetworkStream.DataAvailable)
                    {
                        ReadInData();
                    }
                }
            }
            catch (Exception e)
            {
                // log the error and dispose of the network object
                Debug.LogError(e);
                lock (streamLock) clientNetworkStream?.Dispose();

                // set the cancel bool to true to stop the other thread
                cancel = true;
            }
        }
    }

    /// <summary>
    /// Reads from the TCPClient stream and sends the data to the DataDistributor.
    /// </summary>
    private void ReadInData()
    {
        try
        {
            // initialize buffer
            byte[] payloadLength = new byte[4];
            int lengthRead = 0;
            lock (streamLock)
            {
                // check connection status and return if not connected
                if (client == null) return;
                if (!client.Connected) return;

                // read the first 4 bytes to get message length
                while (lengthRead < 4)
                {
                    lengthRead += clientNetworkStream.Read(payloadLength, lengthRead, 4 - lengthRead);
                }
                lengthRead = BitConverter.ToInt32(payloadLength, 0);

                // initialize state buffer
                byte[] stateData = new byte[lengthRead];
                int read = 0;

                // read in state
                while (read < lengthRead)
                {
                    read += clientNetworkStream.Read(stateData, 0, stateData.Length);
                }

                // send to dataDistributor 
                dataDistributor.AddData(stateData);
                return;
            }
        }
        catch (SocketException e)
        {
            Debug.Log($"SocketException: {e}");
        }
    }

    /// <summary>
    /// Sets the netstats message count message and resets the message count.
    /// </summary>
    /// <param name="sender">Object the method was invoked from.</param>
    /// <param name="e">Event arguments.</param>
    private void OnTimer(object sender, ElapsedEventArgs e)
    {
        netStats[3] = $"Receiving {messageCount} messages per second";
        messageCount = 0;
    }

    /// <summary>
    /// Initializes the timer used to check the message count.
    /// Hooks the OnTimer method to the elapsed event.
    /// </summary>
    private void InitializeTimer()
    {
        messagesTimer = new System.Timers.Timer(1000);
        messagesTimer.Elapsed += OnTimer;
        messagesTimer.AutoReset = true;
        messagesTimer.Enabled = true;
        messagesTimer.Start();
    }

    /// <summary>
    /// Gets the state of the client.
    /// </summary>
    /// <returns>Returns a Boolean indicating whether the DataClient is still connected to the TCPClient.</returns>
    public bool GetClientActive()
    {
        return connected;
    }

    /// <summary>
    /// Gets the network statistics.
    /// </summary>
    /// <returns>Returns a string array with all of the collected network statistics.</returns>
    public string[] GetNetStats()
    {
        return netStats;
    }

    /// <summary>
    /// Adds the given byte array data to the queue.
    /// </summary>
    /// <param name="sendData">The byte array data to add to the queue.</param>
    public void AddSendData(byte[] sendData)
    {
        lock (dataQueue)
        {
            dataQueue.Enqueue(sendData);
        }
    }

    /// <summary>
    /// Creates and writes a robot command message to the TCPClient stream.
    /// </summary>
    /// <param name="robotId">Unique identifier of the robot to create the command for.</param>
    /// <param name="destination">Position of where to direct the robot.</param>
    public void SendRobotCommand(ulong robotId, Vector3 destination)
    {
        byte[] data = FlatbufferSerializer.CreateDirectRobotCommandMessage(robotId, new Vec3(destination.x, destination.y, destination.z));
        try
        {
            lock (streamLock) clientNetworkStream.Write(data);
        }
        catch (Exception e)
        {
            Debug.Log($"Error sending data: {e}");
        }
    }

    /// <summary>
    /// Sets the cancel bool to true to trigger the exit statement of any loops in the threads and allow them to clean themselves up.
    /// </summary>
    void OnApplicationQuit()
    {
        cancel = true;
    }
}
