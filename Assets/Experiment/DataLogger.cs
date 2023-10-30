using MoreMountains.CorgiEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class DataLogger : MonoBehaviour {
    private GameObject _player;
    private GameObject[] _allBots;
    private GameObject[] _allPickUps;
    private LevelManager _levelManager;

    private List<GameObject> _visibleCharacterList;
    private List<GameObject> _visiblePickUpList;
    private List<GameObject> _playerProjectiles;
    private List<GameObject> _enemyProjectiles;
    
    public DataVector _dataVector;
    
    private void Awake() {
    }

    void Start() {
        _levelManager = LevelManager.Instance;
        _dataVector = new DataVector();
        _dataVector.ResetForm();
        StartCoroutine(LateStart());
    }

    void Update() {
        _allBots = GameObject.FindGameObjectsWithTag("Enemy");
        _allPickUps = GameObject.FindGameObjectsWithTag("PickUp");
        _visibleCharacterList = new List<GameObject>();
        foreach (GameObject character in _allBots) {
            if (character.name == "RetroBossRightGun" || character.name == "RetroBossLeftGun") {
                if (character.transform.GetComponent<SpriteRenderer>().isVisible) {
                    _visibleCharacterList.Add(character);
                }
            } else {
                if (_levelManager.game == "gun") {
                    if (character.transform.GetChild(0).GetComponent<SpriteRenderer>().isVisible) {
                        _visibleCharacterList.Add(character);
                    }
                } else {
                    if (character.GetComponent<SpriteRenderer>().isVisible) {
                        _visibleCharacterList.Add(character);
                    }
                }
            }
        }

        _visiblePickUpList = new List<GameObject>();
        foreach (GameObject pickup in _allPickUps) {
            if (pickup.transform.GetComponent<SpriteRenderer>().isVisible) {
                _visiblePickUpList.Add(pickup);
            }
        }

        _playerProjectiles = new List<GameObject>();
        _enemyProjectiles = new List<GameObject>();
        GameObject[] allProjectiles = GameObject.FindGameObjectsWithTag("Projectile");
        foreach (GameObject projectile in allProjectiles) {
            if (projectile.transform.GetComponent<SpriteRenderer>().isVisible) {
                if (projectile.layer == 16) {
                    _playerProjectiles.Add(projectile);
                } else {
                    _enemyProjectiles.Add(projectile);
                }
            }
        }
        UpdateForm();
    }

    // Update is called once per frame
    void UpdateForm() {
        _dataVector.PlayerScore.Add(_levelManager.Score);
        _dataVector.PlayerHasCollisions.Add(_player.GetComponent<CorgiController>().State.HasCollisions ? 1 : 0);
        _dataVector.PlayerIsCollidingAbove.Add(_player.GetComponent<CorgiController>().State.IsCollidingAbove ? 1 : 0);
        _dataVector.PlayerIsCollidingBelow.Add(_player.GetComponent<CorgiController>().State.IsCollidingBelow ? 1 : 0);
        _dataVector.PlayerIsCollidingLeft.Add(_player.GetComponent<CorgiController>().State.IsCollidingLeft ? 1 : 0);
        _dataVector.PlayerIsCollidingRight.Add(_player.GetComponent<CorgiController>().State.IsCollidingRight ? 1 : 0);
        _dataVector.PlayerIsFalling.Add(_player.GetComponent<CorgiController>().State.IsFalling ? 1 : 0);
        _dataVector.PlayerIsGrounded.Add(_player.GetComponent<CorgiController>().State.IsGrounded ? 1 : 0);
        if (_levelManager.game == "endless") {
            _dataVector.PlayerIsJumping.Add(_player.GetComponent<CharacterLadder>().moving ? 1 : 0);
        } else {
            _dataVector.PlayerIsJumping.Add(_player.GetComponent<CorgiController>().State.IsJumping ? 1 : 0);
        }
        if (_levelManager.game == "endless") {
            _dataVector.PlayerSpeedX.Add(_levelManager.scrollSpeed);
        } else {
            _dataVector.PlayerSpeedX.Add(Mathf.Abs(_player.GetComponent<CorgiController>().Speed.x));
        }
        _dataVector.PlayerSpeedY.Add(Mathf.Abs(_player.GetComponent<CorgiController>().Speed.y));
        if (_levelManager.game == "endless") {
            _dataVector.PlayerDeltaDistance.Add((_levelManager._isPlaying ? _levelManager.scrollSpeed : 0)
                                                + _player.GetComponent<CorgiController>().distanceTravelled);
        } else {
            _dataVector.PlayerDeltaDistance.Add(_player.GetComponent<CorgiController>().distanceTravelled);
        }
        _dataVector.PlayerHealth.Add((float)_player.GetComponent<Health>().CurrentHealth / (float)_player.GetComponent<Health>().InitialHealth);
        _dataVector.PlayerDamaged.Add(_player.GetComponent<Health>().damaged ? 1 : 0);
        _dataVector.PlayerDamagedBy.Add(_player.GetComponent<Health>().damagedBy);
        if (_levelManager.game == "endless") {
            _dataVector.PlayerShooting.Add(_player.GetComponent<PlayerAttack>()._attackInProgress ? 1 : 0);
        } else if (_levelManager.game == "platform") {
            _dataVector.PlayerShooting.Add(0);
        } else {
            _dataVector.PlayerShooting.Add(_player.GetComponent<CharacterHandleWeapon>().shooting ? 1 : 0);
        }
        if (_levelManager.game == "platform" || _levelManager.game == "endless") {
            _dataVector.PlayerProjectileCount.Add(0);
            _dataVector.PlayerProjectileDistance.Add(0);
        } else {
            _dataVector.PlayerProjectileCount.Add(_playerProjectiles.Count);
            foreach (GameObject projectile in _playerProjectiles) {
                _dataVector.PlayerProjectileDistance.Add(Vector2.Distance(projectile.transform.position, _player.transform.position));
            }
        }

        if (_levelManager.game == "platform") {
            _dataVector.PlayerHasPowerup.Add(_player.GetComponent<SuperHipsterBrosHealth>().hasPowerUp ? 1: 0);
        } else {
            _dataVector.PlayerHasPowerup.Add(0);
        }

        _dataVector.BotsVisible.Add(_visibleCharacterList.Count);
        foreach (GameObject projectile in _enemyProjectiles) {
            _dataVector.BotProjectilePlayerDistance.Add(Vector2.Distance(projectile.transform.position, _player.transform.position));
            _dataVector.BotProjectileTypes.Add(projectile.name.Split('-')[0]);
        }

        for (int i = 0; i < _visibleCharacterList.Count; i++) {
            GameObject bot = _visibleCharacterList[i];
            _dataVector.BotName.Add(bot.name.Split(' ')[0]);
            if (_levelManager.game == "endless") {
                _dataVector.BotHasCollisions.Add(0);
                _dataVector.BotIsCollidingAbove.Add(0);
                _dataVector.BotIsCollidingBelow.Add(1);
                _dataVector.BotIsCollidingLeft.Add(bot.GetComponent<DamageOnTouch>().colliding ? 1 : 0);
                _dataVector.BotIsCollidingRight.Add(0);
                _dataVector.BotIsFalling.Add(0);
                _dataVector.BotIsGrounded.Add(1);
                _dataVector.BotIsJumping.Add(0);
                _dataVector.BotSpeedX.Add(_levelManager.scrollSpeed);
                _dataVector.BotSpeedY.Add(0);
                _dataVector.BotDeltaDistance.Add(_levelManager.scrollSpeed);
            } else {
                _dataVector.BotHasCollisions.Add(bot.GetComponent<CorgiController>().State.HasCollisions ? 1 : 0);
                _dataVector.BotIsCollidingAbove.Add(bot.GetComponent<CorgiController>().State.IsCollidingAbove ? 1 : 0);
                _dataVector.BotIsCollidingBelow.Add(bot.GetComponent<CorgiController>().State.IsCollidingBelow ? 1 : 0);
                _dataVector.BotIsCollidingLeft.Add(bot.GetComponent<CorgiController>().State.IsCollidingLeft ? 1 : 0);
                _dataVector.BotIsCollidingRight.Add(bot.GetComponent<CorgiController>().State.IsCollidingRight ? 1 : 0);
                _dataVector.BotIsFalling.Add(bot.GetComponent<CorgiController>().State.IsFalling ? 1 : 0);
                _dataVector.BotIsGrounded.Add(bot.GetComponent<CorgiController>().State.IsGrounded ? 1 : 0);
                _dataVector.BotIsJumping.Add(bot.GetComponent<CorgiController>().State.IsJumping ? 1 : 0);
                _dataVector.BotSpeedX.Add(Mathf.Abs(bot.GetComponent<CorgiController>().Speed.x));
                _dataVector.BotSpeedY.Add(Mathf.Abs(bot.GetComponent<CorgiController>().Speed.y));
                _dataVector.BotDeltaDistance.Add(bot.GetComponent<CorgiController>().distanceTravelled);
            }
            _dataVector.BotHealth.Add((float)bot.GetComponent<Health>().CurrentHealth / (float)bot.GetComponent<Health>().InitialHealth);
            if (_levelManager.game == "gun") {
                _dataVector.BotDamaged.Add(bot.GetComponent<Health>().damaged ? 1 : 0);
                _dataVector.BotDamagedBy.Add(bot.GetComponent<Health>().damagedBy);
            }
            if (_levelManager.game == "platform" || _levelManager.game == "endless") {
                _dataVector.BotShooting.Add(0);
                _dataVector.BotProjectileCount.Add(0);
                _dataVector.BotCharging.Add(0);
            } else {
                _dataVector.BotShooting.Add(bot.GetComponent<CharacterHandleWeapon>().shooting ? 1 : 0);
                _dataVector.BotProjectileCount.Add(_enemyProjectiles.Count);
                if (bot.GetComponent<CharacterHandleWeapon>().CurrentWeapon != null) {
                    _dataVector.BotCharging.Add(bot.GetComponent<CharacterHandleWeapon>().CurrentWeapon.charging ? 1 : 0);
                }
            }
            _dataVector.BotPlayerDistance.Add(Vector2.Distance(bot.transform.position, _player.transform.position));
        }
        
        _dataVector.PickUpsVisible.Add(_visiblePickUpList.Count);
        for (int i = 0; i < _visiblePickUpList.Count; i++) {
            GameObject pickup = _visiblePickUpList[i];
            _dataVector.PickUpPlayerDisctance.Add(Vector2.Distance(pickup.transform.position, _player.transform.position));
            _dataVector.PickUpTypes.Add(pickup.name);
        }
        
    }
    
    private IEnumerator LateStart() {
        yield return new WaitForFixedUpdate();
        _player = GameObject.FindWithTag("Player");
        yield return null;
    }

    [Serializable]
    public class DataVector
    {
        public List<int> PlayerScore;
        public List<int> PlayerHasCollisions;
        public List<int> PlayerIsCollidingAbove;
        public List<int> PlayerIsCollidingBelow;
        public List<int> PlayerIsCollidingLeft;
        public List<int> PlayerIsCollidingRight;
        public List<int> PlayerIsFalling;
        public List<int> PlayerIsGrounded;
        public List<int> PlayerIsJumping;
        public List<float> PlayerSpeedX;
        public List<float> PlayerSpeedY;
        public List<float> PlayerDeltaDistance;
        public List<float> PlayerHealth;
        public List<int> PlayerDamaged;
        public List<string> PlayerDamagedBy;
        public List<int> PlayerShooting;
        public List<int> PlayerProjectileCount;
        public List<float> PlayerProjectileDistance;
        public int PlayerHealthPickup ;
        public int PlayerPointPickup;
        public int PlayerPowerPickup;
        public int PlayerBoostPickup;
        public int PlayerSlowPickup;
        public List<int> PlayerHasPowerup;
        public int PlayerKillCount;
        public int PlayerDeath;

        public List<int> BotsVisible;
        public List<string> BotName;
        public List<int> BotHasCollisions;
        public List<int> BotIsCollidingAbove;
        public List<int> BotIsCollidingBelow;
        public List<int> BotIsCollidingLeft;
        public List<int> BotIsCollidingRight;
        public List<int> BotIsFalling;
        public List<int> BotIsGrounded;
        public List<int> BotIsJumping;
        public List<float> BotSpeedX;
        public List<float> BotSpeedY;
        public List<float> BotDeltaDistance;
        public List<float> BotHealth;
        public List<int> BotDamaged;
        public List<string> BotDamagedBy;
        public List<int> BotShooting;
        public List<int> BotCharging;
        public List<float> BotPlayerDistance;
        public List<int> BotProjectileCount;
        public List<float> BotProjectilePlayerDistance;
        public List<string> BotProjectileTypes;
        public List<string> PickUpTypes;
        public List<int> PickUpsVisible;
        public List<float> PickUpPlayerDisctance;

        public int ticks;
        
        public void ResetForm() {
        PlayerScore = new List<int>();
        PlayerHasCollisions = new List<int>();
        PlayerIsCollidingAbove = new List<int>();
        PlayerIsCollidingBelow = new List<int>();
        PlayerIsCollidingLeft = new List<int>();
        PlayerIsCollidingRight = new List<int>();
        PlayerIsFalling = new List<int>();
        PlayerIsGrounded = new List<int>();
        PlayerIsJumping = new List<int>();
        PlayerSpeedX = new List<float>();
        PlayerSpeedY = new List<float>();
        PlayerDeltaDistance = new List<float>();
        PlayerHealth = new List<float>();
        PlayerDamaged = new List<int>();
        PlayerDamagedBy = new List<string>();
        PlayerShooting = new List<int>();
        PlayerProjectileCount = new List<int>();
        PlayerProjectileDistance = new List<float>();
        PlayerHealthPickup = 0;
        PlayerPointPickup = 0;
        PlayerPowerPickup = 0;
        PlayerBoostPickup = 0;
        PlayerSlowPickup = 0;
        PlayerHasPowerup = new List<int>();
        PlayerKillCount = 0;
        PlayerDeath = 0;

        BotsVisible = new List<int>();
        BotName = new List<string>();
        BotHasCollisions = new List<int>();
        BotIsCollidingAbove = new List<int>();
        BotIsCollidingBelow = new List<int>();
        BotIsCollidingLeft = new List<int>();
        BotIsCollidingRight = new List<int>();
        BotIsFalling = new List<int>();
        BotIsGrounded = new List<int>();
        BotIsJumping = new List<int>();
        BotSpeedX = new List<float>();
        BotSpeedY = new List<float>();
        BotDeltaDistance = new List<float>();
        BotHealth = new List<float>();
        BotDamaged = new List<int>();
        BotDamagedBy = new List<string>();
        BotShooting = new List<int>();
        BotCharging = new List<int>();
        BotPlayerDistance = new List<float>();
        BotProjectileCount = new List<int>();
        BotProjectilePlayerDistance = new List<float>();
        BotProjectileTypes = new List<string>();

        PickUpTypes = new List<string>();
        PickUpsVisible = new List<int>();
        PickUpPlayerDisctance = new List<float>();
        ticks = 0;
        }
        public double[] PackageData()
    {
        var state = new double[46];
        state[0] = PlayerScore.DefaultIfEmpty(0).Average();
        state[1] = PlayerHasCollisions.DefaultIfEmpty(0).Average();
        state[2] = PlayerIsCollidingAbove.DefaultIfEmpty(0).Average();
        state[3] = PlayerIsCollidingBelow.DefaultIfEmpty(0).Average();
        state[4] = PlayerIsCollidingLeft.DefaultIfEmpty(0).Average();
        state[5] = PlayerIsCollidingRight.DefaultIfEmpty(0).Average();
        state[6] = PlayerIsFalling.DefaultIfEmpty(0).Average();
        state[7] = PlayerIsGrounded.DefaultIfEmpty(0).Average();
        state[8] = PlayerIsJumping.DefaultIfEmpty(0).Average();
        state[9] = PlayerSpeedX.DefaultIfEmpty(0).Average();
        state[10] = PlayerSpeedY.DefaultIfEmpty(0).Average();
        state[11] = PlayerDeltaDistance.DefaultIfEmpty(-1).Average();
        state[12] = PlayerHealth.DefaultIfEmpty(0).Average();
        state[13] = PlayerDamaged.DefaultIfEmpty(0).Average();
        state[14] = PlayerShooting.DefaultIfEmpty(0).Average();
        state[15] = PlayerProjectileCount.DefaultIfEmpty(0).Average();
        state[16] = PlayerProjectileDistance.DefaultIfEmpty(-1).Average();
        state[17] = PlayerHealthPickup;
        state[18] = PlayerPointPickup;
        state[19] = PlayerPowerPickup;
        state[20] = PlayerBoostPickup;
        state[21] = PlayerSlowPickup;
        state[22] = PlayerHasPowerup.DefaultIfEmpty(0).Average();
        state[23] = PlayerKillCount;
        state[24] = PlayerDeath;
        state[25] = BotsVisible.DefaultIfEmpty(0).Average();
        state[26] = BotHasCollisions.DefaultIfEmpty(0).Average();
        state[27] = BotIsCollidingAbove.DefaultIfEmpty(0).Average();
        state[28] = BotIsCollidingBelow.DefaultIfEmpty(0).Average();
        state[29] = BotIsCollidingLeft.DefaultIfEmpty(0).Average();
        state[30] = BotIsCollidingRight.DefaultIfEmpty(0).Average();
        state[31] = BotIsFalling.DefaultIfEmpty(0).Average();
        state[32] = BotIsGrounded.DefaultIfEmpty(0).Average();
        state[33] = BotIsJumping.DefaultIfEmpty(0).Average();
        state[34] = BotSpeedX.DefaultIfEmpty(0).Average();
        state[35] = BotSpeedY.DefaultIfEmpty(0).Average();
        state[36] = BotDeltaDistance.DefaultIfEmpty(0).Average();
        state[37] = BotHealth.DefaultIfEmpty(0).Average();
        state[38] = BotDamaged.DefaultIfEmpty(0).Average();
        state[39] = BotShooting.DefaultIfEmpty(0).Average();
        state[40] = BotProjectileCount.DefaultIfEmpty(0).Average();
        state[41] = BotCharging.DefaultIfEmpty(0).Average();
        state[42] = BotPlayerDistance.DefaultIfEmpty(0).Average();
        state[43] = BotProjectilePlayerDistance.DefaultIfEmpty(0).Average();
        state[44] = PickUpsVisible.DefaultIfEmpty(0).Average();
        state[45] = PickUpPlayerDisctance.DefaultIfEmpty(0).Average();
        ResetForm();
        return state;
    }
    }
}
