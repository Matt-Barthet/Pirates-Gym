using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scroll : MonoBehaviour {
    private LevelManager levelManager;
    public float speed;
    private float uVcount = 7; //BG repeats 5 times
    private float rendererLength = 200; //BG width
    float floorSpeedUV;

    private void Awake() {
        levelManager = LevelManager.Instance;
        
    }

    void Update() {
        if (levelManager._isPlaying) {
            floorSpeedUV = levelManager.scrollSpeed * uVcount / rendererLength;
            transform.Translate(-Vector2.right * levelManager.scrollSpeed * Time.fixedDeltaTime);
        }

        if (transform.position.x <= -77) {
            Destroy(gameObject);
        }
    }
}
