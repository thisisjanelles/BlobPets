using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManageBlobCtrl : MonoBehaviour {
	
	public PlayerPreferences playerPreferences;

	public int numBlobs;
	public int MAX_BLOBS = 4;

	public Button addBtn;
	public Button breedBtn;
	public GameObject addGO;
	public GameObject breedGO;


	// Use this for initialization
	void Start () {
		numBlobs = playerPreferences.GetNumBlobs ();

		Debug.Log ("numBlobs: " + numBlobs);

		addBtn = addGO.GetComponent<Button> ();
		breedBtn = breedGO.GetComponent<Button> ();

		ManageButtons ();
	}

	/*
	 * Enable or disable buttons based on how many blobs the user owns.
	 * 
	 * If user owns less than MAX_BLOBS, then enable "Add New Blob" button.
	 * If user owns less than MAX_BLOBS and more than 2, then enable "Breed New Blob" button.
	 */

	public void ManageButtons()
	{
		if (numBlobs < MAX_BLOBS) {
			//enable add new blob button
			Debug.Log("Add button enabled...");
			addBtn.enabled = true;
		} else {
			// disable add new blob button
			Debug.Log("Add button disabled...");
			addBtn.enabled = false;
		}

		if (numBlobs < MAX_BLOBS && numBlobs >= 2) {
			// enable breed button
			Debug.Log("Breed button enabled...");
			breedBtn.enabled = true;
		} else {
			// disable breed button
			Debug.Log("Breed button disabled...");
			breedBtn.enabled = false;
		}

	}

}
