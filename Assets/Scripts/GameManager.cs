using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	public GameObject optionButtonPrefab;

	public GameObject powerDownPanel;

	private GameObject player;

	private GameObject bodyPartsContainer;

	private AudioSource audio;

	public Text killCountText;
	private Animator killCountAnim;
	public Image healthbar;
	public GameObject maxHealthBar;

	// player things
	private int killCount;

	private int levelKillCount;

	public void UpdatePlayerHealth(float percentage) {
		healthbar.fillAmount = percentage;
	}
	
	public void UpdatePlayerMaxHealth(int maxHealth) {
		RectTransform rect = maxHealthBar.GetComponent<RectTransform>();
		rect.sizeDelta = new Vector2(maxHealth/100f * 500, rect.rect.height);
	}

	public void FinishedDeathAnimation() {
		GameOver();
		anim.SetTrigger("Dialog");
	}

	public void enemyKilled() {
		killCount++;
		levelKillCount++;
		killCountText.text = killCount + "";
		killCountAnim.SetTrigger("Increase");
	}

	public GameObject leftBound;
	public GameObject rightBound;

	private float leftBoundX;
	private float rightBoundX;

	private enum GameState {
		Intro,
		Tutorial,
		Level1,
		EndlessMode,
		GameOver,
		OptionChosen,
		LevelClear,
	}

	private GameState currentGameState;

	public Text dialogueText;

	private Animator anim;

	private int dialogueIndex = 0;
	private string[] dialogLines;

	void Awake() {
		this.anim = GetComponent<Animator>();
		this.killCountAnim = killCountText.gameObject.GetComponent<Animator>();
		this.player = GameObject.FindGameObjectWithTag("Player").gameObject;
		this.bodyPartsContainer = GameObject.FindGameObjectWithTag("BodyPartContainer").gameObject;
		this.audio = GetComponent<AudioSource>();
	}

	void Start() {
		this.leftBoundX = leftBound.transform.position.x + 1;
		this.rightBoundX = rightBound.transform.position.x - 1;
		this.killCount = 0;
		this.maxLevel = 1;
		this.currentLevel = 0;
		this.levelKillCount = 0;
		this.availablePowerDowns.Add(PowerDown.Attack);
		this.availablePowerDowns.Add(PowerDown.Health);
		this.availablePowerDowns.Add(PowerDown.Agility);
		
		if (RestartManager.Instance().IsRestarting()) {
			StartLevel1();
		} else {
			StartIntro();
		}
	}

	private static readonly string[] introLines = new string [] {
		"Old Man: I was the best gladiator.",
		"Old Man: ... but I've lost my power.",
		"Child: How?",
		"Old Man: Simply by getting old, Lad."
	};

	private static readonly string[] tutorialLines = new string [] {
		"(Use Arrow Keys to move, Space key to attack)"
	};

	private static readonly string[] level1Lines = new string[] {
		"I used to be able to kill 10 men without a blink."
	};

	void DialogueFinish() {
		if (currentGameState == GameState.Intro) {
			if (dialogueIndex >= dialogLines.Length) {
				StartTutorial();
				anim.SetTrigger("StartGame");
				return;
			}
		} else if (currentGameState == GameState.Tutorial) {
			if (dialogueIndex >= dialogLines.Length) {
				StartLevel1();
				anim.SetTrigger("Dialog");
				return;
			}
		} else if (currentGameState == GameState.GameOver) {
			if (dialogueIndex >= dialogLines.Length) {
				RestartManager.Instance().SetRestart();
				SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
				return;
			}
		} else if (currentGameState == GameState.OptionChosen) {
			if (dialogueIndex >= dialogLines.Length) {
				
				if (currentLevel < maxLevel) {

				} else {
					// endless mode
					StartEndlessMode();
					anim.SetTrigger("Dialog");
				}
				return;
			}
		}

		if (dialogueIndex >= dialogLines.Length) {
			return;
		} else {
			dialogueText.text = dialogLines[dialogueIndex];
			dialogueIndex++;
			anim.SetTrigger("Dialog");
		}
	}

	private enum PowerDown {
		Health,
		Attack,
		Agility
	}

	HashSet<PowerDown> availablePowerDowns = new HashSet<PowerDown>();

	void FinishedLevelAnimation() {
		// Pop power down menu
		GameObject optionPanel = powerDownPanel.transform.Find("Panel").gameObject;

		// Clear out old options
		foreach(Transform child in optionPanel.transform) {
			Destroy(child.gameObject);
		}

		// Create options
		if (availablePowerDowns.Count > 0) {
			foreach (PowerDown p in availablePowerDowns) {
				GameObject optionButton = Instantiate(optionButtonPrefab);
				optionButton.transform.SetParent(optionPanel.transform);
				optionButton.transform.localScale = Vector3.one;
				Text buttonText = optionButton.GetComponentInChildren<Text>();
				
				switch(p) {
					case PowerDown.Health:
						buttonText.text = "Lose 50% health";
						optionButton.GetComponent<Button>().onClick.AddListener(() => {
							bool success = player.GetComponent<Player>().LoseMaxHealth();
							powerDownPanel.SetActive(false);
							OptionChosen(new string[] { "I've lost some of my health over the years." });
							anim.SetTrigger("Dialog");
							if (!success) {
								availablePowerDowns.Remove(PowerDown.Health);
							}
						});
						break;
					case PowerDown.Attack:
						buttonText.text = "Lose 25% attack";
						optionButton.GetComponent<Button>().onClick.AddListener(() => {
							bool success = player.GetComponent<Player>().LoseAttack();
							powerDownPanel.SetActive(false);
							OptionChosen(new string[] { "I've lost some of my strength over the years." });
							anim.SetTrigger("Dialog");
							if (!success) {
								availablePowerDowns.Remove(PowerDown.Attack);
							}
						});
						break;
					case PowerDown.Agility:
						buttonText.text = "Lose 50% movement";
						optionButton.GetComponent<Button>().onClick.AddListener(() => {
							bool success = player.GetComponent<Player>().LoseSpeed();
							powerDownPanel.SetActive(false);
							OptionChosen(new string[] { "I've lost some of agility over the years." });
							anim.SetTrigger("Dialog");
							if (!success) {
								availablePowerDowns.Remove(PowerDown.Agility);
							}
						});
						break;
				}
			}
			numLosePower++;

			powerDownPanel.SetActive(true);
		} else {
			OptionChosen(new string[] { "I've lost all my powers." });
			anim.SetTrigger("Dialog");
		}
	}

	void OptionChosen (string[] lines) {
		dialogueIndex = 0;
		dialogLines = lines;
		dialogueText.text = dialogLines[dialogueIndex];
		dialogueIndex++;
		this.currentGameState = GameState.OptionChosen;
	}

	private int numLosePower;

	public int GetNumLosePower() {
		return numLosePower;
	}

	void GameOver() {
		dialogueIndex = 0;
		dialogLines = new string[] { "", 
			"I've killed a total of " + killCount + " men who challenged me.", 
			"but ... it is all in the past now.",
			"You are the only person that listen to my story for so long.",
			"Want to hear some more?"
		};
		dialogueText.text = dialogLines[dialogueIndex];
		dialogueIndex++;
		this.currentGameState = GameState.GameOver;
	}

	void StartIntro() {
		dialogueIndex = 0;
		dialogLines = introLines;
		dialogueText.text = dialogLines[dialogueIndex];
		dialogueIndex++;
		this.currentGameState = GameState.Intro;
	}

	void StartTutorial() {
		dialogueIndex = 0;
		dialogLines = tutorialLines;
		dialogueText.text = dialogLines[dialogueIndex];
		dialogueIndex++;
		this.currentGameState = GameState.Tutorial;
	}

	// Level1

	public GameObject enemyPrefab;

	public int currentLevel;
	public int maxLevel;

	void StartLevel1() {
		dialogueIndex = 0;
		dialogLines = level1Lines;
		dialogueText.text = dialogLines[dialogueIndex];
		dialogueIndex++;
		this.currentGameState = GameState.Level1;
		this.currentLevel = 1;
		this.levelKillCount = 0;
		maxHealthBar.SetActive(true);
		killCountText.gameObject.SetActive(true);
		// Spawn enemies;

		numSpawned = 0;
		numToSpawn = 10;

		SpawnEnemy();
		numSpawned++;
	}

	private void SpawnEnemy() {
		Instantiate(enemyPrefab, new Vector3(Random.Range(leftBoundX, rightBoundX), 6, 0), Quaternion.identity);
	}

	private float spawnTimer;
	private int numSpawned;
	private int numToSpawn;

	private float garbageCollectionTimer;

	void Update() {
		if (this.currentGameState == GameState.Level1) {
			if (levelKillCount == 0) {
				// dont spawn until player killed something
			} else if (numSpawned < numToSpawn) {
				spawnTimer -= Time.deltaTime;
				if (numSpawned < numToSpawn && spawnTimer <= 0) {
					SpawnEnemy();
					numSpawned++;
					spawnTimer = 0.5f + (Random.Range(0, 10 - levelKillCount) / 10f); 
				}
			} else if (levelKillCount >= numSpawned) {
				this.currentGameState = GameState.LevelClear;
				player.SendMessage("LevelClear");
			}
		} else if (this.currentGameState == GameState.EndlessMode) {
			if (numSpawned < numToSpawn) {
				spawnTimer -= Time.deltaTime;
				if (numSpawned < numToSpawn && spawnTimer <= 0) {
					SpawnEnemy();
					numSpawned++;
					spawnTimer = Mathf.Max(0.5f, 3 - killCount / 5f) + (Random.Range(0, 3) / 10f); 
				}
			} else if (levelKillCount >= numSpawned) {
				this.currentGameState = GameState.LevelClear;
				player.SendMessage("LevelClear");
			}
		}

		if (bodyPartsContainer.transform.childCount > 100) {
			if (garbageCollectionTimer <= 0) {
				for(int i = 0; i < 20; i++) {
					Destroy(bodyPartsContainer.transform.GetChild(i).gameObject);
				}
				garbageCollectionTimer = 5f;
			} else {
				garbageCollectionTimer -= Time.deltaTime;
			}
		}

		if (RestartManager.Instance().isMusic()) {
			audio.volume = 1f;
		} else {
			audio.volume = 0f;
		}
	}

	private static string[] endlessModeLines = new string[] { 
		"It gets harder and harder to kill the same 10 men.", 
		"Yet, I still kept fighting and fighting.",  
		"That's the only thing I know how to do."};

	void StartEndlessMode() {
		dialogueIndex = 0;
		dialogLines = endlessModeLines;
		dialogueText.text = dialogLines[dialogueIndex];
		dialogueIndex++;
		this.currentGameState = GameState.EndlessMode;
		this.levelKillCount = 0;
		player.SendMessage("RestoreHealth");

		numSpawned = 0;
		numToSpawn = 10;
	}

	public Sprite musicOn;
	public Sprite musicOff;

	public Image musicButtonImage;

	public void ToggleMusic() {
		RestartManager.Instance().ToggleMusic();
		if (RestartManager.Instance().isMusic()) {
			musicButtonImage.sprite = musicOn;
		} else {
			musicButtonImage.sprite = musicOff;
		}
	}
}
