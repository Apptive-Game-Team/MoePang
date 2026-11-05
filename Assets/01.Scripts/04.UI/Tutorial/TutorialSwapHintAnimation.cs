using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using _01.Scripts._01.ThreeMatch;

// 각 교환 안내에 사용하는 두 타일의 밝기, 외곽 글로우, 크기를 관리합니다.
public sealed class TutorialSwapHintAnimation
{
    private struct Target
    {
        public Transform transform;
        public Vector3 scale;
        public Material material;
        public float highlight;
        public Color outlineColor;
        public bool hasHighlight;
        public bool hasOutlineColor;
    }

    private static readonly int HighlightId = Shader.PropertyToID("_Highlight");
    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private readonly Target[] targets = new Target[2];
    private Sequence sequence;

    public void Play(NormalPuzzleObject first, NormalPuzzleObject second,
        float brightness, float scaleAmount, float pulseDuration, float pulseGap, float repeatDelay,
        Color glowColor)
    {
        Stop();
        if (first == null || second == null)
            return;

        targets[0] = Capture(first);
        targets[1] = Capture(second);
        pulseDuration = Mathf.Max(0.05f, pulseDuration);

        // 전투 시간이 0인 조작 대기 중에도 힌트는 재생합니다.
        sequence = DOTween.Sequence().SetUpdate(true).SetLoops(-1, LoopType.Restart);
        for (int pulse = 0; pulse < 2; pulse++)
        {
            float startTime = sequence.Duration(false);
            foreach (Target target in targets)
            {
                Vector3 enlarged = new Vector3(
                    target.scale.x * (1f + Mathf.Max(0f, scaleAmount)),
                    target.scale.y * (1f + Mathf.Max(0f, scaleAmount)), target.scale.z);

                sequence.Insert(startTime, target.transform.DOScale(enlarged, pulseDuration)
                    .SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo));

                if (target.hasHighlight)
                {
                    float peak = Mathf.Lerp(target.highlight, 1f, Mathf.Clamp01(brightness));
                    sequence.Insert(startTime, target.material.DOFloat(peak, HighlightId, pulseDuration)
                        .SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo));
                }

                if (target.hasOutlineColor)
                {
                    sequence.Insert(startTime, target.material.DOColor(glowColor, OutlineColorId, pulseDuration)
                        .SetEase(Ease.InOutSine).SetLoops(2, LoopType.Yoyo));
                }
            }

            if (pulse == 0)
                sequence.AppendInterval(Mathf.Max(0f, pulseGap));
        }
        sequence.AppendInterval(Mathf.Max(0f, repeatDelay));
    }

    private static Target Capture(NormalPuzzleObject puzzle)
    {
        Image image = puzzle.GetComponent<Image>();
        Material material = image != null ? image.material : null;
        bool hasHighlight = material != null && material.HasProperty(HighlightId);
        bool hasOutlineColor = material != null && material.HasProperty(OutlineColorId);

        return new Target
        {
            transform = puzzle.transform,
            scale = puzzle.transform.localScale,
            material = material,
            hasHighlight = hasHighlight,
            hasOutlineColor = hasOutlineColor,
            highlight = hasHighlight ? material.GetFloat(HighlightId) : 0f,
            outlineColor = hasOutlineColor ? material.GetColor(OutlineColorId) : Color.clear
        };
    }

    public void Stop()
    {
        // 이미 멈춘 힌트가 실제 매치 애니메이션의 크기를 덮어쓰지 않도록 합니다.
        if (sequence == null)
            return;

        sequence.Kill(false);
        sequence = null;
        for (int i = 0; i < targets.Length; i++)
        {
            Target target = targets[i];
            if (target.transform != null)
                target.transform.localScale = target.scale;
            if (target.material != null)
            {
                if (target.hasHighlight)
                    target.material.SetFloat(HighlightId, target.highlight);
                if (target.hasOutlineColor)
                    target.material.SetColor(OutlineColorId, target.outlineColor);
            }
            targets[i] = default;
        }
    }
}
