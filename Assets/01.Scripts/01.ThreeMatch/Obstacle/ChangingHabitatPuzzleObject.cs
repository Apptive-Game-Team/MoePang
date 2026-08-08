using DG.Tweening;
using System;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

namespace _01.Scripts._01.ThreeMatch.Obstacle
{
    public class ChangingHabitatPuzzleObject : ObstaclePuzzleObject
    {
        [SerializeField] private float changingInterval;
        public GameObject[] normalPuzzlePrefabs;

        private static readonly int HighlightAlphaId = Shader.PropertyToID("_Highlight");
        private static readonly int EdgeHighlightAlphaId = Shader.PropertyToID("_EdgeHighlight");
        private PuzzleGenerator _puzzleGenerator;
        private Coroutine _changingCoroutine;
        private Image _image;
        private Material _material;
        
        public void InitialSetting(PuzzleGenerator generator, GameObject[] puzzlePrefabs)
        {
            _puzzleGenerator = generator;
            normalPuzzlePrefabs = puzzlePrefabs;
            _image = GetComponent<Image>();
            _material = new Material(_image.material);
            _image.material = _material;
            habitat = ((Habitat[])Enum.GetValues(typeof(Habitat)))[UnityEngine.Random.Range(0, Enum.GetValues(typeof(Habitat)).Length)];

            _changingCoroutine = StartCoroutine(ChangingCoroutine());
        }

        private IEnumerator ChangingCoroutine()
        {
            while (true)
            {
                ChangeHabitat();
                
                yield return new WaitForSeconds(changingInterval);
                
                yield return new WaitUntil(() => puzzleState == PuzzleState.Idle);
            }
        }

        private void ChangeHabitat()
        {
            Habitat type = _puzzleGenerator.GetRandomSafeHabitat(column, row, habitat);

            Image tileImage = normalPuzzlePrefabs[(int)type].GetComponent<Image>();
            
            _image.sprite = tileImage.sprite;

            habitat = type;
        }
        
        public Tween HighlightEffect()
        {
            DOTween.To(() => 0f, x => _material.SetFloat(EdgeHighlightAlphaId, x), 0f, 0.1f)
                .SetEase(Ease.OutCubic);
            
            return DOTween.To(() => 0f, x => _material.SetFloat(HighlightAlphaId, x), 1f, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutCubic);
        }

        private void OnDestroy()
        {
            StopCoroutine(_changingCoroutine);
        }
    }
}
