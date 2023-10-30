using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public int PointsToAdd = 10;

    private DataLogger logger;
    public GameObject PickEffect;

    private void Awake() {
        logger = GameObject.FindGameObjectWithTag("DataLogger").GetComponent<DataLogger>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == 9) {
            LevelManager.Instance.Score += PointsToAdd;
            logger._dataVector.PlayerPointPickup++;
            if (PickEffect != null) {
                GameObject instantiatedEffect = (GameObject)Instantiate(PickEffect, transform.position, transform.rotation);
                instantiatedEffect.transform.localScale = transform.localScale;
            }
            Destroy(gameObject);
        }
    }
}
