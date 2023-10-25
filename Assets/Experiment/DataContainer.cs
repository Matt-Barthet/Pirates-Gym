using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataContainer : MonoBehaviour
{
    public int botHasCollisions;
    public int botIsCollidingAbove;
    public int botIsCollidingBelow;
    public int botIsCollidingLeft;
    public int botIsCollidingRight;
    public int botIsFalling;
    public int botIsGrounded;
    public int botIsJumping;
    public int botJustGotGrounded;
    public int botWasGroundedLastFrame;
    public int botWasTouchingTheCeilingLastFrame;
    public List<float> botSpeedX;
    public List<float> botSpeedY;
    public float botDeltaPosition;
    public float botHealth;
    public int botDamaged;
    public List<string> botDamagedBy;
    public int botShooting;
    public int botCharging;
    public List<int> botProjectileCount;
    public List<string> botProjectileDistance;
    public List<string> keyPresses;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
