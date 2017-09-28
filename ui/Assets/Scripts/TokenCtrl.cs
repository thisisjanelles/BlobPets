using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using SimpleJSON;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.ComponentModel;
using System.Runtime.Remoting;
using UnityEngine.Networking;

public class TokenCtrl : MonoBehaviour
{

	public string token = "";
	public string userId = "";
	//public string email = "";
	//public string password = "";

	public bool userExists;

	public string invalidServerError = "invalid_credentials";
	public string noTokenServerError = "could_not_create_token";

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
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

			userExists = true;
			Debug.Log("!!! USER EXISTS.");

			ParseTokenJson (N);


		} else {
			Debug.Log ("***WWW Error: " + www.error);
			userExists = false;
			Debug.Log("!!! USER DOESN'T EXIST.");
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

	public bool getUserExists()
	{
		return userExists;
	}

	public string getToken()
	{
		return token;
	}
}
