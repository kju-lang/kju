﻿namespace KJU.Core.Regex
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KJU.Core.Automata;

    public static class RegexToNfaConverter<Symbol>
    {
        public const string InvalidStateMessage = "The given state is invalid";

        public static INfa<Symbol> Convert(Regex<Symbol> regex)
        {
            switch (regex)
            {
                case AtomicRegex<Symbol> atomic:
                    return new AtomicNfa(atomic.Value);
                case ConcatRegex<Symbol> concat:
                    return new ConcatNfa(Convert(concat.Left), Convert(concat.Right));
                case EmptyRegex<Symbol> empty:
                    return new EmptyNfa();
                case EpsilonRegex<Symbol> epsilon:
                    return new EpsilonNfa();
                case StarRegex<Symbol> star:
                    return new StarNfa(Convert(star.Child));
                case SumRegex<Symbol> sum:
                    return new SumNfa(Convert(sum.Left), Convert(sum.Right));
            }

            throw new NotImplementedException();
        }

        private static IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> MapDictionary(
            IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> dictionary, Func<IState, IState> f)
        {
            var result = new Dictionary<Symbol, IReadOnlyCollection<IState>>();
            foreach (var key in dictionary.Keys)
            {
                result[key] = dictionary[key].Select(f).ToList();
            }

            return result;
        }

        private static Func<IState, IState> GetStateWrapper(bool fromRight)
        {
            return state => new StateWithBit(fromRight, state);
        }

        internal abstract class BaseNfa : INfa<Symbol>
        {
            public BaseNfa()
            {
                this.Start = new StateBase(false);
                this.Accept = new StateBase(true);
            }

            public IState Start { get; }

            public IState Accept { get; }

            public IState StartingState()
            {
                return this.Start;
            }

            public virtual IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> StartTransitions()
            {
                return new Dictionary<Symbol, IReadOnlyCollection<IState>>();
            }

            public virtual IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> InnerTransitions(StateWithBit state)
            {
                return new Dictionary<Symbol, IReadOnlyCollection<IState>>();
            }

            public IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> Transitions(IState state)
            {
                if (state.Equals(this.Start))
                {
                    return this.StartTransitions();
                }
                else if (state.Equals(this.Accept))
                {
                    return new Dictionary<Symbol, IReadOnlyCollection<IState>>();
                }
                else if (state is StateWithBit)
                {
                    return this.InnerTransitions((StateWithBit)state);
                }
                else
                {
                    throw new ArgumentException(InvalidStateMessage);
                }
            }

            public virtual IReadOnlyCollection<IState> StartEpsilonTransitions()
            {
                return new List<IState>();
            }

            public virtual IReadOnlyCollection<IState> InnerEpsilonTransitions(StateWithBit state)
            {
                return new List<IState>();
            }

            public IReadOnlyCollection<IState> EpsilonTransitions(IState state)
            {
                if (state.Equals(this.Start))
                {
                    return this.StartEpsilonTransitions();
                }
                else if (state.Equals(this.Accept))
                {
                    return new List<IState>();
                }
                else if (state is StateWithBit)
                {
                    return this.InnerEpsilonTransitions((StateWithBit)state);
                }
                else
                {
                    throw new ArgumentException(InvalidStateMessage);
                }
            }

            public bool IsAccepting(IState state)
            {
                return state.Equals(this.Accept);
            }
        }

        internal class AtomicNfa : BaseNfa
        {
            public AtomicNfa(Symbol value)
            {
                this.Value = value;
            }

            public Symbol Value { get; }

            public override IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> StartTransitions()
            {
                return new Dictionary<Symbol, IReadOnlyCollection<IState>>() { { this.Value, new List<IState>() { this.Accept } } };
            }
        }

        internal class ConcatNfa : BaseNfa
        {
            public ConcatNfa(INfa<Symbol> left, INfa<Symbol> right)
            {
                this.Left = left;
                this.Right = right;
            }

            public INfa<Symbol> Left { get; }

            public INfa<Symbol> Right { get; }

            public override IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> InnerTransitions(StateWithBit state)
            {
                var transitions = state.FromRight ? this.Right.Transitions(state.InternalState) : this.Left.Transitions(state.InternalState);
                return MapDictionary(transitions, GetStateWrapper(state.FromRight));
            }

            public override IReadOnlyCollection<IState> StartEpsilonTransitions()
            {
                return new List<IState>() { new StateWithBit(false, this.Left.StartingState()) };
            }

            public override IReadOnlyCollection<IState> InnerEpsilonTransitions(StateWithBit state)
            {
                if (state.FromRight)
                {
                    if (this.Right.IsAccepting(state.InternalState))
                    {
                        return new List<IState>() { this.Accept };
                    }
                    else
                    {
                        return this.Right.EpsilonTransitions(state.InternalState).Select(GetStateWrapper(true)).ToList();
                    }
                }
                else
                {
                    if (this.Left.IsAccepting(state.InternalState))
                    {
                        return new List<IState>() { new StateWithBit(true, this.Right.StartingState()) };
                    }
                    else
                    {
                        return this.Left.EpsilonTransitions(state.InternalState).Select(GetStateWrapper(false)).ToList();
                    }
                }
            }
        }

        internal class EmptyNfa : BaseNfa
        {
        }

        internal class EpsilonNfa : BaseNfa
        {
            public override IReadOnlyCollection<IState> StartEpsilonTransitions()
            {
                return new List<IState>() { this.Accept };
            }
        }

        internal class StarNfa : BaseNfa
        {
            public StarNfa(INfa<Symbol> inner)
            {
                this.Inner = inner;
            }

            public INfa<Symbol> Inner { get; }

            public override IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> InnerTransitions(StateWithBit state)
            {
                return MapDictionary(this.Inner.Transitions(state.InternalState), GetStateWrapper(false));
            }

            public override IReadOnlyCollection<IState> StartEpsilonTransitions()
            {
                return new List<IState>() { this.Accept, new StateWithBit(false, this.Inner.StartingState()) };
            }

            public override IReadOnlyCollection<IState> InnerEpsilonTransitions(StateWithBit state)
            {
                if (this.Inner.IsAccepting(state.InternalState))
                {
                    return new List<IState>() { this.Accept, new StateWithBit(false, this.Inner.StartingState()) };
                }
                else
                {
                    return this.Inner.EpsilonTransitions(state.InternalState).Select(GetStateWrapper(false)).ToList();
                }
            }
        }

        internal class SumNfa : BaseNfa
        {
            public SumNfa(INfa<Symbol> left, INfa<Symbol> right)
            {
                this.Left = left;
                this.Right = right;
            }

            public INfa<Symbol> Left { get; }

            public INfa<Symbol> Right { get; }

            public override IReadOnlyDictionary<Symbol, IReadOnlyCollection<IState>> InnerTransitions(StateWithBit state)
            {
                var transitions = state.FromRight ? this.Right.Transitions(state.InternalState) : this.Left.Transitions(state.InternalState);
                return MapDictionary(transitions, GetStateWrapper(state.FromRight));
            }

            public override IReadOnlyCollection<IState> StartEpsilonTransitions()
            {
                return new List<IState>()
                {
                    new StateWithBit(false, this.Left.StartingState()),
                    new StateWithBit(true, this.Right.StartingState())
                };
            }

            public override IReadOnlyCollection<IState> InnerEpsilonTransitions(StateWithBit state)
            {
                if (state.FromRight)
                {
                    if (this.Right.IsAccepting(state.InternalState))
                    {
                        return new List<IState>() { this.Accept };
                    }
                    else
                    {
                        return this.Right.EpsilonTransitions(state.InternalState).Select(GetStateWrapper(true)).ToList();
                    }
                }
                else
                {
                    if (this.Left.IsAccepting(state.InternalState))
                    {
                        return new List<IState>() { this.Accept };
                    }
                    else
                    {
                        return this.Left.EpsilonTransitions(state.InternalState).Select(GetStateWrapper(false)).ToList();
                    }
                }
            }
        }

        internal class StateWithBit : IState
        {
            public StateWithBit(bool fromRight, IState internalState)
            {
                this.FromRight = fromRight;
                this.InternalState = internalState;
            }

            public bool FromRight { get; }

            public IState InternalState { get; }

            public override bool Equals(object other)
            {
                if (!(other is StateWithBit))
                {
                    return false;
                }

                var otherStateWithBit = (StateWithBit)other;
                return this.FromRight == otherStateWithBit.FromRight && this.InternalState.Equals(otherStateWithBit.InternalState);
            }

            public bool Equals(IState state)
            {
                return this.Equals((object)state);
            }

            public override int GetHashCode()
            {
                return Tuple.Create(this.FromRight, this.InternalState).GetHashCode();
            }
        }

        internal class StateBase : IState
        {
            public StateBase(bool accepting)
            {
                this.Accepting = accepting;
            }

            public bool Accepting { get; }

            public override bool Equals(object other)
            {
                if (!(other is StateBase))
                {
                    return false;
                }

                var otherStateBase = (StateBase)other;
                return this.Accepting == otherStateBase.Accepting;
            }

            public bool Equals(IState state)
            {
                return this.Equals((object)state);
            }

            public override int GetHashCode()
            {
                return this.Accepting.GetHashCode();
            }
        }
    }
}
