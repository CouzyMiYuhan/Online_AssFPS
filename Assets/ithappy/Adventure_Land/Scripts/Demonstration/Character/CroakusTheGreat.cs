using System;
using System.Collections.Generic;

namespace ithappy.Adventure_Land
{
    public class CroakusTheGreat : CharacterBase
    {
        protected Dictionary<Type, CharacterStateBase> _states;

        protected override Dictionary<Type, CharacterStateBase> States => _states;

        public override void Initialize()
        {
            base.Initialize();

            _states = new Dictionary<Type, CharacterStateBase>
            {
                {
                    typeof(CroakusTheGreatBehavior), new CroakusTheGreatBehavior(this)
                },
            };

            _states[typeof(CroakusTheGreatBehavior)].SetStatesToTransition(new List<CharacterStateBase>());

            TransitionToState(typeof(CroakusTheGreatBehavior));
        }
    }
}
