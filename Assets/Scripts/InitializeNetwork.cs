using UnityEngine;
using System.Collections;
/*#if ENABLE_UNET

namespace UnityEngine.Networking
{*/
	public class InitializeNetwork : MonoBehaviour
	{
//		public bool isPlayer1;
		public GazeController player1;
		public GazeController player2;
//		public NetworkManager manager;

		// Runtime variable
		bool showServer = false;

		void Awake()
		{
			DontDestroyOnLoad (gameObject);
			/*
			manager = GetComponent<NetworkManager>();
			//enable match maker
			manager.StartMatchMaker();

			if (manager.matches == null && isPlayer1) {
				//create internet match
				manager.matchMaker.CreateMatch ("default", 4, true, "", manager.OnMatchCreate);
			}

			if (!isPlayer1) {
				//find internet match
				manager.matchMaker.ListMatches (0, 20, "", manager.OnMatchList);
				if (manager.matches != null) {

					//join match
					Match.MatchDesc match = manager.matches.ToArray()[0];
					manager.matchName = "default";
					manager.matchSize = 4;
					manager.matchMaker.JoinMatch (match.networkId, "", manager.OnMatchJoined);
				}
			}
			*/
		}

		void Update()
		{
			if (Input.GetKey (KeyCode.R)) {
				ResetGame ();
			}
		}

		public void ResetGame()
	{
		Application.LoadLevel (Application.loadedLevel);
		player1.GetComponent<Slug> ().Initialize ();
		player2.GetComponent<Slug> ().Initialize ();
		GameObject.FindGameObjectWithTag ("Human").GetComponent<Human> ().Initialize();
		player1.ResetBalloon ();
		player2.ResetBalloon ();
	}
		
	public void AttachPlayer(GazeController newGaze)
	{
		if (player1 == null)
			player1 = newGaze;
		else
			player2 = newGaze;
	}

}
//}
//#endif //ENABLE_UNET
