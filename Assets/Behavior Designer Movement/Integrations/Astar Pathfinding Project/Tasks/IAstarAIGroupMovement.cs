using UnityEngine;
using Pathfinding;
using System;

namespace BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject
{
    // Abstract class for any task that uses a group of IAstarAI agents
    public abstract class IAstarAIGroupMovement : GroupMovement
    {
        //changed from SharedGameObject[] to list so that it can be assigned
        [Tooltip("All of the agents")]
        public SharedGameObjectList agents = null;
        [Tooltip("The speed of the agents")]
        public SharedFloat speed = 10;

        protected IAstarAI[] aStarAgents;
        protected Transform[] transforms;

        public override void OnStart()
        {
            aStarAgents = new IAstarAI[agents.Value.Count];
            transforms = new Transform[agents.Value.Count];

            // Set the speed and turning speed of all of the agents
            for (int i = 0; i < agents.Value.Count; ++i) {
                aStarAgents[i] = agents.Value[i].GetComponent<IAstarAI>();
                transforms[i] = agents.Value[i].transform;

                aStarAgents[i].maxSpeed = speed.Value;
            }
        }

        protected override bool SetDestination(int index, Vector3 target)
        {
            aStarAgents[index].destination = target;
            aStarAgents[index].canMove = true;
            return true;
        }

        protected override Vector3 Velocity(int index)
        {
            return aStarAgents[index].velocity;
        }

        public override void OnEnd()
        {
            for (int i = 0; i < agents.Value.Count; ++i) {
                aStarAgents[i].destination = transform.position;
                aStarAgents[i].canMove = false;
            }
        }

        // Reset the public variables
        public override void OnReset()
        {
            agents = null;
            speed = 3;
        }
    }
}
