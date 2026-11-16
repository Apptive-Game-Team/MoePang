using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _01.Scripts._00.Manager
{
    public enum StatType { MoveSpeed, AttackSpeed, AttackDamage }
    
    public class Buff
    {
        public StatType StatType { get; private set; }
        public float Multiplier { get; private set; }
        public Coroutine Coroutine { get; set; }

        public Buff(StatType statType, float multiplier)
        {
            StatType = statType;
            Multiplier = multiplier;
        }
    }

    public class BuffManager : MonoBehaviour
    {
        public static BuffManager Instance { get; private set; }
        
        private List<Buff> _activeAllyBuffs = new();
        private List<Buff> _activeEnemyBuffs = new();

        private readonly HashSet<FriendlyUnit> _allies = new();
        private readonly HashSet<EnemyUnit> _enemies = new();
        
        private Coroutine _enemyHealCoroutine;

        private void Awake() => Instance = this;
        
        public void RegisterUnit(Unit unit)
        {
            switch (unit)
            {
                case FriendlyUnit fu:
                    _allies.Add(fu);
                    foreach (Buff buff in _activeAllyBuffs)
                    {
                        // 풀에서 다시 소환되면 기본 스탯이 초기화되므로 재적용한다.
                        fu.RemoveBuff(buff);
                        fu.AddBuff(buff);
                    }
                    break;
                case EnemyUnit eu:
                    _enemies.Add(eu);
                    foreach (Buff buff in _activeEnemyBuffs)
                    {
                        eu.RemoveBuff(buff);
                        eu.AddBuff(buff);
                    }
                    break;
            }
        }
        
        public Buff ApplyAllyBuff(StatType type, float multiplier, float duration)
        {
            Buff newBuff = new(type, multiplier);
            newBuff.Coroutine = StartCoroutine(AllyBuffRoutine(newBuff, duration));
            return newBuff;
        }

        private IEnumerator AllyBuffRoutine(Buff buff, float duration)
        {
            _activeAllyBuffs.Add(buff);
            
            foreach (FriendlyUnit unit in _allies.Where(unit => unit))
            {
                unit.AddBuff(buff);
            }

            yield return new WaitForSeconds(duration);
            
            _activeAllyBuffs.Remove(buff);
            
            foreach (FriendlyUnit unit in _allies.Where(unit => unit))
            {
                unit.RemoveBuff(buff);
            }
        }
        
        public Buff ApplyEnemyBuff(StatType type, float multiplier, float duration)
        {
            Buff newBuff = new(type, multiplier);
            newBuff.Coroutine = StartCoroutine(EnemyBuffRoutine(newBuff, duration));
            return newBuff;
        }

        public void RemoveBuff(Buff buff)
        {
            if (buff == null) return;
            if (buff.Coroutine != null) StopCoroutine(buff.Coroutine);

            if (_activeAllyBuffs.Remove(buff))
            {
                foreach (FriendlyUnit unit in _allies.Where(unit => unit))
                    unit.RemoveBuff(buff);
            }

            if (_activeEnemyBuffs.Remove(buff))
            {
                foreach (EnemyUnit unit in _enemies.Where(unit => unit))
                    unit.RemoveBuff(buff);
            }
        }

        private IEnumerator EnemyBuffRoutine(Buff buff, float duration)
        {
            _activeEnemyBuffs.Add(buff);

            foreach (EnemyUnit unit in _enemies.Where(unit => unit))
            {
                unit.AddBuff(buff);
            }

            yield return new WaitForSeconds(duration);

            _activeEnemyBuffs.Remove(buff);

            foreach (EnemyUnit unit in _enemies.Where(unit => unit))
            {
                unit.RemoveBuff(buff);
            }
        }
        
        public void StartEnemyHealOverTime(float healAmount, float interval)
        {
            if (_enemyHealCoroutine != null)
            {
                StopCoroutine(_enemyHealCoroutine);
            }

            _enemyHealCoroutine = StartCoroutine(EnemyHealOverTimeRoutine(healAmount, interval));
        }
        public void StopEnemyHealOverTime()
        {
            if (_enemyHealCoroutine == null) return;

            StopCoroutine(_enemyHealCoroutine);
            _enemyHealCoroutine = null;

            Debug.Log("Enemy heal over time stopped.");
        }
        
        private IEnumerator EnemyHealOverTimeRoutine(float healAmount, float interval)
        {
            Debug.Log($"Enemy heal over time started. HealAmount: {healAmount}, Interval: {interval}");

            while (true)
            {
                yield return new WaitForSeconds(interval);

                int healedCount = 0;

                foreach (EnemyUnit enemy in _enemies)
                {
                    if (enemy == null) continue;
                    if (!enemy.gameObject.activeInHierarchy) continue;

                    enemy.Heal(healAmount);
                    healedCount++;
                }

                Debug.Log($"Enemy heal over time applied. HealAmount: {healAmount}, HealedCount: {healedCount}");
            }
        }
    }
}
