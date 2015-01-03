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

	private string pointDisplayPrefixText = "SCORE";
	private string lifeDisplayPrefixText = "LIVES";
	private string newHighScoreText = "HIGH SCORE!";
	private string gameOverTextDefault = "GAME OVER";
	private string gameOverText = "GAME OVER";

	private GameObject gameOverGui;
	private GameObject inGameGui;
	private GameObject respawnPosition;
	private GameObject startUI;
	private GameObject takeOffHook;
	private GameObject playerCamera;

	private DonutGen donutGen;
	private PlayerMovement playerController;
	private int lives;
	private float gravitySetting;
	private HighScores leaderboard;


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
		leaderboard = gameOverGui.GetComponent<HighScores>();

		if(firstTimeUse()) {
			startUI.SetActive(true);
		}
		initGame();
	}

	void initGame() {
		lives = defaultLives;
		points = 0;

		lifeDisplay.text = lifeDisplayPrefixText + " "  + lives;
		pointDisplay.text = pointDisplayPrefixText + " " + points;

		gameOverGui.SetActive(false);
		playerController.SetHaltUpdateMovement(true);

		gameOver = false;
		donutGen.initDonuts();
	}

	public void StartGame() {

		inGameGui.SetActive(true);
		startUI.SetActive(false);

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

	private bool firstTimeUse() {
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
		pointDisplay.text = pointDisplayPrefixText + " " + points;
	}

	public void decreaseLife() {
		if(gameOver) return;

		lives--;

		lifeDisplay.text = lifeDisplayPrefixText + " " + lives;

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
		
		statsDisplay.text = string.Format ("HIGH SCORE {0}", PlayerPrefs.GetFloat("highScore"));
		gameOverDisplay.text = gameOverText;


		playerController.SetHaltUpdateMovement(true);
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
