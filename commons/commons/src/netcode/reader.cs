using System;
using System.Threading;
using HIVE.Commons.Utils;

namespace HIVE.Commons.Netcode
{
  /// <summary>
  /// This class is used to read data from the data center.
  /// </summary>
  /// <remarks>
  /// Abstract class that inherits from <see cref="Tickable"/>.
  /// </remarks>
  public abstract class Reader : Tickable
  {

    /// <summary>
    /// Lock object to synchronize access to the queue.
    /// </summary>
    /// <remarks>
    /// This is a private static readonly field.
    /// </remarks>
    private static readonly Lock _lock = new();

    /// <summary>
    ///   Queue to hold the data to be read.
    /// </summary>
    /// <remarks>
    /// This is a private static readonly field.
    /// </remarks>
    private static Client _Client;

    /// <summary>
    /// Constructor for the Reader class.
    /// </summary>
    /// <param name="client">Client object to read data from the data center.</param>
    /// <param name="TPS">Ticks per second.</param>
    /// <remarks>
    /// This constructor initializes the <see cref="_Client"/> field and calls the base constructor.
    /// </remarks>
    public Reader(ref Client client, int TPS) : base(TPS) { _Client = client; }

    /// <summary>
    /// Enqueues data to be read from the data center.
    /// </summary>
    /// <remarks>
    /// This method is public and static. It must be overridden in derived classes.
    public override void OnTick()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Reads data from the data center.
    /// </summary>
    /// <param name="data">Data to be read.</param>
    /// <remarks>
    /// This method is public and static. It must be overridden in derived classes.
    /// </remarks>
    public abstract void OnRead(byte[] data);
  }
}