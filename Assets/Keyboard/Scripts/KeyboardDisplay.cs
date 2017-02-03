using UnityEngine;
using UnityEngine.UI;

namespace Normal.UI {
    public class KeyboardDisplay : MonoBehaviour {
        [SerializeField]
        private Text _text;

        [SerializeField]
        private Keyboard _keyboard;
        public  Keyboard  keyboard { get { return _keyboard; } set { SetKeyboard(value); } }

        void Awake() {
            StartObservingKeyboard(_keyboard);
        }

        void OnDestroy() {
            StopObservingKeyboard(_keyboard);
        }

        void SetKeyboard(Keyboard keyboard) {
            if (keyboard == _keyboard)
                return;

            StopObservingKeyboard(_keyboard);
            StartObservingKeyboard(keyboard);

            _keyboard = keyboard;
        }

        void StartObservingKeyboard(Keyboard keyboard) {
            if (keyboard == null)
                return;

            keyboard.keyPressed += KeyPressed;
        }

        void StopObservingKeyboard(Keyboard keyboard) {
            if (keyboard == null)
                return;

            keyboard.keyPressed -= KeyPressed;
        }

        void KeyPressed(Keyboard keyboard, string keyPress) {
            string text = _text.text;

            if (keyPress == "\b") {
                // Backspace
                if (text.Length > 0)
                    text = text.Remove(text.Length - 1);
            } else {
                // Regular key press
                text += keyPress;
            }

            _text.text = text;
        }
    }
}
