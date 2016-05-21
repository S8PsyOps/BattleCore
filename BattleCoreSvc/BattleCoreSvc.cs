using System;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Threading;

namespace BattleCoreService
{
   class BattleCoreSvc : ServiceBase
   {
      private System.ComponentModel.IContainer components;
      private Process coreProcess;
      /// <summary>
      /// Battle Core handler
      /// </summary>
      private BattleCore.BattleCore m_alphaCore = new BattleCore.BattleCore ();

      /// <summary>
      /// Public Constructor for WindowsService.
      /// - Put all of your Initialization code here.
      /// </summary>
      public BattleCoreSvc ()
      {

         this.ServiceName = "BattleCore Management Service";
         this.EventLog.Source = "BattleCore Management Service";
         this.EventLog.Log = "Application";

         // These Flags set whether or not to handle that specific
         //  type of event. Set to true if you need it, false otherwise.
         this.CanHandlePowerEvent = true;
         this.CanHandleSessionChangeEvent = true;
         this.CanPauseAndContinue = true;
         this.CanShutdown = true;
         this.CanStop = true;

         if (!EventLog.SourceExists ("BattleCore Management Service"))
            EventLog.CreateEventSource ("BattleCore Management Service", "Application");

         // Initialize the components
         InitializeComponent ();
      }

      /// <summary>
      /// The Main Thread: This is where your Service is Run.
      /// </summary>
      static void Main ()
      {
         ServiceBase.Run (new BattleCoreSvc ());
      }

      /// <summary>
      /// Dispose of objects that need it here.
      /// </summary>
      /// <param name="disposing">Whether or not disposing is going on.</param>
      protected override void Dispose (bool disposing)
      {
         base.Dispose (disposing);
      }

      /// <summary>
      /// OnStart: Put startup code here
      ///  - Start threads, get inital data, etc.
      /// </summary>
      /// <param name="args"></param>
      protected override void OnStart (string[] args)
      {
         // Send the event to the core
         m_alphaCore.OnStart (args);

         // Call to the base class
         base.OnStart (args);
      }

      /// <summary>
      /// OnStop: Put your stop code here
      /// - Stop threads, set final data, etc.
      /// </summary>
      protected override void OnStop ()
      {
         // Send the event to the core
         m_alphaCore.OnStop ();

         // call to the base class
         base.OnStop ();
      }

      /// <summary>
      /// OnPause: Put your pause code here
      /// - Pause working threads, etc.
      /// </summary>
      protected override void OnPause ()
      {
         // Send the event to the core
         m_alphaCore.OnPause ();

         base.OnPause ();
      }

      /// <summary>
      /// OnContinue: Put your continue code here
      /// - Un-pause working threads, etc.
      /// </summary>
      protected override void OnContinue ()
      {
         // Send the event to the core
         m_alphaCore.OnContinue ();

         base.OnContinue ();
      }

      /// <summary>
      /// OnShutdown(): Called when the System is shutting down
      /// - Put code here when you need special handling
      ///   of code that deals with a system shutdown, such
      ///   as saving special data before shutdown.
      /// </summary>
      protected override void OnShutdown ()
      {
         // Send the event to the core
         m_alphaCore.OnShutdown ();

         base.OnShutdown ();
      }

      /// <summary>
      /// OnCustomCommand(): If you need to send a command to your
      ///   service without the need for Remoting or Sockets, use
      ///   this method to do custom methods.
      /// </summary>
      /// <param name="command">Arbitrary Integer between 128 & 256</param>
      protected override void OnCustomCommand (int command)
      {
         //  A custom command can be sent to a service by using this method:
         //#  int command = 128; //Some Arbitrary number between 128 & 256
         //#  ServiceController sc = new ServiceController("NameOfService");
         //#  sc.ExecuteCommand(command);

         // Send the event to the core
         m_alphaCore.OnCustomCommand (command);

         base.OnCustomCommand (command);
      }

      /// <summary>
      /// OnPowerEvent(): Useful for detecting power status changes,
      ///   such as going into Suspend mode or Low Battery for laptops.
      /// </summary>
      /// <param name="powerStatus">The Power Broadcase Status (BatteryLow, Suspend, etc.)</param>
      protected override bool OnPowerEvent (PowerBroadcastStatus powerStatus)
      {
         // Send the event to the core
         m_alphaCore.OnPowerEvent (powerStatus);

         return base.OnPowerEvent (powerStatus);
      }

      /// <summary>
      /// OnSessionChange(): To handle a change event from a Terminal Server session.
      ///   Useful if you need to determine when a user logs in remotely or logs off,
      ///   or when someone logs into the console.
      /// </summary>
      /// <param name="changeDescription"></param>
      protected override void OnSessionChange (SessionChangeDescription changeDescription)
      {
         // Send the event to the core
         m_alphaCore.OnSessionChange (changeDescription);

         base.OnSessionChange (changeDescription);
      }

      private void InitializeComponent ()
      {
         this.components = new System.ComponentModel.Container ();
         this.coreProcess = new System.Diagnostics.Process ();
         // 
         // coreProcess
         // 
         this.coreProcess.StartInfo.Domain = "";
         this.coreProcess.StartInfo.LoadUserProfile = false;
         this.coreProcess.StartInfo.Password = null;
         this.coreProcess.StartInfo.StandardErrorEncoding = null;
         this.coreProcess.StartInfo.StandardOutputEncoding = null;
         this.coreProcess.StartInfo.UserName = "";
      }
   }
}
