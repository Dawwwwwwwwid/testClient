using System.Collections.Generic;
using System.Threading;
using HIVE.Commons.Utils;


namespace HIVE.Commons.Netcode
{
  /// <summary>
  /// This class is used to send data to the data center.
  /// </summary>
  /// <remarks>
  /// It is public and inherits from <see cref="Tickable"/>.
  /// </remarks>
  public class Writer : Tickable
  {

    /// <summary>
    /// Lock object to synchronize access to the queue.
    /// </summary>
    /// <remarks>
    /// This is a private static readonly field.
    /// </remarks>
    private static readonly Lock _lock = new();

    /// <summary>
    /// Queue to hold the data to be sent.
    /// </summary>
    /// <remarks>
    /// This is a private static readonly field.
    /// </remarks>
    private static readonly Queue<byte[]> _queue = new();

    /// <summary>
    /// Client object to send data to the data center.
    /// </summary>
    /// <remarks>
    /// This is a private static field.
    /// </remarks>
    private static Client _Client;

    /// <summary>
    /// Constructor for the Writer class.
    /// </summary>
    /// <param name="client">Client object to send data to the data center.</param>
    /// <param name="TPS">Ticks per second.</param>
    /// <remarks>
    /// This constructor initializes the <see cref="_Client"/> field and calls the base constructor.
    /// </remarks>
    public Writer(ref Client client, int TPS) : base(TPS) { _Client = client; }

    /// <summary>
    /// Enqueues data to be sent to the data center.
    /// </summary>
    /// <param name="data">Data to be sent.</param>
    /// <remarks>
    /// This method is public and static.
    /// </remarks>
    public static void Write(byte[] data)
    {
      using (_lock.EnterScope()) { _queue.Enqueue(data); }
    }

    /// <summary>
    /// Sends the data in the queue to the data center if there are messages in the <see cref="_queue"/>.
    /// </summary>
    /// <remarks>
    /// This method overrides the <see cref="Tickable.OnTick"/> method.
    /// </remarks>
    public override void OnTick()
    {
      if (_queue.Count > 0)
      {
        using (_lock.EnterScope())
        {
          byte[] data = _queue.Dequeue();
          _Client.Write(data);
        }
      }
    }
  }
}