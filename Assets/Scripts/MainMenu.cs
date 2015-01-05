using UnityEngine;
using Holoville.HOTween;
using System.Collections;
using System.Collections.Generic;
using Facebook.MiniJSON;

public class MainMenu : MonoBehaviour {

	GameObject cam;
	GameObject highScoresMenu;
	HighScores highScoresScript;
	CanvasGroup fader;

	private AsyncOperation async;

	void Start() {
		cam = GameObject.FindWithTag("MainCamera");
		highScoresMenu = GameObject.Find ("HighScores");
		highScoresScript = highScoresMenu.GetComponent<HighScores>();
		fader = GameObject.Find ("Fader").GetComponent<CanvasGroup> ();
	}

	public void NavigateToTitleMenu() {
		HOTween.To(cam.transform, 1, new TweenParms()
		           .Prop("rotation", new Vector3(337.8755f, 295.8947f, 1.907367f), false)
		           .Ease(EaseType.EaseInOutCubic)
		           );
	}

	public void NavigateToHighScores() {
		highScoresScript.Init();
		HOTween.To(cam.transform, 1, new TweenParms()
		           .Prop("rotation", new Vector3(2.870561f, 67.17326f, 358.1583f), false)
		           .Ease(EaseType.EaseInOutCubic)
		           );

	}

	IEnumerator LoadFreeFall() {
		async = Application.LoadLevelAsync("Freefall");
		async.allowSceneActivation = false;
		yield return async.isDone;
	}

	public void PlayOnClick() {
		HOTween.To (cam.transform, 2, new TweenParms()
		            .Prop("rotation", new Vector3 (283.9822f, 289.4968f, 7.331421f), false)
		            .Prop ("position", new Vector3(cam.transform.position.x, cam.transform.position.y + 10, cam.transform.position.z), false)
		            .Ease(EaseType.EaseInCubic)
		            );

		HOTween.To (fader, 1, new TweenParms ().Prop ("alpha", 1.0f)
		            .Delay(1f)
		            .OnComplete (() => async.allowSceneActivation = true));
		            ;

		StartCoroutine("LoadFreeFall");
	}                                                                                        
	
}
