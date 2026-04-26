using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class MainMenuUILoading : MonoBehaviour
{
    [Header("UI 组件")]
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private RectTransform menuPlatform;

    [Header("动画时间设置")]
    [Tooltip("黑幕渐隐时间")]
    [SerializeField] private float fadeDuration = 1.0f;
    [Tooltip("菜单掉落动画持续时间")]
    [SerializeField] private float dropDuration = 1.2f;
    [Tooltip("菜单掉落延迟时间——黑幕动画开始后多久进行菜单掉落")]
    [SerializeField] private float startDelay = 0.3f;

    [Header("位置配置")]
    [Tooltip("菜单最终位置")]
    [SerializeField] private Vector2 finalAnchoredPosition = new Vector2(-700f, -200f);
    [Tooltip("菜单掉落高度偏移")]
    [SerializeField] private float dropHeightOffset = 1500f;

    [Header("自定义 Bounce 设置")]
    [Tooltip("反弹高度倍率：1.0 是原版，0.5 是弹一半高，0 则不反弹")]
    [Range(0f, 2f)] 
    [SerializeField] private float bounceMultiplier = 0.18f;

    private void Awake()
    {
        // 1. 初始化黑幕为全黑，并挡住鼠标点击
        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 1f;
            fadeOverlay.blocksRaycasts = true;
        }

        // 2. 初始化平台到屏幕上方
        if (menuPlatform != null)
        {
            Vector2 startPos = finalAnchoredPosition;
            startPos.y += dropHeightOffset;
            menuPlatform.anchoredPosition = startPos;
        }
    }

    private void Start()
    {
        PlayIntroAnimation();
    }

    private void PlayIntroAnimation()
    {
        if (fadeOverlay == null || menuPlatform == null) return;

        // 【DOTween 核心：创建一个动画序列 (Sequence)】
        // Sequence 就像是一个视频剪辑的时间轴，可以精细控制什么时间播放什么动画
        Sequence introSeq = DOTween.Sequence();

        // 1. 插入渐隐动画：从第 0 秒开始，执行 DOFade(目标值, 时间)
        introSeq.Insert(0, fadeOverlay.DOFade(0f, fadeDuration));

        // 2. 插入下落动画：从第 startDelay 秒开始执行
        introSeq.Insert(startDelay, 
            menuPlatform.DOAnchorPos(finalAnchoredPosition, dropDuration)
                        .SetEase(CustomScaledBounce) // <--- 调用掉落回弹函数
        );

        // 3. 绑定动画结束后的回调 (OnComplete)
        introSeq.OnComplete(() =>
        {
            // 动画播放完毕后，开放鼠标点击，并彻底隐藏黑幕以优化性能
            fadeOverlay.blocksRaycasts = false;
            fadeOverlay.gameObject.SetActive(false);
        });
    }
    private float CustomScaledBounce(float time, float duration, float overshootOrAmplitude, float period)
    {
        // 1. 标准化时间 (0 到 1)
        float x = time / duration;
        float n1 = 7.5625f;
        float d1 = 2.75f;
        float y = 0f;

        // 2. 原始的 Robert Penner Bounce 公式
        if (x < 1f / d1) {
            y = n1 * x * x;
        } else if (x < 2f / d1) {
            y = n1 * (x -= 1.5f / d1) * x + 0.75f;
        } else if (x < 2.5f / d1) {
            y = n1 * (x -= 2.25f / d1) * x + 0.9375f;
        } else {
            y = n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }

        // 3. 拦截并缩放反弹高度
        // 如果动画进入了反弹阶段（即 x 越过了第一次砸地的时间点 1/2.75）
        if (time / duration > 1f / 2.75f)
        {
            // 在数学系中，Y=1 是地面。此时 Y 在半空中（比如 0.75）。
            // 它距离地面的真实高度是 (1 - y)
            float distanceToGround = 1f - y;
            
            // 将真实高度乘以你的系数，然后再从地面算回去
            y = 1f - (distanceToGround * bounceMultiplier);
        }

        return y;
    }
}