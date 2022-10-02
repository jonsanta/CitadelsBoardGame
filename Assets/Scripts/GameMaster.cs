using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Linq;

//Master Game controll class (Only 1 for each room)
public class GameMaster : MonoBehaviour
{
	//Photon Message constants
	private const byte ADD_PLAYER = 1;
	private const byte REMOVE_PLAYER = 2;
	private const byte PHASE_END = 3;
	private const byte GIVE_TURN = 4;
	private const byte GIVE_CARDS = 5;
	private const byte ASK_CARDS = 6;
	private const byte KILL = 7;
	private const byte CLEAR_CHARACTERS = 10;
	private const byte RETURN_CARD = 11;

	private Dictionary<int, int> numPlayers = new(){ { 2, 4 }, { 3, 3 }, { 4, 2 }, { 5, 1 }, { 6, 0 }, { 7, 0 }, { 8, 0 } }; //num of player / Discarded characters

	private Dictionary<string , string> playerList = new(); //player ID and Player name
	private static List<string> order; //Stores player playing order - Random order every new game
	private Queue<string> deck; //Deck
	//private int dealed = 0; //Dealed amount of cards

	private static GameMaster gameMaster;

	private int turn = 0; //switch between players during phases
	private string phase = "phase1"; //Turn has 2 Phases (Phase 1 : Character Selection, Phase 2: Turn Actions)

	private Dictionary<string, string> selectedCharacters = new(); //Stores character name - Player ID
	private List<string> discardedCharacters = new(); //Stores round discarded characters

	private string crown = ""; //Contains the user id of the player who owns the crown
	private string dead = ""; //Contains dead character's Sprite name

	private void Awake()
	{
		//This script Object is unique and only for Game Master
		if (gameMaster == null)
			gameMaster = this;
		else
			Destroy(gameObject);

		DontDestroyOnLoad (this);

		//Load Game Scene, shuffle Player List & Deck
		GameObject.FindGameObjectWithTag("StartGame").GetComponent<Button>().onClick.AddListener(() => {
			if (playerList.Count >= 2)
			{
				order = playerList.Keys.ToList();
				order.Sort((a, b) => 1 - 2 * Random.Range(0, 2));
				List<string> temp = Deck.Get();
				temp.Sort((a, b) => 1 - 2 * Random.Range(0, 2));
				deck = new Queue<string>(temp);

				//Start game & close Room
				PhotonNetwork.CurrentRoom.IsOpen = false;
				PhotonNetwork.CurrentRoom.IsVisible = false;
				SceneManager.LoadScene(1);
			}
		});
	}

	public void AddPlayer(Player player)
    {
		playerList.Add(player.UserId, player.NickName);
    }

	//Get x cards from deck
	private string[] GetCards(int num)
	{
		string[] cardArr = new string[num];
		for (int i = 0; i < num; i++)
		{
			cardArr[i] = deck.Dequeue();
		}
		return cardArr;
	}

	private void discardCharacters()
    {
		discardedCharacters.Clear();
		discardedCharacters.Add(Random.Range(1, 9).ToString());
		while (discardedCharacters.Count < numPlayers[playerList.Count] + 1)
		{
			int aux = Random.Range(1, 9);
			if (aux != 4 && !discardedCharacters.Contains(aux.ToString()))
				discardedCharacters.Add(aux.ToString()); 
		}
		string[] arr = new string[] { crown, phase.ToString()};
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
		PhotonNetwork.RaiseEvent(GIVE_TURN, arr.Concat(discardedCharacters.ToArray()).ToArray(), raiseEventOptions, SendOptions.SendUnreliable); //Give turn to crown player
	}

	//Character selection phase
	private void selectionPhase()
    {
		List<string> newOrder = order;
		newOrder.Remove(crown); //remove crown player from list
		newOrder.Insert(0, crown); //set crown player as first
		string[] arr = new string[] { newOrder[turn], phase}; //array containing userID and current phase
		arr = arr.Concat(discardedCharacters.ToArray()).ToArray();

		//Send Phase 1 turn to Player
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
		PhotonNetwork.RaiseEvent(GIVE_TURN, arr.Concat(selectedCharacters.Keys.ToArray()).ToArray(), raiseEventOptions, SendOptions.SendUnreliable);
	}

	private void actionsPhase()
    {
		List<int> orders = selectedCharacters.Keys.ToList().Select(s => int.Parse(s)).ToList();
		orders.Sort(); //Play order sorted by game rules character order (Assassin -> Thief --> Mage --> King --> Bishop --> Merchant --> Architect --> Warrior)

		if (dead == orders[turn].ToString()) //if current turn player is dead lose turn
        {
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient};
			PhotonNetwork.RaiseEvent(PHASE_END, "", raiseEventOptions, SendOptions.SendUnreliable);
		}
        else
        {
			if (orders[turn].ToString() == "4")
				crown = selectedCharacters[orders[turn].ToString()]; //If player character == King, set new crown player

			string[] arr = { selectedCharacters[orders[turn].ToString()], phase }; //array containing userID and current phase

			//Send Phase 2 turn to Player
			RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
			PhotonNetwork.RaiseEvent(GIVE_TURN, arr, raiseEventOptions, SendOptions.SendUnreliable);
		}
	}

	private void OnEnable()
	{
		PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (scene == SceneManager.GetSceneByBuildIndex(1)) //If game scene is loaded
        {
			foreach (string player in order) 
            {
				string[] arr = new string[] { player };
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
				PhotonNetwork.RaiseEvent(GIVE_CARDS, arr.Concat(GetCards(4)).ToArray(), raiseEventOptions, SendOptions.SendUnreliable); //Give Five cards to each player
			}
			crown = order[turn];
			discardCharacters();
		}
	}

	//Game networking communication
    private void NetworkingClient_EventReceived(EventData obj)
	{

		if (obj.Code == ADD_PLAYER) //
        {
			object[] data = (object[])obj.CustomData;
			playerList.Add((string)data[0], (string)data[1]);
		}

		if (obj.Code == REMOVE_PLAYER)
        {
			object[] data = (object[])obj.CustomData;
			playerList.Remove((string)data[0]);
		}

		if (obj.Code == PHASE_END)
        {
			turn++;

			if(phase == "phase1")
            {
				object[] data = (object[])obj.CustomData;

				selectedCharacters.Add((string)data[0], (string)data[1]); //Add character selection

				if (turn >= playerList.Count) //If all players have selected a character swap phase
				{
					turn = 0;
					phase = "phase2";
					actionsPhase();
				}
				else
					selectionPhase();

			}
			else
            {
				if (turn >= playerList.Count) //If round has ended clear round data nad start new round phase1
				{
					selectedCharacters.Clear();
					RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
					PhotonNetwork.RaiseEvent(CLEAR_CHARACTERS, "", raiseEventOptions, SendOptions.SendUnreliable);
					turn = 0;
					phase = "phase1";
					dead = "";
					discardCharacters();
				}
				else
					actionsPhase();
			}
		}

		if(obj.Code == ASK_CARDS)
        {
			object[] data = (object[])obj.CustomData;

			//Join 2 arrays so we get next structure --> arr[0] = Player ID, arr[1] = card1 ID, arr[2] = card2 ID, ...
			string[] arr = new string[] { (string)data[0] }.Concat(GetCards(int.Parse((string)data[1]))).ToArray();

			RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
			PhotonNetwork.RaiseEvent(GIVE_CARDS, arr, raiseEventOptions, SendOptions.SendUnreliable);
		}

		if(obj.Code == RETURN_CARD)
        {
			object data = obj.CustomData;
			deck.Enqueue((string)data);
        }

		if (obj.Code == KILL)
			dead = (string)obj.CustomData;
	}
}