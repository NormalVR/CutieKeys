using UnityEngine;
using System.Collections;
using UnityEngine.XR;


namespace Normal.UI {
    [RequireComponent(typeof(Rigidbody))]
    public class KeyboardMallet : MonoBehaviour {
        [SerializeField]
        private Transform _trackedObject;
        [SerializeField]
        private XRNode xRNode;

        [SerializeField]
        private Transform _head;

        [HideInInspector]
        public  Vector3    malletHeadPosition;
        private Vector3   _newMalletHeadPosition;

        // Internal
        [HideInInspector]
        public Keyboard _keyboard;

        void Awake() {
            // Configure the rigidbody
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity  = false;
            rigidbody.isKinematic = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        // Position this object at the center of the controller that we're tracking.
        void PositionGeometry(Vector3 position, Quaternion rotation) {
            transform.position = position;
            transform.rotation = rotation;
        }

        // Mallet collision. Check if we've hit a keyboard key or not.
        void OnTriggerEnter(Collider other) {
            if (_keyboard == null) {
                Debug.LogError("Huh, I, this keyboard mallet, have struck something. However, I am not the child of a keyboard. A lost soul. It pains me to ignore this collision event. What does it mean? Who was it meant for? Unfortunately I am given no choice.");
                return;
            }

            Rigidbody keyRigidbody = other.attachedRigidbody;
            if (keyRigidbody == null)
                return;

            KeyboardKey key = keyRigidbody.GetComponent<KeyboardKey>();
            if (key != null) {
                if (key.IsMalletHeadInFrontOfKey(this)) {
                    _keyboard._MalletStruckKeyboardKey(this, key);

                    TriggerHapticPulse();
                }
            } else {
                // Trigger haptic pulse (originally I wanted to limit this to just key strikes, but I guess haptics make sense if you hit anything...)
                TriggerHapticPulse();
            }
        }

        void Update() {
            // I want the value from the previous frame.
            malletHeadPosition = _newMalletHeadPosition;
            _newMalletHeadPosition = _head.transform.position;

            Vector3 worldPosition = _trackedObject.position;
            Quaternion worldRotation = _trackedObject.rotation;
            PositionGeometry(worldPosition, worldRotation);
        }

        // Haptic pulse
        void TriggerHapticPulse() {
            var device = InputDevices.GetDeviceAtXRNode(xRNode);
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    float amplitude = 0.5f;
                    float duration = 0.1f;
                    device.SendHapticImpulse(channel, amplitude, duration);
                }
            }
        }
    }
}
