using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraKostyl : MonoBehaviour {

    public Transform papa;
    public float speed;

	// Update is called once per frame
	void FixedUpdate () {
        Vector3 a = transform.position;
        transform.position = Vector3.Lerp(a, papa.position, speed);
	}
}
