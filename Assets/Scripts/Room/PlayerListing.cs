using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerListing : MonoBehaviourPunCallbacks
{
	private Dictionary<Player, GameObject> playerList = new Dictionary<Player, GameObject>();

	[SerializeField] private GameObject playerListItemPrefab;

	public override void OnEnable()
	{
		base.OnEnable();
		LoadPlayerList();
	}

	public override void OnDisable()
	{
		//Clear Players
		base.OnDisable();
		foreach (GameObject gameObject in playerList.Values)
			Destroy(gameObject);

		playerList.Clear();
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		//Add Player to list
		Debug.Log($"{newPlayer.NickName} joined");
		InstantiatePlayer(newPlayer);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		//Remove player from list
		Debug.Log($"{otherPlayer.NickName} left");
		Destroy(playerList[otherPlayer]);
		playerList.Remove(otherPlayer);
	}

	private void LoadPlayerList()
    {
		//Load full player list
		foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
			InstantiatePlayer(playerInfo.Value);
	}

	private void InstantiatePlayer(Player player)
	{
		//Instantiate prefab
		GameObject pInfo = Instantiate(playerListItemPrefab, transform);
		pInfo.GetComponentInChildren<Text>().text = player.NickName;
		playerList.Add(player, pInfo);
	}
}
