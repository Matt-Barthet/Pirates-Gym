using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddPoints : MonoBehaviour
{
    private LevelManager levelManager;
    private float nextActionTime = 0.0f;
    public float period = 1f;

    private void Awake() {
        levelManager = LevelManager.Instance;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update() {
        float realPeriod = period - levelManager.scrollSpeed/10;
        if (Time.time > nextActionTime) {
            nextActionTime += period;
            if (levelManager._isPlaying) {
                levelManager.Score++;
            }
        }
    }
}
