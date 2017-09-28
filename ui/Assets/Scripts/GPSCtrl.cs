using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SimpleJSON;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GPSCtrl : MonoBehaviour
{
	public PlayerPreferences pp;

	public int eid;

	public Text distance_label;
	public GameObject distanceObject;

	public Text end;
	public GameObject endObject;


	public double long1 = 0;
	public double lat1 = 0;

	public double long2 = 0;
	public double lat2 = 0;

	public double currLong;
	public double currLat;

	public double distanceWalked = 0;

	public string token;

	void Start ()
	{
		
		
		distance_label = distanceObject.GetComponent<Text> ();
		end = endObject.GetComponent<Text> ();

		distance_label.text = " ";
		end.text = " ";

	}


	/// <summary>
	/// Makes a call to start GPS service when Start button is clicked.
	/// </summary>
	public void StartButtonClicked ()
	{
		if (Input.location.isEnabledByUser) {
			distance_label.text = "You and your blobs have walked " + Math.Round (distanceWalked, 2) + " km so far on this trip.";
		}

		StartCoroutine (StartLocationService ());
	}

	/// <summary>
	/// Stops GPS service when Stop button is clicked.
	/// </summary>
	public void StopButtonClicked ()
	{
		double roundedDistance = RoundDistanceWalked ();
		Input.location.Stop ();
		end.text = "You've ended your trip. Good job! " + roundedDistance + " km will be added to your exercise record.";


		//StartCoroutine(UpdateExerciseRecord (eid, roundedDistance));
		string email = pp.GetEmail ();
		string password = pp.GetPassword ();
		SendTokenRequest (email, password);
	}




	/// <summary>
	/// Starts the location service.
	/// </summary>
	public IEnumerator StartLocationService ()
	{
		// First, check if user has location service enabled
		if (!Input.location.isEnabledByUser)
			//yield break;
			distance_label.text = "Your phone's location setting is turned off. Go to your phone's Settings -> Location to turn on this service.";

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
			distance_label.text = "Timed out";
			yield break;
		}
			
		// Connection has failed
		if (Input.location.status == LocationServiceStatus.Failed) {
			distance_label.text = "Unable to determine device location";
			yield break;
		} else {
			long1 = long2 = Input.location.lastData.longitude;
			lat1 = lat2 = Input.location.lastData.latitude;

			InvokeRepeating ("GetCurrentLocation", 10, 10);
			InvokeRepeating ("HasTraveled", 10, 10);
		}

		// Stop service if there is no need to query location updates continuously
		//Input.location.Stop ();
	}

	/// <summary>
	/// Gets the current location.
	/// </summary>
	public void GetCurrentLocation ()
	{

		if (Input.location.status == LocationServiceStatus.Running) {
			lat2 = Input.location.lastData.latitude;
			long2 = Input.location.lastData.longitude;
		}
	}

	/// <summary>
	/// Calculates distance traveled by user. Sets current geo coordinates as the original geo coordinates.
	/// </summary>
	public void HasTraveled ()
	{
		distanceWalked += CalculateDistance (lat1, long1, lat2, long2);
		double roundedDistance = RoundDistanceWalked ();
		distance_label.text = "You and your blobs have walked " + roundedDistance + " km so far on this trip.";
		long1 = long2;
		lat1 = lat2;
	}


	/// <summary>
	/// Calculate the distance between two geo coordinates.
	/// </summary>
	/// <returns>Distance traveled in km.</returns>
	/// <param name="latitude1">Latitude1.</param>
	/// <param name="longitude1">Longitude1.</param>
	/// <param name="latitude2">Latitude2.</param>
	/// <param name="longitude2">Longitude2.</param>
	public double CalculateDistance (double latitude1, double longitude1, double latitude2, double longitude2)
	{
		double radiansOverDegrees = (Math.PI / 180.0);
		double R = 6371;	// Earth radius in km

		double radiansLat1 = latitude1 * radiansOverDegrees;
		double radiansLong1 = longitude1 * radiansOverDegrees;

		double radiansLat2 = latitude2 * radiansOverDegrees;
		double radiansLong2 = longitude2 * radiansOverDegrees;

		double dLat = radiansLat2 - radiansLat1;
		double dLong = radiansLong2 - radiansLong1;

		double a = Math.Sin (dLat / 2) * Math.Sin (dLat / 2) +
		           Math.Sin (dLong / 2) * Math.Sin (dLong / 2) * Math.Cos (radiansLat1) * Math.Cos (radiansLat2);
		double c = 2 * Math.Atan2 (Math.Sqrt (a), Math.Sqrt (1 - a));
		double d = R * c;

		return d;
	}

	/// <summary>
	/// Sends the token request.
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

			double roundedDistance = RoundDistanceWalked ();
			/*
			string exerciseKey = "ExerciseRecordId";
			if (PlayerPrefs.HasKey (exerciseKey)) {
				int eid = PlayerPrefs.GetInt (exerciseKey);
				*/
			eid = pp.GetExercise ();

			Debug.Log ("eid: " + eid);
			StartCoroutine (UpdateExerciseRecord (eid, roundedDistance));

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

	/// <summary>
	/// Rounds the distance walked to 2 decimal places.
	/// </summary>
	/// <returns>The distance walked.</returns>
	public double RoundDistanceWalked ()
	{
		return Math.Round (distanceWalked, 2);
	}

	// PUT request
	IEnumerator UpdateExerciseRecord (int eid, double roundedDistance)
	{
		Debug.Log ("In UpdateExerciseRecord()...");

		string exerciseUrl = "http://104.131.144.86/api/exercises/" + eid + "?distance=" + roundedDistance + "&token=" + token;
		Debug.Log ("Update exercise URL: " + exerciseUrl);

		using (UnityWebRequest www = UnityWebRequest.Put (exerciseUrl, "Hello")) {
			yield return www.Send ();

			if (www.isError) {
				Debug.Log ("Put error: " + www.error);
			} else {
				Debug.Log ("Update OK.");

			}
		}

	}






}
