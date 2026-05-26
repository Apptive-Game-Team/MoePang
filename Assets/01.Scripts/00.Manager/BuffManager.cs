using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _01.Scripts._00.Manager
{
    public enum StatType { MoveSpeed, AttackSpeed, AttackDamage }

    public class BuffManager : MonoBehaviour
    {
        public static BuffManager Instance { get; private set; }

        private Dictionary<StatType, float> _allyMultiplier = new() {
            { StatType.MoveSpeed, 1f },
            { StatType.AttackSpeed, 1f },
            { StatType.AttackDamage, 1f }
        };
        
        private Dictionary<StatType, float> _enemyMultiplier = new() {
            { StatType.MoveSpeed, 1f },
            { StatType.AttackSpeed, 1f },
            { StatType.AttackDamage, 1f }
        };
        
        private Dictionary<StatType, Coroutine> _allyCoroutines = new();
        private Dictionary<StatType, Coroutine> _enemyCoroutines = new();

        private readonly HashSet<FriendlyUnit> _allies = new();
        private readonly HashSet<EnemyUnit> _enemies = new();

        private void Awake() => Instance = this;

        public void RegisterUnit(Unit unit)
        {
            if (unit is FriendlyUnit fu)
            {
                if (_allies.Add(fu))
                {
                    ApplyAllyMultiplier(fu);
                }
            }
            else if (unit is EnemyUnit eu)
            {
                if (_enemies.Add(eu))
                {
                    ApplyEnemyMultiplier(eu);
                }
            }
        }

        private void ApplyAllyMultiplier(Unit unit)
        {
            foreach (var stat in _allyMultiplier)
            {
                unit.OnStatChanged(stat.Key, stat.Value);
            }
        }
        
        private void ApplyEnemyMultiplier(Unit unit)
        {
            foreach (var stat in _enemyMultiplier)
            {
                unit.OnStatChanged(stat.Key, stat.Value);
            }
        }

        public void ApplyAllyBuff(StatType type, float multiplier, float duration)
        {
            if (_allyCoroutines.ContainsKey(type) && _allyCoroutines[type] != null)
            {
                StopCoroutine(_allyCoroutines[type]);
            }

            _allyCoroutines[type] = StartCoroutine(AllyBuffRoutine(type, multiplier, duration));
        }

        private IEnumerator AllyBuffRoutine(StatType type, float mul, float duration)
        {
            _allyMultiplier[type] = mul;
            foreach (FriendlyUnit unit in _allies)
            {
                unit.OnStatChanged(type, mul);
            }

            yield return new WaitForSeconds(duration);

            _allyMultiplier[type] = 1f;
            foreach (FriendlyUnit unit in _allies)
            {
                unit.OnStatChanged(type, 1f);
                unit.RestoreStats();
            }

            _allyCoroutines[type] = null;
        }

        public void ApplyEnemyBuff(StatType type, float multiplier, float duration)
        {
            if (_enemyCoroutines.ContainsKey(type) && _enemyCoroutines[type] != null)
            {
                StopCoroutine(_enemyCoroutines[type]);
            }

            _enemyCoroutines[type] = StartCoroutine(EnemyBuffRoutine(type, multiplier, duration));
        }

        private IEnumerator EnemyBuffRoutine(StatType type, float mul, float duration)
        {
            _enemyMultiplier[type] = mul;
            foreach (EnemyUnit unit in _enemies)
            {
                unit.OnStatChanged(type, mul);
            }

            yield return new WaitForSeconds(duration);

            _enemyMultiplier[type] = 1f;
            foreach (EnemyUnit unit in _enemies)
            {
                unit.OnStatChanged(type, 1f);
                unit.RestoreStats();
            }

            _enemyCoroutines[type] = null;
        }
    }
}