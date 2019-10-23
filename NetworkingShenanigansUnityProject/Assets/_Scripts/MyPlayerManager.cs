
using UnityEngine;

using Photon.Pun;

public class MyPlayerManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [Tooltip("The current Health of our player")]
    public float _Health = 1f;

    #region PRIVATE FIELDS 

    [Tooltip("The Beams GameObject to control")]
    [SerializeField]
    private GameObject _Beams;
    // True, when the user is firing
    private bool _IsFiring = false;

    #endregion


    #region UNITY CALLBACKS
    
    public override void OnEnable()
    {
        base.OnEnable();

        if (_Beams == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
        }   
        else
        {
            _Beams.SetActive(false);
        }

        MyCameraWork _cameraWork = gameObject.GetComponent<MyCameraWork>();

        if (_cameraWork != null)
        {
            if (photonView.IsMine)
            {
                _cameraWork.StartFollowing();
            }         
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> MyCameraWork component on player prefab.", this);
        }
    }

    void Update()
    {
        // we only process inputs if we are the local player
        if (photonView.IsMine)
        {
            Debug.Log("photon view is mine");

            ProcessInputs();
        }

        if (_Beams != null && _IsFiring != _Beams.activeSelf)
        {
            _Beams.SetActive(_IsFiring);
        }

        //MyGameManager.instance.LeaveRoomWrapper();
    }

    /// <summary>
    /// Affect health of the player if the collider is a beam
    /// Note: when jumping and firing at the same time, you'll find the player's own beam intersects with itself.
    /// One could move the collider further away to prevent this or check if the beam belongs to the player. 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
        {
            return;
        }
        // We are only interested in the Beamers
        // we should be using tags but for the sake of distribution, let's simply check by name.
        if (!other.name.Contains("Beam"))
        {
            return;
        }
        _Health -= 0.1f;
    }

    private void OnTriggerStay(Collider other)
    {
        // we don't do anything if we are not the local player.
        if (!photonView.IsMine)
        {
            return;
        }
        // We are only interested in beamers
        // we should be using tags but for the sake of distribution, let's imply check by name.
        if (!other.name.Contains("Beam"))
        {
            return;
        }
        // we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
        _Health -= 0.1f * Time.deltaTime;
    }

    #endregion


    #region CUSTOM

    /// <summary>
    /// Process the inputs. Maintain a flag for when the user is pressing Fire.
    /// </summary>
    void ProcessInputs()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("FireDown");
            if (_IsFiring == false)
            {
                _IsFiring = true;
            }
        }
        if (Input.GetButtonUp("Fire1"))
        { 
            Debug.Log("FireUp");

            if (_IsFiring)
            {

                _IsFiring = false;
            }
        }
    }

    #endregion


    #region IPunObservable implementation

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(this._IsFiring);
            stream.SendNext(this._Health);
        }
        else
        {
            // Network player, receive data
            this._IsFiring = (bool)stream.ReceiveNext();
            this._Health = (float)stream.ReceiveNext();
        }
    }

    #endregion
}
