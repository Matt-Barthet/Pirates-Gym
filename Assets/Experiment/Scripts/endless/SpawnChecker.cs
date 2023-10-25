using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnChecker : MonoBehaviour
{
    private GameObject[] spawns;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate() {
        spawns = GameObject.FindGameObjectsWithTag("Enemy");
        if (spawns.Length > 1) {
            if ((Mathf.RoundToInt(spawns[spawns.Length - 1].transform.position.x) == Mathf.RoundToInt(spawns[spawns.Length - 2].transform.position.x)) ||
                (Mathf.RoundToInt(spawns[spawns.Length - 1].transform.position.x + 0.5f) == Mathf.RoundToInt(spawns[spawns.Length - 2].transform.position.x)) ||
                (Mathf.RoundToInt(spawns[spawns.Length - 1].transform.position.x - 0.5f) == Mathf.RoundToInt(spawns[spawns.Length - 2].transform.position.x))) {
                spawns[spawns.Length - 1].transform.position = new Vector2(spawns[spawns.Length - 1].transform.position.x + 20, spawns[spawns.Length - 1].transform.position.y);
            }
        }
    }
}
