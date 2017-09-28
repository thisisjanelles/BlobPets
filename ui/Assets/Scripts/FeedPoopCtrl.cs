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

// 2017-03-07 21:14:17

// server uses UTC time
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Configuration;

public class FeedPoopCtrl : MonoBehaviour
{
	public PlayerPreferences playerPreferences;

	public double longitude;
	public double latitude;

	// JSON
	public JSONNode N;
	public string nextCleanTime;
	public string nextFeedTime;
	public string endRestTime;
	public string level;
	public string cleanlinessLevel;
	public string healthLevel;
	public string exerciseLevel;
	public string blobLevel;
	public bool isResting;


	//private string nameKey = "Name";
	private string emailKey = "Email";
	private string passwordKey = "Password";
	private string idKey = "UserId";

	public bool userExists;
	public string userId;
	public string email;
	public string password;

	public string url = "http://104.131.144.86/api/blobs/";
	public string blobId;
	public string blobName0 = "";
	public string blobName1 = "";

	public string token;

	public Image imgPoop;
	public Image imgHam;
	public Image imgThoughtBub;

	public GameObject poopGO;
	public GameObject hamGO;
	public GameObject thoughtGO;

	public bool needsCleaning = false;
	public bool needsFeeding = false;

	public DateTime dateTime;
	public DateTime cleanTime;
	public DateTime feedTime;
	public DateTime endRest;
	public int cleanComp;
	public int feedComp;

	// level bars
	public Text h;
	// health level
	public Text e;
	// exercise level
	public Text c;
	// cleanliness level
	public Text b;
	// blob level

	public GameObject hGO;
	// health level
	public GameObject eGO;
	// exercise level
	public GameObject cGO;
	// cleanliness level
	public GameObject bGO;
	// blob level

	public Button battleBtn;
	public GameObject blueBattleBtnGO;

	// battle notification warning
	public Image battleWarning;
	public GameObject battleGO;

	public Text warning_label;
	public GameObject WL_GO;

	public Text warning_body;
	public GameObject WB_GO;

	public Button yes_button;
	public GameObject yes_GO;

	public Button no_button;
	public GameObject no_GO;

	public Button ok_button;
	public GameObject ok_GO;

	// battle
	public GPSCtrl battleGPS;
	private GameObject gpsObject;
	public double currentLat;
	public double currentLong;

	// Use this for initialization
	void Start ()
	{
		ok_button = ok_GO.GetComponent<Button> ();

		battleBtn = blueBattleBtnGO.GetComponent<Button> ();

		battleWarning = battleGO.GetComponent<Image> ();
		warning_label = WL_GO.GetComponent<Text> ();
		warning_body = WB_GO.GetComponent<Text> ();
		yes_button = yes_GO.GetComponent<Button> ();
		no_button = no_GO.GetComponent<Button> ();

		h = hGO.GetComponent<Text> ();
		c = cGO.GetComponent<Text> ();
		e = eGO.GetComponent<Text> ();
		b = bGO.GetComponent<Text> ();


		DisableWarningWindow (0);

		string blobIdKey = "RequestedBlobId";
		if (PlayerPrefs.HasKey (blobIdKey)) {
			blobId = PlayerPrefs.GetString (blobIdKey);
		}

		Debug.Log ("blobId: " + blobId);

		if (PlayerPrefs.HasKey (emailKey)) {
			email = PlayerPrefs.GetString (emailKey);
		}

		if (PlayerPrefs.HasKey (passwordKey)) {
			password = PlayerPrefs.GetString (passwordKey);
		}

		if (PlayerPrefs.HasKey (idKey)) {
			userId = PlayerPrefs.GetString (idKey);
		}

		Debug.Log ("IN FEEDPOOP START()...");

		Debug.Log ("email: " + email);
		Debug.Log ("password: " + password);

		imgPoop = poopGO.GetComponent<Image> ();
		imgHam = hamGO.GetComponent<Image> ();
		imgThoughtBub = thoughtGO.GetComponent<Image> ();

		imgPoop.enabled = false;
		imgHam.enabled = false;
		imgThoughtBub.enabled = false;

		SendTokenRequest (email, password);

	}



	// Update is called once per frame
	void Update ()
	{

		dateTime = DateTime.Now.ToUniversalTime ();
		//Debug.Log ("Current time in UTC: " + dateTime.ToString ());

		cleanTime = Convert.ToDateTime (nextCleanTime);
		feedTime = Convert.ToDateTime (nextFeedTime);

		cleanComp = DateTime.Compare (dateTime, cleanTime);
		feedComp = DateTime.Compare (dateTime, feedTime);
	}

	/// <summary>
	/// Sends the token request via POST request to API.
	/// </summary>
	/// <param name="email">Email address of logged in user. </param>
	/// <param name="password">Password of logged in user. </param>
	public void SendTokenRequest (string email, string password)
	{
		Debug.Log ("IN SENDTOKENREQUEST...");
		string tokenUrl = "http://104.131.144.86/api/users/authenticate";
		WWWForm form = new WWWForm ();
		form.AddField ("email", email);
		form.AddField ("password", password);
		WWW www = new WWW (tokenUrl, form);
		StartCoroutine (WaitForRequest (www));
	}

	/// <summary>
	/// Waits for request.
	/// </summary>
	/// <returns>The for request.</returns>
	/// <param name="www">Www.</param>
	IEnumerator WaitForRequest (WWW www)
	{
		yield return www;

		// check for errors
		if (www.error == null) {
			
			userExists = true;
			JSONNode N = JSON.Parse (www.text);
			ParseJson (N);

			Debug.Log ("Got token. Token: " + token);
			GetBlob ();

			PlayerPrefs.SetString (emailKey, email);
			PlayerPrefs.SetString (passwordKey, password);
			PlayerPrefs.SetInt (idKey, Int32.Parse (userId));
			PlayerPrefs.Save ();

		} else {
			Debug.Log ("Didn't get token.");
			Debug.Log ("***WWW Error: " + www.error);
			userExists = false;

		}    
	}

	public void ParseJson (JSONNode data)
	{
		token = data ["token"].Value;
		userId = data ["id"].Value;
	}

	public void GetBlob ()
	{
		Debug.Log ("IN GETBLOB()....");

		WWW www = new WWW (url + blobId);
		StartCoroutine (GetBlobInfo (www));
	}


	IEnumerator GetBlobInfo (WWW www)
	{
		yield return www;

		// check for errors
		if (www.error == null) {
			N = JSON.Parse (www.text);
			Debug.Log("GetBlobInfo: " + www.text);

			ParseJson ();
			PrintLevelBars ();
			CompareTimes ();
			Debug.Log ("GetBlobInfo OK: " + www.text);
		} else {
			Debug.Log ("WWW Error: " + www.error);
		}    
	}

	public void ParseJson ()
	{
		nextCleanTime = N ["next_cleanup_time"].Value;
		nextFeedTime = N ["next_feed_time"].Value;
		cleanlinessLevel = N ["cleanliness_level"].Value;
		healthLevel = N ["health_level"].Value;
		exerciseLevel = N ["exercise_level"].Value;
		blobName0 = N ["name"].Value;
		blobLevel = N ["level"].Value;
		endRestTime = N ["end_rest"].Value;

		/*
		Debug.Log ("Parsing blob info in ParseJson()...");
		Debug.Log ("nextCleanTime: " + nextCleanTime);
		Debug.Log ("nextFeedTime: " + nextFeedTime);
		Debug.Log ("endRestTime: " + endRestTime);
		*/
	
	}


	public void CompareTimes ()
	{

		DateTime dateTime = DateTime.Now.ToUniversalTime ();
		Debug.Log ("Current time in UTC: " + dateTime.ToString ());

		DateTime cleanTime = Convert.ToDateTime (nextCleanTime);
		DateTime feedTime = Convert.ToDateTime (nextFeedTime);
		DateTime endTime = Convert.ToDateTime (endRestTime);

		int cleanComp = DateTime.Compare (dateTime, cleanTime);
		int feedComp = DateTime.Compare (dateTime, feedTime);
		int endComp = DateTime.Compare (dateTime, endTime);

		Debug.Log ("cleanComp: " + cleanComp);
		Debug.Log ("feedComp: " + feedComp);
		Debug.Log ("endComp: " + endComp);

		if (cleanComp < 0) {
			// dateTime is earlier than cleanTime

			if (imgPoop.enabled != false) {
				imgPoop.enabled = false;
			}

		} else if (cleanComp == 0 || cleanComp > 0) {
			// == 0 : dateTime same as CleanTime
			// > 0 : dateTime is later than cleanTime

			// print poop img

			if (imgPoop.enabled != true) {
				Debug.Log ("PRINTING POOP...");

				needsCleaning = true;
				imgPoop.enabled = true;
			}
		}

		if (feedComp < 0) {
			// dateTime is earlier than cleanTime
			imgThoughtBub.enabled = false;
			imgHam.enabled = false;
		} else if (feedComp == 0 || cleanComp > 0) {
			// == 0 : dateTime same as CleanTime
			// > 0 : dateTime is later than cleanTime
			Debug.Log ("PRINTING FOOD...");
			needsFeeding = true;
			imgThoughtBub.enabled = true;
			Invoke ("EnableHam", 1);
		}


		if (endComp < 0) {
			// dateTime is earlier than end_rest
			isResting = true;
			//DisableWarningWindow (0);
		} else if (endComp == 0 || endComp > 0) {
			// == 0 : dateTime same as CleanTime
			// > 0 : dateTime is later than cleanTime

			isResting = false;
			//SetWarningWindowText (1);
			//EnableWarningWindow (1);
		}

	}



	public void FeedButtonClicked ()
	{
		// do something if blob needs feeding
		if (needsFeeding) {
			Debug.Log ("Feed button clicked.");
			StartCoroutine (UpdateBlob ("feed"));

		}
	}

	public void CleanButtonClicked ()
	{
		// do something if blob needs cleaning
		if (needsCleaning) {
			Debug.Log ("Clean button clicked.");
			StartCoroutine (UpdateBlob ("clean"));
		}
	}

	// PUT request
	IEnumerator UpdateBlob (string button)
	{
		string myData = "Hello";	// need to send a dummy string in UnityWebReqest.Put, otherwise it won't work
		string finalUrl;

		if (button == "feed") {
			finalUrl = url + blobId + "?token=" + token + "&health_level=" + healthLevel;
		} else {
			finalUrl = url + blobId + "?token=" + token + "&cleanliness_level=" + cleanlinessLevel;
		}

		Debug.Log ("final URL... " + finalUrl);

		using (UnityWebRequest www = UnityWebRequest.Put (finalUrl, myData)) {
			yield return www.Send ();

			if (www.isError) {
				Debug.Log ("PUT ERROR: " + www.error);
			} else {
				Debug.Log ("PUT REQUEST SUCCESSFUL.");
				Debug.Log (www.url.ToString ());

				GetBlob ();
				// get next clean and/or feed times

				if (button == "feed") {
					needsFeeding = false;
					imgHam.enabled = false;
					Invoke ("DisableBubble", 1);


				} else if (button == "clean") {
					needsCleaning = false;
					imgPoop.enabled = false;
				}

				//GetBlob ();
			}

		}
	}

	// show ham img
	public void EnableHam ()
	{
		imgHam.enabled = true;
	}

	// hide thought bubble img
	public void DisableBubble ()
	{
		imgThoughtBub.enabled = false;
	}

	public void BattleButtonClicked ()
	{
		Debug.Log ("Battle Button clicked...");

		int healthLevelInt;
		int cleanlinessLevelInt;
		int exerciseLevelInt;

		decimal val = Decimal.Parse (exerciseLevel);
		exerciseLevelInt = Convert.ToInt32 (val);


		Int32.TryParse (healthLevel, out healthLevelInt);	// feed
		Int32.TryParse (cleanlinessLevel, out cleanlinessLevelInt);	// poop

		/*
		Debug.Log ("Lat: " + currentLat + " Long: " + currentLong);

		Debug.Log ("battle button clicked");
		Debug.Log ("string exercise: " + exerciseLevel);

		Debug.Log ("health: " + healthLevelInt);
		Debug.Log ("clean: " + cleanlinessLevelInt);
		Debug.Log ("exercise: " + exerciseLevelInt);
*/


		if (healthLevelInt < 10) {
			SetWarningWindowText (2);
			EnableWarningWindow (1);
		} else if (cleanlinessLevelInt < 10 || exerciseLevelInt < 10) {
			SetWarningWindowText (0);
			EnableWarningWindow (0);
		} else if (isResting){
			SetWarningWindowText (1);
			EnableWarningWindow (1);
		}

			else {
			
			StartCoroutine (StartLocationService ());
			//StartCoroutine (BattlePutRequest ());

			/*
			gpsObject = GameObject.Find ("GPSCTRL");
			battleGPS = (GPSCtrl)gpsObject.GetComponent (typeof(GPSCtrl));
			battleGPS.StartLocationService ();
			battleGPS.GetCurrentLocation ();
			currentLat = battleGPS.lat2;
			currentLong = battleGPS.long2;
			StartCoroutine (BattlePutRequest ());
			*/
		}
	}

	/// <summary>
	/// Starts the location service.
	/// </summary>
	public IEnumerator StartLocationService ()
	{
		// First, check if user has location service enabled
		if (!Input.location.isEnabledByUser) {
			SetWarningWindowText (3);
			EnableWarningWindow (1);
			yield break;
		}
		// Start service before querying location
		Input.location.Start (5f, 5f);	// accuracy = 5m, update frequency = 5m


		// Wait until service initializes
		int maxWait = 20;
		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
			yield return new WaitForSeconds (1);
			maxWait--;
		}

		// Service didn't initialize in 20 seconds
		if (maxWait < 1) {
			SetWarningWindowText (5);
			EnableWarningWindow (1);
			yield break;
		}

		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed) {
			SetWarningWindowText (4);
			EnableWarningWindow (1);
			yield break;
		} else {
			longitude = Input.location.lastData.longitude;
			latitude = Input.location.lastData.latitude;
			Debug.Log ("Long: " + longitude);
			Debug.Log ("Lat: " + latitude);
			StartCoroutine (BattlePutRequest ());
		}

		// Stop service if there is no need to query location updates continuously
		Input.location.Stop ();
	}



	/// <summary>
	/// PUT request to set lat and lon. Then calls GetBattleUsers() to GET list of users near you.
	/// </summary>
	/// <returns>The put request.</returns>
	IEnumerator BattlePutRequest ()
	{
		double testLat = 49.2641983;
		double testLong = -123.1583633;

		string myData = "Hello";	// need to send a dummy string in UnityWebReqest.Put, otherwise it won't work
		string userURL = "http://104.131.144.86/api/users/";
		string finalUrl = userURL + userId + "?token=" + token + "&lat=" + latitude + "&long=" + longitude;
		Debug.Log ("final URL... " + finalUrl);

		using (UnityWebRequest www = UnityWebRequest.Put (finalUrl, myData)) {
			yield return www.Send ();

			if (www.isError) {
				Debug.Log ("BattlePutRequest failed: " + www.error);
			} else {
				Debug.Log ("BattlePutRequest successful");
				Debug.Log (www.url.ToString ());
				//string battleGetURL = userURL + "?type=nearby&lat=" + latitude + "&long=" + longitude;

				// serverAddress.com/api/users/?type=nearby&lat=<>&long=<>
				string battleGetURL = userURL + "?type=nearby&lat=" + latitude + "&long=" + longitude;
				Debug.Log ("battleGetURL: " + battleGetURL);
				GetBattleUsers (battleGetURL);
			}
		}
	}

	public void GetBattleUsers (string url)
	{
		WWW www = new WWW (url);
		StartCoroutine (BattleGetRequest (www));
	}

	IEnumerator BattleGetRequest (WWW www)
	{
		yield return www;

		// check for errors
		if (www.error == null) {
			PlayerPrefs.SetString ("battleUserList", www.text);
			Debug.Log ("WWW Ok!: " + www.text);
			SceneManager.LoadScene ("BattleMain");
			Debug.Log ("WWW Ok!: " + www.text);
		} else {
			Debug.Log ("WWW Error: " + www.error);

		}    
	}

	public void WarningButtonClicked (string cmd)
	{
		if (cmd == "yes") {
			gpsObject = GameObject.Find ("GPSCTRL");
			battleGPS = (GPSCtrl)gpsObject.GetComponent (typeof(GPSCtrl));
			battleGPS.StartLocationService ();
			battleGPS.GetCurrentLocation ();
			currentLat = battleGPS.lat2;
			currentLong = battleGPS.long2;
			StartCoroutine (BattlePutRequest ());
		} else if (cmd == "no") {
			DisableWarningWindow (0);
		}
	}

	public void SetWarningWindowText (int msgCode)
	{
		if (msgCode == 0) {
			warning_body.text = "Your blob is too weak to battle. Would you like to do it anyway?";
		} else if (msgCode == 1) {
			Debug.Log ("setting rest message.");
			warning_body.text = "Your blob is tired and needs to rest before it can battle again. Try again later.";
		} else if (msgCode == 2) {
			warning_body.text = "Your blob is too unhealthy to battle. Feed it better and try again once its health level reaches 10.";
		} else if (msgCode == 3) {
			warning_body.text = "Your phone's location setting is turned off. You need this service to battle. Go to your phone's Settings -> Location to turn on this service."; 
		} else if (msgCode == 4) {
			warning_body.text = "Unable to determine device location.";
		} else if (msgCode == 5) {
			warning_body.text = "Couldn't initialize GPS. Timed out.";
		}
	}

	public void DisableWarningWindow (int msgCode)
	{
		battleWarning.enabled = false;
		warning_label.enabled = false;
		warning_body.enabled = false;
		yes_GO.SetActive (false);
		no_GO.SetActive (false);
		ok_GO.SetActive (false);
	}

	public void EnableWarningWindow (int msgCode)
	{
		battleWarning.enabled = true;
		warning_label.enabled = true;
		warning_body.enabled = true;

		if (msgCode == 0) {
			yes_GO.SetActive (true);
			no_GO.SetActive (true);
			ok_GO.SetActive (false);
		} else if (msgCode == 1) {
			yes_GO.SetActive (false);
			no_GO.SetActive (false);
			ok_GO.SetActive (true);
		}
	}

	public void PrintLevelBars ()
	{
		decimal val = Decimal.Parse (exerciseLevel);
		//int level = Convert.ToInt32 (val);
		int levelRounded = (int)Math.Round (val, 0);

		h.text = healthLevel;
		c.text = cleanlinessLevel;
		e.text = levelRounded.ToString ();
		b.text = blobLevel;


	}

	public void OkayButtonClicked ()
	{
		DisableWarningWindow (0);
	}
}