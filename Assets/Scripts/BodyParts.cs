using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyParts : MonoBehaviour {

	private Rigidbody2D body;

	// Use this for initialization
	void Start () {
		body = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
		ParticleSystem ps = GetComponent<ParticleSystem>();
		if (ps != null) {
			if(ps.isStopped) {
				Destroy(this.gameObject);
			}
			return;
		}

		if (body == null) {
			Destroy(this);
		}
		
		if (!body.IsSleeping()) {
			return;
		}

		Destroy(body);
		Destroy(GetComponent<BoxCollider2D>());
		Destroy(this);
	}
}
