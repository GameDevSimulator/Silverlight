using Assets.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BatteryIndicator : Singleton<BatteryIndicator>
    {
        [Range(0f, 1f)]
        public float Value = 1f;

        public Gradient ValueGradient;

        private RectTransform _mask;
        private CanvasGroup _canvasGroup;
        private Vector2 _initialMaskSize = Vector2.zero;
        private Image _fill;

        void Start ()
        {
            _mask = transform.Find("Mask") as RectTransform;
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_mask != null)
            {
                _initialMaskSize = _mask.sizeDelta;
            }

            var fillOjb = transform.FindChild("Mask/Fill");
            if (fillOjb != null)
            {
                _fill = fillOjb.GetComponent<Image>();
            }
        }
	
        void Update ()
        {
            if (_mask != null)
            {
                _mask.sizeDelta = new Vector2(_initialMaskSize.x, _initialMaskSize.y * (Value + 0.01f));
                if (Value > 0.25f)
                {
                    _canvasGroup.alpha = 1f;
                }
                else
                {
                    _canvasGroup.alpha = Value * 4f;
                }

                if (_fill != null)
                {
                    _fill.color = ValueGradient.Evaluate(Value);
                }
            }
        }
    }
}
