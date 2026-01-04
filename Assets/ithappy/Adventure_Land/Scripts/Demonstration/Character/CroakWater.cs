using System;
using System.Collections.Generic;
using UnityEngine;

namespace ithappy.Adventure_Land
{
    public class CroakWater : CharacterBase
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
                    typeof(CroakWaterBehavior), new CroakWaterBehavior(this, _movement, _targetPoints)
                },
            };

            _states[typeof(CroakWaterBehavior)].SetStatesToTransition(new List<CharacterStateBase>());

            TransitionToState(typeof(CroakWaterBehavior));
        }
    }
}
