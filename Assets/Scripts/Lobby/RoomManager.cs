using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class RoomManager : MonoBehaviourPunCallbacks
{
    // Create a room with a given name and max player count
    public void CreateRoom(string roomName)
    {
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 20 };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    // Join a room by name
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    // This callback is called when the room is successfully joined
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        // Optionally load a new scene for the multiplayer game here
         PhotonNetwork.LoadLevel("SampleScene");
    }

    // Callback for failed room creation
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room creation failed: " + message);
    }
}
