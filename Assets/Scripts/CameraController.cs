using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	private Vector2 smoothVelocity;
	private float shakeTimer;
	private float shakePower;

	private int dx;

	private int dy;

	public float smoothTimeY;
    public float smoothTimeX;

	void Start() {
		smoothTimeX = 0.3f;
		smoothTimeY = 0.3f;
	}

	void Update() {
		if (shakeTimer > 0) {
			Vector2 shakePos = Random.insideUnitCircle * shakePower;
			transform.position = new Vector3(shakePos.x * dx, shakePos.y * dy, transform.position.z);
			this.shakeTimer -= Time.deltaTime;
		}

		float posX = Mathf.SmoothDamp(transform.position.x, 0, ref smoothVelocity.x, smoothTimeX);
        float posY = Mathf.SmoothDamp(transform.position.y, 0, ref smoothVelocity.y, smoothTimeY);
 
        transform.position = new Vector3(posX, posY, transform.position.z);
	}

	public void ShakeCamera(float shakePower, float shakeDuration, int dx, int dy) {
		this.shakePower = shakePower;
		this.shakeTimer = shakeDuration;
		this.dx = dx;
		this.dy = dy;
	}
}
