using System;
using System.Collections.Generic;
using UnityEngine;

namespace ithappy.Adventure_Land
{
    public class Bloomjaw : CharacterBase
    {
        [SerializeField] private List<Transform> _targetPoints;
        
        protected Dictionary<Type, CharacterStateBase> _states;

        protected override Dictionary<Type, CharacterStateBase> States => _states;

        public override void Initialize()
        {
            base.Initialize();

            _states = new Dictionary<Type, CharacterStateBase>
            {
                {
                    typeof(BloomjawBehavior), new BloomjawBehavior(this, _movement, _targetPoints)
                },
            };

            _states[typeof(BloomjawBehavior)].SetStatesToTransition(new List<CharacterStateBase>());

            TransitionToState(typeof(BloomjawBehavior));
        }
    }
}
