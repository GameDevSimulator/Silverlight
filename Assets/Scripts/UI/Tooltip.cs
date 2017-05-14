using Assets.Scripts.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Tooltip : Singleton<Tooltip>
    {
        public float FadeTime = 1f;
        private Text _text;
        private float _val = 0f;
        private bool _isShowing = false;
        private CanvasGroup _canvasGroup;
        
        void Start ()
        {
            _text = GetComponentInChildren<Text>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }
        
        void Update ()
        {
            if (_isShowing)
            {
                if (_val < FadeTime)
                {
                    _val += Time.deltaTime;
                }
                
            }
            else
            {
                if (_val > 0)
                {
                    _val -= Time.deltaTime;
                    if (_val < 0)
                    {
                        _val = 0;
                        _isShowing = false;
                    }
                }
            }

            _canvasGroup.alpha = Mathf.Clamp01(_val/FadeTime);
        }

        public void Show(string text)
        {
            if (_text != null)
            {
                _text.text = text;
                _isShowing = true;
            }
        }

        public void Hide()
        {
            _isShowing = false;
        }
    }
}
