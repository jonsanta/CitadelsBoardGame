using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class Room : MonoBehaviourPunCallbacks
{
	[SerializeField] private GameObject roomCanvas;
	[SerializeField] private Text _roomName;

	//Photon Message constants
	private const byte ADD_PLAYER_CODE = 1;
	private const byte REMOVE_PLAYER_CODE = 2;

	private static Room room;

    private void Awake()
    {
		//Unique Object
		if (room == null)
			room = this;
		else
			Destroy(transform.parent.gameObject);
    }

    public override void OnJoinedRoom()
	{
		//Show Room UI
		Show();
		if (!PhotonNetwork.IsMasterClient)
		{
			//Send Player data to Master
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
			PhotonNetwork.RaiseEvent(ADD_PLAYER_CODE, new string[] { PhotonNetwork.LocalPlayer.UserId, PhotonNetwork.LocalPlayer.NickName}, raiseEventOptions, SendOptions.SendUnreliable);
        }
        else
        {
			//Add Master Player data 
			GameObject master = new GameObject("Master");
			master.tag = "Master";
			master.AddComponent<GameMaster>();
			master.GetComponent<GameMaster>().AddPlayer(PhotonNetwork.LocalPlayer);
			Instantiate(master);
		}
	}

	public override void OnCreatedRoom()
	{
		Debug.Log("Sala creada correctamente");
		Show();
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		Debug.Log("No se ha podido crear la sala" + message);
	}

	public void OnClick_CreateRoom()
	{
		if (PhotonNetwork.IsConnected)
		{
			//Create Room
			RoomOptions options = new RoomOptions();
			options.MaxPlayers = 8;
			PhotonNetwork.JoinOrCreateRoom(_roomName.text, options, TypedLobby.Default);
		}
	}

	public void OnClick_LeaveRoom()
	{
		Debug.Log("Has salido de la sala");
		if (PhotonNetwork.InRoom == true && !PhotonNetwork.IsMasterClient)
		{
			//Inform Master that Player left room
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
			PhotonNetwork.RaiseEvent(REMOVE_PLAYER_CODE, new string[]{ PhotonNetwork.LocalPlayer.UserId}, raiseEventOptions, SendOptions.SendUnreliable);
		}
		PhotonNetwork.LeaveRoom(true);
		Hide();
	}

	public void Show()
	{
		roomCanvas.SetActive(true);
	}

	public void Hide()
	{
		roomCanvas.SetActive(false);
	}
}
