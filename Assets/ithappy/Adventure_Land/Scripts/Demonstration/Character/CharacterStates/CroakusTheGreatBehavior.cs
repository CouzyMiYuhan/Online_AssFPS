using UnityEngine;

namespace ithappy.Adventure_Land
{
    public class CroakusTheGreatBehavior : CharacterStateBase
    {
        private float _angryCooldown = 10f;
        private float _currentAngryCooldown;
        
        public CroakusTheGreatBehavior(CharacterBase context) : base(context)
        {
        }

        public override void Update()
        {
            _currentAngryCooldown -= Time.deltaTime;
            if (_currentAngryCooldown <= 0)
            {
                _currentAngryCooldown = _angryCooldown;
                CharacterBase.CharacterAnimator.ChangeIdle();
            }
        }

        public override void Exit()
        {
            
        }

        public override bool ShouldEnter()
        {
            return true;
        }
    }
}
