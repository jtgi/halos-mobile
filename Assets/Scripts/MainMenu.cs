using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;

public class MainMenu : MonoBehaviour {

	GameObject cam;
	GameObject highScoresMenu;
	GameObject titleMenu;
	HighScores highScoresScript;

	void Start() {
		cam = GameObject.FindWithTag("MainCamera");
		highScoresMenu = GameObject.Find ("HighScores");
		highScoresScript = highScoresMenu.GetComponent<HighScores>();
		titleMenu = GameObject.Find ("MainMenu");

	}

	public void NavigateToTitleMenu() {
		cam.transform.LookAt(titleMenu.transform);
	}

	public void NavigateToHighScores() {
		cam.transform.LookAt(highScoresMenu.transform);
		highScoresScript.Init();
	}

	public void PlayOnClick() {
		Debug.Log ("Clicked");
		Application.LoadLevel("Freefall");
	}                                                                                        
	
}
