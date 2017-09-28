using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Locks screen in portrait orientation.

public class ScreenCtrl : MonoBehaviour {

	void Start () {
		Screen.orientation = ScreenOrientation.Portrait;
		
	}
	

}
