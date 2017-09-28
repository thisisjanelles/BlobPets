using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using SimpleJSON;

//using UnityEditor.Sprites;
using System;
using System.Security.Policy;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using System.Security.Cryptography;
using UnityEngine.Networking;
//using UnityEditor;

public class UserProfileCtrl : MonoBehaviour
{

	public PlayerPreferences playerPreferences;

	public int userId;
	public string username = "-1";
	public string numWins = "-1";
	public int numBlobs = -1;
	public string exerciseLevel;
	public int eid;
	public string token;
	public string email;
	public string password;
	public string weeklyGoal;
	public string remainingExercise;

	public string deadBlobNum;

	public string blobName0;
	public string blobName1;
	public string blobName2;
	public string blobName3;

	public string blobColor0;
	public string blobColor1;
	public string blobColor2;
	public string blobColor3;

	public string blobId0;
	public string blobId1;
	public string blobId2;
	public string blobId3;

	public string aliveStatus0; 
	public string aliveStatus1; 
	public string aliveStatus2; 
	public string aliveStatus3; 

	public JSONNode result;

	public Text nameLabel;
	public Text winsLabel;
	public Text blobCountLabel;
	public Text exercise;

	public Text blob0_label;
	public Text blob1_label;
	public Text blob2_label;
	public Text blob3_label;

	public Button b0;
	public Button b1;
	public Button b2;
	public Button b3;

	public Image img0;
	public Image img1;
	public Image img2;
	public Image img3;

	public Sprite blueBlob;
	public Sprite orangeBlob;
	public Sprite greenBlob;
	public Sprite pinkBlob;
	public Sprite lockIcon;

	public GameObject imageGO0;
	public GameObject imageGO1;
	public GameObject imageGO2;
	public GameObject imageGO3;


	public GameObject nameGameObject;
	public GameObject winsGameObject;
	public GameObject blobCountGameObject;
	public GameObject exerciseGameObject;

	public GameObject blob0_GameObject;
	public GameObject blob1_GameObject;
	public GameObject blob2_GameObject;
	public GameObject blob3_GameObject;

	public GameObject b0GO;
	public GameObject b1GO;
	public GameObject b2GO;
	public GameObject b3GO;

	// dead blob window
	public Image dead_window;
	public GameObject dead_windowGO;
	public Text dead_header;
	public GameObject dead_headerGO;
	public Text dead_tombName;
	public GameObject dead_tombNameGO;
	public GameObject dead_okButton;
	public Image dead_tombPic;
	public GameObject dead_tombPicGO;
	public Text dead_body;
	public GameObject dead_bodyGO;
	public Text dead_tombMsg;
	public GameObject dead_tombMsgGO;


	// Use this for initialization
	void Start ()
	{
		dead_window = dead_windowGO.GetComponent<Image> ();
		dead_header = dead_headerGO.GetComponent<Text> ();
		dead_tombName = dead_tombNameGO.GetComponent<Text> ();
		dead_tombPic = dead_tombPicGO.GetComponent<Image> ();
		dead_body = dead_bodyGO.GetComponent<Text> ();
		dead_tombMsg = dead_tombMsgGO.GetComponent<Text> ();

		nameLabel = nameGameObject.GetComponent<Text> ();
		winsLabel = winsGameObject.GetComponent<Text> ();
		blobCountLabel = blobCountGameObject.GetComponent<Text> ();
		exercise = exerciseGameObject.GetComponent<Text> ();

		// blob names text labels
		blob0_label = blob0_GameObject.GetComponent<Text> ();
		blob1_label = blob1_GameObject.GetComponent<Text> ();
		blob2_label = blob2_GameObject.GetComponent<Text> ();
		blob3_label = blob3_GameObject.GetComponent<Text> ();

		// blob icon buttons
		b0 = b0GO.GetComponent<Button> ();
		b1 = b1GO.GetComponent<Button> ();
		b2 = b2GO.GetComponent<Button> ();
		b3 = b3GO.GetComponent<Button> ();

		// blob icon images
		img0 = imageGO0.GetComponent<Image> ();
		img1 = imageGO1.GetComponent<Image> ();
		img2 = imageGO2.GetComponent<Image> ();
		img3 = imageGO3.GetComponent<Image> ();

		// Reset RequestedBlobId
		playerPreferences.ResetRequestedBlob ();

		userId = playerPreferences.GetUser ();

		email = playerPreferences.GetEmail ();
		password = playerPreferences.GetPassword ();

		DisableDeadBlobWindow ();

		//SendTokenRequest (email, password);
		CallAPI ();

	}

	public void EnableDeadBlobWindow()
	{
		dead_window.enabled = true;
		dead_header.enabled = true;
		dead_tombName.enabled = true;
		dead_okButton.SetActive (true);
		dead_tombPic.enabled = true;
		dead_body.enabled = true;
		dead_tombMsg.enabled = true;

	}

	public void DisableDeadBlobWindow()
	{
		dead_window.enabled = false;
		dead_header.enabled = false;
		dead_tombName.enabled = false;
		dead_okButton.SetActive (false);
		dead_tombPic.enabled = false;
		dead_body.enabled = false;
		dead_tombMsg.enabled = false;
	}

	public void SetDeadBlobWindow(string blobName)
	{
		// tombstone
		dead_tombName.text = blobName;

		// header
		dead_header.text = "RIP " + blobName;
	}

	IEnumerator DeleteBlob(string blobNum)
	{
		string blobid = "-1";

		if (blobNum == "0") {
			blobid = blobId0;
		} else if (blobNum == "1") {
			blobid = blobId1;
		} else if (blobNum == "2") {
			blobid = blobId2;
		} else if (blobNum == "3") {
			blobid = blobId3;
		}

		string url = "http://104.131.144.86/api/blobs/";

		string fullUrl = url + blobid + "?token=" + token;

		Debug.Log ("full URL: " + fullUrl);

		using (UnityWebRequest www = UnityWebRequest.Delete (fullUrl)) {
			yield return www.Send ();

			if (www.isError) {
				Debug.Log ("Delete blob error: " + www.error);
			} else {
				// reload scene after delete
				SceneManager.LoadScene ("UserProfileUI");
		
			}
		}

	}

	public void DeadBlobButtonClicked()
	{
		StartCoroutine (DeleteBlob (deadBlobNum));
	}
		
	/// <summary>
	/// Prints user info on the scene.
	/// </summary>
	public void SetHeader ()
	{
		nameLabel.text = username;
		winsLabel.text = numWins;
		blobCountLabel.text = numBlobs.ToString ();
		exercise.text = remainingExercise;
	}


	/// <summary>
	/// Prints blob names on the scene.
	/// </summary>
	public void SetBlobNames ()
	{
		if (String.IsNullOrEmpty (blobName0)) {
			blob0_label.text = " ";
			img0.sprite = lockIcon;
		} else {
			blob0_label.text = blobName0;
			PrintBlobImage (blobColor0, 0);

		}

		if (String.IsNullOrEmpty (blobName1)) {
			blob1_label.text = " ";
			img1.sprite = lockIcon;
		} else {
			blob1_label.text = blobName1;
			PrintBlobImage (blobColor1, 1);
		}

		if (String.IsNullOrEmpty (blobName2)) {
			blob2_label.text = " ";
			img2.sprite = lockIcon;
		} else {
			blob2_label.text = blobName2;
			PrintBlobImage (blobColor2, 2);
		}

		if (String.IsNullOrEmpty (blobName3)) {
			blob3_label.text = " ";
			img3.sprite = lockIcon;
		} else {
			blob3_label.text = blobName3;
			PrintBlobImage (blobColor3, 3);
		}
	}

	/// <summary>
	/// This is for he blob icons under "Your Blobs."
	/// It stores the blob ID of the blob selected in PlayerPrefs.
	/// </summary>
	/// <param name="blobNum">Blob number selected (from 0 thru 3).</param>
	public void BlobButtonClick (string blobNum)
	{
		if (blobNum == "0" && !String.IsNullOrEmpty (blobName0)) {
			Debug.Log ("blob 0 selected. color: " + blobColor0);
			playerPreferences.SetRequestedBlob (blobId0);
			ChangeBlobRoom (blobColor0);
		} else if (blobNum == "1" && !String.IsNullOrEmpty (blobName1)) {
			Debug.Log ("blob 1 selected. color: " + blobColor1);
			playerPreferences.SetRequestedBlob (blobId1);
			ChangeBlobRoom (blobColor1);
		} else if (blobNum == "2" && !String.IsNullOrEmpty (blobName2)) {
			Debug.Log ("blob 2 selected. color: " + blobColor2);
			playerPreferences.SetRequestedBlob (blobId2);
			ChangeBlobRoom (blobColor2);
		} else if (blobNum == "3" && !String.IsNullOrEmpty (blobName3)) {
			Debug.Log ("blob 3 selected. color: " + blobColor3);
			playerPreferences.SetRequestedBlob (blobId3);
			ChangeBlobRoom (blobColor3);
		}
	}

	public void ChangeBlobRoom (string blobColor)
	{
		if (blobColor == "orange") {
			SceneManager.LoadScene ("OrangeMain");
		} else if (blobColor == "blue") {
			SceneManager.LoadScene ("BlueMain");
		} else if (blobColor == "green") {
			SceneManager.LoadScene ("GreenMain");
		} else if (blobColor == "pink") {
			SceneManager.LoadScene ("PinkMain");
		}
	}

	/// <summary>
	/// Enable or disable blob icons based on the number of blobs owned.
	/// If numBlobs == 1, then enable b0 ion. If numBlobs == 2, then enable 
	/// b0 and b1 icons...and so on.
	/// </summary>
	public void ManageBlobButtons ()
	{
		//Debug.Log ("Exercise... " + playerPreferences.GetExercise ());

		if (numBlobs == 1) {
			b0.enabled = true;
			b1.enabled = false;
			b2.enabled = false;
			b3.enabled = false;

		} else if (numBlobs == 2) {
			b0.enabled = true;
			b1.enabled = true;
			b2.enabled = false;
			b3.enabled = false;

		} else if (numBlobs == 3) {
			b0.enabled = true;
			b1.enabled = true;
			b2.enabled = true;
			b3.enabled = false;

		} else if (numBlobs == 4) {
			b0.enabled = true;
			b1.enabled = true;
			b2.enabled = true;
			b3.enabled = true;

		} else {
			b0.enabled = false;
			b1.enabled = false;
			b2.enabled = false;
			b3.enabled = false;
		}
	}


	public void PrintBlobImage (string blobColor, int imageNumber)
	{
		if (imageNumber == 0) {
			if (blobColor == "orange") {
				img0.sprite = orangeBlob;
			} else if (blobColor == "pink") {
				img0.sprite = pinkBlob;
			} else if (blobColor == "green") {
				img0.sprite = greenBlob;
			} else if (blobColor == "blue") {
				img0.sprite = blueBlob;
			}

		} else if (imageNumber == 1) {
			if (blobColor == "orange") {
				img1.sprite = orangeBlob;
			} else if (blobColor == "pink") {
				img1.sprite = pinkBlob;
			} else if (blobColor == "green") {
				img1.sprite = greenBlob;
			} else if (blobColor == "blue") {
				img1.sprite = blueBlob;
			}
		
		} else if (imageNumber == 2) {
			if (blobColor == "orange") {
				img2.sprite = orangeBlob;
			} else if (blobColor == "pink") {
				img2.sprite = pinkBlob;
			} else if (blobColor == "green") {
				img2.sprite = greenBlob;
			} else if (blobColor == "blue") {
				img2.sprite = blueBlob;
			}

		} else if (imageNumber == 3) {
			if (blobColor == "orange") {
				img3.sprite = orangeBlob;
			} else if (blobColor == "pink") {
				img3.sprite = pinkBlob;
			} else if (blobColor == "green") {
				img3.sprite = greenBlob;
			} else if (blobColor == "blue") {
				img3.sprite = blueBlob;
			}
		}
		
	}

		
	/// <summary>
	/// Erase values saved in PlayerPrefs when user clicks Log Out button.
	/// </summary>
	public void LogOutButtonClick ()
	{
		playerPreferences.ResetNumBlobs ();
		playerPreferences.ResetRequestedBlob ();
		playerPreferences.ResetUser ();

		//Debug.Log ("numBlobs after reset: " + playerPreferences.GetNumBlobs ());
		//Debug.Log ("requested blob ID after reset: " + playerPreferences.GetRequestedBlob ());
		//Debug.Log ("userID after reset: " + playerPreferences.GetUser ());
	}

	public void CallAPI ()
	{
		string url = "http://104.131.144.86/api/users/";
		string fullUrl = url + userId;
		Debug.Log ("FULL URL: " + fullUrl);
		WWW www = new WWW (fullUrl);
		StartCoroutine (GetUserInfo (www)); 
	}

	IEnumerator GetUserInfo (WWW www)
	{
		yield return www;

		if (www.error == null) {
			result = JSON.Parse (www.text);
			ParseJson (result);
			//CallExerciseAPI ();
			//SetHeader ();
			CheckBlobStatus(0);
			CheckBlobStatus(1);
			CheckBlobStatus(2);
			CheckBlobStatus(3);
			SetBlobNames ();
			ManageBlobButtons ();
			SendTokenRequest (email, password);
		} else {

		}
	}

	public void ParseJson (JSONNode userResult)
	{
		// set user info
		username = result ["name"].Value;
		numWins = result ["battles_won"].Value;
		numBlobs = result ["blobs"].Count;
		string er_id = result ["er_id"].Value;
		Int32.TryParse (er_id, out eid);

		// save number of blobs owned by user in memory
		playerPreferences.SetNumBlobs (numBlobs);
		playerPreferences.SetExercise (eid);

		// set blob names	
		blobName0 = result ["blobs"] [0] ["name"].Value;
		blobName1 = result ["blobs"] [1] ["name"].Value;
		blobName2 = result ["blobs"] [2] ["name"].Value;
		blobName3 = result ["blobs"] [3] ["name"].Value;

		// set blob IDs
		blobId0 = result ["blobs"] [0] ["id"].Value;
		blobId1 = result ["blobs"] [1] ["id"].Value;
		blobId2 = result ["blobs"] [2] ["id"].Value;
		blobId3 = result ["blobs"] [3] ["id"].Value;

		// set blob colors
		blobColor0 = result ["blobs"] [0] ["color"].Value;
		blobColor1 = result ["blobs"] [1] ["color"].Value;
		blobColor2 = result ["blobs"] [2] ["color"].Value;
		blobColor3 = result ["blobs"] [3] ["color"].Value;

		// blob alive status
		aliveStatus0 = result ["blobs"] [0] ["alive"].Value;
		aliveStatus1 = result ["blobs"] [1] ["alive"].Value;
		aliveStatus2 = result ["blobs"] [2] ["alive"].Value;
		aliveStatus3 = result ["blobs"] [3] ["alive"].Value;

		// set exercise level
		exerciseLevel = result ["blobs"] [0] ["exercise_level"].Value;
	}

	public void CheckBlobStatus(int blobNum)
	{
		Debug.Log ("Checking blob status...");
		Debug.Log ("b0 alive: " + aliveStatus0);
		Debug.Log ("b1 alive: " + aliveStatus1);
		Debug.Log ("b2 alive: " + aliveStatus2);
		Debug.Log ("b3 alive: " + aliveStatus3);

		if (blobNum == 0 && !String.IsNullOrEmpty (aliveStatus0)) {
			if (aliveStatus0 == "0") {
				Debug.Log ("Blob 0 dead.");
				EnableDeadBlobWindow ();
				deadBlobNum = "0";
				PrintDeadBlobWarning ("0");
			}
		} else if (blobNum == 1 && !String.IsNullOrEmpty (aliveStatus1)) {
			if (aliveStatus1 == "0") {
				Debug.Log ("Blob 1 dead.");
				EnableDeadBlobWindow ();
				deadBlobNum = "1";
				PrintDeadBlobWarning ("1");
			}
		} else if (blobNum == 2 && !String.IsNullOrEmpty (aliveStatus2)) {
			if (aliveStatus2 == "0") {
				Debug.Log ("Blob 2 dead.");
				EnableDeadBlobWindow ();
				deadBlobNum = "2";
				PrintDeadBlobWarning ("2");
			}
		} else if (blobNum == 3 && !String.IsNullOrEmpty (aliveStatus3)) {
			if (aliveStatus3 == "0") {
				Debug.Log ("Blob 3 dead.");
				EnableDeadBlobWindow ();
				deadBlobNum = "3";
				PrintDeadBlobWarning ("3");
			}
		}
	}

	public void PrintDeadBlobWarning(string blobNum)
	{
		string deadBlobName = "";

		if (blobNum == "0") {
			deadBlobName = blobName0;
		} else if (blobNum == "1") {
			deadBlobName = blobName1;
		} else if (blobNum == "2") {
			deadBlobName = blobName2;
		} else if (blobNum == "3") {
			deadBlobName = blobName3;
		}

		dead_header.text = "RIP " + deadBlobName;
		dead_tombName.text = deadBlobName;
	}


	/// <summary>
	/// Sends the token request via POST request to API.
	/// </summary>
	/// <param name="email">Email.</param>
	/// <param name="password">Password.</param>
	public void SendTokenRequest (string email, string password)
	{
		string tokenUrl = "http://104.131.144.86/api/users/authenticate";
		WWWForm form = new WWWForm ();
		form.AddField ("email", email);
		form.AddField ("password", password);
		WWW www = new WWW (tokenUrl, form);
		StartCoroutine (WaitForTokenRequest (www));


	}

	IEnumerator WaitForTokenRequest (WWW www)
	{
		yield return www;

		// check for errors
		if (www.error == null) {


			JSONNode N = JSON.Parse (www.text);

			Debug.Log("User exists, token found.");

			ParseTokenJson (N);
			CallExerciseAPI ();
			//CallAPI ();


		} else {
			Debug.Log ("***WWW Error: " + www.error);
			Debug.Log("User doesn't exist, token not found.");
			if (www.error == "400 Bad Request") {
				// alert for duplicate email address
			}
		}    
	}

	public void ParseTokenJson(JSONNode data) 
	{
		token = data ["token"].Value;
		Debug.Log ("Parsed, token is: " + token);
	}

	/// <summary>
	/// Call API to get exercise record.
	/// </summary>
	public void CallExerciseAPI() 
	{

		string url = "http://104.131.144.86/api/exercises/" + eid + "?token=" + token;
		//string fullUrl = url + userId;
		Debug.Log ("Call exercise API url: " + url);
		WWW www = new WWW (url);
		StartCoroutine (GetExerciseInfo (www)); 
	}

	IEnumerator GetExerciseInfo(WWW www) 
	{
		yield return www;

		// check for errors
		if (www.error == null) {
			Debug.Log ("Exercise record found.");
			JSONNode N = JSON.Parse (www.text);
			ParseExerciseJson (N);
			Debug.Log (www.text);
			Debug.Log ("exercise weekly goal: " + weeklyGoal);
			Debug.Log ("exercise remaining: " + remainingExercise);
			SetHeader ();
		} else {
			Debug.Log ("Exercise record error: " + www.error);
		}    

	}

	public void ParseExerciseJson(JSONNode N)
	{
		weeklyGoal = N ["weekly_goal"].Value;
		remainingExercise = N ["remaining_exercise"].Value;
	}






		
}