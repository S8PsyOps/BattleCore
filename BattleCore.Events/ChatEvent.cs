//-----------------------------------------------------------------------
//
// NAME:        ChatEvent.cs
//
// PROJECT:     Battle Core Events Library
//
// COMPILER:    Microsoft Visual Studio .NET 2005
//
// DESCRIPTION: Chat Event implementation.
//
// NOTES:       None.
//
// $History: ChatEvent.cs $
//
//-----------------------------------------------------------------------

// Namespace usage
using System;
using BattleCore;

// Namespace declaration
namespace BattleCore.Events
{
   /// <summary>
   /// ChatEvent object.  This event is used to receive chat messages
   /// from the server, and send chat messages to the server.
   /// </summary>
   /// <remarks>
   /// <para>
   /// Usage: Implement this in a method to handle chat events.</para>
   /// <code lang="C#" escaped="true">
   /// public void OnChatEvent (object sender, ChatEvent e) 
   /// { 
   ///    ... 
   /// }
   /// </code>
   /// <para>
   /// To send a chat mesage to the server, create the ChatEvent object</para>
   /// <code lang="C#" escaped="true">
   /// ChatEvent e = new ChatEvent();
   /// e.Message = "Hello World";
   /// e.ChatType = ChatTypes.Public;
   /// </code>
   /// <para>
   /// Then send it by calling: </para>
   /// <code>SendGameEvent (e);</code>
   /// </remarks>
   public class ChatEvent : EventArgs
   {
      String     m_strMessage = "";             // Chat message
      String     m_strPlayerName = "";          // Player Name
      ChatTypes  m_chatType = ChatTypes.Public; // Chat Type enumeration
      SoundCodes m_soundCode = SoundCodes.None; // Sound Code enumeration
      UInt16     m_nPlayerId = 0xFFFF;          // Player identifier
      ModLevels  m_modLevel = ModLevels.None;   // Moderator Level
      String     m_strCustomMod;                // If ModLevel is custom - stor etype here

      ///<summary>The Message Property represents the chat message</summary>
      /// <value>The Message property gets/sets the message string data member.</value>
      public String Message
      {
         set { m_strMessage = value; }
         get { return m_strMessage; }
      }

      ///<summary>The PlayerName Property represents the player name</summary>
      /// <value>The PlayerName property gets/sets the player name data member.</value>
      /// <remarks>
      /// <para>
      /// When sending a chat event to the server, this value may be used to identify
      /// the receiving player in a private chat message</para>
      /// </remarks>
       public String PlayerName 
      {
         set { m_strPlayerName = value; }
         get { return m_strPlayerName; }
      }

      ///<summary>The ChatType Property represents the ChatTypes value <seealso cref="ChatTypes"/></summary>
      /// <value>The ChatType property gets/sets the chat type data member.</value>
      ///<summary>Chat Type Property</summary>
      public ChatTypes ChatType 
      {
         set { m_chatType = value; }
         get { return m_chatType; }
      }

      ///<summary>The SoundCode Property represents the SoundCodes value <seealso cref="SoundCodes"/></summary>
      /// <value>The SoundCode property gets/sets the sound code data member.</value>
      ///<summary>Sound Code Property</summary>
      public SoundCodes SoundCode 
      {
         set { m_soundCode = value; }
         get { return m_soundCode; }
      }

      ///<summary>The PlayerId Property represents the player identifier value</summary>
      /// <value>The PlayerId property gets/sets the player identifier data member.</value>
      /// <remarks>
      /// <para>
      /// When sending a chat event to the server, this value may be used to identify
      /// the receiving player in a private chat message</para>
      /// </remarks>
      public UInt16 PlayerId 
      {
         set { m_nPlayerId = value; }
         get { return m_nPlayerId; }
      }

      ///<summary>The ModLevel Property represents the player moderator level</summary>
      /// <value>The PlayerId property gets/sets the player identifier data member.</value>
      public ModLevels ModLevel 
      {
         set { m_modLevel = value; }
         get { return m_modLevel; }
      }
      ///<summary>The Message Property represents the chat message</summary>
      /// <value>The Message property gets/sets the message string data member.</value>
      public String CustomMod
      {
          set { m_strCustomMod = value; }
          get { return m_strCustomMod; }
      }
   }
}
