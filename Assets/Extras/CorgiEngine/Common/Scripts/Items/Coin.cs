using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Coin manager
	/// </summary>
	[AddComponentMenu("Corgi Engine/Items/Coin")]
	public class Coin : PickableItem
	{
		/// The amount of points to add when collected
		public int PointsToAdd = 10;

        private DataLogger logger;

        private void Awake() {
            logger = GameObject.FindGameObjectWithTag("DataLogger").GetComponent<DataLogger>();
        }

        /// <summary>
        /// Triggered when something collides with the coin
        /// </summary>
        /// <param name="collider">Other.</param>
        protected override void Pick() 
		{
			// we send a new points event for the GameManager to catch (and other classes that may listen to it too)
			CorgiEnginePointsEvent.Trigger(PointsMethods.Add, PointsToAdd);
            LevelManager.Instance.Score += PointsToAdd;
            logger._dataVector.PlayerPointPickup++;
        }
	}
}