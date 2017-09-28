using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SimpleJSON;
using System.Runtime.Remoting;
//using UnityEditor;
using System.Runtime.InteropServices;
using System.ComponentModel;


// Create new user with given name, email, and password.

public class NewUser : MonoBehaviour
{
	public InputFieldCtrl ifctrl;
	public ValidInputsCtrl validInputsCtrl;
	public ButtonCtrl buttonCtrl;
	public PlayerPreferences playerPreferences;


	public string username = "-1";
	public string email = "-1";
	public string password = "-1";
	public int userId;
	public string token;

	public string url = "http://104.131.144.86/api/users/";

	public bool validName = false;
	public bool validEmail = false;
	public bool validPW = false;
	public bool validNewUserCombo = false;

	public GameObject namePanel;
	public GameObject emailPanel;
	public GameObject passwordPanel;

	public JSONNode result;
	private string errorMsg;
	public string emailServerError = "Email address has already been taken";
	public string otherServerError = "Missing required input fields";

	// Use this for initialization
	void Start ()
	{

		HideNamePanel ();
		HideEmailPanel ();
		HidePasswordPanel ();
	}



	public void ButtonClick ()
	{
		Debug.Log ("BUTTON CLICKED");

		username = ifctrl.getName ();
		password = ifctrl.getPassword ();
		email = ifctrl.getEmail ();

		validName = validInputsCtrl.IsValidName (username);
		validPW = validInputsCtrl.IsValidPassword (password);
		validEmail = validInputsCtrl.IsValidEmailAddress (email);

		CreateUser ();
	}

	public string getErrorMessage ()
	{
		return errorMsg;
	}


	/// <summary>
	/// Sends POST request to create new user if name, email, and password
	/// are valid.
	/// </summary>
	private void CreateUser ()
	{
		if (validName && validEmail && validPW) {
			Debug.Log ("ENTERED VALID INPUTS.");
			CallAPI ();
		} else {
			Debug.Log ("ENTERED INVALID INPUTS.");

			if (validName == false) {
				ShowNamePanel ();
			} else {
				HideNamePanel ();
			}

			if (validEmail == false) {
				ShowEmailPanel ();
			} else {
				HideEmailPanel ();
			}

			if (validPW == false) {
				ShowPasswordPanel ();
			} else {
				HidePasswordPanel ();
			}
		}
	}


	private void CallAPI ()
	{
		Debug.Log ("SENDING API CALL...");
		Debug.Log (username + " " + email + " " + password);

		WWWForm form = new WWWForm ();
		form.AddField ("name", username);
		form.AddField ("email", email);
		form.AddField ("password", password);
		WWW www = new WWW (url, form);
		StartCoroutine (GetUserInfo (www));
	}

	IEnumerator GetUserInfo (WWW www)
	{
		//Debug.Log("Entering GetUserInfo()");
			
		yield return www;

		//Debug.Log("Entering GetUserInfo()....2");

		// check for errors
		if (www.error == null) {
			Debug.Log ("WWW Ok! User created.");

			userId = Convert.ToInt32 (www.text);

			//Debug.Log ("userId after parse: " + userId);

			SaveUserInfo (email, password, userId);
			SendTokenRequest (email, password);


			Invoke ("ChangeScene", 5);

		} else {
			Debug.Log ("WWW Error: " + www.error);
			result = JSON.Parse (www.text);
			errorMsg = ParseJson ("error");

			if (errorMsg == emailServerError) {
				ShowEmailPanel ();
			}

			if (errorMsg == otherServerError) {
				ButtonClick ();
			}
		} 
	}

	/// <summary>
	/// Go to UserProfileUI scene.
	/// </summary>
	public void ChangeScene()
	{
		SceneManager.LoadScene ("UserProfileUI");
	}

	private string ParseJson (string name)
	{
		return result [name].Value;
	}

	/// <summary>
	/// Parses the string value for the given JSONNode and ID.
	/// </summary>
	/// <returns>String value</returns>
	/// <param name="id">Identifier.</param>
	/// <param name="data">Data.</param>
	public string ParseJson(string id, JSONNode data)
	{
		return data [id].Value;
	}

	private void SaveUserInfo(string emailAddr, string passwrd, int userid) 
	{
		/*
		playerPreferences.SetEmail (emailAddr);
		playerPreferences.SetPassword (passwrd);
		playerPreferences.SetUser (userid);
		*/

		PlayerPrefs.SetString ("Email", emailAddr);
		PlayerPrefs.SetString ("Password", passwrd);
		PlayerPrefs.SetInt ("UserId", userid);
		PlayerPrefs.Save ();
	}

	public void ShowNamePanel ()
	{
		namePanel.gameObject.SetActive (true);
	}

	public void ShowEmailPanel ()
	{
		emailPanel.gameObject.SetActive (true);
	}

	public void ShowPasswordPanel ()
	{
		passwordPanel.gameObject.SetActive (true);
	}

	public void HideNamePanel ()
	{
		namePanel.gameObject.SetActive (false);
	}

	public void HideEmailPanel ()
	{
		emailPanel.gameObject.SetActive (false);
	}

	public void HidePasswordPanel ()
	{
		passwordPanel.gameObject.SetActive (false);
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

			Debug.Log("USER EXISTS, TOKEN SUCCESSFUL");

			token = ParseJson ("token", N);

			SendExerciseRequest ();
		} else {
			Debug.Log ("***WWW Error: " + www.error);

			Debug.Log("!!! USER DOESN'T EXIST.");
			if (www.error == "400 Bad Request") {
				// alert for duplicate email address
			}
		}    
	}


	/// <summary>
	/// Sends the exercise request via POST request to API.
	/// </summary>
	public void SendExerciseRequest ()
	{
		Debug.Log ("In SendExerciseRequest()...");        
		Debug.Log ("token: " + token);
		Debug.Log ("userId: " + userId);
		Dictionary<string, string> headers = new Dictionary<string, string> ();
		headers.Add ("Content-Type", "application/json");
		headers.Add ("Authorization", "Bearer " + token);
		string exerciseUrl = "http://104.131.144.86/api/exercises";
		WWWForm form = new WWWForm ();
		//form.AddField ("owner_id", userId);
		form.AddField ("token", token);
		byte[] rawData = form.data;
		WWW www = new WWW (exerciseUrl, rawData, headers);
		StartCoroutine (WaitForExerciseRequest (www));
	}

	IEnumerator WaitForExerciseRequest(WWW www)
	{
		Debug.Log ("In WaitForExerciseRequest()...");

		yield return www;

		if (www.error == null) {
			JSONNode N = JSON.Parse (www.text);
			string eid = ParseJson ("ExerciseRecordID", N);
			int exerciseId = Int32.Parse (eid);
			//playerPreferences.SetExercise (exerciseId);


			string exerciseKey = "ExerciseRecordId";
			PlayerPrefs.SetInt (exerciseKey, exerciseId);
			PlayerPrefs.Save ();


			Debug.Log ("ExerciseRecordID created + saved...EID: " + exerciseId);
		} else {
			Debug.Log ("Exercise WWW error: " + www.error);
			JSONNode N = JSON.Parse (www.text);
			Debug.Log("error message: " + ParseJson ("error", N));
			//JSONNode N = JSON.Parse (www.text);
			//Debug.Log("Exercise API error: " + ParseJson ("error", N));
		}
	}





}


