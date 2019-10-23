using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MyLauncher : MonoBehaviourPunCallbacks
{

    #region PRIVATE SERIALIZABLE FIELDS

    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.")]
    [SerializeField]
    private byte _MaxPlayersPerRoom = 4;
    [Tooltip("The UI Panel to let the user enter name, connect and play")]
    [SerializeField]
    private GameObject _ControlPanel;
    [Tooltip("The UI Label to inform the user that the connection is in progress")]
    [SerializeField]
    private GameObject _ProgressLabel;

    #endregion


    #region PRIVATE FIELDS

    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    private string gameVersion = "1";
    /// <summary>
    /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
    /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
    /// Typically this is used for the OnConnectedToMaster() callback.
    /// </summary>
    private bool _ShouldConnect = false;

    #endregion


    #region MONOBEHAVIOUR CALLBACKS

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    private void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically. 
        PhotonNetwork.ShouldAutomaticallySyncScene = true;
    }

    /// <summary>
    /// Monobehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    private void Start()
    {
        OpenLobby();
    }

    #endregion


    #region PRIVATE METHODS

    private void OpenLobby()
    {
        _ProgressLabel.SetActive(false);
        _ControlPanel.SetActive(true);
    }

    #endregion


    #region PUBLIC METHODS

    /// <summary>
    /// Start the connection process. 
    /// - If already connected, we attempt joining a random room  #RANDOM ROOM MAY NOT FIT GAME
    /// - If not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect()
    {
        // switching UI
        _ControlPanel.SetActive(false);
        _ProgressLabel.SetActive(true);

        // keep track of whether we want to join a room because when we come back from the game we will get a callback that we're connected, so we know what to do then. 
        _ShouldConnect = true;

        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one. 
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    #endregion


    #region MONOBEHAVIOUR PUN CALLBACKS 

    public override void OnConnectedToMaster()
    {
        Debug.Log("MyLauncher : OnConnectedToMaster() was called by PUN");

        // we don't want to do anything if we are not attempting to join a room.
        // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called,
        // in that case, we don't want to do anything.
        if (_ShouldConnect)
        {
            Debug.Log("MyLauncher : OnConnectedToMaster attempting to JoinRandomRoom()");
            // #Critical: the first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
        }
        _ShouldConnect = false;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _ProgressLabel.SetActive(false);
        _ControlPanel.SetActive(true);
        Debug.LogWarningFormat("MyLauncher : OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("MyLauncher : OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = _MaxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("MyLauncher : OnJoinedRoom() called by PUN. Now this client is in a room!");


        // #CRITICAL We only load scene if we are the first player in the room, else we rely on PhotonNetwork.AutomaticallySyncScene to sync our instance scene.
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1 && !PhotonNetwork.OfflineMode)
        {
            Debug.Log("We load the room for 1");

            PhotonNetwork.LoadLevel(1);
        }      
    }

    public override void OnLeftRoom()
    {
        Debug.Log("MyLauncher : Left Room");
    }

    #endregion
}
