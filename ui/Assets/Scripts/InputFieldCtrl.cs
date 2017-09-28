using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
//using UnityEditor;
using System.Security.Cryptography;

public class InputFieldCtrl : MonoBehaviour {

	public string username = "";
	public string email = "";
	public string password = "";

	//public GameObject inputField;
	public InputField Name_Field;
	public InputField Password_Field;
	public InputField Email_Field;



	// Use this for initialization
	void Start () {
		
	}

	public string getName()
	{
		return username;
	}

	public string getEmail()
	{
		return email;
	}

	public string getPassword()
	{
		return password;
	}

	


	public void setName (string name)
	{
		
		username = name;
		Debug.Log ("Name: " + username);
	}
		
	/*
	 * Get input email for new user.
	 * 
	 * Input(s):
	 * - pwField: new password inputted into password field.
	 */

	public void setEmail (string inputField)
	{
		email = inputField.ToLower ();
		Debug.Log ("Email: " + email);

	}

	/*
	 * Get input password for new user.
	 * 
	 * Input(s):
	 * - pwField: new password inputted into password field.
	 */

	public void setPassword (string inputField)
	{
		password = inputField;
		Debug.Log ("Password: " + password);
	}

}
