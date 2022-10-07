using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class Connection : MonoBehaviourPunCallbacks
{
	[SerializeField] private InputField nameInputField = null;
	[SerializeField] private GameObject lobbyPrefab = null;

	private const string PlayerPrefsNameKey = "Name";

	//fill nameInputField with playerName from PlayerPrefs
	private void Start(){
		nameInputField.text = !string.IsNullOrEmpty(PlayerPrefs.GetString(PlayerPrefsNameKey)) ? PlayerPrefs.GetString(PlayerPrefsNameKey) : "";
	}

	public void OnButtonPressed()
	{
        if (!string.IsNullOrEmpty(nameInputField.text))
        {
			//Save new name on PlayerPrefs
			PlayerPrefs.SetString(PlayerPrefsNameKey, nameInputField.text);
			//Connect to Game server
			Debug.Log("Conectando..");
			PhotonNetwork.AutomaticallySyncScene = true;
			PhotonNetwork.NickName = PlayerPrefs.GetString(PlayerPrefsNameKey);
			PhotonNetwork.GameVersion = "1.0";
			PhotonNetwork.ConnectUsingSettings();
		}
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log($"Bienvenido {PhotonNetwork.LocalPlayer.NickName}");
		if (!PhotonNetwork.InLobby)
		{
			PhotonNetwork.JoinLobby();
		}
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Debug.Log("Desconectado del servidor - error" + cause.ToString());
	}

	public override void OnJoinedLobby()
	{
		Instantiate(lobbyPrefab);
	}
}
