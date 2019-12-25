using System;
namespace CppParser
{
    public class NestedClassState
    {
        enum State
        {
            OUTER,
            SAW_CLASS,
            IN_CLASS,
            IN_METHOD,
        }

        System.Collections.Generic.Stack<State> state = new System.Collections.Generic.Stack<State>();

        public NestedClassState()
        {
            state.Push(State.OUTER);
        }

        /** Called when a class(-like) token is encountered */
        public void saw_class()
        {
            state.Push(State.SAW_CLASS);
        }

        /** Called when a class(-like) token possibly encountered can no longer be used to define a class */
        public void unsaw_class()
        {
            var value = state.Pop(); state.Push(value);
            if (value == State.SAW_CLASS)
                state.Pop();
        }


        /** Called when an opening brace is encountered */
        public void saw_open_brace()
        {
            var value = state.Pop(); state.Push(value);
            switch (value)
            {
                case State.OUTER:
                    // C++ functions
                    state.Push(State.IN_METHOD);
                    break;
                case State.SAW_CLASS:
                    state.Pop(); state.Push(State.IN_CLASS);
                    break;
                case State.IN_CLASS:
                case State.IN_METHOD:
                    state.Push(State.IN_METHOD);
                    break;
            }
        }

        /** Called when an closing brace is encountered */
        public void saw_close_brace()
        {
            var value = state.Pop(); state.Push(value);
            switch (value)
            {
                case State.OUTER:
                case State.SAW_CLASS:
                    /* Should not happen; ignore */
                    break;
                case State.IN_CLASS:
                case State.IN_METHOD:
                    state.Pop();
                    break;
            }
        }


        /** Return true if processing a method or function body */
        public bool in_method() { var value = state.Pop(); state.Push(value); return value == State.IN_METHOD; }
    }
}
