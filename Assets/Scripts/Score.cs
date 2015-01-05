using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Holoville.HOTween;

public class Score : MonoBehaviour {
	
	public Text lifeDisplay;
	public Text pointDisplay;
	public Text statsDisplay;
	public Text gameOverDisplay;
	public Button playAgain;

	public int defaultLives = 5;

	private int donutsSurvived;
	private float points;
	private bool gameOver = false;

	private string newHighScoreText = "HIGH SCORE!";
	private string gameOverTextDefault = "GAME OVER";
	private string gameOverText = "GAME OVER";

	private GameObject gameOverGui;
	private GameObject inGameGui;
	private GameObject respawnPosition;
	private GameObject startUI;
	private GameObject takeOffHook;
	private GameObject playerCamera;
	private GameObject missDetector;

	private DonutGen donutGen;
	private PlayerMovement playerController;
	private int lives;
	private float gravitySetting;
	private HighScores leaderboard;
	private CanvasGroup fader;
	
	void Start () {
		points = 0;
		donutsSurvived = 0;

		playerCamera = GameObject.Find ("Camera");
		playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
		respawnPosition = GameObject.Find("Respawn");
		donutGen = GameObject.FindGameObjectWithTag("MainScript").GetComponent<DonutGen>();
		startUI = GameObject.Find("StartUI");
		takeOffHook =  GameObject.Find ("TakeOffHook");
		gameOverGui = GameObject.Find ("GameOverGUI");
		inGameGui = GameObject.Find ("InGameGUI");
		missDetector = GameObject.Find ("MissDetector");
		leaderboard = gameOverGui.GetComponent<HighScores>();
		fader = GameObject.Find ("Fader").GetComponent<CanvasGroup> ();

		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		initGame();
		FadeInAndStart ();
	}
	
	void initGame() {
		lives = defaultLives;
		points = 0;

		lifeDisplay.text = lives.ToString();
		pointDisplay.text = points.ToString();

		gameOverGui.SetActive(false);
		inGameGui.SetActive(false);
		startUI.SetActive(false);
		missDetector.collider.enabled = true;
		playerController.SetHaltUpdateMovement(true);

		gameOver = false;
		donutGen.initDonuts();
	}

	public void StartGame() {

		inGameGui.SetActive(true);
		startUI.SetActive(false);

		AnimatePlayerJump ();
	}

	private void FadeInAndStart() {
		HOTween.To (fader, 1, new TweenParms ().Prop ("alpha", 0f)
		            .Delay (1f)
		            .Ease (EaseType.EaseOutQuad)
		            .OnComplete(() => {
			if(FirstTimeUse() == false) {
				StartGame();
			} else {
				startUI.SetActive(true);
			}
		}
		));	
	}
	
	private void AnimatePlayerJump() {
		HOTween.To(playerController.transform, 1.5f, new TweenParms()
		           .Prop("position", takeOffHook.transform.position, false)
		           .Ease(EaseType.EaseInQuad)
		           );
		
		HOTween.To(playerCamera.transform, 2, new TweenParms()
		           .Prop("rotation", new Vector3(90, 0, 0), true)
		           .Ease(EaseType.EaseInOutQuad)
		           .Delay(0.5f)
		           .OnComplete(() => playerController.SetHaltUpdateMovement(false))
		           );
	}

	private bool FirstTimeUse() {
		if(PlayerPrefs.HasKey("firstTimeUse")) {
			return false;
		} else {
			PlayerPrefs.SetInt ("firstTimeUse", 1);
			return true;
		}
	}

	public void IncreasePoints(float p) {
		if(gameOver) return;

		donutsSurvived++;

		points += p;
		pointDisplay.text = points.ToString();
	}

	public void decreaseLife() {
		if(gameOver) return;

		lives--;

		lifeDisplay.text = lives.ToString();

		if(lives <= 0) {
			GameOver();
		}
	}

	public void GameOver() {
	
		float highScore = PlayerPrefs.GetFloat("highScore");
	
		bool newHighScore = highScore < points;

		if(newHighScore) {
			PlayerPrefs.SetFloat("highScore", points);
			PlayerPrefs.Save();
			gameOverText = newHighScoreText;


		} else {
			gameOverText = gameOverTextDefault;
		}

		if(FB.IsLoggedIn) {
			leaderboard.SyncScore();
		}


		statsDisplay.text = string.Format ("SCORE: {0}   HIGH SCORE: {1}", points, PlayerPrefs.GetFloat("highScore"));
		gameOverDisplay.text = gameOverText;

		missDetector.collider.enabled = false;;
		playerController.SetHaltUpdateMovement(true);
		inGameGui.SetActive(false);
		gameOverGui.SetActive(true);
		gameOver = true;
	}
	
	public void Respawn() {
		GameObject[] rings = GameObject.FindGameObjectsWithTag("RingWrap");
		foreach(GameObject ring in rings) {
			Destroy(ring);
		}

		playerController.transform.position = respawnPosition.transform.position;
		playerController.transform.rotation = respawnPosition.transform.rotation;
		playerCamera.transform.localEulerAngles = new Vector3(0, 0, 0);
		initGame();
		StartGame();
	}

	public void NavigateToHighScores() {
		Application.LoadLevel("MainMenu");
	}
}
