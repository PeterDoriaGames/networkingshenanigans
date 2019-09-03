using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

public class MyPlayerNameInputField : MonoBehaviour
{
    #region PRIVATE CONSTANTS

    //Store the PlayerPref Key to avoid typos
    const string playerNamePrefKey = "PlayerName";

    #endregion

    #region MONOBEHAVIOUR CALLBACKS

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    private void Start()
    {
        string defaultName = string.Empty;
        InputField _inputField = this.GetComponent<InputField>();
        if (_inputField == null)
        {
            gameObject.AddComponent<MyPlayerNameInputField>();
        }

        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                _inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }

    #endregion

    #region PUBLIC METHODS

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value">The name of the Player</param>
    public void SetPlayerName(string value)
    {
        // #Important
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }
        PhotonNetwork.NickName = value;


        PlayerPrefs.SetString(playerNamePrefKey, value);
    }

    

    #endregion


}
