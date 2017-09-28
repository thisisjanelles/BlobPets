using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using System;

public class LoginCtrl : MonoBehaviour {
	public InputFieldCtrl ifctrl;
	public ButtonCtrl buttonCtrl;

	//private string nameKey = "Name";
	private string emailKey = "Email";
	private string passwordKey = "Password";
	private string idKey = "UserId";

	public string email;
	public string password;
	public string token;
	public string userId;
	public bool userExists;
	public GameObject panel;

	public JSONNode result;


	// Use this for initialization
	void Start () {
		HidePanel ();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void SaveUserInfo(string user, string emailAddr, string passwrd, int userid) 
	{
		PlayerPrefs.SetString("Name", user);
		PlayerPrefs.SetString ("Email", emailAddr);
		PlayerPrefs.SetString ("Password", passwrd);
		PlayerPrefs.SetInt ("UserId", userid);
	}



	public void ButtonClick()
	{
		Debug.Log ("BUTTON CLICKED");

		email = ifctrl.getEmail ();
		password = ifctrl.getPassword ();

		SendTokenRequest (email, password);
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
			Debug.Log("!!! USER EXISTS.");
			userExists = true;
			JSONNode N = JSON.Parse (www.text);
			ParseJson (N);

			PlayerPrefs.SetString (emailKey, email);
			PlayerPrefs.SetString (passwordKey, password);
			PlayerPrefs.SetInt (idKey, Int32.Parse (userId));


				
				

			Debug.Log (email);
			Debug.Log (password);
			Debug.Log (userId);

			SceneManager.LoadScene ("UserProfileUI");


		} else {
			Debug.Log("!!! USER DOESN'T EXIST.");
			Debug.Log ("***WWW Error: " + www.error);
			userExists = false;
			ShowPanel ();
		}    
	}

	public void ParseJson(JSONNode data) 
	{
		token = data ["token"].Value;
		userId = data ["id"].Value;
	}

	public void ShowPanel()
	{
		panel.gameObject.SetActive (true);
	}

	public void HidePanel()
	{
		panel.gameObject.SetActive (false);
	}


}
