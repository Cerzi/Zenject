using System;
using ModestTree;
using UnityEngine;

namespace Zenject.SpaceFighter
{
    public class EnemyStateFollow : IEnemyState
    {
        readonly EnemyCommonSettings _commonSettings;
        readonly Settings _settings;
        readonly EnemyTunables _tunables;
        readonly EnemyStateManager _stateManager;
        readonly Enemy _enemy;
        readonly PlayerFacade _player;

        bool _strafeRight;
        float _lastStrafeChangeTime;

        public EnemyStateFollow(
            PlayerFacade player,
            Enemy enemy,
            EnemyStateManager stateManager,
            EnemyTunables tunables,
            Settings settings,
            EnemyCommonSettings commonSettings)
        {
            _commonSettings = commonSettings;
            _settings = settings;
            _tunables = tunables;
            _stateManager = stateManager;
            _enemy = enemy;
            _player = player;
        }

        public void EnterState()
        {
            _strafeRight = UnityEngine.Random.Range(0, 1) == 0;
            _lastStrafeChangeTime = Time.realtimeSinceStartup;
        }

        public void ExitState()
        {
        }

        public void Update()
        {
            if (_player.IsDead)
            {
                _stateManager.ChangeState(EnemyStates.Idle);
                return;
            }

            var distanceToPlayer = (_player.Position - _enemy.Position).magnitude;

            Assert.That(_settings.TeleportDistance > _settings.TeleportNewDistance);

            // If they are far enough away, just teleport them to the other side of the player
            // This is good because otherwise the best strategy is just to keep running away and shooting
            // and it gets boring
            if (distanceToPlayer > _settings.TeleportDistance)
            {
                var playerDir = (_player.Position - _enemy.Position).normalized;
                _enemy.Position = _player.Position + playerDir * _settings.TeleportNewDistance;
            }

            // Always look towards the player
            _enemy.DesiredLookDir = (_player.Position - _enemy.Position).normalized;

            // Strafe back and forth over the given interval
            // This helps avoiding being too easy a target
            if (Time.realtimeSinceStartup - _lastStrafeChangeTime > _settings.StrafeChangeInterval)
            {
                _lastStrafeChangeTime = Time.realtimeSinceStartup;
                _strafeRight = !_strafeRight;
            }

            if (distanceToPlayer < _commonSettings.AttackDistance)
            {
                _stateManager.ChangeState(EnemyStates.Attack);
            }
        }

        public void FixedUpdate()
        {
            MoveTowardsPlayer();
            Strafe();
        }

        void Strafe()
        {
            // Strafe to avoid getting hit too easily
            if (_strafeRight)
            {
                _enemy.AddForce(_enemy.RightDir * _settings.StrafeMultiplier * _tunables.Speed);
            }
            else
            {
                _enemy.AddForce(-_enemy.RightDir * _settings.StrafeMultiplier * _tunables.Speed);
            }
        }

        void MoveTowardsPlayer()
        {
            var playerDir = (_player.Position - _enemy.Position).normalized;

            _enemy.AddForce(playerDir * _tunables.Speed);
        }

        [Serializable]
        public class Settings
        {
            public float StrafeMultiplier;
            public float StrafeChangeInterval;
            public float TeleportDistance;
            public float TeleportNewDistance;
        }
    }
}

