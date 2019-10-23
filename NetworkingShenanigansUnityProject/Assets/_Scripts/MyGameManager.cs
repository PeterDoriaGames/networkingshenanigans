using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using Photon.Pun;
using Photon.Realtime;

public class MyGameManager : MonoBehaviourPunCallbacks
{
    public static MyGameManager instance;

    #region PRIVATE METHODS

    // Relies on Photon rather than Unity to load this level on all connected clients in the room, since we've enabled PhotonNetwork.AutomaticallySyncScene for this game.  
    // PhotonNetwork.LoadLevel() should only be called if we are the MasterClient. It will be the responsibility of the caller to also check for this. 
    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);

        PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    #endregion 


    #region  PUBLIC METHODS

    public override void OnEnable()
    {
        base.OnEnable();
        if (instance == null)
            instance = this;
    }
    public override void OnDisable()
    {
        base.OnDisable();
        instance = null;
    }

    /// <summary>
    /// Makes network player leave the Photon Network room. May want to implement saving data or confirmation step that the user will be leaving the game.
    /// </summary>
    public void LeaveRoomWrapper()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion


    #region PHOTON CALLBACKS

    /// <summary>
    /// Called when the local user/client left a room, so the game's logic can clean up it's internal state.
    /// </summary>
    /// <remarks>
    /// When leaving a room, the LoadBalancingClient will disconnect the Game Server and connect to the Master Server.
    /// This wraps up multiple internal actions.
    ///
    /// Wait for the callback OnConnectedToMaster, before you use lobbies and join or create rooms.
    /// </remarks>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Called when a remote player entered the room. This Player is already added to the playerlist.
    /// </summary>
    /// <remarks>
    /// If your game starts with a certain number of players, this callback can be useful to check the
    /// Room.playerCount and find out if you can start.
    /// </remarks>
    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            LoadArena();
        }
    }

    /// <summary>
    /// Called when a remote player left the room or became inactive. Check otherPlayer.IsInactive.
    /// </summary>
    /// <remarks>
    /// If another player leaves the room or if the server detects a lost connection, this callback will
    /// be used to notify your game logic.
    ///
    /// Depending on the room's setup, players may become inactive, which means they may return and retake
    /// their spot in the room. In such cases, the Player stays in the Room.Players dictionary.
    ///
    /// If the player is not just inactive, it gets removed from the Room.Players dictionary, before
    /// the callback is called.
    /// </remarks>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", otherPlayer.NickName);

        if (PhotonNetwork.IsMasterClient )
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            LoadArena();
        }
    }

    #endregion

}
