using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float walkVelocity;
	public float jumpForce;

	private Transform groundChecker;

	private Rigidbody2D body;
	private Animator anim;
	private BoxCollider2D weaponHitBox;
	private bool attacking;

	private ParticleSystem blood;

	private GameObject gm;

	private bool dead;

	private bool levelCleared;

	private int playerMaxHp;
	private int playerHp;

	void Awake() {
		body = this.GetComponent<Rigidbody2D>();
		anim = this.GetComponent<Animator>();
		weaponHitBox = this.transform.Find("Spear").GetComponent<BoxCollider2D>();
		groundChecker = this.transform.Find("GroundChecker");
		blood = this.transform.Find("ParticleSystem").gameObject.GetComponent<ParticleSystem>();
		gm = GameObject.FindGameObjectWithTag("GameController");
	}
	
	// Use this for initialization
	void Start () {
		walkVelocity = 10f;
		jumpForce = 50f;
		weaponHitBox.enabled = false;
		dead = false;
		levelCleared = false;
		this.playerHp = this.playerMaxHp = 100;
	}
	
	// Update is called once per frame
	void Update () {
		if (!attacking && Input.GetKey(KeyCode.Space)) {
			attacking = true;
			walkVelocity /= 2;
			this.anim.SetTrigger("AttackHorizontal");
		}
	}

	void AttackAnimationFinish() {
		attacking = false;
		walkVelocity *= 2;
	}

	void EnableDamage() {
		weaponHitBox.enabled = true;
	}

	void DisableDamage() {
		weaponHitBox.enabled = false;
	}

	void FixedUpdate() {
		bool grounded = Physics2D.Linecast(transform.position, groundChecker.position, 1<< LayerMask.NameToLayer("Ground"));

		if (dead) {
			return;
		}

		if (levelCleared) {
			return;
		}


		float xInput = Input.GetAxisRaw("Horizontal");
		
		body.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * walkVelocity, body.velocity.y);
		
		if (Mathf.Abs(xInput) > float.Epsilon) {
			float xInputDir = Mathf.Sign(xInput);
		
			// Flip the character
			Vector2 newScale = body.transform.localScale;
			newScale.x = Mathf.Abs(newScale.x) * xInputDir;
			body.transform.localScale = newScale;
		}
		
		if (grounded && anim.GetBool("Jumping")) {
			anim.SetBool("Jumping", false);
		}
		if (Input.GetKey(KeyCode.UpArrow) && grounded) {
			body.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
			anim.SetBool("Jumping", true);
		}
	}

	void Damage(int dirDmg) {
		int dir = Mathf.FloorToInt(Mathf.Sign(dirDmg));
		int dmg = Mathf.RoundToInt(Mathf.Abs(dirDmg));

		blood.emission.SetBursts(new UnityEngine.ParticleSystem.Burst[] {new UnityEngine.ParticleSystem.Burst(0,(short)500)});
		if (blood.isPlaying && !blood.isEmitting) blood.Stop();
		if (blood.isStopped) blood.Play();

		Vector3 newScale = blood.transform.localScale;
		newScale.z = dir * Mathf.Abs(newScale.z);
		blood.transform.localScale = newScale;

		playerHp -= dmg;
		gm.SendMessage("UpdatePlayerHealth",  Mathf.Max(playerHp / (float)playerMaxHp, 0));
		
		if (playerHp <= 0) {
			dead = true;
			this.gameObject.layer = LayerMask.NameToLayer("BodyParts");
			anim.SetTrigger("Dead");
		}
	}

	void FinishedDealthAnimation() {
		gm.SendMessage("FinishedDeathAnimation");
	}

	void LevelClear() {
		levelCleared = true;
		anim.SetTrigger("LevelClear");
	}

	void LevelClearScreenShake() {
		int numTimesLosePower = gm.GetComponent<GameManager>().GetNumLosePower();
		Camera.main.GetComponent<CameraController>().ShakeCamera(0.5f - (0.49f * Mathf.Min(numTimesLosePower/6f, 1)), 0.2f - (0.19f * Mathf.Min(numTimesLosePower/6f, 1)), 1, 1);
	}

	void LevelClearAnimationEnd() {
		levelCleared = false;
		gm.SendMessage("FinishedLevelAnimation");
	}

	public bool LoseMaxHealth() {
		this.playerMaxHp = Mathf.RoundToInt(this.playerMaxHp * 0.5f);
		if (this.playerHp > this.playerMaxHp) {
			this.playerHp = this.playerMaxHp;
		}
		gm.SendMessage("UpdatePlayerMaxHealth", this.playerMaxHp);
		gm.SendMessage("UpdatePlayerHealth", Mathf.Max(playerHp / (float)playerMaxHp, 0));

		if (this.playerMaxHp <= 13) {
			return false;
		} else {
			return true;
		}
	}

	public bool LoseAttack() {
		DamageTrigger dt = GetComponentInChildren<DamageTrigger>();
		dt.damage = Mathf.RoundToInt(dt.damage * 0.75f);
		if (dt.damage <= 12) {
			return false;
		} else {
			return true;
		}
	}

	public bool LoseSpeed() {
		if (walkVelocity > 2.6) {
			walkVelocity = walkVelocity * 0.5f;
			Debug.Log("Speed is " + walkVelocity);
		}
		if (jumpForce > 20) {
			jumpForce = jumpForce * 0.5f;
			Debug.Log("jump is " + jumpForce);
		}

		if (walkVelocity <= 2.6 && jumpForce <= 20) {
			return false;
		} else {
			return true;
		}
	}

	public void RestoreHealth() {
		this.playerHp = this.playerMaxHp;
		gm.SendMessage("UpdatePlayerHealth", Mathf.Max(playerHp / (float)playerMaxHp, 0));
	}
}
