using System;

namespace ChatClient
{
    public class FiniteStateMachine
    {
        // Enum to represent different states
        public enum State
        {
            Start,
            Auth,
            Open,
            Error,
            End
        }

        private State currentState;

        public FiniteStateMachine()
        {
            currentState = State.Start; // Initial state
        }

        // Method to transition to a new state
        public void TransitionTo(State newState)
        {
            currentState = newState;
            Console.WriteLine($"Transitioned to state: {currentState}");
        }

        public State GetState()
        {
            return currentState;
        }
    }
}
