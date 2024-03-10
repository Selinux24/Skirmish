using System;
using System.Collections.Generic;

namespace TerrainSamples.SceneNavMeshTest
{
    class StateManager
    {
        private readonly Dictionary<States, Action> stateStartingCallbacks = [];
        private readonly Dictionary<States, Action> stateUpdatingCallbacks = [];

        public States GameState { get; private set; } = States.Default;

        public void InitializeState(States state, Action startingAction, Action updatingAction)
        {
            if (!stateStartingCallbacks.TryAdd(state, startingAction))
            {
                stateStartingCallbacks[state] = startingAction;
            }

            if (!stateUpdatingCallbacks.TryAdd(state, updatingAction))
            {
                stateUpdatingCallbacks[state] = updatingAction;
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
