using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreMountains.CorgiEngine;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.SideChannels;
using UnityEngine;

namespace Go_Blend.Scripts
{
    public class MySideChannel : SideChannel
    {
        private readonly Rigidbody2D _rigidbody2D;
        private readonly CorgiController _corgiController;
        private GameObject _currentTarget;
        
        public MySideChannel(GoBlendGymController pAgent, Rigidbody2D rigidbody2D, CorgiController corgiController)
        {
            ChannelId = new Guid("621f0a70-4f87-11ea-a6bf-784f4387d1f7");
            _rigidbody2D = rigidbody2D;
            _corgiController = corgiController;
        }
        
        protected override void OnMessageReceived(IncomingMessage incomingMessage)
        {
            var test = incomingMessage.ReadString();
            if (test.Contains("[Cell Name]:"))
            {
                var name = test.Split(":")[1];
                StateSaveLoad.LoadEnvironment(name, true);
            } else if (test.Contains("[Set Position]:"))
            {
                // Do something
                var message = test.Split(":")[1];

                var positionString = message.Split("/")[0].Split(",");
                var rotationString = message.Split("/")[1];
                var velocityString = message.Split("/")[2].Split(",");
                
                float[] position = { float.Parse(positionString[0]), float.Parse(positionString[1])};
                float[] velocity = { float.Parse(velocityString[0]), float.Parse(velocityString[1])};

                _rigidbody2D.transform.position = (new Vector2(position[0], position[1]));
                _corgiController.Speed = new Vector2(velocity[0], velocity[1]);
                _corgiController._movementDirection = int.Parse(rotationString);
            }
        }

        public void SendMessage(OutgoingMessage msg)
        {
            QueueMessageToSend(msg);
        }
    }

    public class GoBlendGymController : Agent
    {
        private MySideChannel _mySideChannel;
        private CharacterHorizontalMovement _inputController;
        private CharacterJump _jumpController;
        private CorgiController _corgiController;
        private SuperHipsterBrosHealth _health;
        private Rigidbody2D _rigidBody;
        public GridSensor customSensor;
        private int _targetMovement, _targetJump;

        private void Start()
        {
            if (!isActiveAndEnabled) return;
            _rigidBody = GetComponent<Rigidbody2D>();
            _corgiController = GetComponent<CorgiController>();
            _inputController = GetComponent<CharacterHorizontalMovement>();
            _jumpController = GetComponent<CharacterJump>();
            _health = GetComponent<SuperHipsterBrosHealth>();
            _mySideChannel = new MySideChannel(this, _rigidBody, _corgiController);
            SideChannelManager.RegisterSideChannel(_mySideChannel);
        }
        
        public override void OnEpisodeBegin()
        {
            if (!isActiveAndEnabled) return;
            ExperimentManager.playerEnded = false;
        }
        
        public override void CollectObservations(VectorSensor sensor)
        {
            // Vector sensors from previous state representation:
            sensor.AddObservation(transform.position.x);
            sensor.AddObservation(transform.position.y);
            sensor.AddObservation(_corgiController.Speed.x); 
            sensor.AddObservation(_corgiController.Speed.y);
            sensor.AddObservation(_corgiController._movementDirection); 
            sensor.AddObservation(_health.CurrentHealth); 
            sensor.AddObservation(_health.hasPowerUp);
            
            // Custom messages to send through our side channel:
            var dirMessage = new OutgoingMessage();
            var facing = transform.forward;
            dirMessage.WriteString($"[Direction]:{facing.x},{facing.y},{facing.z}");
            _mySideChannel.SendMessage(dirMessage);
            
            // var position = transform.position;
            // sensor.AddObservation(position.x);
            // sensor.AddObservation(position.y);
            
            /*var startingVec = Vector3.right;
            const int numberOfRaycasts = 32;
            const int maxDistance = 20;
            var layerMask = LayerMask.GetMask("Platforms", "Enemies", "LevelBounds", "Item");

            var raycast = new float[numberOfRaycasts];
            var raycastNames = new int[numberOfRaycasts];
            
            for (var i = 0; i < numberOfRaycasts; i++)
            {            
                var direction = transform.TransformDirection(Quaternion.Euler(0, 0, -360 / numberOfRaycasts * i) * startingVec);
                var  hit = Physics2D.Raycast(transform.position + Vector3.down *0.6f, direction, maxDistance, layerMask);
                if (hit)
                {
                    Debug.DrawRay(transform.position + Vector3.down *0.6f, direction * hit.distance, Color.white);
                    var value = hit.distance;
                    raycast[i] = value;
                    raycastNames[i] = StateSaveLoad.names[hit.collider.name];
                }
                else
                {
                    raycast[i] = maxDistance;
                    raycastNames[i] = -1;
                }
            }
            
            for (var i = 0; i < numberOfRaycasts; i++)
            {            
                sensor.AddObservation(raycast[i]);
            }
            for (var i = 0; i < numberOfRaycasts; i++)
            {            
                sensor.AddObservation(raycastNames[i]);
            }

            positions.Add(new [] { position.x, position.y});
            rotations.Add((int) corgiController._movementDirection);
            velocities.Add(new [] { corgiController.Speed.x, corgiController.Speed.y});
            raycasts.Add(raycast);
            scores.Add(LevelManager.Instance.Score);
            
            */
        }
        
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            _targetMovement = actionBuffers.DiscreteActions[0];
            _targetJump = actionBuffers.DiscreteActions[1];
            _inputController.SetHorizontalMove(_targetMovement);
            if (_targetJump == 1)
            {
                _jumpController.JumpStart();
            }
            SetReward(LevelManager.Instance.Score);
            
            if (!ExperimentManager.playerEnded)
            {
                return;
            }
            
            var finishedMessage = new OutgoingMessage();
            finishedMessage.WriteString("[Level Ended]");
            _mySideChannel.SendMessage( finishedMessage);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;
            discreteActionsOut[0] = (int) Input.GetAxisRaw("Horizontal");
            discreteActionsOut[1] = Input.GetKey(KeyCode.Space) ? 1:0;
        }
    }
}