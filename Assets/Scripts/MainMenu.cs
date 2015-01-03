using UnityEngine;
using Holoville.HOTween;
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
		HOTween.To(cam.transform, 1, new TweenParms()
		           .Prop("rotation", new Vector3(337.8755f, 295.8947f, 1.907367f), false)
		           .Ease(EaseType.EaseInOutCubic)
		           );
	}

	public void NavigateToHighScores() {
		//cam.transform.LookAt(highScoresMenu.transform);

		highScoresScript.Init();
		HOTween.To(cam.transform, 1, new TweenParms()
		           .Prop("rotation", new Vector3(2.870561f, 67.17326f, 358.1583f), false)
		           .Ease(EaseType.EaseInOutCubic)
		           );

	}

	public void PlayOnClick() {
		Debug.Log ("Clicked");
		Application.LoadLevel("Freefall");
	}                                                                                        
	
}
