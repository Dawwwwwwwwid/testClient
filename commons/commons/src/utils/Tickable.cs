using System;
using System.Threading;

namespace HIVE.Commons.Utils
{
    /// <summary>
    /// Represents a class that can be ticked.
    /// </summary>
    /// <remarks>
    /// This class is abstract.
    /// </remarks>
    public abstract class Tickable {

      /// <summary>
      /// The thread that the tickable class uses.
      /// </summary>
      /// <remarks>
      /// This field is private.
      /// </remarks>
      private Thread _thread;

      /// <summary>
      /// The lock object for the thread.
      /// </summary>
      /// <remarks>
      /// This field is private and read-only.
      /// </remarks>
      private readonly Lock _tlock = new();

      /// <summary>
      /// Flag to check if the thread has started.
      /// </summary>
      /// <remarks>
      /// This property is read-only.
      /// </remarks>
      public bool IsStarted { get; private set; }

      /// <summary>
      /// Flag to check if the thread is running.
      /// </summary>
      /// <remarks>
      /// This property is read-only.
      /// </remarks>
      public bool IsRunning { get; private set; }

      /// <summary>
      /// How many times OnTick is called per second.
      /// </summary>
      /// <remarks>
      /// This property is public and can be accessed directly.
      /// </remarks>
      public int TPS { get; set; }

      /// <summary>
      /// How many milliseconds per tick.
      /// </summary>
      /// <remarks>
      /// This property is read-only.
      /// </remarks>
      public int MSPT { get => 1000 / TPS; }

      /// <summary>
      /// Initializes a new instance of the <see cref="Tickable"/> class with the specified ticks per second.
      /// </summary>
      /// <param name="TPS">The number of ticks per second.</param>
      /// <remarks>
      /// This constructor is protected.
      /// </remarks>
      internal Tickable (int TPS) { this.TPS = TPS; }

      /// <summary>
      /// Starts Ticking the thread.
      /// </summary>
      /// <remarks>
      /// This method is public and can be accessed directly.
      /// </remarks>
      public void Start() {
  
        try { _thread = new Thread(() => Tick()); } 
        catch (Exception e) { throw new Exception(e.Message); } 
        finally {
          IsStarted = true;
          IsRunning = true;
          _thread.Start();
        }
      }

      /// <summary>
      /// This method makes sure the thread is running with the correct timing. 
      /// </summary>
      /// <remarks>
      /// This method is internal and should not be called directly.
      /// </remarks>
      internal void Tick() {  
    
        while (!IsStarted) { Thread.Sleep(1); }

        do {

          if (!IsRunning) { return; }

          long t1 = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
          using (_tlock.EnterScope()) { OnTick(); }
          long t2 = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;
        
          long delta = t2 - t1;
          long sleep = MSPT - delta;

          if (sleep > 0) { Thread.Sleep((int)sleep); }
        } while (IsStarted);
      }

      /// <summary>
      /// This method is called every tick.
      /// </summary>
      /// <remarks>
      /// This method is abstract, it is not implemented in this class.
      /// </remarks>
      public abstract void OnTick();

      /// <summary>
      /// Halts the thread.
      /// </summary>
      /// <remarks>
      /// This method is public and can be accessed directly.
      /// </remarks>
      public void Halt() { using (_tlock.EnterScope()) { IsRunning = false; } }

      /// <summary>
      /// Resumes the thread.
      /// </summary>
      /// <remarks>
      /// This method is public and can be accessed directly.
      /// </remarks>
      public void Resume() { using (_tlock.EnterScope()) { IsRunning = true; } }

      /// <summary>
      /// Stops the thread.
      /// </summary>
      /// <remarks>
      /// This method is public and can be accessed directly.
      /// </remarks>
      public void Stop() { using (_tlock.EnterScope()) { IsStarted = false; } }
  

    }
}