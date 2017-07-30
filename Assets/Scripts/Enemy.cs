using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
	private static readonly float flipChance = 1; // %

	public float shakePower;
	public float shakeDuration;

	public float explosionPower;

	public float walkSpeed;

	public float jumpForce;

	public float chargeSpeed;

	private Rigidbody2D body;

	private ParticleSystem blood;

	private GameObject sword;

	private GameObject bloodObj;

	private static Vector3 empty = new Vector3(0, 0, 1000);
	private Vector3 lastSeenPlayerPos;

	private Animator anim;

	private BoxCollider2D weaponHitBox;

	private Transform groundChecker;

	private bool initialised;
	private bool died;

	public int hp;

	private GameObject gm;

	void Awake() {
		body = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		bloodObj = this.transform.Find("ParticleSystem").gameObject;
		blood = bloodObj.GetComponent<ParticleSystem>();
		sword = this.transform.Find("EnemySword").gameObject;
		weaponHitBox = sword.GetComponent<BoxCollider2D>();
		groundChecker = this.transform.Find("GroundChecker");
		this.gm = GameObject.FindGameObjectWithTag("GameController");
	}

	void Start () {
		this.shakePower = 0.05f;
		this.shakeDuration = 0.1f;
		this.walkSpeed = 12f;
		this.lastSeenPlayerPos = empty;
		this.chargeSpeed = 5.5f;
		this.jumpForce = 8f;
		this.initialised = false;
		this.hp = 10;
		this.explosionPower = 0.5f;
		this.died = false;
	}

	void Update() {
	}

	void FixedUpdate() {
		bool grounded = Physics2D.Linecast(transform.position, groundChecker.position, 1<< LayerMask.NameToLayer("Ground"));
		if (grounded && anim.GetBool("Jumping")) {
			anim.SetBool("Jumping", false);
		}

		if (grounded && !initialised) {
			initialised = true;
		}

		if (!initialised) {
			return;
		}

		if (died) {
			return;
		}

		if (lastSeenPlayerPos != empty) {
			float dist = Vector2.Distance(this.transform.position, lastSeenPlayerPos);
			
			Vector2 diff = this.transform.position - lastSeenPlayerPos;

			// face the correct direction;
			Flip(diff.x);
			if (dist >= 1f) {
				body.AddForce(new Vector2(Random.Range(Mathf.Max(dist, 2) * (chargeSpeed - 2), dist * chargeSpeed) * -Mathf.Sign(diff.x), 0), ForceMode2D.Force);

				if (grounded && diff.y < -2f && dist < 3f) {
					body.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
				}
			} else {
				lastSeenPlayerPos = empty;
			}			
		} else {
			body.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * Random.Range(walkSpeed-2, walkSpeed), 0), ForceMode2D.Force);
			if (Random.Range(0, 1000) <= flipChance) {
				Flip();
			}
		}
	}

	void Flip(float dir) {

		dir = Mathf.Sign(dir);

		// Flip the character
		Vector2 newScale = body.transform.localScale;
		newScale.x = Mathf.Abs(newScale.x) * dir;
		body.transform.localScale = newScale;
	}

	void Flip() {
		Vector2 newScale = body.transform.localScale;
		newScale.x *= -1;
		body.transform.localScale = newScale;
	}

	void Damage(float dirDmg) {
		float dir = Mathf.FloorToInt(Mathf.Sign(dirDmg));
		float dmg = Mathf.RoundToInt(Mathf.Abs(dirDmg));

		blood.emission.SetBursts(new UnityEngine.ParticleSystem.Burst[] {new UnityEngine.ParticleSystem.Burst(0,(short)dmg)});
		if (blood.isPlaying && !blood.isEmitting) blood.Stop();
		if (blood.isStopped) blood.Play();

		Vector3 newScale = blood.transform.localScale;
		newScale.z = dir * Mathf.Abs(newScale.z);
		blood.transform.localScale = newScale;


		int numLosePower = gm.GetComponent<GameManager>().GetNumLosePower();
		float shakePowerDegradsion = ((shakePower-0.01f) * Mathf.Min(numLosePower/6f, 1));
		float shakeDurationDegradsion = ((shakeDuration-0.01f) * Mathf.Min(numLosePower/6f, 1));

		this.hp -= Mathf.RoundToInt(dmg);
		if (this.hp <= 0) {
			died = true;
			SelfDestruct(numLosePower);
			Camera.main.GetComponent<CameraController>().ShakeCamera(shakePower + (0.1f * Mathf.Min(numLosePower / 6, 1)) - shakePowerDegradsion, shakeDuration - shakeDurationDegradsion, 1, 1);
		} else {
			Camera.main.GetComponent<CameraController>().ShakeCamera(shakePower - shakePowerDegradsion, shakeDuration - shakeDurationDegradsion, 1, 1);
		}
	}

	void SelfDestruct(int numLosePower) {
		gm.SendMessage("enemyKilled");
		GameObject[] bodyParts = new GameObject[] {
			sword,
			this.transform.Find("EnemyArm").gameObject,
			this.transform.Find("Enemy").gameObject
		};
		
		foreach(var part in bodyParts) {
			// disable scripts
			part.layer = LayerMask.NameToLayer("BodyParts");
			MonoBehaviour[] scripts = part.GetComponents<MonoBehaviour>();
			foreach(var s in scripts) {
				Destroy(s);
			}

			// remove audio if there is
			AudioSource[] acs = part.GetComponents<AudioSource>();
			foreach(var ac in acs) {
				Destroy(ac);
			}

			// remove audio if there is
			BoxCollider2D[] cs = part.GetComponents<BoxCollider2D>();
			foreach(var c in cs) {
				c.enabled = true;
				c.isTrigger = false;
			}

			SpriteRenderer sr = part.GetComponent<SpriteRenderer>();
			sr.sortingLayerName = "Default";
			sr.sortingOrder = 0;

			part.AddComponent<BodyParts>();

			part.transform.SetParent(GameObject.FindGameObjectWithTag("BodyPartContainer").transform);
			Rigidbody2D partBody = part.AddComponent<Rigidbody2D>();
			partBody.mass = 0.5f;
			float diff = ((explosionPower-0.01f) * Mathf.Min(numLosePower/6f, 1));
			partBody.AddForceAtPosition(new Vector2(Random.Range(-explosionPower + diff, explosionPower - diff), explosionPower - diff), Random.insideUnitCircle, ForceMode2D.Impulse);
		}

		if (bloodObj != null) {
			bloodObj.transform.SetParent(GameObject.FindGameObjectWithTag("BodyPartContainer").transform.transform);
			bloodObj.gameObject.AddComponent<BodyParts>();
		}

		Destroy(this.gameObject);	
	}

	void StartAttack() {
		if (!initialised) {
			return;
		}
		anim.SetTrigger("Attack");
	}

	void DetectPlayer(Vector3 playerPos) {
		this.lastSeenPlayerPos = playerPos;
	}

	void LostPlayer() {
		this.lastSeenPlayerPos = empty;
	}

	void CloseToWall() {
		if (lastSeenPlayerPos == empty) {
			Flip();
		}
	}

	void EnableDamage() {
		weaponHitBox.enabled = true;
	}

	void DisableDamage() {
		weaponHitBox.enabled = false;
	}
}
