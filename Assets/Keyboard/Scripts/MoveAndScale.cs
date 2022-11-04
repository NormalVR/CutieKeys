using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;

namespace Normal.UI {
    public class MoveAndScale : MonoBehaviour {
        public TrackedPoseDriver leftController;
        public TrackedPoseDriver rightController;
    
        enum State {
            Idle,
            Move,
            Scale,
        }
    
        private State _state = State.Idle;

        // Move
        private TrackedPoseDriver _moveController;
        private TrackedPoseDriver _idleController;
        private Vector3               _positionOffsetFromController;
        private Quaternion            _rotationOffsetFromController;

        // Scale
        private Vector3    _positionOffset;
        private Quaternion _rotationOffset;
        private Vector3    _scaleOffset;

        // Animation
        private Vector3    _targetPosition;
        private Quaternion _targetRotation;
        private Vector3    _targetScale;

        void OnEnable() {
            _targetPosition = transform.position;
            _targetRotation = transform.rotation;
            _targetScale    = transform.localScale;
        }

        void Update() {
            HandleGripState();

            transform.position   =     Vector3.Lerp(transform.position,   _targetPosition, Time.deltaTime * 20.0f);
            transform.rotation   = Quaternion.Slerp(transform.rotation,   _targetRotation, Time.deltaTime * 20.0f);
            transform.localScale =     Vector3.Lerp(transform.localScale, _targetScale,    Time.deltaTime * 20.0f);
        }

        void HandleGripState() {
            // Run the correct update operation for each state. Check whether the current state is valid.
            if (_state == State.Idle) {
                BeginMoveOrScaleIfNeeded();

            } else if (_state == State.Move) {
                bool moveGrip = GetGrip(_moveController);
                bool idleGrip = GetGrip(_idleController);

                // Do we need to transition to scaling? Are we still moving?
                if (moveGrip && !idleGrip) {
                    // Continue moving
                    Move();
                } else {
                    // Stop moving
                    EndMove();

                    // Begin scaling or begin moving with the opposite hand if needed
                    BeginMoveOrScaleIfNeeded();
                }

            } else if (_state == State.Scale) {
                bool  leftGrip = GetGrip( leftController);
                bool rightGrip = GetGrip(rightController);

                // Do we need to transition to moving? Are we still scaling?
                if (leftGrip && rightGrip) {
                    // Continue scaling
                    Scale();
                } else {
                    // Stop scaling
                    EndScale();

                    // Begin moving if needed
                    BeginMoveOrScaleIfNeeded();
                }
            }
        }

        // Move / Scale state change
        void BeginMoveOrScaleIfNeeded() {
            bool  leftGrip = GetGrip( leftController);
            bool rightGrip = GetGrip(rightController);

            if (leftGrip && rightGrip)
                BeginScale();
            else if (leftGrip)
                BeginMove(leftController, rightController);
            else if (rightGrip)
                // Begin moving with the right controller.
                BeginMove(rightController, leftController); 
        }
    
        // Move
        void BeginMove(TrackedPoseDriver moveController, TrackedPoseDriver idleController) {
            _state = State.Move;
            _moveController = moveController;
            _idleController = idleController;

            // Save current position / rotation offset
            _positionOffsetFromController = _moveController.transform.InverseTransformPoint(transform.position);
            _rotationOffsetFromController = Quaternion.Inverse(_moveController.transform.rotation) * transform.rotation;

            Move();
        }

        void Move() {
            if (_state != State.Move)
                return;

            // Take the current orientation of the controller, ask it to convert our offsets from earlier to world space.
            // This will apply any position/rotation changes that have happened since we started grabbing.
            // If the hand hasn't moved at all, we should end up positioning the object exactly where it started.
            _targetPosition = _moveController.transform.TransformPoint(_positionOffsetFromController);
            _targetRotation = _moveController.transform.rotation * _rotationOffsetFromController;
        }

        void EndMove() {
            _state = State.Idle;
            _moveController = null;
            _idleController = null;
            _positionOffsetFromController = Vector3.zero;
            _rotationOffsetFromController = Quaternion.identity;
        }

        // Scale
        void BeginScale() {
            _state = State.Scale;

            // Create a matrix for the centroid of the two controllers.
            Matrix4x4 centroid = GetControllerCentroidTransform();

            // Get the position/rotation/scale in local space of the centroid matrix.
            _positionOffset = centroid.inverse.MultiplyPoint(transform.position);
            _rotationOffset = Quaternion.Inverse(GetControllerOrientation()) * transform.rotation;
            _scaleOffset    = 1.0f/GetControllerDistance() * transform.localScale;
        }

        void Scale() {
            if (_state != State.Scale)
                return;

            // Use it to transform the offsets calculated at the start of the scale operation.
            _targetPosition = GetControllerCentroidTransform().MultiplyPoint(_positionOffset);
            _targetRotation = GetControllerOrientation() * _rotationOffset;
            _targetScale    = GetControllerDistance()    * _scaleOffset;
        }

        void EndScale() {

        }

        // OpenXR
        bool GetGrip(TrackedPoseDriver tpd) {
            var poseSource = tpd.poseSource;
            switch (poseSource)
            {
                case TrackedPoseDriver.TrackedPose.LeftPose:
                    if(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out var leftPressed))
                    {
                        return leftPressed;
                    }
                    break;
                case TrackedPoseDriver.TrackedPose.RightPose:
                    if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out var rightPressed))
                    {
                        return rightPressed;
                    }
                    break;
                default:
                    break;
            }
  
            return false;
        }

        Vector3 GetControllerCentroid() {
            return (leftController.transform.position + rightController.transform.position) / 2.0f;
        }

        Quaternion GetControllerOrientation() {
            Vector3 direction = rightController.transform.position - leftController.transform.position;
            Vector3 up = (leftController.transform.forward + rightController.transform.forward) / 2.0f;
            return Quaternion.LookRotation(direction, up);
        }

        float GetControllerDistance() {
            return Vector3.Distance(leftController.transform.position, rightController.transform.position);
        }

        Matrix4x4 GetControllerCentroidTransform() {
            Vector3    position = GetControllerCentroid();
            Quaternion rotation = GetControllerOrientation();
            float      scale    = GetControllerDistance();
            Matrix4x4  centroid = Matrix4x4.TRS(position, rotation, new Vector3(scale, scale, scale));

            return centroid;
        }
    }
}
