using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectBlob : MonoBehaviour {
	public Texture selectTexture;
	public Button button;
	private float xCoord;
	private float yCoord;
	private float width;
	private float height;
	public bool blueGuiEnable = false;
	public bool orangeGuiEnable = false;
	public bool pinkGuiEnable = false;
	public bool greenGuiEnable = false;

	// Use this for initialization
	void Start () {
//		button = GetComponent<Button>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void setBlueTrue () {
		blueGuiEnable = true;
		orangeGuiEnable = false;
	 	pinkGuiEnable = false;
		greenGuiEnable = false;
		Debug.Log ("BLUE CLICKED");
		Debug.Log (blueGuiEnable);
	}

	public void setOrangeTrue () {
		orangeGuiEnable = true;
		blueGuiEnable = false;
		pinkGuiEnable = false;
		greenGuiEnable = false;
		Debug.Log ("ORANGE CLICKED");
		Debug.Log (orangeGuiEnable);
	}

	public void setPinkTrue () {
		pinkGuiEnable = true;
		blueGuiEnable = false;
		orangeGuiEnable = false;
		greenGuiEnable = false;
		Debug.Log ("PINK CLICKED");
		Debug.Log (pinkGuiEnable);
	}

	public void setGreenTrue () {
		greenGuiEnable = true;
		blueGuiEnable = false;
		orangeGuiEnable = false;
		pinkGuiEnable = false;
		Debug.Log ("GREEN CLICKED");
		Debug.Log (greenGuiEnable);
	}

	void OnGUI() {
		if (!selectTexture) {
			Debug.LogError("Assign a Texture in the inspector.");
			return;
		}
		//		xCoord = button.GetComponent<RectTransform>().position.x;
		if (GameObject.Find("BlueBlobSelectButton") && blueGuiEnable == true) {
//			xCoord = 60;
//			yCoord = 180;
//			GUI.DrawTexture (new Rect (xCoord, yCoord, 200, 170), selectTexture, ScaleMode.StretchToFill, true, 10.0F);
		} else if (GameObject.Find("OrangeBlobSelectButton") && orangeGuiEnable == true) {
//			xCoord = 270;
//			yCoord = 180;
//			GUI.DrawTexture (new Rect (xCoord, yCoord, 200, 170), selectTexture, ScaleMode.StretchToFill, true, 10.0F);
		} else if (GameObject.Find("PinkBlobSelectButton") && pinkGuiEnable == true) {
//			xCoord = 60;
//			yCoord = 360;
//			GUI.DrawTexture (new Rect (xCoord, yCoord, 200, 170), selectTexture, ScaleMode.StretchToFill, true, 10.0F);
		} else if (GameObject.Find("GreenBlobSelectButton") && greenGuiEnable == true) {
//			xCoord = 270;
//			yCoord = 360;
//			GUI.DrawTexture (new Rect (xCoord, yCoord, 200, 170), selectTexture, ScaleMode.StretchToFill, true, 10.0F);
		}
	}

//	public void selectBlob() {
//		if (!selectTexture) {
//			Debug.LogError("Assign a Texture in the inspector.");
//			return;
//		}
////		xCoord = button.GetComponent<RectTransform>().position.x;
//		xCoord = -56.5f;
//		yCoord = 90;
//		GUI.DrawTexture(new Rect(xCoord, yCoord, 113, 79), selectTexture, ScaleMode.ScaleToFit, true, 10.0F);
//	}
}
