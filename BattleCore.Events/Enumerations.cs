/***********************************************************************
*
* NAME:        Enumerations.h
*
* PROJECT:     Battle Core Events Library
*
* COMPILER:    Microsoft Visual Studio .NET 2005
*
* DESCRIPTION: Event Enumerations implementation.
*
* NOTES:       None.
*
* HISTORY:
* DATE      AUTHOR     CHANGES
* --------  ---------  -------------------------------------------------
* 12-29-06  udp        Initial Creation
*
************************************************************************/

// Namespace usage
using System;

// Namespace declaration
namespace BattleCore
{
   /// <summary>
   /// Chat type enumeration 
   /// </summary>
   public enum ChatTypes
   {
      /// <summary>
      /// Arena Message 
      /// </summary>
      Arena,
      /// <summary>
      /// Public Macro Message 
      /// </summary>
      Macro,
      /// <summary>
      /// Public Message 
      /// </summary>
      Public,
      /// <summary>
      /// Team Message 
      /// </summary>
      Team,
      /// <summary>
      /// Private Team Message 
      /// </summary>
      TeamPrivate,
      /// <summary>
      /// Private Message 
      /// </summary>
      Private,
      /// <summary>
      /// Server Warning Message 
      /// </summary>
      Warning,
      /// <summary>
      /// Remote Private Message 
      /// </summary>
      RemotePrivate,
      /// <summary>
      /// Server Message 
      /// </summary>
      Server,
      /// <summary>
      /// Channel Message 
      /// </summary>
      Channel, 
      /// <summary>
      /// Zone Message 
      /// </summary>
      Zone,
      /// <summary>
      /// Help Request
      /// </summary>
      Help,
      /// <summary>
      /// Cheater Message
      /// </summary>
      Cheater
   };

   /// <summary>
   /// Ship type enumeration 
   /// </summary>
   public enum ShipTypes
   {
      /// <summary>
      /// Warbird Ship Type 
      /// </summary>
      Warbird,
      /// <summary>
      /// Javelin Ship Type 
      /// </summary>
      Javelin,
      /// <summary>
      /// Spider Ship Type 
      /// </summary>
      Spider,
      /// <summary>
      /// Leviathon Ship Type 
      /// </summary>
      Leviathon,
      /// <summary>
      /// Terrier Ship Type 
      /// </summary>
      Terrier,
      /// <summary>
      /// Weasel Ship Type 
      /// </summary>
      Weasel,
      /// <summary>
      /// Lancaster Ship Type
      /// </summary>
      Lancaster,
      /// <summary>
      /// Shark Ship Type 
      /// </summary>
      Shark,
      /// <summary>
      /// Spectator Mode 
      /// </summary>
      Spectator 
   };

   /// <summary>
   /// Weapon type enumeration 
   /// </summary>
   public enum WeaponTypes 
   {
      /// <summary>
      /// No Weapon
      /// </summary>
      NoWeapon,
      /// <summary>
      /// Regular Bullet 
      /// </summary>
      Bullet,
      /// <summary>
      /// Bouncing Bullet 
      /// </summary>
      BounceBullet,
      /// <summary>
      /// Regular Bomb 
      /// </summary>
      Bomb,
      /// <summary>
      /// Proximity Bomb 
      /// </summary>
      ProxBomb,
      /// <summary>
      /// Repel 
      /// </summary>
      Repel,
      /// <summary>
      /// Decoy 
      /// </summary>
      Decoy,
      /// <summary>
      /// Burst 
      /// </summary>
      Burst,
      /// <summary>
      /// Thor 
      /// </summary>
      Thor,
      /// <summary>
      /// Inactive 
      /// </summary>
      Inactive,
      /// <summary>
      /// Burst 
      /// </summary>
      Shrapnel,
      /// <summary>
      /// Multifire Bullet 
      /// </summary>
      MultifireBullet,
      /// <summary>
      /// Multifile Bouncing Bullet 
      /// </summary>
      MultiBounceBullet,
      /// <summary>
      /// Mine 
      /// </summary>
      Mine,
      /// <summary>
      /// Proximity Mine 
      /// </summary>
      ProxMine,
      /// <summary>
      /// Bouncing Shrapnel 
      /// </summary>
      BounceShrapnel
   };

   /// <summary>
   /// Moderator Level enumeration.  The bot retrieves the moderator
   /// list from the server by periodicly issuing a *listmod command.
   /// </summary>
   public enum ModLevels 
   {
      /// <summary>
      /// Not a moderator
      /// </summary>
      None,
      /// <summary>
      /// Moderator
      /// </summary>
      Mod,
      /// <summary>
      /// Super Moderator
      /// </summary>
      SMod,
      /// <summary>
      /// Sysop
      /// </summary>
      Sysop, 
      /// <summary>
      /// Custom Moderator
      /// </summary>
      Custom
   };

   /// <summary>
   /// Chat Sound Codes enumeration 
   /// </summary>
   public enum SoundCodes
   {
      /// <summary>
      /// No sound
      /// </summary>
      None = 0,
      /// <summary>
      /// Bass Beep
      /// </summary>
      BassBeep = 1,
      /// <summary>
      /// Trebble Beep 
      /// </summary>
      TrebleBeep = 2,
      /// <summary>
      /// You're not dealing with ATT 
      /// </summary>
      ATT = 3,
      /// <summary>
      /// Parental discretion is advised
      /// </summary>
      Discretion = 4,
      /// <summary>
      /// Hallellula
      /// </summary>
      Hallellula = 5,
      /// <summary>
      /// Ronald Reagan
      /// </summary>
      RonaldReagan = 6,
      /// <summary>
      /// Inconceivable
      /// </summary>
      Inconcievable = 7,
      /// <summary>
      /// Winston Churchill
      /// </summary>
      WinstonChurchill = 8,
      /// <summary>
      /// Listen to me, you pebble farting snot licker
      /// </summary>
      FartingSnotlicker = 9,
      /// <summary>
      /// Crying
      /// </summary>
      BabyCrying = 10,
      /// <summary>
      /// Burp
      /// </summary>
      Burp = 11,
      /// <summary>
      /// Girl
      /// </summary>
      SexyGirl = 12,
      /// <summary>
      /// Scream
      /// </summary>
      GirlScream = 13,
      /// <summary>
      /// Fart
      /// </summary>
      Fart = 14,
      /// <summary>
      /// Fart2
      /// </summary>
      Fart2 = 15,
      /// <summary>
      /// Phone Ring
      /// </summary>
      PhoneRing = 16,
      /// <summary>
      /// The world is under attack at this very moment
      /// </summary>
      WorldUnderAttack = 17,
      /// <summary>
      /// Gibberish
      /// </summary>
      Gibberish = 18,
      /// <summary>
      /// Oooooo
      /// </summary>
      Oooooo = 19,
      /// <summary>
      /// Geeeee
      /// </summary>
      Geeeee = 20,
      /// <summary>
      /// Ohhhhh
      /// </summary>
      Ohhhhh = 21,
      /// <summary>
      /// Ahhhhh
      /// </summary>
      Ahhhhh = 22,
      /// <summary>
      /// This game sucks
      /// </summary>
      ThisGameSucks = 23,
      /// <summary>
      /// Sheep
      /// </summary>
      Sheep = 24,
      /// <summary>
      /// I can't log in!
      /// </summary>
      CantLogin = 25,
      /// <summary>
      /// Message Alarm Beep
      /// </summary>
      MessageAlarm = 26,
      /// <summary>
      /// Start music playing
      /// </summary>
      StartMusic = 100,
      /// <summary>
      /// Stop music
      /// </summary>
      StopMusic = 101,
      /// <summary>
      /// Play music for 1 iteration then stop
      /// </summary>
      PlayMusicOnce = 102,
      /// <summary>
      /// Victory bell
      /// </summary>
      VictoryBell = 103,
      /// <summary>
      /// Goal!
      /// </summary>
      Goal = 104
   };

   /// <summary>
   /// Prize Green Types
   /// </summary>
   public enum PrizeTypes
   {
      /// <summary>
      /// Unknown
      /// </summary>
      Unknown,
      /// <summary>
      /// Recharge
      /// </summary>
      Recharge,
      /// <summary>
      /// Energy
      /// </summary>
      Energy,
      /// <summary>
      /// Rotation
      /// </summary>
      Rotation,
      /// <summary>
      /// Stealth
      /// </summary>
      Stealth,
      /// <summary>
      /// Cloak
      /// </summary>
      Cloak,
      /// <summary>
      /// XRadar
      /// </summary>
      XRadar,
      /// <summary>
      /// Warp
      /// </summary>
      Warp,
      /// <summary>
      /// Guns
      /// </summary>
      Guns,
      /// <summary>
      /// Bombs
      /// </summary>
      Bombs,
      /// <summary>
      /// BounceBullets
      /// </summary>
      BounceBullets,
      /// <summary>
      /// Thruster
      /// </summary>
      Thruster,
      /// <summary>
      /// Speed
      /// </summary>
      Speed,
      /// <summary>
      /// FullCharge
      /// </summary>
      FullCharge,
      /// <summary>
      /// EngineShutdown
      /// </summary>
      EngineShutdown,
      /// <summary>
      /// Multifire
      /// </summary>
      Multifire,
      /// <summary>
      /// Proximity
      /// </summary>
      Proximity,
      /// <summary>
      /// Super
      /// </summary>
      Super,
      /// <summary>
      /// Shields
      /// </summary>
      Shields,
      /// <summary>
      /// Shrapnel
      /// </summary>
      Shrapnel,
      /// <summary>
      /// Antiwarp
      /// </summary>
      Antiwarp,
      /// <summary>
      /// Repel
      /// </summary>
      Repel,
      /// <summary>
      /// Burst
      /// </summary>
      Burst,
      /// <summary>
      /// Decoy
      /// </summary>
      Decoy,
      /// <summary>
      /// Thor
      /// </summary>
      Thor,
      /// <summary>
      /// Multiprize
      /// </summary>
      Multiprize,
      /// <summary>
      /// Brick
      /// </summary>
      Brick,
      /// <summary>
      /// Rocket
      /// </summary>
      Rocket,
      /// <summary>
      /// Portal
      /// </summary>
      Portal		 
   };
}