using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement; 
using System;
using System.Net;
using System.Text.RegularExpressions;
using SimpleJSON;
using System.ComponentModel;
using System.Runtime.Remoting;

/* Update User Profile*/


public class EditCtrl : MonoBehaviour
{

	private string emailKey = "Email";
	private string passwordKey = "Password";

	private string userId;
	private string email;
	private string originalPW;
	public bool userExists;

	private string newName = "-1";
	private string newPW = "-1";
	//private string confirmedPW = "-1";

	private string token;

	private bool nameChangeRequested = false;
	private bool pwChangeRequested = false;

	public Text matched;

	public InputFieldCtrl ifctrl;
	public ValidInputsCtrl validInputsCtrl;
	public TokenCtrl tokenCtrl;

	public Text invalid_name;
	public Text invalid_password;
	public GameObject nameObject;
	public GameObject passwordObject;

	public void Start ()
	{
		invalid_name = nameObject.GetComponent<Text> ();
		invalid_password = passwordObject.GetComponent<Text> ();

		Debug.Log ("EditCtrl start");

		if (PlayerPrefs.HasKey (emailKey)) {
			email = PlayerPrefs.GetString (emailKey);
		}

		if (PlayerPrefs.HasKey (passwordKey)) {
			originalPW = PlayerPrefs.GetString (passwordKey);
		}
			
		Debug.Log ("email: " + email);
		Debug.Log ("original pw: " + originalPW);

		SendTokenRequest (email, originalPW);
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

			userExists = true;
			Debug.Log("!!! USER EXISTS.");

			ParseJson (N);


		} else {
			Debug.Log ("***WWW Error: " + www.error);
			userExists = false;
			Debug.Log("!!! USER DOESN'T EXIST.");
			if (www.error == "400 Bad Request") {
				// alert for duplicate email address
			}
		}    
	}

	public void ParseJson(JSONNode data) 
	{
		token = data ["token"].Value;
		userId = data ["id"].Value;
	}


	/* 
	 * If inputs are valid, send PUT request to API to update user info.
	 * If inputs are invalid, do nothing.
	 */ 

	public void Button_Click ()
	{
		Debug.Log ("Save button clicked.");

		newName = ifctrl.username;
		newPW = ifctrl.password;

		CheckVariables ();

		if (nameChangeRequested || pwChangeRequested) {
			StartCoroutine(UpdateUser());
		} else {
			// do nothing
			// user didn't enter new name or password
		}
	}

	/*
	 * This is the PUT request. 
	 * If www returns with no errors, save new name and/or password in PlayerPrefs.
	 */

	IEnumerator UpdateUser ()
	{
		string fullUrl = ""; 
		string url = "http://104.131.144.86/api/users/";

		if (nameChangeRequested && pwChangeRequested) {
			fullUrl = url + userId + "?token=" + token + "&name=" + newName + "&password=" + newPW;
		} else if (nameChangeRequested) {
			fullUrl = url + userId + "?token=" + token + "&name=" + newName;
		} else if (pwChangeRequested) {
			fullUrl = url + userId + "?token=" + token + "&password=" + newPW;
		}

		using (UnityWebRequest www = UnityWebRequest.Put (fullUrl, "Hello")) {
			yield return www.Send ();

			if (www.isError) {
				Debug.Log ("Put error: " + www.error);
			} else {
				/*
				if (nameChangeRequested) {
					PlayerPrefs.SetString ("Name", newName);
					PlayerPrefs.Save ();
				} 
				*/

				if (pwChangeRequested) {
					PlayerPrefs.SetString ("Password", newPW);
					PlayerPrefs.Save ();
				}

				Debug.Log ("Put request successful");
				LoadScene ("UserProfileUI");
			}
		}

	}

	/*
	 * Check if inputted text is empty and if inputted text is valid.
	 */ 

	public void CheckVariables() {
		bool isNameEmpty = IsEmpty (newName);
		bool isPWEmpty = IsEmpty (newPW);
		bool isNameValid = validInputsCtrl.IsValidName (newName);
		bool isPWValid = validInputsCtrl.IsValidPassword (newPW);

		if (!isNameEmpty && isNameValid) {
			nameChangeRequested = true;
		}

		if (!isPWEmpty && isPWValid ) {
			pwChangeRequested = true;
		}

		if (!isNameEmpty && !isNameValid) {
			invalid_name.text = "New name is invalid. Please select a name that's 1-25 characters long.";
			nameChangeRequested = false;
			pwChangeRequested = false;
		}
			
		if (!isPWEmpty && !isPWValid) {
			invalid_password.text = "New password is invalid. Please select a password with at least 6 characters and no spaces.";
			nameChangeRequested = false;
			pwChangeRequested = false;
	
		}
	}
		
	public void LoadScene (string sceneName)
	{
		SceneManager.LoadScene (sceneName);
	}
		

	/* 
	 * Checks to see if inputted string is empty or null once
	 * white spaces have been removed.
	 */

	public bool IsEmpty(string str) {
	string noSpaces = Regex.Replace (str, @"\s+", ""); // remove whitespaces in string
		if (String.IsNullOrEmpty (noSpaces)) {
			return true;
		} else {
			return false;
		}
	}


}