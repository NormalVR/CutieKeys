using UnityEngine;
using UnityEngine.UI;

namespace Normal.UI {
    [ExecuteInEditMode]
    public class KeyboardKey : MonoBehaviour {
        public string character = "a";
        
        // These are overrides in case you need something different.
        public string displayCharacter      = null;
        public string shiftCharacter        = null;
        public string shiftDisplayCharacter = null;

        private bool _shift = false;
        public  bool  shift { get { return _shift; } set { SetShift(value); } }

        [SerializeField]
        private Text _text;

        [SerializeField]
        private Transform _geometry;
        private float     _position       = 0.0f;
        private float     _targetPosition = 0.0f;

        [SerializeField]
        private AudioSource _audioSource;

        // Internal
        [HideInInspector]
        public Keyboard _keyboard;

        void Awake() {
            // Configure the rigidbody
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity  = false;
            rigidbody.isKinematic = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            RefreshDisplayCharacter();
        }

        public bool IsMalletHeadInFrontOfKey(KeyboardMallet mallet) {
            Vector3 localMalletHeadPosition = transform.InverseTransformPoint(mallet.malletHeadPosition);

            return localMalletHeadPosition.y >= 0.0f;
        }

        public void KeyPressed() {
            _position = -0.1f;

            if (_audioSource != null) {
                if (_audioSource.isPlaying)
                    _audioSource.Stop();

                float scalePitch = 1.0f/(_keyboard.transform.lossyScale.x + 0.2f);
                float pitchVariance = Random.Range(0.95f, 1.05f);
                _audioSource.pitch = scalePitch * pitchVariance;
                _audioSource.Play();
            }
        }

        void SetShift(bool shift) {
            if (shift == _shift)
                return;

            _shift = shift;

            RefreshDisplayCharacter();
        }

        // Key animation
        void Update() {
            // Animate bounce
            _position = Mathf.Lerp(_position, _targetPosition, Time.deltaTime * 20.0f);

            // Set position
            Vector3 localPosition = _geometry.localPosition;
            localPosition.y = _position;
            _geometry.localPosition = localPosition;
        }

        public void RefreshDisplayCharacter() {
            _text.text = GetDisplayCharacter();
        }

        // Helper functions
        string GetDisplayCharacter() {
            // Start with the character
            string dc = character;
            if (dc == null)
                dc = "";

            // If we've got a display character, swap for that.
            if (displayCharacter != null && displayCharacter != "")
                dc = displayCharacter;

            // If we're in shift mode, check our shift overrides.
            if (_shift) {
                if (shiftDisplayCharacter != null && shiftDisplayCharacter != "")
                    dc = shiftDisplayCharacter;
                else if (shiftCharacter != null && shiftCharacter != "")
                    dc = shiftCharacter;
                else
                    dc = dc.ToUpper();
            }

            return dc;
        }

        public string GetCharacter() {
            if (shift) {
                if (shiftCharacter != null && shiftCharacter != "")
                    return shiftCharacter;
                else
                    return character.ToUpper();
            }

            return character;
        }
    }
}
