using UnityEngine;

namespace ManusVR
{
    public class MVR_ColliderCopy : MonoBehaviour
    {
        public CapsuleCollider referenceCollider;
        public bool isTrigger = true;
        [Range(-0.3f, 0.3f)]
        public float increasePercentage = 0.05f;

        protected CapsuleCollider createdCollider;

        private bool debugColliderCopy = true;

        protected virtual void Awake()
        {
            referenceCollider = referenceCollider != null ? referenceCollider : GetComponent<CapsuleCollider>();

            if (referenceCollider && !createdCollider)
            {
                createdCollider = CopyCapsuleCollider(referenceCollider, gameObject);
                createdCollider.isTrigger = isTrigger;

                IncreaseColliderSize(createdCollider, 1 + increasePercentage);
            }
        }

        protected virtual void IncreaseColliderSize(CapsuleCollider collider, float percentage)
        {
            collider.radius *= percentage;
            collider.height *= percentage;
        }

        CapsuleCollider CopyCapsuleCollider(CapsuleCollider referenceCollider, GameObject destination)
        {
            CapsuleCollider newCollider = destination.AddComponent<CapsuleCollider>();

            newCollider.center = referenceCollider.center;
            newCollider.radius = referenceCollider.radius;
            newCollider.height = referenceCollider.height;
            newCollider.direction = referenceCollider.direction;

            return newCollider;
        }

        private void Log(string log)
        {
            if (debugColliderCopy)
            {
                Debug.Log(log);
            }
        }
    }
}
