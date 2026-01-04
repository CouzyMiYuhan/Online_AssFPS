using System.Collections.Generic;
using UnityEngine;

namespace ithappy.Adventure_Land
{
    public class BloomjawBehavior : CharacterStateBase
    {
        private Vector2 _moveCooldownMinMax = new Vector2(25f, 45f);
        private float _currentMoveCooldown;
        private List<Transform> _targetPoints;
        private int _currentPointIndex = 0;
        private bool _isMoving;
        private MovementBase _movement;
        
        public BloomjawBehavior(CharacterBase context, MovementBase movement, List<Transform> targetPoints) : base(context)
        {
            _targetPoints =  targetPoints;
            _movement =  movement;
            
            _currentMoveCooldown = Random.Range(_moveCooldownMinMax.x, _moveCooldownMinMax.y);
        }

        public override void Update()
        {
            if (_isMoving)
            {
                return;
            }
            
            _currentMoveCooldown -= Time.deltaTime;
            if (_currentMoveCooldown <= 0)
            {
                _isMoving = true;
                _currentMoveCooldown = Random.Range(_moveCooldownMinMax.x, _moveCooldownMinMax.y);
                Move();
            }
        }

        private void Move()
        {
            _movement.NavMeshMoveToTarget(_targetPoints[_currentPointIndex].position, (isReached) =>
            {
                _isMoving = false;
                _currentPointIndex++;
                if (_currentPointIndex >= _targetPoints.Count)
                {
                    _currentPointIndex = 0;
                }
            });
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
