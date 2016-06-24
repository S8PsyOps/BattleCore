//-----------------------------------------------------------------------
//
// NAME:        BattleCoreTest.cs
//
// PROJECT:     Battle Core Test Application
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Battle Core Test implementation.
//
// NOTES:       None.
//
// $History: BattleCoreTest.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using BattleCore;

// Namespace definition
namespace BattleCoreTest
{
   /// <summary>
   /// BattleCore test application main form
   /// </summary>
   public partial class BattleCoreTest : Form
   {
      // Create a private instance of the core
      private BattleCore.BattleCore m_core = new BattleCore.BattleCore ();

      /// <summary>
      /// Default Constructor
      /// </summary>
      public BattleCoreTest ()
      {
         // Initialize the form components
         InitializeComponent ();
         m_core.OnStart(null);
      }

      /// <summary>
      /// Method called when the Start button is pressed
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="e">Event Arguments</param>
      private void btnStart_Click (object sender, EventArgs e)
      {
         // Notify the core to start
         m_core.OnStart (null);
      }

      /// <summary>
      /// Method called when the Stop button is pressed
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="e">Event Arguments</param>
      private void btnStop_Click (object sender, EventArgs e)
      {
         // Notify the core to stop
         m_core.OnStop ();
      }

      /// <summary>
      /// Event triggered when the form is closing
      /// </summary>
      /// <param name="sender">Sending object</param>
      /// <param name="e">Event Arguments</param>
      private void BattleCoreTest_FormClosing (object sender, FormClosingEventArgs e)
      {
         // Notify the core to stop
         m_core.OnStop ();
      }
   }
}