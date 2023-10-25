using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public List<GameObject> spawnList;
    public int possibleSpawns = 1;
    public float minInterval = 1;
    public float maxInterval = 3;
    public bool random;
    public List<float> spawnTimes;
    private Queue<float> timeQueue;
    private Queue<GameObject> spawnQueue;

    private LevelManager levelManager;

    private void Awake() {
        levelManager = LevelManager.Instance;
    }

    // Start is called before the first frame update
    void Start() {
        spawnQueue = new Queue<GameObject>(spawnList);
        timeQueue = new Queue<float>(spawnTimes);
        if (!random) {
            levelManager.OnGameStarts.AddListener(delegate {
                StartCoroutine(SpawnNext());
            });
        } else {
            levelManager.OnGameStarts.AddListener(delegate {
                StartCoroutine(SpawnNew(minInterval, maxInterval));
            });
        }
    }

    // Update is called once per frame
    void Update() {

    }

    IEnumerator SpawnNext() {
        while (spawnQueue.Count > 0) {
            if (spawnTimes.Count > 0) {
                yield return new WaitForSecondsRealtime(timeQueue.Dequeue());
            } else {
                yield return new WaitForSecondsRealtime(1);
            }
            GameObject nextSpawn = Instantiate(spawnQueue.Dequeue());
            nextSpawn.transform.parent = gameObject.transform;
            nextSpawn.transform.position = transform.position;
            if (spawnQueue.Count == 0) {
                spawnQueue = new Queue<GameObject>(spawnList);
                timeQueue = new Queue<float>(spawnTimes);
            }
        }
        yield return null;
    }

    IEnumerator SpawnNew(float min, float max) {
        while (true) {
            float m = max - ((levelManager.scrollSpeed - 5) / 8);
            float randomTime = Random.Range(min, m);
            randomTime = randomTime < 0.5f ? Random.Range(0.1f, 0.5f) : randomTime;
            int randomIndex = Random.Range(0, spawnList.Count);
            yield return new WaitForSecondsRealtime(randomTime);
            GameObject spawn = Instantiate(spawnList[randomIndex]);
            spawn.transform.parent = gameObject.transform;
            spawn.transform.position = transform.position;
        }
    }

}
