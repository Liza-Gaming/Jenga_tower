using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    // Singleton instance (optional)
    public static PhotonLobby lobby;

    private void Awake()
    {
        lobby = this;
    }

    void Start()
    {
        // Connects to the Photon Master server using the settings defined in PhotonServerSettings
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
        // Once connected, join the default lobby
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby successfully.");
        // Here you can update your UI to show available rooms or options to create a new room
    }
}
