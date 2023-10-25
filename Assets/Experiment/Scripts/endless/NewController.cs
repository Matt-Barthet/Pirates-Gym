using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewController : MonoBehaviour
{
    public float positionOne;
    public float positionTwo;
    public float speed;
    private Vector2 currentPosition;
    private Vector2 targetPosition;
    private bool moving;

    // Start is called before the first frame update
    void Start()
    {
        currentPosition = new Vector2(transform.position.x, positionOne);
        targetPosition = currentPosition;
        LevelManager.Instance.Players[0].GetComponent<Health>().OnPlayerDeath.AddListener(delegate {
            transform.position = new Vector2(transform.position.x, positionOne);
            targetPosition = currentPosition;
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (LevelManager.Instance.Players[0].GetComponent<Health>().CurrentHealth > 0 && LevelManager.Instance._isPlaying) {
            if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && !moving) {
                targetPosition = new Vector2(transform.position.x, positionTwo);
                moving = true;
            }
            if ((Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) && !moving) {
                targetPosition = new Vector2(transform.position.x, positionOne);
                moving = true;
            }
        }
        
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
        if (Vector2.Distance(transform.position, targetPosition) < 0.05) {
            transform.position = targetPosition;
            moving = false;
        }

    }
}
