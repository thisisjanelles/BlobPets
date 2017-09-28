using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Battle : MonoBehaviour
{
	private string blobID1;
	private string blobID2;

	public string token;
	public string email;
	public string password;

	// battle notification warning
	public Image battleWarning;
	public GameObject battleGO;

	public Text warning_label;
	public GameObject WL_GO;

	public Text warning_body;
	public GameObject WB_GO;

	public Button ok_button;
	public GameObject ok_GO;

	public JSONNode results;

	string url = "http://104.131.144.86/api/battles";

	public PlayerPreferences pp;

	// Use this for initialization
	void Start ()
	{
		email = pp.GetEmail ();
		password = pp.GetPassword ();

		Debug.Log ("email: " + email);
		Debug.Log ("password: " + password);

		blobID1 = PlayerPrefs.GetString ("RequestedBlobId");
		blobID2 = PlayerPrefs.GetString ("opponentBlobId");

		ok_button = ok_GO.GetComponent<Button> ();

		battleWarning = battleGO.GetComponent<Image> ();
		warning_label = WL_GO.GetComponent<Text> ();
		warning_body = WB_GO.GetComponent<Text> ();

		DisableWarningWindow (0);

		SendTokenRequest (email, password);
	}

	// Update is called once per frame
	void Update ()
	{
		
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
			CallBattleAPI ();
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
	}

	public void CallBattleAPI() 
	{
		
		Debug.Log ("BATTLE.CS - entering CallBlobAPI()...");
		Debug.Log ("token: " + token);
		Debug.Log ("blob1 : " + blobID1);
		Debug.Log ("blob2: " + blobID2);

		Dictionary<string, string> headers = new Dictionary<string, string> ();
		headers.Add("Content-Type", "application/x-www-form-urlencoded");
		headers.Add ("Authorization", "Bearer " + token);
		WWWForm form = new WWWForm ();
		form.AddField ("token", token);
		form.AddField ("blob1", blobID1);
		form.AddField ("blob2", blobID2);
		byte[] rawData = form.data;
		WWW www = new WWW (url, rawData, headers);
		StartCoroutine (WaitForBattleRequest (www));
	}

	IEnumerator WaitForBattleRequest (WWW www)
	{
		yield return www;

		// check for errors
		if (www.error == null) {
			Debug.Log ("!!! BATTLE OK.");
			results = JSON.Parse(www.text);
			string battleRecordID = results ["BattleRecordID"].Value;
			string recordURL = url + "/" + battleRecordID;
			getResults (recordURL);
		} else {
			Debug.Log ("!!! BATTLE ERROR.");
			Debug.Log ("***WWW Error: " + www.error);
			JSONNode N = JSON.Parse (www.text);
			Debug.Log (N ["error"].Value);

		}    
	}

	public void getResults(string url){
		// serveraddress.com/api/battles/[id]
		WWW www = new WWW (url);
		StartCoroutine (GetResultRequest (www));
	}

	IEnumerator GetResultRequest (WWW www)
	{
		yield return www;

		// check for errors
		if (www.error == null) {
			results = JSON.Parse (www.text);
			string winner = results ["winnerBlobID"].Value;
			string loser = results ["loserBlobID"].Value;
			Debug.Log ("winner: " + winner);
			Debug.Log ("loser: " + loser);
			if (blobID1 == winner) {
				SetWarningWindowText (0);
			} else {
				SetWarningWindowText (1);
			}
			yield return new WaitForSeconds (2);
			EnableWarningWindow (0);
			Debug.Log ("WWW Ok!: " + www.text);
		} else {
			Debug.Log ("WWW Error: " + www.error);
		}    
	}

	// loserBlobID":2,"winnerBlobID":5

	public void SetWarningWindowText (int msgCode)
	{
		if (msgCode == 0) {
			warning_body.text = "You won!";
		} else if (msgCode == 1) {
			warning_body.text = "You lost...";
		}
	}

	public void EnableWarningWindow(int msgCode)
	{
		battleWarning.enabled = true;
		warning_label.enabled = true;
		warning_body.enabled = true;

		if (msgCode == 0) {
			ok_GO.SetActive (true);
		} else if (msgCode == 1) {
			ok_GO.SetActive (false);
		}
	}

	public void DisableWarningWindow(int msgCode)
	{
		battleWarning.enabled = false;
		warning_label.enabled = false;
		warning_body.enabled = false;
		ok_GO.SetActive (false);
	}

	public void OkayButtonClicked()
	{
		DisableWarningWindow (0);
		SceneManager.LoadScene ("UserProfileUI");
	}
}
