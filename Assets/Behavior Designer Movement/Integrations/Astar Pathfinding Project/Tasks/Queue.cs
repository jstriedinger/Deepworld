using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Movement.AstarPathfindingProject
{
    [TaskDescription("Queue in a line with the A* Pathfinding Project.")]
    [TaskCategory("Movement/A* Pathfinding Project")]
    [HelpURL("https://www.opsive.com/support/documentation/behavior-designer-movement-pack/")]
    [TaskIcon("c671469908c78284c909ff1905020250", "454cd90f13f2a9a4f93ade9379f9b3c9")]
    public class Queue : IAstarAIGroupMovement
    {
        [Tooltip("Agents less than this distance apart are neighbors")]
        public SharedFloat neighborDistance = 10;
        [Tooltip("The distance that the agents should be separated")]
        public SharedFloat separationDistance = 2;
        [Tooltip("The distance the the agent should look ahead to see if another agent is in the way")]
        public SharedFloat maxQueueAheadDistance = 2;
        [Tooltip("The radius that the agent should check to see if another agent is in the way")]
        public SharedFloat maxQueueRadius = 20;
        [Tooltip("The multiplier to slow down if an agent is in front of the current agent")]
        public SharedFloat slowDownSpeed = 0.15f;
        [Tooltip("The target to seek towards")]
        public SharedGameObject target;

        // The agents will always be flocking so always return running
        public override TaskStatus OnUpdate()
        {
            // Determine a destination for each agent
            for (int i = 0; i < agents.Value.Count; ++i) {
                if (AgentAhead(i)) {
                    SetDestination(i, transforms[i].position + transforms[i].forward * slowDownSpeed.Value + DetermineSeparation(i));
                } else {
                    SetDestination(i, target.Value.transform.position);
                }
            }
            return TaskStatus.Running;
        }

        // Returns the agent that is ahead of the current agent
        private bool AgentAhead(int index)
        {
            // queueAhead is the distance in front of the current agent
            var queueAhead = Velocity(index) * maxQueueAheadDistance.Value;
            for (int i = 0; i < agents.Value.Count; ++i) {
                // Return the first agent that is ahead of the current agent
                if (index != i && Vector3.SqrMagnitude(queueAhead - transforms[i].position) < maxQueueRadius.Value) {
                    return true;
                }
            }
            return false;
        }

        // Determine the separation between the current agent and all of the other agents also queuing
        private Vector3 DetermineSeparation(int agentIndex)
        {
            var separation = Vector3.zero;
            int neighborCount = 0;
            var agentTransform = transforms[agentIndex];
            // Loop through each agent to determine the separation
            for (int i = 0; i < agents.Value.Count; ++i) {
                // The agent can't compare against itself
                if (agentIndex != i) {
                    // Only determine the parameters if the other agent is its neighbor
                    if (Vector3.SqrMagnitude(transforms[i].position - agentTransform.position) < neighborDistance.Value) {
                        // This agent is the neighbor of the original agent so add the separation
                        separation += transforms[i].position - agentTransform.position;
                        neighborCount++;
                    }
                }
            }

            // Don't move if there are no neighbors
            if (neighborCount == 0) {
                return Vector3.zero;
            }
            // Normalize the value
            return ((separation / neighborCount) * -1).normalized * separationDistance.Value;
        }

        // Reset the public variables
        public override void OnReset()
        {
            base.OnReset();

            neighborDistance = 10;
            separationDistance = 2;
            maxQueueAheadDistance = 2;
            maxQueueRadius = 20;
            slowDownSpeed = 0.15f;
        }
    }
}