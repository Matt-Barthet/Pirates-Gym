using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Manages the health of the SuperHipsterBros character
	/// </summary>
	public class SuperHipsterBrosHealth : Health
	{
	    protected Vector3 _initialScale;
        public bool hasPowerUp = false;

        protected Vector3 _startingScale;

        void Awake()
        {
	        base.Awake();
	        _startingScale = transform.localScale;
        }
        
        /// <summary>
        /// Grabs useful components, enables damage and gets the inital color
        /// </summary>
        protected override void Initialization()
	    {
	    	base.Initialization();
			_initialScale = transform.localScale; 
	    }
        
        public void SetSize(bool hasPowerUp)
        {
	        this.hasPowerUp = hasPowerUp;
	        if (hasPowerUp)
	        {
		        transform.localScale = _startingScale * 1.3f;
	        }
	        else
	        {
		        transform.localScale = _startingScale;
	        }
        }
        
	    /// <summary>
		/// Called when the player takes damage
		/// </summary>
		/// <param name="damage">The damage applied.</param>
		/// <param name="instigator">The damage instigator.</param>
		public override void Damage(int damage, GameObject instigator, float flickerDuration, float invincibilityDuration)
	    {
            base.damaged = true;
            base.damagedBy = instigator.name;
            // When the character takes damage, we create an auto destroy hurt particle system
            if (DamageEffect != null)
	        {
				Instantiate(DamageEffect, transform.position, transform.rotation);
	        }
	        if (transform.localScale.y==_initialScale.y)
	        {
				LevelManager.Instance.KillPlayer(_character);
	        }
	        else
	        {
	            // we prevent the character from colliding with layer 12 (Projectiles) and 13 (Enemies)        
	            DamageDisabled();
		        // StartCoroutine(DamageEnabled(1f)); not using the delay to avoid go-explore issues.
	            Invulnerable = false;
	            Shrink(1.3f);
                // We make the character's sprite flicker
                if (GetComponent<Renderer>() != null && !hasPowerUp) {
                    Color flickerColor = new Color32(255, 20, 20, 255);
                    // StartCoroutine(MMImage.Flicker(_renderer, _initialColor, flickerColor, 0.05f, 1f));
                }
            }
	    }

		/// <summary>
	    /// Doubles the size of the character
	    /// </summary>
	    public virtual void Grow(float growthFactor)
	    {
            if (!hasPowerUp) {
                // StartCoroutine(GrowChar(growthFactor));
                GetComponent<CharacterJump>().JumpStart();
                transform.localScale *= growthFactor;
                hasPowerUp = true;
                if (GetComponent<Renderer>() != null) {
                    Color flickerColor = new Color32(255, 255, 255, 255);
                    StartCoroutine(MMImage.Flicker(_renderer, _initialColor, flickerColor, 0.05f, 1f));
                }
            }
	    }

        IEnumerator GrowChar(float growthFactor) {
            GetComponent<CharacterJump>().JumpStart();
            yield return new WaitForSeconds(0.05f);
            transform.localScale *= growthFactor;
        }

	    /// <summary>
	    /// Shrinks the size of the character
	    /// </summary>
	    public virtual void Shrink(float shrinkFactor)
	    {
            if (hasPowerUp) {
                transform.localScale /= shrinkFactor;
                hasPowerUp = false;
                if (GetComponent<Renderer>() != null) {
                    Color flickerColor = new Color32(255, 20, 20, 255);
                    // StartCoroutine(MMImage.Flicker(_renderer, _initialColor, flickerColor, 0.05f, 1f));
                }
            }
	    }

	    /// <summary>
	    /// Resets the size of the character
	    /// </summary>
	    public virtual void ResetScale(float growthFactor)
	    {
	        transform.localScale = _initialScale;
	    }

	    /// <summary>
		/// Kills the character, sending it in the air
		/// </summary>

		public override void Kill()
	    {
			// we make our handheld device vibrate
			#if UNITY_ANDROID || UNITY_IPHONE
				if (VibrateOnDeath)
				{
					Handheld.Vibrate();
				}
			#endif
	        _controller.SetForce(new Vector2(0, 0));
			// we make it ignore the collisions from now on
			_controller.CollisionsOff();
			GetComponent<Collider2D>().enabled=false;
			// we set its dead state to true
			_character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
			// we set its health to zero (useful for the healthbar)
			CurrentHealth=0;
			// we reset the parameters
			_controller.ResetParameters();
			// we send it in the air
	        _controller.SetForce(new Vector2(0, 20));
            Shrink(1.3f);
            if (GetComponent<Renderer>() != null) {
                Color flickerColor = new Color32(255, 20, 20, 255);
                // StartCoroutine(MMImage.Flicker(_renderer, _initialColor, flickerColor, 0.05f, 1f));
            }
            base.logger.playerDeath++;
        }
	
	}
}
