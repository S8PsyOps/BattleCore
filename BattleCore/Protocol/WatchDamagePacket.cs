//-----------------------------------------------------------------------
//
// NAME:        WatchDamagePacket.cs
//
// PROJECT:     Battle Core Protocol Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Watch Damage Packet implementation.
//
// NOTES:       None.
//
// $History: WatchDamagePacket.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using System.Collections.Generic;
using System.Text;
using BattleCore.Events;
using System.IO;

// Namespace declaration
namespace BattleCore.Protocol
{
   /// <summary>
   /// Watch Damage Packet
   /// </summary>
   internal class WatchDamagePacket
   {
      /// <summary>Watch Damage Packet Event Data</summary>
      private List<WatchDamageEvent> m_events = new List<WatchDamageEvent>();

      ///<summary>Reliable Packet Property</summary>
      public Boolean Reliable { get { return false; } }

      /// <summary>
      /// Watch Damage Event Property
      /// </summary>
      public List<WatchDamageEvent> Events
      {
         set { m_events = value; }
         get { return m_events; }
      }

      /// <summary>
      /// Packet Data Property
      /// </summary>
      public Byte[] Packet
      {
         set
         {
            // Get the player identifier
            UInt16 nPlayerId = BitConverter.ToUInt16 (value, 1);

            // Get the damage Timestamp
            UInt32 nTimeStamp = BitConverter.ToUInt32 (value, 3);

            // Calculate the size and start of the data payload
            int nPayloadSize  = (value.Length - 7);
            int nPayloadStart = 7;

            // Create the weapon information
            WeaponInfo weaponInfo = new WeaponInfo();

            // Check if the payload has the correct length
            if ((nPayloadSize >= 9) && ((nPayloadSize % 9) == 0))
            {
               int nLength = nPayloadSize / 9;

               // Set the damage event data from the packet
               for (UInt32 nIndex = 0; nIndex < nLength; nIndex++)
               {
                  WatchDamageEvent damageEvent = new WatchDamageEvent ();
                  // Set the watchdamage event information
                  damageEvent.PlayerId = nPlayerId;
                  damageEvent.TimeStamp = nTimeStamp;

                  // Get teh attacker identifier
                  damageEvent.AttackerId = BitConverter.ToUInt16 (value, nPayloadStart);
                  
                  // Get the weapon information
                  weaponInfo.Value = BitConverter.ToUInt16 (value, nPayloadStart + 2);
                  damageEvent.Weapon = weaponInfo.Type;

                  // Set the energy and damage
                  damageEvent.Energy = BitConverter.ToUInt16 (value, nPayloadStart + 4);
                  damageEvent.Damage = BitConverter.ToUInt16 (value, nPayloadStart + 6);

                  m_events.Add (damageEvent);

                  // next payload value
                  nPayloadStart += 9;
               }
            }
         }

         get
         {
            return null;
         }
      }
   }
}
