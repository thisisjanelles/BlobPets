using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using SimpleJSON;
using UnityEngine.UI;
using System;

public class RenameBlobCtrl : MonoBehaviour {

	public InputFieldCtrl ifCtrl;
	public PlayerPreferences pp;
	public string newName;
	public string token;
	public string userId;
	public string email;
	public string password;

	public Text missing_name;
	public GameObject MissingGO;

	// Use this for initialization
	void Start () {

		missing_name = MissingGO.GetComponent<Text> ();
		missing_name.enabled = false;

		email = pp.GetEmail ();
		password = pp.GetPassword ();

		Debug.Log ("RenameBlobCtrl - email: " + email);
		Debug.Log ("RenameBlobCtrl - password: " + password);
	}

	/*
	 * Retrieve name entered in input field upon button click.
	 */

	public void SubmitButtonClick()
	{
		//Debug.Log ("Submit button clicked...");

		newName = ifCtrl.getName ();

		if (String.IsNullOrEmpty (newName)) {
			missing_name.enabled = true;
		} else {
			SendTokenRequest (email, password);
		}


	}

	/*
	 * Send PUT request to API to update name for newly created blob.
	 */

	IEnumerator UpdateBlobName()
	{
		string url = "http://104.131.144.86/api/blobs/";
		string myData = "dummy";
		string newBlobId = GetNewBlobId();

		string finalUrl = url + newBlobId + "?token=" + token + "&name=" + newName;

		Debug.Log ("final URL: " + finalUrl);

		using (UnityWebRequest www = UnityWebRequest.Put (finalUrl, myData)) {
			yield return www.Send ();

			if (www.isError) {
				Debug.Log ("PUT error: " + www.error);
			} else {
				Debug.Log ("PUT request successful...");
				Debug.Log (www.url.ToString ());

				SceneManager.LoadScene ("UserProfileUI");

			}
		}
	
	}

	/*
	 * Sends POST request to the API to get token for the 
	 * user with the given email and password.
	 */

	public void SendTokenRequest (string email, string password)
	{
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

			Debug.Log("Token request OK. User found.");

			ParseJson (N);

			StartCoroutine (UpdateBlobName());


		} else {
			Debug.Log ("Token request error: " + www.error);

			Debug.Log("!!! USER DOESN'T EXIST.");
			if (www.error == "400 Bad Request") {
				// alert for duplicate email address
			}
		}    
	}

	public void ParseJson(JSONNode data) 
	{
		Debug.Log ("token parsed... " + token);

		token = data ["token"].Value;
		userId = data ["id"].Value;
	}


	/// <summary>
	/// Gets the new BLOB identifier.
	/// </summary>
	/// <returns>The new BLOB identifier.</returns>
	public string GetNewBlobId() 
	{
		string key = "NewBlobID";
		if (PlayerPrefs.HasKey (key)) {
			return PlayerPrefs.GetString (key);
		} else {
			return "";
		}
	}




}
