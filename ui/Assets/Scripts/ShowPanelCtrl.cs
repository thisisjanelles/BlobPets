using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using System.Text.RegularExpressions;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.Remoting;
//using UnityEditor;

public class ShowPanelCtrl : MonoBehaviour {

	public GameObject namePanel;
	public GameObject emailPanel;
	public GameObject passwordPanel;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ShowNamePanel() {
		Debug.Log ("ShowNamePanel");
		namePanel.gameObject.SetActive (true);
	}
		
	public void ShowEmailPanel() {
		Debug.Log ("ShowEmailPanel");
		emailPanel.gameObject.SetActive (true);
	}


	public void ShowPasswordPanel() {
		Debug.Log ("ShowPasswordPanel");
		passwordPanel.gameObject.SetActive (true);
	}

	public void HideNamePanel() {
		Debug.Log ("HideNamePanel");
		namePanel.gameObject.SetActive (false);
	}

	public void HideEmailPanel() {
		Debug.Log ("HideEmailPanel");
		emailPanel.gameObject.SetActive (false);
	}

	public void HidePasswordPanel() {
		Debug.Log ("HidePasswordPanel");
		passwordPanel.gameObject.SetActive (false);
	}
}
