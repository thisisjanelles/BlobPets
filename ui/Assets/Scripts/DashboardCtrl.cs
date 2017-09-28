using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using System.IO;
using System;

public class DashboardCtrl : MonoBehaviour
{
	public string type;

	public string name0;
	public string name1;
	public string name2;
	public string name3;
	public string name4;

	public string attribute0;
	public string attribute1;
	public string attribute2;
	public string attribute3;
	public string attribute4;

	public string ownerName0;
	public string ownerName1;
	public string ownerName2;
	public string ownerName3;
	public string ownerName4;

	public Text p0;
	public Text p1;
	public Text p2;
	public Text p3;
	public Text p4;

	public GameObject GO0;
	public GameObject GO1;
	public GameObject GO2;
	public GameObject GO3;
	public GameObject GO4;

	// Use this for initialization
	void Start ()
	{
		p0 = GO0.GetComponent<Text> ();
		p1 = GO1.GetComponent<Text> ();
		p2 = GO2.GetComponent<Text> ();
		p3 = GO3.GetComponent<Text> ();
		p4 = GO4.GetComponent<Text> ();

		CallDashboardAPI (type);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	/// <summary>
	/// Calls the dashboard API.
	/// </summary>
	/// <param name="type">The dashboard type requested. Either "user" or "blob".</param>
	public void CallDashboardAPI (string type)
	{
		string url = "http://104.131.144.86/api/dashboards/?type=" + type;
		Debug.Log ("url: " + url);
		WWW www = new WWW (url);
		StartCoroutine (GetDashboard (www, type));
	}

	IEnumerator GetDashboard (WWW www, string type)
	{
		

		yield return www;

		if (www.error == null) {
			JSONNode data = JSON.Parse (www.text);

			if (type == "user") {
				name0 = ParseJson (0, "name", data);
				name1 = ParseJson (1, "name", data);
				name2 = ParseJson (2, "name", data);
				name3 = ParseJson (3, "name", data);
				name4 = ParseJson (4, "name", data);

				attribute0 = ParseJson (0, "battles_won", data);
				attribute1 = ParseJson (1, "battles_won", data);
				attribute2 = ParseJson (2, "battles_won", data);
				attribute3 = ParseJson (3, "battles_won", data);
				attribute4 = ParseJson (4, "battles_won", data);


				p0.text = "Player Name: " + name0 + "\nBattles Won: " + attribute0;
				p1.text = "Player Name: " + name1 + "\nBattles Won: " + attribute1;
				p2.text = "Player Name: " + name2 + "\nBattles Won: " + attribute2;
				p3.text = "Player Name: " + name3 + "\nBattles Won: " + attribute3;
				p4.text = "Player Name: " + name4 + "\nBattles Won: " + attribute4;


			} else if (type == "blob") {
				name0 = ParseJson (0, "name", data);
				name1 = ParseJson (1, "name", data);
				name2 = ParseJson (2, "name", data);
				name3 = ParseJson (3, "name", data);
				name4 = ParseJson (4, "name", data);

				// user IDs for the top blobs
				attribute0 = ParseJson (0, "owner_id", data);
				attribute1 = ParseJson (1, "owner_id", data);
				attribute2 = ParseJson (2, "owner_id", data);
				attribute3 = ParseJson (3, "owner_id", data);
				attribute4 = ParseJson (4, "owner_id", data);

				// get the name that corresponds with the user IDs
				GetUserName (attribute0, "ownerName0");
				GetUserName (attribute1, "ownerName1");
				GetUserName (attribute2, "ownerName2");
				GetUserName (attribute3, "ownerName3");
				GetUserName (attribute4, "ownerName4");

				//Invoke ("PrintTopBlobs", 3);
			}
		} else {

		}
	}

	/// <summary>
	/// Gets the name of the user.
	/// </summary>
	/// <param name="userId">User identifier.</param>
	/// <param name="variable">Variable.</param>
	public void GetUserName (string userId, string variable)
	{
		string url = "http://104.131.144.86/api/users/";
		string fullUrl = url + userId;
		WWW www = new WWW (fullUrl);

		StartCoroutine (GetUserInfo (www, variable)); 
	}

	IEnumerator GetUserInfo (WWW www, string variable)
	{
		yield return www;

		if (www.error == null) {
			//Debug.Log (www.text);
			JSONNode result = JSON.Parse (www.text);

			if (variable == "ownerName0") {
				ownerName0 = ParseJson ("name", result);
			} else if (variable == "ownerName1") {
				ownerName1 = ParseJson ("name", result);
			} else if (variable == "ownerName2") {
				ownerName2 = ParseJson ("name", result);
			} else if (variable == "ownerName3") {
				ownerName3 = ParseJson ("name", result);
			} else if (variable == "ownerName4") {
				ownerName4 = ParseJson ("name", result);
			}

			if (!String.IsNullOrEmpty (ownerName0) &&
				!String.IsNullOrEmpty (ownerName1) &&
				!String.IsNullOrEmpty (ownerName2) &&
				!String.IsNullOrEmpty (ownerName3) &&
				!String.IsNullOrEmpty (ownerName4)) {

				PrintTopBlobs ();
			}
		}
	}

	/// <summary>
	/// Prints the top blobs on the scene.
	/// </summary>
	public void PrintTopBlobs ()
	{
		p0.text = "Blob Name: " + name0 + "\nOwner Name: " + ownerName0;
		p1.text = "Blob Name: " + name1 + "\nOwner Name: " + ownerName1;
		p2.text = "Blob Name: " + name2 + "\nOwner Name: " + ownerName2;
		p3.text = "Blob Name: " + name3 + "\nOwner Name: " + ownerName3;
		p4.text = "Blob Name: " + name4 + "\nOwner Name: " + ownerName4;
	}


	/// <summary>
	/// Overload method. Parse JSON array.
	/// </summary>
	/// <returns>String value corresponding to the given inputs</returns>
	/// <param name="position">Array index</param>
	/// <param name="id">JSON ID</param>
	/// <param name="data">JSON to be parsed</param>
	public string ParseJson (int position, string id, JSONNode data)
	{
		return data [position] [id].Value;
	}


	/// <summary>
	/// Overload method. Parse JSON.
	/// </summary>
	/// <returns>String value corresponding to the given inputs</returns>
	/// <param name="id">JSON ID</param>
	/// <param name="data">JSON to be parsed</param>
	public string ParseJson (string id, JSONNode data)
	{
		return data [id].Value;
	}
}
