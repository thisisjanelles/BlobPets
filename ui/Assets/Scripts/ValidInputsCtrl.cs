using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

/*
 * This script is for methods that determine if the inputted username, email, 
 * and/or password are valid. This script is called when registering a new user
 * and editing an existing user's profile.
 */

public class ValidInputsCtrl : MonoBehaviour {

	/*
	 * Returns true if inputted name is a valid name, else false.
	 * A valid name is at least 1 character (that isn't a white space) long
	 * and no longer than 25 characters long.
	 * 
	 * Input(s):
	 * - n: new name inputted into name text field.
	 */

	public bool IsValidName(string n) 
	{
		int len = n.Length;

		string nNoSpaces = Regex.Replace (n, @"\s+", "");
		int lenNoSpaces = nNoSpaces.Length;

		if ((lenNoSpaces >= 1) && len <= 25) {
			//HideNamePanel ();
			return true;
		} else {
			return false;
		}
	}

	/*
	 * Returns true if inputted email address is a valid email 
	 * address format, else false.
	 * 
	 * Input(s):
	 * - emailAddress: new email address inputted into email address text field.
	 */

	public bool IsValidEmailAddress (string emailAddress)
	{
		Regex rx = new Regex (
			@"^[-!#$%&'*+/0-9=?A-Z^_a-z{|}~](\.?[-!#$%&'*+/0-9=?A-Z^_a-z{|}~])*@[a-zA-Z](-?[a-zA-Z0-9])*(\.[a-zA-Z](-?[a-zA-Z0-9])*)+$"
			           );
		bool res= rx.IsMatch (emailAddress);
		if (res == true) {
			//HideEmailPanel();
			return true;
		} else {
			return false;
		}

	}

	/*
	 * Returns true if inputted password is valid, else false.
	 * A valid password is at least 6 characters long and doesn't contain spaces.
	 * 
	 * Input(s):
	 * - pw: new password inputted into password text field.
	 */

	public bool IsValidPassword (string pw)
	{
		bool containsWhiteSpace = pw.Contains (" ");
		int len = pw.Length;

		if ((len >= 6) && !containsWhiteSpace) {
			//HideEmailPanel ();
			return true;
		} else {
			return false;
		}
	}

	/*
	 * Checks to see if someone is already registered in the system under the
	 * inputted email and password combo by sending a token request with the listed info.
	 * Returns true if inputted info do not belong to an existing user, else false.
	 * 
	 * Input(s):
	 * - n: new name inputted into name text field.
	 * - emailAddress: new email address inputted into email address text field.
	 * - pw: new password inputted into password text field.
	 */

	/*
	public bool IsValidNewUserCombo (string emailAddress, string pw)
	{
		bool userExists = tokenCtrl.userExists;

		if (userExists) {
			return true;
		} else {
			return false;
		}
	}
	*/

	/*
	* Get input name for new user. 
		* 
		* Input(s):
		* - nameField: new name inputted into name text field.
		*/


}
