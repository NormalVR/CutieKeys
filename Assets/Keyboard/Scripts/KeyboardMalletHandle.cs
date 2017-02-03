using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Normal.UI {
    public class KeyboardMalletHandle : MonoBehaviour {
        public float worldZScale = 0.15f;

        void LateUpdate() {
            // Counter the keyboard scale in the y-direction
            Vector3 localScale   = transform.localScale;
            localScale.z         = worldZScale / transform.parent.lossyScale.z;
            transform.localScale = localScale;

            // Fix position
            Vector3 localPosition   = transform.localPosition;
            localPosition.z         = -localScale.z/2.0f;
            transform.localPosition = localPosition;
        }
    }
}