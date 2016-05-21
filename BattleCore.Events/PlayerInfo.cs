//-----------------------------------------------------------------------
//
// NAME:        PlayerInfo.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Player Information implementation.
//
// NOTES:       None.
//
// $History: PlayerInfo.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using BattleCore;

// Namespace declaration
namespace BattleCore.Events
{
   /// <summary>
   /// Object contianing all known information about a player
   /// </summary>
   public class PlayerInfo
   {
      #region Private Player Information Member Data
      private String m_playerName;       // Player name
      private String m_IP;               // Player IP      - set by custom Mod
      private String m_UserID;           // Player User ID - set by custom Mod
      private String m_MacID;            // Player Mac ID  - set by custom Mod
      private String m_CustomMod;        // Player Custom 
      private String m_squadName;        // Squad name
      private ShipTypes m_shipType;         // Ship type    
      private ModLevels m_modLevel;         // Moderator status
      private UInt16 m_nPlayerId;        // Player identifier
      private Boolean m_bAcceptsAudio;    // Accepts Audio messages
      private UInt32 m_nFlagPoints;      // Flag Points
      private UInt32 m_nKillPoints;      // Kill Points
      private UInt16 m_nFrequency;       // Player Frequency
      private UInt16 m_nWins;            // Number of wins
      private UInt16 m_nLosses;          // Number of losses
      private UInt16 m_nPositionX;       // X map position
      private UInt16 m_nPositionY;       // Y map position
      private Byte m_nShipRotation;    // Player ship Rotation
      private UInt16 m_nBounty;          // Bounty
      private UInt16 m_nEnergy;          // Energy Level
      private UInt16 m_nPing;            // Player Ping Time
      private UInt16 m_nVelocityX;       // Player X Velocity
      private UInt16 m_nVelocityY;       // Player Y Velocity
      private UInt16 m_nS2CLag;          // Server to client lag
      private UInt16 m_nTurretPlayerId;  // Id of the turret player
      private UInt16 m_nFlagsCarried;    // Number of flags carried
      private Boolean m_bHasKOTH;         // King of the hill state
      private ItemInfo m_ItemInfo = new ItemInfo();
      private WeaponInfo m_WeaponInfo = new WeaponInfo ();
      private ShipStateInfo m_ShipStateInfo = new ShipStateInfo();
      #endregion

      /// <summary>
      /// Property to update the player information with the 
      /// player entered event.
      /// </summary>
      public PlayerEnteredEvent PlayerEntered
      {
         set
         {
            m_playerName = value.PlayerName;
            m_squadName = value.SquadName;
            m_shipType = value.ShipType;
            m_nPlayerId = value.PlayerId;
            m_bAcceptsAudio = value.AcceptsAudio;
            m_nFlagPoints = value.FlagPoints;
            m_nKillPoints = value.KillPoints;
            m_nFrequency = value.Frequency;
            m_nWins = value.Wins;
            m_nLosses = value.Losses;
            m_nTurretPlayerId = value.TurretPlayerId;
            m_nFlagsCarried = value.FlagsCarried;
            m_bHasKOTH = value.HasKOTH;
         }
      }

      /// <summary>
      /// Score Update event Property
      /// </summary>
      public ScoreUpdateEvent ScoreUpdate
      {
         set
         {
            m_nFlagPoints = value.FlagPoints;
            m_nKillPoints = value.KillPoints;
            m_nWins = value.Wins;
            m_nLosses = value.Losses;
         }
      }

      /// <summary>
      /// Player position event property
      /// </summary>
      public PlayerPositionEvent Position
      {
         set
         {
            m_nPositionX = value.MapPositionX;
            m_nPositionY = value.MapPositionY;
            m_nShipRotation = value.ShipRotation;
            m_nBounty = value.Bounty;
            m_nEnergy = value.Energy;
            m_nPing = value.Ping;
            m_nVelocityX = value.VelocityX;
            m_nVelocityY = value.VelocityY;
            m_nS2CLag = value.ServerToClientLag;
            m_ItemInfo.Value = value.Items.Value;
            m_WeaponInfo.Value = value.Weapon.Value;
            m_ShipStateInfo.Value = value.ShipState.Value;
         }

         get
         {
            PlayerPositionEvent position = new PlayerPositionEvent ();

            position.MapPositionX = m_nPositionX; 
            position.MapPositionY = m_nPositionY;
            position.ShipRotation = m_nShipRotation;
            position.Bounty = m_nBounty;
            position.Energy = m_nEnergy; 
            position.Ping = m_nPing;
            position.VelocityX = m_nVelocityX; 
            position.VelocityY = m_nVelocityY;
            position.ServerToClientLag = m_nS2CLag;
            position.Items.Value = m_ItemInfo.Value;
            position.ShipState.Value = m_ShipStateInfo.Value;
            position.Weapon.Value = m_WeaponInfo.Value;

            return position;
         }
      }

      /// <summary>
      /// Player Name Property
      /// </summary>
      public String PlayerName { get { return m_playerName; } }

      /// <summary>
      /// Squad Name property
      /// </summary>
      public String SquadName { get { return m_squadName; } }

      /// <summary>
      /// Ship State information 
      /// </summary>
      public ShipStateInfo ShipState { get { return m_ShipStateInfo; } }

      /// <summary>
      /// Items information 
      /// </summary>
      public ItemInfo Items { get { return m_ItemInfo; } }

      /// <summary>
      /// Ship Type Property
      /// </summary>
      public ShipTypes Ship
      {
         set { m_shipType = value; }
         get { return m_shipType; }
      }

      /// <summary>
      /// Player Identifier Property
      /// </summary>
      public UInt16 PlayerId
      {
         set { m_nPlayerId = value; }
         get { return m_nPlayerId; }
      }

      /// <summary>
      /// Player Weapon Property
      /// </summary>
      public WeaponInfo Weapon
      {
          set { m_WeaponInfo = value; }
          get { return m_WeaponInfo; }
      }

      /// <summary>
      /// Accepts Audio Propery
      /// </summary>
      public Boolean AcceptsAudio
      {
         set { m_bAcceptsAudio = value; }
         get { return m_bAcceptsAudio; }
      }

      /// <summary>
      /// Flag Points Propery
      /// </summary>
      public UInt32 FlagPoints
      {
         set { m_nFlagPoints = value; }
         get { return m_nFlagPoints; }
      }

      /// <summary>
      /// Kill Points Propery
      /// </summary>
      public UInt32 KillPoints
      {
         set { m_nKillPoints = value; }
         get { return m_nKillPoints; }
      }

      /// <summary>
      /// Frequency Propery
      /// </summary>
      public UInt16 Frequency
      {
         set { m_nFrequency = value; }
         get { return m_nFrequency; }
      }

      /// <summary>
      /// Wins Propery
      /// </summary>
      public UInt16 Wins
      {
         set { m_nWins = value; }
         get { return m_nWins; }
      }

      /// <summary>
      /// Losses Propery
      /// </summary>
      public UInt16 Losses
      {
         set { m_nLosses = value; }
         get { return m_nLosses; }
      }

      /// <summary>
      /// Turret Player Id Propery
      /// </summary>
      public UInt16 TurretId
      {
         set { m_nTurretPlayerId = value; }
         get { return m_nTurretPlayerId; }
      }

      /// <summary>
      /// Flags Carried Propery
      /// </summary>
      public UInt16 FlagsCarried
      {
         set { m_nFlagsCarried = value; }
         get { return m_nFlagsCarried; }
      }

      /// <summary>
      /// HasKOTH Propery
      /// </summary>
      public Boolean HasKOTH
      {
         set { m_bHasKOTH = value; }
         get { return m_bHasKOTH; }
      }

      /// <summary>
      /// Moderator Level Propery
      /// </summary>
      public ModLevels ModeratorLevel
      {
         set { m_modLevel = value; }
         get { return m_modLevel; }
      }

      /// <summary>
      /// UserID Propery
      /// </summary>
      public String UserID
      {
          set { m_UserID = value; }
          get { return m_UserID; }
      }

      /// <summary>
      /// MacID Propery
      /// </summary>
      public String MacID
      {
          set { m_MacID = value; }
          get { return m_MacID; }
      }

      /// <summary>
      /// IP Propery
      /// </summary>
      public String IP
      {
          set { m_IP = value; }
          get { return m_IP; }
      }
      /// <summary>
      /// Custom Mod Propery
      /// </summary>
      public String CustomMod
      {
          set { m_CustomMod = value; }
          get { return m_CustomMod; }
      }
   }
}
