using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HIVE.Commons.Netcode
{
  /// <summary>
  /// Client class for connecting to the data center.
  /// </summary>
  /// <remarks>
  /// This class is public
  /// </remarks>
  public class Client
  {
    /// <summary>
    /// Client object for connecting to the data center.
    /// </summary>
    /// <remarks>
    /// This is a private field.
    private TcpClient _client;

    /// <summary>
    /// NetworkStream object for reading and writing data.
    /// </summary>
    /// <remarks>
    /// This is a private field.
    /// </remarks>
    private NetworkStream _stream;

    /// <summary>
    /// Lock object to synchronize access to the stream.
    /// </summary>
    /// <remarks>
    /// This is a private readonly field.
    /// </remarks>
    private readonly Lock _lock = new();

    /// <summary>
    /// Flag to indicate if the client is connected to the data center.
    /// </summary>
    /// <remarks>
    /// This is a public property, that can be set privately.
    /// </remarks>
    public bool IsConnected { get; private set; } = false;

    /// <summary>
    /// Constructor for the Client class.
    /// </summary>
    /// <param name="ip">IP address of the data center.</param>
    /// <param name="port">Port number of the data center.</param>
    /// <remarks>
    /// This constructor initializes the <see cref="_client"/> and <see cref="_stream"/> fields.
    /// </remarks>
    public Client(string ip, int port)
    {
      Connect(ip, port);

      // TODO: send static data
    }

    /// <summary>
    /// Connects to the data center.
    /// </summary>
    /// <param name="ip">IP address of the data center.</param>
    /// <param name="port">Port number of the data center.</param>
    /// <remarks>
    /// This method is public. 
    /// </remarks>
    public void Connect(string ip, int port)
    {
      try
      {
        _client = new TcpClient(ip, port);

        if (ip == string.Empty) { _client.Connect(IPAddress.Loopback, port); }
        else { _client.Connect(ip, port); }

        _stream = _client.GetStream();

        const int magic = 0x23476945;
        _stream.Write(BitConverter.GetBytes(magic), 0, 4);

        IsConnected = true;
      }
      catch (Exception e)
      {
        throw new Exception(e.Message);
      }
      finally
      {
        IsConnected = false;
        using (_lock.EnterScope())
        {
          _client.Close();
          _stream.Close();
        }
      }
    }

    /// <summary>
    /// Reads data from the data center.
    /// </summary>
    /// <returns>
    /// Byte array containing the data read from the data center.
    /// </returns>
    /// <remarks>
    /// This method is public and returns a byte array.
    /// </remarks>
    public byte[] Read()
    {
      if (IsConnected)
      {
        using (_lock.EnterScope())
        {
          // read first 4 bytes
          byte[] byte_span = new byte[4];
          _stream.ReadExactly(byte_span, 0, 4);
          int length = BitConverter.ToInt32(byte_span, 0);

          byte[] temp_data = new byte[length];
          _stream.ReadExactly(temp_data, 0, length);

          return temp_data;
        }
      }
      else
      {
        return Array.Empty<byte>();
      }
    }

    /// <summary>
    ///  Writes data to the data center.
    /// </summary>
    /// <param name="data">Byte array containing the data to be written.</param>
    /// <remarks>
    /// This method is public.
    /// </remarks>
    public void Write(byte[] data)
    {
      if (IsConnected)
      {
        using (_lock.EnterScope())
        {
          _stream.Write(data, 0, data.Length);
        }
      }
    }

    /// <summary>
    /// Disconnects from the data center.
    /// </summary>
    /// <remarks>
    /// This method is public.
    /// </remarks>
    public void Disconnect()
    {
      if (IsConnected)
      {
        _client.Close();
        _stream.Close();
        IsConnected = false;
      }
    }
  }
}