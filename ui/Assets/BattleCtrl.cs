using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleJSON;
using UnityEngine.EventSystems;

public class BattleCtrl : MonoBehaviour {
//	public string url = "http://104.131.144.86/api/users/";

	private string battleData;
	private JSONNode battleListData;

	private string blobsData;
	private JSONNode blobsListData;

	private int chosenUserIndex;

	// Blob info
	private string blobName;
	public List<string> blobList;

//	public Text p0;
//	public Text p1;
//	public Text p2;
//	public Text p3;
//	public Text p4;
//
//	public GameObject GO0;
//	public GameObject GO1;
//	public GameObject GO2;
//	public GameObject GO3;
//	public GameObject GO4;

	// Use this for initialization
	void Start () {
		battleData = PlayerPrefs.GetString ("battleUserList");
		battleListData = JSON.Parse (battleData);

		if (battleListData != null) {
			Debug.Log ("DATA IS NOT NULL");
		} else {
			Debug.Log ("DATA IS NULL");
		}
		blobList = new List<string> ();
		BuildBlobList ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// GET THE LIST OF THE CHOSEN USER'S BLOBS AND POPULATE THE BUTTONS
	public void BuildBlobList ()
	{
		Button[] buttons = this.GetComponentsInChildren<Button> ();
		Debug.Log ("NUMBER OF BUTTONS: " + buttons.Length);

		int chosenUserIndex = PlayerPrefs.GetInt ("chosenUserIndex");
		Debug.Log ("index: " + chosenUserIndex);

		for (int blobIndex = 0; blobIndex < 4; blobIndex++) {
			string tempName = battleListData [chosenUserIndex] ["blobs"] [blobIndex] ["name"].Value;
			if (tempName != "" && tempName != null) {
				string blobID = battleListData [chosenUserIndex] ["blobs"] [blobIndex] ["id"].Value.ToString();
				tempName = blobID + ". " + tempName;
				blobList.Add (tempName);
			}
		}

		for (int i = 0; i < buttons.Length; i++) {
			string buttonName = buttons [i].name;
			GameObject.Find (buttonName).GetComponentInChildren<Text> ().text = blobList [i];
		}
	}
}
