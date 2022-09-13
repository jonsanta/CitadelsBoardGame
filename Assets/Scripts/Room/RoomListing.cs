using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListing : MonoBehaviourPunCallbacks
{
	[SerializeField] private GameObject roomListItemPrefab;

	private Dictionary<RoomInfo, GameObject> rooms = new Dictionary<RoomInfo, GameObject>();

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{

		foreach (RoomInfo info in roomList)
		{
			if (info.PlayerCount > 0)
            {
				string name = $"{info.Name} - {info.PlayerCount} / {info.MaxPlayers} players";
				if (rooms.ContainsKey(info))
                {
					//Update Room Info on UI Room Listing
					rooms[info].GetComponentInChildren<Text>().text = name;
				}
				else
                {
					//Add Room To UI Room Listing
					GameObject roomInfoUI = Instantiate(roomListItemPrefab, transform);
					roomInfoUI.GetComponentInChildren<Text>().text = name;
					roomInfoUI.GetComponent<Button>().onClick.AddListener(() => {
						PhotonNetwork.JoinRoom(info.Name);
						Debug.Log("JoinedRoom");
					});

					rooms.Add(info, roomInfoUI);
				}
			}
			else if(rooms.ContainsKey(info))
            {
				//Remove Room from UI Room Listing
				Destroy(rooms[info]);
				rooms.Remove(info);
			}
		}
	}
}
