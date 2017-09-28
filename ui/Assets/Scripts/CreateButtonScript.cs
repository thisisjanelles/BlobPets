using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using SimpleJSON;
using System;


public class CreateButtonScript : MonoBehaviour
{
	private string scene;

	public InputFieldToText nameInput;
	private GameObject nameInputObject;
	public JSONNode result;

	private SelectBlob selectBlueScript;
	private SelectBlob selectOrangeScript;
	private SelectBlob selectPinkScript;
	private SelectBlob selectGreenScript;

	private GameObject selectedBlue;
	private GameObject selectedOrange;
	private GameObject selectedPink;
	private GameObject selectedGreen;

	public string url = "http://104.131.144.86/api/users/";
	public string token;
	public bool userExists;
	public string blobName = "";
	public string blobColor = "";
	public string blobType = "type A";

	// PlayerPrefs keys
	public string emailPPKey = "Email";
	public string passwordPPKey = "Password";
	public string userIdPPKey = "UserId";
	public string blobPPKey = "RequestedBlobId";

	public int userId;
	public string email;
	public string password;
	public string newBlobId;

	// Use this for initialization
	void Start ()
	{
		selectedBlue = GameObject.Find ("BlueBlobSelectionScriptObject");
		selectBlueScript = (SelectBlob)selectedBlue.GetComponent (typeof(SelectBlob));

		selectedOrange = GameObject.Find ("OrangeBlobSelectionScriptObject");
		selectOrangeScript = (SelectBlob)selectedOrange.GetComponent (typeof(SelectBlob));

		selectedPink = GameObject.Find ("PinkBlobSelectionScriptObject");
		selectPinkScript = (SelectBlob)selectedPink.GetComponent (typeof(SelectBlob));

		selectedGreen = GameObject.Find ("GreenBlobSelectionScriptObject");
		selectGreenScript = (SelectBlob)selectedGreen.GetComponent (typeof(SelectBlob));
	}

	// Update is called once per frame
	void Update ()
	{

	}

	public void LoadScene ()
	{
		nameInputObject = GameObject.Find ("InputFieldObject");
		nameInput = (InputFieldToText)nameInputObject.GetComponent (typeof(InputFieldToText));
		blobName = nameInput.TextBox.text;
		Debug.Log (blobName);

		if (nameInput != null) {
			if (selectBlueScript.blueGuiEnable == true) {
				scene = "BlueMain";
				blobColor = "blue";
				//			SendTokenRequest (email, password);
				Debug.Log (selectBlueScript.blueGuiEnable);
				Debug.Log ("Blue Scene load");
			}

			if (selectOrangeScript.orangeGuiEnable == true) {
				scene = "OrangeMain";
				blobColor = "orange";
				Debug.Log (selectOrangeScript.orangeGuiEnable);
				Debug.Log ("Orange Scene load");
			}

			if (selectPinkScript.pinkGuiEnable == true) {
				scene = "PinkMain";
				blobColor = "pink";
				Debug.Log (selectPinkScript.pinkGuiEnable);
				Debug.Log ("Pink Scene load");
			}

			if (selectGreenScript.greenGuiEnable == true) {
				scene = "GreenMain";
				blobColor = "green";
				Debug.Log (selectGreenScript.greenGuiEnable);
				Debug.Log ("Green Scene load");
			}
		} else {
			Debug.Log ("Error: no name");
		}

		//		GetPlayerPrefs ();
		CreateBlob ();
		SceneManager.LoadScene (scene);
	}

	/*
	 * Retrieve the User ID, email, and password for logged in user.
	 */

	private void GetPlayerPrefs ()
	{
		if (PlayerPrefs.HasKey (emailPPKey)) {
			email = PlayerPrefs.GetString (emailPPKey);
		}

		if (PlayerPrefs.HasKey (passwordPPKey)) {
			password = PlayerPrefs.GetString (passwordPPKey);
		}
		Debug.Log (email);
		Debug.Log (password);
	}


	// POST request
	public void CreateBlob ()
	{
		Debug.Log ("blobname: " + blobName);
		if (blobName != "") {
			GetPlayerPrefs ();
			SendTokenRequest (email, password);
			//			CallAPI ();
		} else {
			Debug.Log ("No name entered");
		}
	}


	//
	public void CallAPI ()
	{
		string blobURL = "http://104.131.144.86/api/blobs";

		WWWForm form = new WWWForm ();
		form.AddField ("name", blobName);
		form.AddField ("type", blobType);
		form.AddField ("color", blobColor);
		WWW www = new WWW (blobURL, form);
		Debug.Log ("sent call");
		StartCoroutine (WaitForRequest (www));
	}

	// !!!
	public void CallBreedAPI ()
	{

		Debug.Log ("Entering CallBreedAPI()...");

		/*
		WWWForm form = new WWWForm ();
		form.AddField ("id1", test_id1);
		form.AddField ("id2", test_id1);
		form.AddField ("token", test_token);
		WWW www = new WWW (breedUrl, form);

		StartCoroutine (WaitForRequest2 (www));
		*/
		Debug.Log ("token: " + token);
		Debug.Log ("email: " + email);
		Debug.Log ("password: " + password);

		//string test_tok = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOjEsImlzcyI6Imh0dHA6XC9cLzEyNy4wLjAuMTo4MDAwXC9hcGlcL3VzZXJzXC9hdXRoZW50aWNhdGUiLCJpYXQiOjE0OTExNzgzMzUsImV4cCI6MTQ5MTE4MTkzNSwibmJmIjoxNDkxMTc4MzM1LCJqdGkiOiJkNWEzMWUxNGQ0NzA2MmQ2OGRjOTljN2NjM2Q2MTQ2OSJ9.RHMMZrcp3kj2B-apGn7A95mz-nHvlTjT1HVeSljy4CA";

		Dictionary<string, string> headers = new Dictionary<string, string> ();
		//headers.Add ("Content-Type", "application/json");
		headers.Add ("Content-Type", "application/x-www-form-urlencoded");
		headers.Add ("Authorization", "Bearer " + token);
		string breedUrl = "http://104.131.144.86/api/blobs";
		WWWForm form = new WWWForm ();
		form.AddField ("token", token);
		form.AddField ("name", blobName);
		form.AddField ("type", blobType);
		form.AddField ("color", blobColor);
		byte[] rawData = form.data;
		WWW www = new WWW (breedUrl, rawData, headers);
		StartCoroutine (WaitForRequest (www));
	}

	public void SendTokenRequest (string email, string password)
	{
		Debug.Log ("In token request...");

		string tokenUrl = "http://104.131.144.86/api/users/authenticate";
		WWWForm form = new WWWForm ();
		form.AddField ("email", email);
		form.AddField ("password", password);
		WWW www = new WWW (tokenUrl, form);
		StartCoroutine (WaitForRequest (www));
	}

	IEnumerator WaitForRequest (WWW www)
	{
		yield return www;

		// check for errors
		if (www.error == null) {

			JSONNode N = JSON.Parse (www.text);

			userExists = true;
			Debug.Log ("User exists.");

			ParseTokenJson (N);
			CallAPI ();
		} else {
			Debug.Log ("Token WWW Error: " + www.error);
			userExists = false;
			Debug.Log ("User doesn't exist.");
			if (www.error == "400 Bad Request") {
				// alert for duplicate email address
			}
		}    
	}

	public void ParseTokenJson (JSONNode data)
	{
		token = data ["token"].Value;
		//Debug.Log ("Parsed, token is: " + token);
	}
}