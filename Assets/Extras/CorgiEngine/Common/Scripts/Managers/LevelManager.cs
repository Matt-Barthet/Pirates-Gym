using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.MMInterface;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MoreMountains.CorgiEngine
{
    public struct LevelNameEvent
    {
        public string LevelName;

        /// <summary>
        /// Initializes a LevelNameEvent
        /// </summary>
        /// <param name="levelName"></param>
        public LevelNameEvent(string levelName)
        {
            LevelName = levelName;
        }

        static LevelNameEvent e;
        public static void Trigger(string levelName)
        {
            e.LevelName = levelName;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// Spawns the player, handles checkpoints and respawn
    /// </summary>
    [AddComponentMenu("Corgi Engine/Managers/Level Manager")]
	public class LevelManager : Singleton<LevelManager>
	{
        public string game;
        public float maxTimeOnLevel = 2;
        public float scrollSpeed;
        public float originalScrollSpeed;
        [Header("UI")]
        /// Text object where start countdown is shown
        public Text StartGameCountdown;
        public int StartGameCountDownTime;
        /// Panel shown when game has ended
        public RectTransform EndGamePanel;
        /// Text object for players ranking at the end of the game
        public Text EndGameRanking;
        /// Back button to return to lobby screen 
        public Text ScoreLabel;
        public Text ScoreText;
        public Text TimerText;
        public RectTransform HUD;
        public int Score;
        public int KillCount;
        public Image Tutorial;

        /// the possible checkpoint axis
        public enum CheckpointsAxis { x, y, z}
		public enum CheckpointDirections { Ascending, Descending }
		public enum OneWayLevelModes { None, Left, Right, Top, Down }
		public enum OneWayLevelKillModes { NoKill, Kill }

		/// the prefab you want for your player
		[Header("Playable Characters")]
		[Information("The LevelManager is responsible for handling spawn/respawn, checkpoints management and level bounds. Here you can define one or more playable characters for your level..",InformationAttribute.InformationType.Info,false)]
		/// the list of player prefabs to instantiate
		public Character[] PlayerPrefabs ;
		/// should the player IDs be auto attributed (usually yes)
		public bool AutoAttributePlayerIDs = true;

		[Header("Checkpoints")]
		[Information("Here you can select a checkpoint attribution axis (if your level is horizontal go for X, Y if it's vertical), and a debug spawn where your player character will spawn from while in editor mode.",InformationAttribute.InformationType.Info,false)]
		/// Debug spawn
		public CheckPoint DebugSpawn;
		/// the axis on which objects should be compared
		public CheckpointsAxis CheckpointAttributionAxis = CheckpointsAxis.x;

		public CheckpointDirections CheckpointAttributionDirection = CheckpointDirections.Ascending;

		[ReadOnly]
		/// the current checkpoint
		public CheckPoint CurrentCheckPoint;

		[Space(10)]
		[Header("Points of Entry")]
		public Transform[] PointsOfEntry;

		[Space(10)]
		[Header("Intro and Outro durations")]
		[Information("Here you can specify the length of the fade in and fade out at the start and end of your level. You can also determine the delay before a respawn.",InformationAttribute.InformationType.Info,false)]
		/// duration of the initial fade in (in seconds)
		public float IntroFadeDuration=1f;
		/// duration of the fade to black at the end of the level (in seconds)
		public float OutroFadeDuration=1f;
		/// duration between a death of the main character and its respawn
		public float RespawnDelay = 2f;


		[Space(10)]
		[Header("Level Bounds")]
		[Information("The level bounds are used to constrain the camera's movement, as well as the player character's. You can see it in real time in the scene view as you adjust its size (it's the yellow box).",InformationAttribute.InformationType.Info,false)]
		/// the level limits, camera and player won't go beyond this point.
		public Bounds LevelBounds = new Bounds(Vector3.zero,Vector3.one*10);

		[Space(10)]
		[Header("One Way Levels")]
		[Information("Just like in early Mario levels for example, you can prevent your character from going back in the level on one axis Here you can define that axis and the size of the NoGoingBack object that will be created. Make it big enough so your character can't go through it. The default size should be fine in most situations. You can also decide whether touching this new boundary should kill your player or not, and the distance it should maintain from the player and the level bounds.",InformationAttribute.InformationType.Info,false)]
		/// if this is set to anything but None, the level manager will instantiate (on start) a NoGoingBack object that will prevent movement from the player on the specified axis if it tries to go back.
		public OneWayLevelModes OneWayLevelMode = OneWayLevelModes.None;
		/// the size of the NoGoingBack collider
		public Vector2 NoGoingBackColliderSize = new Vector2(5,5);
		/// the maximum distance at which the NoGoingBack object should stay from the player character
		public float NoGoingBackThreshold = 5f;
		/// the maximum distance the NoGoingBack object can approach the level bounds
		public float NoGoingBackMinDistanceFromBounds = 10f;
		/// if you set this to Kill, the player will be killed when touching the One Way boundary
		public OneWayLevelKillModes OneWayLevelKillMode = OneWayLevelKillModes.NoKill;


		/// the elapsed time since the start of the level
		public TimeSpan RunningTime { get { return DateTime.UtcNow - _started ;}}
		public CameraController LevelCameraController { get; set; }

	    // private stuff
		public List<Character> Players { get; protected set; }
	    public List<CheckPoint> Checkpoints { get; protected set; }
	    public DateTime _started;
	    protected int _savedPoints;
		public NoGoingBack NoGoingBackObject { get; protected set; }
		protected string _nextLevel = null;

        public UnityEvent OnGameStarts;
        public UnityEvent OnGameEnds;

        public bool _isPlaying = false;
        private bool canStart = true;
        public float _currentGameTime;

        /// <summary>
        /// On awake, instantiates the player
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			InstantiatePlayableCharacters ();
            Instance.Score = 0;
            Instance.KillCount = 0;
            originalScrollSpeed = scrollSpeed;
		}

        public bool instantiatePlayers;
        public Character player;
		/// <summary>
		/// Instantiate playable characters based on the ones specified in the PlayerPrefabs list in the LevelManager's inspector.
		/// </summary>
		protected virtual void InstantiatePlayableCharacters()
		{
			Players = new List<Character> ();

			if (!instantiatePlayers)
			{
				Players.Add(player);
				return;
			}
			
			// we check if there's a stored character in the game manager we should instantiate
			if (GameManager.Instance.StoredCharacter != null)
			{
				Character newPlayer = (Character)Instantiate (GameManager.Instance.StoredCharacter, new Vector3 (0, 0, 0), Quaternion.identity);
				newPlayer.name = GameManager.Instance.StoredCharacter.name;
				Players.Add(newPlayer);
				return;
			}

			if (PlayerPrefabs == null) { return; }

			// player instantiation
			if (PlayerPrefabs.Count() != 0)
			{
				foreach (Character playerPrefab in PlayerPrefabs)
				{
					Character newPlayer = (Character)Instantiate (playerPrefab, new Vector3 (0, 0, 0), Quaternion.identity);
					newPlayer.name = playerPrefab.name;
					Players.Add(newPlayer);

					if (playerPrefab.CharacterType != Character.CharacterTypes.Player)
					{
						Debug.LogWarning ("LevelManager : The Character you've set in the LevelManager isn't a Player, which means it's probably not going to move. You can change that in the Character component of your prefab.");
					}
				}
			}
			else
			{
				//Debug.LogWarning ("LevelManager : The Level Manager doesn't have any Player prefab to spawn. You need to select a Player prefab from its inspector.");
				return;
			}
		}

		/// <summary>
		/// Initialization
		/// </summary>
		public virtual void Start() {
            _isPlaying = false;
            FreezeCharacters();
            if (Players == null || Players.Count == 0) { return; }

			Initialization ();
			LevelGUIStart ();

            if (EndGamePanel != null) {
                EndGamePanel.gameObject.SetActive(false);
            }

            if (ScoreText != null) {
                ScoreLabel.text = "";
                ScoreText.text = "";
            }

            if (TimerText.text != null) {
                TimerText.text = "";
            }

            if (HUD != null) {
                HUD.gameObject.SetActive(false);
            }


            // we handle the spawn of the character(s)
            if (Players.Count == 1)
			{
				SpawnSingleCharacter ();
				InstantiateNoGoingBack ();
			}
			else
			{
				SpawnMultipleCharacters ();
			}

			CheckpointAssignment ();

			// we trigger a level start event
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.LevelStart);
			MMGameEvent.Trigger("Load");
			StartGameCountdownCoroutine();
		}

		/// <summary>
		/// Gets current camera, points number, start time, etc.
		/// </summary>
		protected virtual void Initialization()
		{
			// storage
			LevelCameraController = FindObjectOfType<CameraController>();
			_savedPoints=GameManager.Instance.Points;
			_started = DateTime.UtcNow;

			// we store all the checkpoints present in the level, ordered by their x value
			if ((CheckpointAttributionAxis == CheckpointsAxis.x) && (CheckpointAttributionDirection == CheckpointDirections.Ascending))
			{
				Checkpoints = FindObjectsOfType<CheckPoint> ().OrderBy (o => o.transform.position.x).ToList ();
			}
			if ((CheckpointAttributionAxis == CheckpointsAxis.x) && (CheckpointAttributionDirection == CheckpointDirections.Descending))
			{
				Checkpoints = FindObjectsOfType<CheckPoint> ().OrderByDescending (o => o.transform.position.x).ToList ();
			}
			if ((CheckpointAttributionAxis == CheckpointsAxis.y) && (CheckpointAttributionDirection == CheckpointDirections.Ascending))
			{
				Checkpoints = FindObjectsOfType<CheckPoint> ().OrderBy (o => o.transform.position.y).ToList ();
			}
			if ((CheckpointAttributionAxis == CheckpointsAxis.y) && (CheckpointAttributionDirection == CheckpointDirections.Descending))
			{
				Checkpoints = FindObjectsOfType<CheckPoint> ().OrderByDescending (o => o.transform.position.y).ToList ();
			}
			if ((CheckpointAttributionAxis == CheckpointsAxis.z) && (CheckpointAttributionDirection == CheckpointDirections.Ascending))
			{
				Checkpoints = FindObjectsOfType<CheckPoint> ().OrderBy (o => o.transform.position.z).ToList ();
			}
			if ((CheckpointAttributionAxis == CheckpointsAxis.z) && (CheckpointAttributionDirection == CheckpointDirections.Descending))
			{
				Checkpoints = FindObjectsOfType<CheckPoint> ().OrderByDescending (o => o.transform.position.z).ToList ();
			}

			// we assign the first checkpoint
			CurrentCheckPoint = Checkpoints.Count > 0 ? Checkpoints[0] : null;
		}

		/// <summary>
		/// Assigns all respawnable objects in the scene to their checkpoint
		/// </summary>
		public virtual void CheckpointAssignment()
		{
			// we get all respawnable objects in the scene and attribute them to their corresponding checkpoint
			IEnumerable<Respawnable> listeners = FindObjectsOfType<MonoBehaviour>().OfType<Respawnable>();
			foreach(Respawnable listener in listeners)
			{
				for (int i = Checkpoints.Count - 1; i>=0; i--)
				{
					Vector3 vectorDistance = ((MonoBehaviour) listener).transform.position - Checkpoints[i].transform.position;

					float distance = 0;
					if (CheckpointAttributionAxis == CheckpointsAxis.x)
					{
						distance = vectorDistance.x;
					}
					if (CheckpointAttributionAxis == CheckpointsAxis.y)
					{
						distance = vectorDistance.y;
					}
					if (CheckpointAttributionAxis == CheckpointsAxis.z)
					{
						distance = vectorDistance.z;
					}

					// if the object is behind the checkpoint (on the attribution axis), we move on to the next checkpoint
					if ((distance < 0) && (CheckpointAttributionDirection == CheckpointDirections.Ascending))
					{
						continue;
					}
					if ((distance > 0) && (CheckpointAttributionDirection == CheckpointDirections.Descending))
					{
						continue;
					}

					// if the object is further on the attribution axis compared to the checkpoint, we assign it to the checkpoint, and proceed to the next object
					Checkpoints[i].AssignObjectToCheckPoint(listener);
					break;
				}
			}
		}

		/// <summary>
		/// Initializes GUI stuff
		/// </summary>
		protected virtual void LevelGUIStart()
		{
            // set the level name in the GUI
            LevelNameEvent.Trigger(SceneManager.GetActiveScene().name);
			// fade in
			MMFadeOutEvent.Trigger(IntroFadeDuration);			
		}

		/// <summary>
		/// Spawns a playable character into the scene
		/// </summary>
		protected virtual void SpawnSingleCharacter()
		{
			// in debug mode we spawn the player on the debug spawn point
			#if UNITY_EDITOR
			if (DebugSpawn!= null)
			{
				DebugSpawn.SpawnPlayer(Players[0]);
				return;
			}
			else
			{
				RegularSpawnSingleCharacter();
			}
			#else
				RegularSpawnSingleCharacter();
			#endif
		}

		/// <summary>
		/// Spawns the character at the selected entry point if there's one, or at the selected checkpoint.
		/// </summary>
		protected virtual void RegularSpawnSingleCharacter()
		{
			PointsOfEntryStorage point = GameManager.Instance.GetPointsOfEntry(SceneManager.GetActiveScene().name);
			if ((point != null) && (PointsOfEntry.Length >= (point.PointOfEntryIndex + 1)))
			{
				Players[0].RespawnAt(PointsOfEntry[point.PointOfEntryIndex], Character.FacingDirections.Right);
				return;
			}

			if (CurrentCheckPoint != null)
			{
				CurrentCheckPoint.SpawnPlayer(Players[0]);
				return;
			}
		}

		/// <summary>
		/// Spawns multiple playable characters into the scene
		/// </summary>
		protected virtual void SpawnMultipleCharacters()
		{
			int checkpointCounter = 0;
			int characterCounter = 1;
			bool spawned = false;
			foreach (Character player in Players)
			{
				spawned = false;

				if (AutoAttributePlayerIDs)
				{
					player.SetPlayerID("Player"+characterCounter);
				}

				player.name += " - " + player.PlayerID;

				if (Checkpoints.Count > checkpointCounter+1)
				{
					if (Checkpoints[checkpointCounter] != null)
					{
						Checkpoints[checkpointCounter].SpawnPlayer(player);
						characterCounter++;
						spawned = true;
						checkpointCounter++;
					}
				}
				if (!spawned)
				{
					Checkpoints[checkpointCounter].SpawnPlayer(player);
					characterCounter++;
				}
			}
		}

		/// <summary>
		/// Instantiates (if needed) the no going back object that will prevent back movement in the level
		/// </summary>
		protected virtual void InstantiateNoGoingBack ()
		{
			if (OneWayLevelMode == OneWayLevelModes.None)
			{
				return;
			}
			// we instantiate the new no going back object
			GameObject newObject = new GameObject();
			newObject.name = "NoGoingBack";
			newObject.layer = LayerMask.NameToLayer ("PlatformsPlayerOnly");
			Vector3 newPosition = Players[0].transform.position;
			if (OneWayLevelMode == OneWayLevelModes.Left)	{ newPosition.x -= NoGoingBackThreshold + NoGoingBackColliderSize.x/2f; }
			if (OneWayLevelMode == OneWayLevelModes.Right)	{ newPosition.x += NoGoingBackThreshold + NoGoingBackColliderSize.x/2f; }
			if (OneWayLevelMode == OneWayLevelModes.Down)	{ newPosition.y -= NoGoingBackThreshold + NoGoingBackColliderSize.y/2f; }
			if (OneWayLevelMode == OneWayLevelModes.Top)	{ newPosition.y += NoGoingBackThreshold + NoGoingBackColliderSize.y/2f; }
			newObject.transform.position = newPosition;

			WallClingingOverride wallClingingOverride = newObject.AddComponent<WallClingingOverride> ();
			wallClingingOverride.CanWallClingToThis = false;

			BoxCollider2D collider2D = newObject.AddComponent<BoxCollider2D> ();
			collider2D.size = NoGoingBackColliderSize;

			NoGoingBackObject = newObject.AddComponent<NoGoingBack>();
			NoGoingBackObject.ThresholdDistance = NoGoingBackThreshold;
			NoGoingBackObject.Target = Players [0].transform;
			NoGoingBackObject.OneWayLevelMode = OneWayLevelMode;
			NoGoingBackObject.NoGoingBackColliderSize = NoGoingBackColliderSize;
			NoGoingBackObject.MinDistanceFromBounds = NoGoingBackMinDistanceFromBounds;

			if (OneWayLevelKillMode == OneWayLevelKillModes.Kill)
			{
				newObject.AddComponent<KillPlayerOnTouch> ();
			}
		}

		/// <summary>
		/// Every frame we check for checkpoint reach
		/// </summary>
		public virtual void Update()
		{
			if (Players == null)
			{
				return;
			}

			_savedPoints = GameManager.Instance.Points;
			_started = DateTime.UtcNow;

            if(_isPlaying) {                
                _currentGameTime += Time.deltaTime;
                System.TimeSpan ts = System.TimeSpan.FromSeconds(_currentGameTime);
                TimerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);

                if (ScoreText != null) {
                    ScoreLabel.text = "score";
                    ScoreText.text = Score.ToString();
                }

                if (HUD != null && game!="platform") {
                    HUD.gameObject.SetActive(true);
                }

                if (_currentGameTime / 60 > maxTimeOnLevel) {
                    _isPlaying = false;
                    GotoLevel(null);
                }
            }
        }

        private void LateUpdate() {
            KillCount = 0;
        }

        /// <summary>
        /// Sets the current checkpoint.
        /// </summary>
        /// <param name="newCheckPoint">New check point.</param>
        public virtual void SetCurrentCheckpoint(CheckPoint newCheckPoint)
		{
			CurrentCheckPoint = newCheckPoint;
		}

		public virtual void SetNextLevel (string levelName)
		{
			_nextLevel = levelName;
		}

		public virtual void GotoNextLevel()
		{
			GotoLevel (_nextLevel);
			_nextLevel = null;
		}

		public static bool levelEnded = false;
		
		/// <summary>
		/// Gets the player to the specified level
		/// </summary>
		/// <param name="levelName">Level name.</param>
		public virtual void GotoLevel(string levelName)
		{
            Instance.OnGameEnds.Invoke();
            // levelEnded = true;

            // CorgiEngineEvent.Trigger(CorgiEngineEventTypes.LevelEnd);
            // MMGameEvent.Trigger("Save");

            // MMFadeInEvent.Trigger(OutroFadeDuration);
            // StartCoroutine(GotoLevelCo(levelName));
		}

	    /// <summary>
	    /// Waits for a short time and then loads the specified level
	    /// </summary>
	    /// <returns>The level co.</returns>
	    /// <param name="levelName">Level name.</param>
	    protected virtual IEnumerator GotoLevelCo(string levelName)
		{
			if (Players != null && Players.Count > 0)
	        {
				foreach (Character player in Players)
				{
					player.Disable ();
				}
	        }

	        if (Time.timeScale > 0.0f)
	        {
	            yield return new WaitForSeconds(OutroFadeDuration);
			}
			// we trigger an unPause event for the GameManager (and potentially other classes)
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.UnPause);

	        if (string.IsNullOrEmpty(levelName))
	        {
				LoadingSceneManager.LoadScene("StartScreen");
			}
			else
			{
				LoadingSceneManager.LoadScene(levelName);
			}
		}

		/// <summary>
		/// Kills the player.
		/// </summary>
		public virtual void KillPlayer(Character player)
		{
			Health characterHealth = player.GetComponent<Health>();
			if (characterHealth == null)
			{
				return;
			}
			else
			{
				// we kill the character
				characterHealth.Kill ();
				CorgiEngineEvent.Trigger(CorgiEngineEventTypes.PlayerDeath);

				// if we have only one player, we restart the level
				if (Players.Count < 2)
				{
					StartCoroutine (SoloModeRestart ());
				}
			}
		}

		public void StopRestartCoroutine()
		{
			StopAllCoroutines();
			CurrentCheckPoint.SpawnPlayer(Players[0]);
		}
		
	    /// <summary>
	    /// Coroutine that kills the player, stops the camera, resets the points.
	    /// </summary>
	    /// <returns>The player co.</returns>
	    protected IEnumerator SoloModeRestart()
		{
			if (PlayerPrefabs.Count() <= 0)
			{
				yield break;
			}
			
			if (LevelCameraController != null)
			{
				LevelCameraController.FollowsPlayer=false;
			}

			yield return new WaitForSeconds(RespawnDelay);

			if (LevelCameraController != null)
			{
				LevelCameraController.FollowsPlayer=true;
			}

			if (CurrentCheckPoint != null)
			{
				CurrentCheckPoint.SpawnPlayer(Players[0]);
			}
			_started = DateTime.UtcNow;
			// we send a new points event for the GameManager to catch (and other classes that may listen to it too)
			CorgiEnginePointsEvent.Trigger(PointsMethods.Set, 0);
			// we trigger a respawn event
			CorgiEngineEvent.Trigger(CorgiEngineEventTypes.Respawn);
		}

		/// <summary>
		/// Freezes the character(s)
		/// </summary>
		public virtual void FreezeCharacters()
		{
			foreach (Character player in Players)
			{
                // StartCoroutine(PlayerFreeze(player));
            }
		}
		
		/// <summary>
		/// Unfreezes the character(s)
		/// </summary>
		public virtual void UnFreezeCharacters()
		{
			foreach (Character player in Players)
			{
				//player.UnFreeze();
			}

        }

		/// <summary>
		/// Toggles Character Pause
		/// </summary>
		public virtual void ToggleCharacterPause()
		{
			foreach (Character player in Players)
			{

				CharacterPause characterPause = player.GetComponent<CharacterPause>();
				if (characterPause == null)
				{
					break;
				}

				if (GameManager.Instance.Paused)
				{
					characterPause.PauseCharacter();
				}
				else
				{
					characterPause.UnPauseCharacter();
				}
			}
		}
		
        private void StartGameCountdownCoroutine() {
            Instance.OnGameStarts.Invoke();
            _isPlaying = true;
            UnFreezeCharacters();
            Players[0].GetComponent<CorgiController>().lastPosition = Players[0].transform.position;
            _currentGameTime = 0f;
        }
    }
}
