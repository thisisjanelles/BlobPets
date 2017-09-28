using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.SceneManagement;


public class CreateNewBlobCtrl : MonoBehaviour {

	public string blobColor;
	public string blobName;
	public string type = "A";

	public string token;
	public string email;
	public string password;

	public InputFieldCtrl ifCtrl;
	public PlayerPreferences pp;


	// Use this for initialization
	void Start () {
		email = pp.GetEmail ();
		password = pp.GetPassword ();

		Debug.Log ("email: " + email);
		Debug.Log ("password: " + password);
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void BlobButtonClicked(string color) 
	{
		blobColor = color;
	}

	public void CreateButtonClicked()
	{
		blobName = ifCtrl.getName ();

		SendTokenRequest (email, password);

	}

	/// <summary>
	/// Sends the token request.
	/// </summary>
	/// <param name="email">Email.</param>
	/// <param name="password">Password.</param>
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

			Debug.Log ("User exists.");

			ParseTokenJson (N);
			CallBlobAPI ();

		} else {
			Debug.Log ("Token WWW Error: " + www.error);
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


	public void CallBlobAPI() 
	{
		/*
		Debug.Log ("create scene - entering CallBlobAPI()...");
		Debug.Log ("token: " + token);
		Debug.Log ("blob name: " + blobName);
		Debug.Log ("type: " + type);
		Debug.Log ("blob color: " + blobColor);
		*/

		Dictionary<string, string> headers = new Dictionary<string, string> ();
		//headers.Add ("Content-Type", "application/json");
		headers.Add("Content-Type", "application/x-www-form-urlencoded");
		headers.Add ("Authorization", "Bearer " + token);
		string breedUrl = "http://104.131.144.86/api/blobs";
		WWWForm form = new WWWForm ();
		form.AddField ("token", token);
		form.AddField ("name", blobName);
		form.AddField ("type", type);
		form.AddField ("color", blobColor);
		byte[] rawData = form.data;
		WWW www = new WWW (breedUrl, rawData, headers);
		StartCoroutine (WaitForRequest2 (www));
	}

	IEnumerator WaitForRequest2 (WWW www)
	{
		yield return www;

		JSONNode N = JSON.Parse (www.text);

		if (www.error == null) {
			//ParseNewBlobJson (N);
			SceneManager.LoadScene ("UserProfileUI");
		} else {
			string err = N ["error"].Value;
			Debug.Log ("WWW breed error..." + err);
		}
	}

	/*
	 * Returns the blob ID of the newly breeded blob.
	 */

	/*
	public void ParseNewBlobJson (JSONNode N)
	{
		newBlobId = N ["blobID"].Value;

		PlayerPrefs.SetString ("NewBlobID", newBlobId);
		PlayerPrefs.Save ();
	}
	*/

}
