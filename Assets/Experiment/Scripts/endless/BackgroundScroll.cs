using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    private LevelManager levelManager;
    private Renderer rendererComponent;
    Vector2 offset = new Vector2(0, 0);
    private float uVcount = 7; //BG repeats 5 times
    private float rendererLength = 200; //BG width
    float floorSpeedUV;

    private void Awake() {
        levelManager = LevelManager.Instance;
        rendererComponent = GetComponent<Renderer>();
        
    }

    void Update() {
        if (levelManager._isPlaying) {
            floorSpeedUV = levelManager.scrollSpeed * uVcount / rendererLength;
            rendererComponent.material.mainTextureOffset += new Vector2(floorSpeedUV * Time.fixedDeltaTime, 0.0f);

        }
    }
}
