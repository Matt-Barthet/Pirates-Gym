using MoreMountains.CorgiEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class DataLogger : MonoBehaviour {
    private GameObject player;
    private GameObject[] allBots;
    private GameObject[] allPickUps;
    private LevelManager levelManager;

    private List<GameObject> visibleCharacterList;
    private List<GameObject> visiblePickUpList;
    private List<GameObject> playerProjectiles;
    private List<GameObject> enemyProjectiles;
    private List<int> playerScore;
    private List<int> playerHasCollisions;
    private List<int> playerIsCollidingAbove;
    private List<int> playerIsCollidingBelow;
    private List<int> playerIsCollidingLeft;
    private List<int> playerIsCollidingRight;
    private List<int> playerIsFalling;
    private List<int> playerIsGrounded;
    private List<int> playerIsJumping;
    private List<float> playerSpeedX;
    private List<float> playerSpeedY;
    private List<float> playerDeltaDistance;
    private List<float> playerHealth;
    private List<int> playerDamaged;
    private List<string> playerDamagedBy;
    private List<int> playerShooting;
    private List<int> playerProjectileCount;
    private List<float> playerProjectileDistance;
    public int playerHealthPickup ;
    public int playerPointPickup;
    public int playerPowerPickup;
    public int playerBoostPickup;
    public int playerSlowPickup;
    private List<int> playerHasPowerup;
    public int playerKillCount;
    public int playerDeath;

    private List<int> botsVisible;
    private List<string> botName;
    private List<int> botHasCollisions;
    private List<int> botIsCollidingAbove;
    private List<int> botIsCollidingBelow;
    private List<int> botIsCollidingLeft;
    private List<int> botIsCollidingRight;
    private List<int> botIsFalling;
    private List<int> botIsGrounded;
    private List<int> botIsJumping;
    private List<float> botSpeedX;
    private List<float> botSpeedY;
    private List<float> botDeltaDistance;
    private List<float> botHealth;
    private List<int> botDamaged;
    private List<string> botDamagedBy;
    private List<int> botShooting;
    private List<int> botCharging;
    private List<float> botPlayerDistance;
    private List<int> botProjectileCount;
    private List<float> botProjectilePlayerDistance;
    private List<string> botProjectileTypes;
    private List<string> pickUpTypes;
    private List<int> pickUpsVisible;
    private List<float> pickUpPlayerDisctance;
    
    private void Awake() {
        ResetForm();
    }

    void Start() {
        levelManager = LevelManager.Instance;
        StartCoroutine(LateStart());
    }

    void Update() {
        allBots = GameObject.FindGameObjectsWithTag("Enemy");
        allPickUps = GameObject.FindGameObjectsWithTag("PickUp");
        visibleCharacterList = new List<GameObject>();
        foreach (GameObject character in allBots) {
            if (character.name == "RetroBossRightGun" || character.name == "RetroBossLeftGun") {
                if (character.transform.GetComponent<SpriteRenderer>().isVisible) {
                    visibleCharacterList.Add(character);
                }
            } else {
                if (levelManager.game == "gun") {
                    if (character.transform.GetChild(0).GetComponent<SpriteRenderer>().isVisible) {
                        visibleCharacterList.Add(character);
                    }
                } else {
                    if (character.GetComponent<SpriteRenderer>().isVisible) {
                        visibleCharacterList.Add(character);
                    }
                }
            }
        }

        visiblePickUpList = new List<GameObject>();
        foreach (GameObject pickup in allPickUps) {
            if (pickup.transform.GetComponent<SpriteRenderer>().isVisible) {
                visiblePickUpList.Add(pickup);
            }
        }

        playerProjectiles = new List<GameObject>();
        enemyProjectiles = new List<GameObject>();
        GameObject[] allProjectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject projectile in allProjectiles) {
            if (projectile.transform.GetComponent<SpriteRenderer>().isVisible) {
                if (projectile.layer == 16) {
                    playerProjectiles.Add(projectile);
                } else {
                    enemyProjectiles.Add(projectile);
                }
            }
        }
        UpdateForm();
    }

    // Update is called once per frame
    void UpdateForm() {
        playerScore.Add(levelManager.Score);
        playerHasCollisions.Add(player.GetComponent<CorgiController>().State.HasCollisions ? 1 : 0);
        playerIsCollidingAbove.Add(player.GetComponent<CorgiController>().State.IsCollidingAbove ? 1 : 0);
        playerIsCollidingBelow.Add(player.GetComponent<CorgiController>().State.IsCollidingBelow ? 1 : 0);
        playerIsCollidingLeft.Add(player.GetComponent<CorgiController>().State.IsCollidingLeft ? 1 : 0);
        playerIsCollidingRight.Add(player.GetComponent<CorgiController>().State.IsCollidingRight ? 1 : 0);
        playerIsFalling.Add(player.GetComponent<CorgiController>().State.IsFalling ? 1 : 0);
        playerIsGrounded.Add(player.GetComponent<CorgiController>().State.IsGrounded ? 1 : 0);
        if (levelManager.game == "endless") {
            playerIsJumping.Add(player.GetComponent<CharacterLadder>().moving ? 1 : 0);
        } else {
            playerIsJumping.Add(player.GetComponent<CorgiController>().State.IsJumping ? 1 : 0);
        }
        if (levelManager.game == "endless") {
            playerSpeedX.Add(levelManager.scrollSpeed);
        } else {
            playerSpeedX.Add(Mathf.Abs(player.GetComponent<CorgiController>().Speed.x));
        }
        playerSpeedY.Add(Mathf.Abs(player.GetComponent<CorgiController>().Speed.y));
        if (levelManager.game == "endless") {
            playerDeltaDistance.Add((levelManager._isPlaying ? levelManager.scrollSpeed : 0)
                + player.GetComponent<CorgiController>().distanceTravelled);
        } else {
            playerDeltaDistance.Add(player.GetComponent<CorgiController>().distanceTravelled);
        }
        playerHealth.Add((float)player.GetComponent<Health>().CurrentHealth / (float)player.GetComponent<Health>().InitialHealth);
        playerDamaged.Add(player.GetComponent<Health>().damaged ? 1 : 0);
        playerDamagedBy.Add(player.GetComponent<Health>().damagedBy);
        if (levelManager.game == "endless") {
            playerShooting.Add(player.GetComponent<PlayerAttack>()._attackInProgress ? 1 : 0);
        } else if (levelManager.game == "platform") {
            playerShooting.Add(0);
        } else {
            playerShooting.Add(player.GetComponent<CharacterHandleWeapon>().shooting ? 1 : 0);
        }
        if (levelManager.game == "platform" || levelManager.game == "endless") {
            playerProjectileCount.Add(0);
            playerProjectileDistance.Add(0);
        } else {
            playerProjectileCount.Add(playerProjectiles.Count);
            foreach (GameObject projectile in playerProjectiles) {
                playerProjectileDistance.Add(Vector2.Distance(projectile.transform.position, player.transform.position));
            }
        }

        if (levelManager.game == "platform") {
            playerHasPowerup.Add(player.GetComponent<SuperHipsterBrosHealth>().hasPowerUp ? 1: 0);
        } else {
            playerHasPowerup.Add(0);
        }

        botsVisible.Add(visibleCharacterList.Count);
        foreach (GameObject projectile in enemyProjectiles) {
            botProjectilePlayerDistance.Add(Vector2.Distance(projectile.transform.position, player.transform.position));
            botProjectileTypes.Add(projectile.name.Split('-')[0]);
        }

        for (int i = 0; i < visibleCharacterList.Count; i++) {
            GameObject bot = visibleCharacterList[i];
            botName.Add(bot.name.Split(' ')[0]);
            if (levelManager.game == "endless") {
                botHasCollisions.Add(0);
                botIsCollidingAbove.Add(0);
                botIsCollidingBelow.Add(1);
                botIsCollidingLeft.Add(bot.GetComponent<DamageOnTouch>().colliding ? 1 : 0);
                botIsCollidingRight.Add(0);
                botIsFalling.Add(0);
                botIsGrounded.Add(1);
                botIsJumping.Add(0);
                botSpeedX.Add(levelManager.scrollSpeed);
                botSpeedY.Add(0);
                botDeltaDistance.Add(levelManager.scrollSpeed);
            } else {
                botHasCollisions.Add(bot.GetComponent<CorgiController>().State.HasCollisions ? 1 : 0);
                botIsCollidingAbove.Add(bot.GetComponent<CorgiController>().State.IsCollidingAbove ? 1 : 0);
                botIsCollidingBelow.Add(bot.GetComponent<CorgiController>().State.IsCollidingBelow ? 1 : 0);
                botIsCollidingLeft.Add(bot.GetComponent<CorgiController>().State.IsCollidingLeft ? 1 : 0);
                botIsCollidingRight.Add(bot.GetComponent<CorgiController>().State.IsCollidingRight ? 1 : 0);
                botIsFalling.Add(bot.GetComponent<CorgiController>().State.IsFalling ? 1 : 0);
                botIsGrounded.Add(bot.GetComponent<CorgiController>().State.IsGrounded ? 1 : 0);
                botIsJumping.Add(bot.GetComponent<CorgiController>().State.IsJumping ? 1 : 0);
                botSpeedX.Add(Mathf.Abs(bot.GetComponent<CorgiController>().Speed.x));
                botSpeedY.Add(Mathf.Abs(bot.GetComponent<CorgiController>().Speed.y));
                botDeltaDistance.Add(bot.GetComponent<CorgiController>().distanceTravelled);
            }
            botHealth.Add((float)bot.GetComponent<Health>().CurrentHealth / (float)bot.GetComponent<Health>().InitialHealth);
            if (levelManager.game == "gun") {
                botDamaged.Add(bot.GetComponent<Health>().damaged ? 1 : 0);
                botDamagedBy.Add(bot.GetComponent<Health>().damagedBy);
            }
            if (levelManager.game == "platform" || levelManager.game == "endless") {
                botShooting.Add(0);
                botProjectileCount.Add(0);
                botCharging.Add(0);
            } else {
                botShooting.Add(bot.GetComponent<CharacterHandleWeapon>().shooting ? 1 : 0);
                botProjectileCount.Add(enemyProjectiles.Count);
                if (bot.GetComponent<CharacterHandleWeapon>().CurrentWeapon != null) {
                    botCharging.Add(bot.GetComponent<CharacterHandleWeapon>().CurrentWeapon.charging ? 1 : 0);
                }
            }
            botPlayerDistance.Add(Vector2.Distance(bot.transform.position, player.transform.position));
        }
        
        pickUpsVisible.Add(visiblePickUpList.Count);
        for (int i = 0; i < visiblePickUpList.Count; i++) {
            GameObject pickup = visiblePickUpList[i];
            pickUpPlayerDisctance.Add(Vector2.Distance(pickup.transform.position, player.transform.position));
            pickUpTypes.Add(pickup.name);
        }
        
    }

    private double[] PackageData()
    {
        var state = new double[46];
        state[0] = playerScore.DefaultIfEmpty(0).Average();
        state[1] = playerHasCollisions.DefaultIfEmpty(0).Average();
        state[2] = playerIsCollidingAbove.DefaultIfEmpty(0).Average();
        state[3] =  playerIsCollidingBelow.DefaultIfEmpty(0).Average();
        state[4] = playerIsCollidingLeft.DefaultIfEmpty(0).Average();
        state[5] = playerIsCollidingRight.DefaultIfEmpty(0).Average();
        state[6] = playerIsFalling.DefaultIfEmpty(0).Average();
        state[7] = playerIsGrounded.DefaultIfEmpty(0).Average();
        state[8] = playerIsJumping.DefaultIfEmpty(0).Average();
        state[9] = playerSpeedX.DefaultIfEmpty(0).Average();
        state[10] = playerSpeedY.DefaultIfEmpty(0).Average();
        state[11] = playerDeltaDistance.DefaultIfEmpty(-1).Average();
        state[12] = playerHealth.DefaultIfEmpty(0).Average();
        state[13] = playerDamaged.DefaultIfEmpty(0).Average();
        state[14] = playerShooting.DefaultIfEmpty(0).Average();
        state[15] = playerProjectileCount.DefaultIfEmpty(0).Average();
        state[16] = playerProjectileDistance.DefaultIfEmpty(-1).Average();
        state[17] = playerHealthPickup;
        state[18] = playerPointPickup;
        state[19] = playerPowerPickup;
        state[20] = playerBoostPickup;
        state[21] = playerSlowPickup;
        state[22] = playerHasPowerup.DefaultIfEmpty(0).Average();
        state[23] = playerKillCount;
        state[24] = playerDeath;
        state[25] = botsVisible.DefaultIfEmpty(0).Average();
        state[26] = botHasCollisions.DefaultIfEmpty(0).Average();
        state[27] = botIsCollidingAbove.DefaultIfEmpty(0).Average();
        state[28] = botIsCollidingBelow.DefaultIfEmpty(0).Average();
        state[29] = botIsCollidingLeft.DefaultIfEmpty(0).Average();
        state[30] = botIsCollidingRight.DefaultIfEmpty(0).Average();
        state[31] = botIsFalling.DefaultIfEmpty(0).Average();
        state[32] = botIsGrounded.DefaultIfEmpty(0).Average();
        state[33] = botIsJumping.DefaultIfEmpty(0).Average();
        state[34] = botSpeedX.DefaultIfEmpty(0).Average();
        state[35] = botSpeedY.DefaultIfEmpty(0).Average();
        state[36] = botDeltaDistance.DefaultIfEmpty(0).Average();
        state[37] = botHealth.DefaultIfEmpty(0).Average();
        state[38] = botDamaged.DefaultIfEmpty(0).Average();
        state[39] = botShooting.DefaultIfEmpty(0).Average();
        state[40] = botProjectileCount.DefaultIfEmpty(0).Average();
        state[41] = botCharging.DefaultIfEmpty(0).Average();
        state[42] = botPlayerDistance.DefaultIfEmpty(0).Average();
        state[43] = botProjectilePlayerDistance.DefaultIfEmpty(0).Average();
        state[44] = pickUpsVisible.DefaultIfEmpty(0).Average();
        state[45] = pickUpPlayerDisctance.DefaultIfEmpty(0).Average();
        return state;
    }
    
    private void ResetForm() {
        playerScore = new List<int>();
        playerHasCollisions = new List<int>();
        playerIsCollidingAbove = new List<int>();
        playerIsCollidingBelow = new List<int>();
        playerIsCollidingLeft = new List<int>();
        playerIsCollidingRight = new List<int>();
        playerIsFalling = new List<int>();
        playerIsGrounded = new List<int>();
        playerIsJumping = new List<int>();
        playerSpeedX = new List<float>();
        playerSpeedY = new List<float>();
        playerDeltaDistance = new List<float>();
        playerHealth = new List<float>();
        playerDamaged = new List<int>();
        playerDamagedBy = new List<string>();
        playerShooting = new List<int>();
        playerProjectileCount = new List<int>();
        playerProjectileDistance = new List<float>();
        playerHealthPickup = 0;
        playerPointPickup = 0;
        playerPowerPickup = 0;
        playerBoostPickup = 0;
        playerSlowPickup = 0;
        playerHasPowerup = new List<int>();
        playerKillCount = 0;
        playerDeath = 0;

        botsVisible = new List<int>();
        botName = new List<string>();
        botHasCollisions = new List<int>();
        botIsCollidingAbove = new List<int>();
        botIsCollidingBelow = new List<int>();
        botIsCollidingLeft = new List<int>();
        botIsCollidingRight = new List<int>();
        botIsFalling = new List<int>();
        botIsGrounded = new List<int>();
        botIsJumping = new List<int>();
        botSpeedX = new List<float>();
        botSpeedY = new List<float>();
        botDeltaDistance = new List<float>();
        botHealth = new List<float>();
        botDamaged = new List<int>();
        botDamagedBy = new List<string>();
        botShooting = new List<int>();
        botCharging = new List<int>();
        botPlayerDistance = new List<float>();
        botProjectileCount = new List<int>();
        botProjectilePlayerDistance = new List<float>();
        botProjectileTypes = new List<string>();

        pickUpTypes = new List<string>();
        pickUpsVisible = new List<int>();
        pickUpPlayerDisctance = new List<float>();
    }
    

    private IEnumerator LateStart() {
        yield return new WaitForFixedUpdate();
        player = GameObject.FindWithTag("Player");
        yield return null;
    }
}
