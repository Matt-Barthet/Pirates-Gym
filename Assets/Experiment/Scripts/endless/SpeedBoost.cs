using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    public float boost;
    private LevelManager levelManager;
    private DataLogger logger;
    public GameObject PickEffect;

    private void Awake() {
        logger = GameObject.FindGameObjectWithTag("DataLogger").GetComponent<DataLogger>();
        levelManager = LevelManager.Instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if(collision.gameObject.layer == 9) {
            if(boost > 0) {
                logger.playerBoostPickup++;
            }
            if (boost < 0) {
                logger.playerSlowPickup++;
            }
            levelManager.scrollSpeed = levelManager.scrollSpeed + boost;
            if (PickEffect != null) {
                GameObject instantiatedEffect = (GameObject)Instantiate(PickEffect, transform.position, transform.rotation);
                instantiatedEffect.transform.localScale = transform.localScale;
            }
            Destroy(gameObject);
        }
    }
}
