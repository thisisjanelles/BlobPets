using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleJSON;
using UnityEngine.EventSystems;

public class ChooseBattleBlob : MonoBehaviour {
	private string chosenBlob;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	// playerprefs string "RequestedBlobId" --> current user's blob that will battle

	public void chooseBlob()
	{
		// get name of button clicked
		string buttonName = EventSystem.current.currentSelectedGameObject.name;
		chosenBlob = GameObject.Find (buttonName).GetComponentInChildren<Text> ().text;

		Debug.Log ("Blob to battle chosen... : " + chosenBlob);

		// get blob ID from text of button clicked
		string chosenBlobID = chosenBlob.Substring (0, 1);
//		int chosenBlobInt = int.Parse (chosenBlobID);
//		Debug.Log ("Chosen blob ID: " + chosenBlobInt);

		// set in playerprefs
		PlayerPrefs.SetString ("opponentBlobId", chosenBlobID);

		// go to choosing blob scene
		SceneManager.LoadScene ("BattleAnimationScreen");
	}
}
