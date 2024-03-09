using System;
using System.Collections.Generic;

namespace TerrainSamples.SceneNavMeshTest
{
    class StateManager
    {
        private readonly Dictionary<States, Action> stateStartingCallbacks = new();
        private readonly Dictionary<States, Action> stateUpdatingCallbacks = new();

        public States GameState { get; private set; } = States.Default;

        public void InitializeState(States state, Action startingAction, Action updatingAction)
        {
            if (stateStartingCallbacks.ContainsKey(state))
            {
                stateStartingCallbacks[state] = startingAction;
            }
            else
            {
                stateStartingCallbacks.Add(state, startingAction);
            }

            if (stateUpdatingCallbacks.ContainsKey(state))
            {
                stateUpdatingCallbacks[state] = updatingAction;
            }
            else
            {
                stateUpdatingCallbacks.Add(state, updatingAction);
            }
        }

        public void StartState(States state)
        {
            if (GameState == state)
            {
                return;
            }

            GameState = state;

            if (stateStartingCallbacks.TryGetValue(GameState, out Action action))
            {
                action();
            }
        }
        public bool UpdateState()
        {
            if (stateUpdatingCallbacks.TryGetValue(GameState, out Action action))
            {
                action();

                return true;
            }

            return false;
        }
    }
}
