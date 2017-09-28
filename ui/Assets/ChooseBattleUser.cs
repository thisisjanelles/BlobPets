using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleJSON;
using UnityEngine.EventSystems;

public class ChooseBattleUser : MonoBehaviour {
	private string chosenUser;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void chooseUser()
	{
		// get name of button clicked
		string buttonName = EventSystem.current.currentSelectedGameObject.name;
		chosenUser = GameObject.Find (buttonName).GetComponentInChildren<Text> ().text;

		Debug.Log ("User to battle chosen... : " + chosenUser);

		// get user index from text of button clicked
		string chosenUserID = chosenUser.Substring (0, 1);
		int chosenUserIndex = int.Parse (chosenUserID);
		Debug.Log ("Chosen user index: " + chosenUserIndex);

		// set in playerprefs
		PlayerPrefs.SetInt ("chosenUserIndex", chosenUserIndex);

		// go to choosing blob scene
		SceneManager.LoadScene ("ChooseBlobMain");
	}
}
