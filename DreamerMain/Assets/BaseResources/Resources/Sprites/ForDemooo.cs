using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForDemooo : MonoBehaviour {

    public GameObject g;
    public float endDeadLine = -10000f;
	
	// Update is called once per frame
	void Update () {
        if (transform.position.y <= endDeadLine)
        {
            g.SetActive(true);
            Application.LoadLevel("demooooo");
        }
	}
}
