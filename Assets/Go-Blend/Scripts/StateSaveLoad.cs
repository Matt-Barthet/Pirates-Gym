using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using MoreMountains.CorgiEngine;
using Unity.Services.Analytics;
using UnityEngine;

namespace Go_Blend.Scripts
{
    public static class StateSaveLoad
    {
        private static GameObject _player;
        public static Transform EnemyContainer, CoinContainer, MushroomContainer, BonusBlocks;
        private static string _currentPath;
        public static Dictionary<string, int> names;

        public static void Init()
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            EnemyContainer = GameObject.FindGameObjectWithTag("Entity Container").transform;
            CoinContainer = GameObject.FindGameObjectWithTag("Coin Container").transform;
            MushroomContainer = GameObject.FindGameObjectWithTag("Mushroom Container").transform;
            BonusBlocks = GameObject.FindGameObjectWithTag("Bonus tiles").transform;
            names = new Dictionary<string, int>
            {
                { "BulkyDude", 0 },
                { "BadassDude", 1 },
                { "retro_coin", 2 },
                { "HipsterMushroom", 3 },
                { "super-hipster-bros", 4},
                { "Platforms", 5},
                { "HipsterBonus", 6}
            };

        }

        public static void SaveEnvironment(string filename, string cwd="")
        {
            var characterDict = (from Transform entity in EnemyContainer select SaveCharacter(entity.gameObject)).ToList();
            var coinDict = (from Transform entity in CoinContainer select SaveCharacter(entity.gameObject)).ToList();
            var mushroomDict = (from Transform entity in MushroomContainer select SaveCharacter(entity.gameObject)).ToList();
            var bonusBlocks = new int[BonusBlocks.childCount];
            for (var i = 0; i < bonusBlocks.Length; i++)
            {
                bonusBlocks[i] = BonusBlocks.GetChild(i).GetComponent<BonusBlock>()._numberOfHitsLeft;
            }

            int checkpointCounter = 0;
            for (var i = 0; i < LevelManager.Instance.Checkpoints.Count; i++)
            {
                if (LevelManager.Instance.Checkpoints[i] != null)
                {
                    checkpointCounter = i;
                    break;
                }
            }
            
            var newSave = new SaveFile
            {
                currentScore = LevelManager.Instance.Score,
                currentTime = LevelManager.Instance._currentGameTime,
                CharacterDict = characterDict,
                PlayerDict = SaveCharacter(_player),
                CoinDict = coinDict,
                MushroomDict = mushroomDict,
                BonusBlocks = bonusBlocks,
                hasPowerup = _player.GetComponent<SuperHipsterBrosHealth>().hasPowerUp,
            };

            var bf = new BinaryFormatter();
            var file = File.Create($"{cwd}{filename}.save");
            bf.Serialize(file, newSave);
            file.Close();
        }

        private static Dictionary<string, float> SaveCharacter(GameObject character)
        {
            var controller = character.GetComponent<CorgiController>();
            var charTransform = character.transform.position;
            var dict = new Dictionary<string, float>();

            try
            {
                dict.Add("Character", names[character.name]);
                dict.Add("PositionX", charTransform.x);
                dict.Add("PositionY", charTransform.y);
                dict.Add("Enabled", character.activeSelf ? 1 : 0);
                dict.Add("SpeedX", controller.Speed.x);
                dict.Add("SpeedY", controller.Speed.y);
                dict.Add("Rotation", controller._storedMovementDirection);
                dict.Add("Direction", character.GetComponent<Character>().IsFacingRight ? 1 : 0);
            }
            catch (NullReferenceException)
            {
            }
            
            var horizontalMovement = character.GetComponent<CharacterHorizontalMovement>();
            var jump = character.GetComponent<CharacterJump>();
            if (horizontalMovement != null)
            {
                dict.Add("HorizontalMovement", horizontalMovement.GetHorizontalValue());
            }
            if (jump != null)
            {
                dict.Add("NumberOfJumps", jump.NumberOfJumpsLeft);
            }
            return dict;
        }

        private static void LoadCharacter(IReadOnlyDictionary<string, float> dict, Transform container)
        {
            if (dict["Enabled"] == 0) return;
            var id = names.FirstOrDefault(x => x.Value == (int) dict["Character"]).Key;
            var characterGo = Resources.Load<GameObject>(id);
            var newCharacter = _player;
            
            if (id != "super-hipster-bros")
            {
                newCharacter = GameObject.Instantiate(characterGo, container).gameObject;
                newCharacter.name = id;
            }
            
            newCharacter.transform.position = new Vector2(dict["PositionX"], dict["PositionY"]);
            
            try
            {
                var newController = newCharacter.GetComponent<CorgiController>();
                newController.SetVerticalForce(dict["SpeedY"]);
                newController.SetHorizontalForce(dict["SpeedX"]);
                newController._movementDirection = dict["Rotation"];
                newController._storedMovementDirection = dict["Rotation"];
            } catch (KeyNotFoundException) { }
            
            var horizontalMovement = newCharacter.GetComponent<CharacterHorizontalMovement>();
            var jump = newCharacter.GetComponent<CharacterJump>();
            var characterController = newCharacter.GetComponent<Character>();
            var AIWalk = newCharacter.GetComponent<AIWalk>();

            try
            {
                if (dict["Direction"] == 1f)
                {
                    characterController.InitialFacingDirection = Character.FacingDirections.Right;
                    characterController.DirectionOnSpawn = Character.SpawnFacingDirections.Right;
                    characterController.Face(Character.FacingDirections.Right);

                    if (AIWalk != null)
                    {
                        AIWalk._direction = Vector2.right;
                    }
                }
                else
                {
                    characterController.InitialFacingDirection = Character.FacingDirections.Left;
                    characterController.DirectionOnSpawn = Character.SpawnFacingDirections.Left;
                    characterController.Face(Character.FacingDirections.Left);
                    if (AIWalk != null)
                    {
                        AIWalk._direction = Vector2.left;
                    }
                }
            } catch (KeyNotFoundException){}


            if (horizontalMovement != null)
            {
                horizontalMovement.SetHorizontalMove(dict["HorizontalMovement"]);
            }

            if (jump != null)
            {
                jump.NumberOfJumpsLeft = (int) dict["NumberOfJumps"];
                jump.Reset();
                jump.JumpStop();
            }

            // Debug.Break();
        }
        
        public static void LoadEnvironment(string filename, string cwd="")
        {
            var bf = new BinaryFormatter();
            var file = File.Open($"{cwd}{filename}.save", FileMode.Open);
            var save = (SaveFile)bf.Deserialize(file);
            
            LevelManager.Instance.StopAllCoroutines();
            LevelManager.Instance.CurrentCheckPoint.SpawnPlayer(_player.GetComponent<Character>());
            LevelManager.Instance._started = DateTime.UtcNow;
            LevelManager.Instance.LevelCameraController.FollowsPlayer = true;
            
            // we send a new points event for the GameManager to catch (and other classes that may listen to it too)
            CorgiEnginePointsEvent.Trigger(PointsMethods.Set, 0);
            // we trigger a respawn event
            CorgiEngineEvent.Trigger(CorgiEngineEventTypes.Respawn);
            
            foreach (Transform child in CoinContainer)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach (Transform child in EnemyContainer)
            {
                GameObject.Destroy(child.gameObject);
            }           
            foreach (Transform child in MushroomContainer)
            {
                GameObject.Destroy(child.gameObject);
            }
            
            LevelManager.Instance.Score = save.currentScore;
            LevelManager.Instance._currentGameTime = save.currentTime;
            
            LoadCharacter(save.PlayerDict, null);
            
            foreach (var character in save.CharacterDict)
            {
                LoadCharacter(character, EnemyContainer);
            }
            for(var i = 0; i < BonusBlocks.childCount; i++)
            {
                BonusBlocks.GetChild(i).GetComponent<BonusBlock>().ReInitialise(save.BonusBlocks[i]);
            }
            foreach (var coin in save.CoinDict)
            {
                LoadCharacter(coin, CoinContainer);
            }
            foreach (var mushroom in save.MushroomDict)
            {
                LoadCharacter(mushroom, MushroomContainer);
            }
            
            _player.GetComponent<SuperHipsterBrosHealth>().SetSize(save.hasPowerup);
            file.Close();
            
        }
    }
    
    [Serializable]
    public class SaveFile
    {
        public int currentScore;
        public float currentTime;
        public List<Dictionary<string, float>> CharacterDict, CoinDict, MushroomDict;
        public Dictionary<string, float> PlayerDict;
        public int[] BonusBlocks;
        public bool hasPowerup;
    }
}
