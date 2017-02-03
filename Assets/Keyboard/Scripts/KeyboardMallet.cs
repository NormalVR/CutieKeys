using UnityEngine;
using System.Collections;
using Valve.VR;

namespace Normal.UI {
    [RequireComponent(typeof(Rigidbody))]
    public class KeyboardMallet : MonoBehaviour {
        [SerializeField]
        private SteamVR_TrackedObject _trackedObject;

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

        // Watch for new poses event from SteamVR.
        void OnEnable() {
            SteamVR_Events.NewPoses.Listen(OnNewPoses);
        }

        void OnDisable() {
            SteamVR_Events.NewPoses.Remove(OnNewPoses);
        }

        private void OnNewPoses(TrackedDevicePose_t[] poses) {
            if (_trackedObject.index == SteamVR_TrackedObject.EIndex.None)
                return;

            int i = (int)_trackedObject.index;
            if (poses.Length <= i)
                return;

            if (!poses[i].bDeviceIsConnected)
                return;

            if (!poses[i].bPoseIsValid)
                return;

            SteamVR_Utils.RigidTransform pose = new SteamVR_Utils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);
            Vector3    worldPosition = _trackedObject.transform.parent.TransformPoint(pose.pos);
            Quaternion worldRotation = _trackedObject.transform.parent.rotation * pose.rot;
            PositionGeometry(worldPosition, worldRotation);
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
        }

        // Haptic pulse
        void TriggerHapticPulse() {
            StartCoroutine(HapticPulse());
        }

        IEnumerator HapticPulse() {
            if (_trackedObject.index == SteamVR_TrackedObject.EIndex.None) {
                yield break;
            }

            SteamVR_Controller.Device device = SteamVR_Controller.Input((int)_trackedObject.index);
            // When I wrote this, two frames of vibration felt like a really solid hit without it feeling sloppy.
            // Also using WaitForEndOfFrame() to match my implementation for the Oculus Touch controllers. Not sure if this is ideal.
            device.TriggerHapticPulse(1500);
            yield return new WaitForEndOfFrame();
            device.TriggerHapticPulse(1500);
            yield return new WaitForEndOfFrame();
        }
    }
}
