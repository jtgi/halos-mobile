using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

	private DonutGen donutGen;
	private PlayerMovement playerController;
	private int lives;
	private float gravitySetting;
	private HighScores leaderboard;


	void Start () {
		points = 0;
		donutsSurvived = 0;

		playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
		respawnPosition = GameObject.Find("Respawn");
		donutGen = GameObject.FindGameObjectWithTag("MainScript").GetComponent<DonutGen>();

		initGame();
	}

	void Update() {

	}

	void initGame() {
		lives = defaultLives;
		points = 0;

		lifeDisplay.text = lifeDisplayPrefixText + " "  + lives;
		pointDisplay.text = pointDisplayPrefixText + " " + points;

		gameOverGui = GameObject.Find ("GameOverGUI");
		inGameGui = GameObject.Find ("InGameGUI");

		gameOverGui.SetActive(false);
		leaderboard = gameOverGui.GetComponent<HighScores>();

		inGameGui.SetActive(true);

		gameOver = false;

		playerController.SetHaltUpdateMovement(false);
		donutGen.initDonuts();
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

		if(FB.IsLoggedIn) {
			leaderboard.FetchScores();
		}
	}
	
	public void Respawn() {
		GameObject[] rings = GameObject.FindGameObjectsWithTag("RingWrap");
		foreach(GameObject ring in rings) {
			Destroy(ring);
		}

		playerController.transform.position = respawnPosition.transform.position;
		initGame();
	}

	public void NavigateToHighScores() {
		Debug.Log ("High Scores selected");
		Application.LoadLevel("MainMenu");
	}
}
