using System.Collections;
using UnityEngine;
using VRTK;

namespace ManusVR
{
    /// <summary>
    /// Provides a custom controller hand model with psuedo finger functionality and physical interaction.
    /// </summary>
    public class VRTK_MVRPhysicsAvatarHandController : VRTK_MVRAvatarHandController
    {
        [Header("Velocity Hand Movement")]
        [Range(0, 10f)]
        [Tooltip("The maximum amount of velocity of the hand when the hand is colliding with an object.")]
        public float MaxVelocityWhenColliding = 2f;
        [Range(0, 100f)]
        [Tooltip("The velocity of the hand movement will be multiplied by this amount.")]
        public float VelocityMovementMultiplier = 30f;

        [Header("Kinematic Hand Movement")]
        [Range(0, 0.2f)]
        [Tooltip("The maximum distance delta when the glove is moving kinematic.")]
        public float MaxDistanceDeltaWhenKinematic = 0.03f;
        [Range(0, 5f)]
        [Tooltip("The amount of seconds before the hand rigidbody will become kinematic after not colliding anymore.")]
        public float TimeBeforeTurningNonKinematic = 0.5f;
        [Range(0f, 2f)]
        [Tooltip("The maximum amount of distance between the target and the hand before it will disable collision.")]
        public float MaxDisconnectDistance = 0.3f;

        [Header("Joint Connection Settings")]
        [Tooltip("Connect the palm with a joint for the rotation of the palm.")]
        public bool CreateJointForRotation = true;
        [Tooltip("The amount of force that is required to break the rotation joint.")]
        public float BreakTorque = 10000;

        [Header("ManusVR Custom Settings")]
        [Tooltip("The rigidbody that is used to move the hand. If this is left blank if will search for a rigidbody that is attached to this gameobject.")]
        public Rigidbody rigidbodySelf;
        [Tooltip("The controller script alias that has the kinematic rigidbody attached to it. The joint will connect to the rigidbody that is attached to this controller. If this is left blank it will use the parent of the parent object.")]
        public Transform controllerScriptAlias;
        [Tooltip("The physics will move and rotate towards the given transform. If this is left blank the parent transform will be used.")]
        public Transform targetTransform;
        [Tooltip("The collision toggler will be used to enable and disable to collision of the hand. If this is left blank it will look for the script in the parent.")]
        public MVR_CollisionToggler collisionToggler;
        [Tooltip("The collision detector will be used to check if the hand is colliding with objects. If this is left blank it will look for the script in the parent.")]
        public MVR_CollisionDetector collisionDetector;

        protected ConfigurableJoint wristJoint;
        protected Coroutine kinematicRoutine;

        private bool debugController = false;

        /// <summary>
        /// The distance between the actual hand position and the target position.
        /// </summary>
        public virtual float DisconnectDistance => Vector3.Distance(targetTransform.position, transform.position);

        /// <summary>
        /// The angle between the actual hand rotation and the target rotation.
        /// </summary>
        public virtual float CurrentAngleDelta => Quaternion.Angle(targetTransform.rotation, transform.rotation);

        protected override void OnEnable()
        {
            base.OnEnable();
            rigidbodySelf = (rigidbodySelf != null ? rigidbodySelf : GetComponent<Rigidbody>());
            controllerScriptAlias = (controllerScriptAlias != null ? controllerScriptAlias : transform.parent.transform.parent);
            targetTransform = (targetTransform != null) ? targetTransform : transform.parent;

            collisionToggler = collisionToggler != null ? collisionToggler : GetComponentInParent<MVR_CollisionToggler>();
            collisionDetector = collisionDetector != null ? collisionDetector : GetComponentInParent<MVR_CollisionDetector>();

            StartCoroutine(UnParent());
            rigidbodySelf.isKinematic = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // Destroy wrist joint when the gameobject gets disabled.
            Destroy(wristJoint);
        }

        protected virtual IEnumerator UnParent()
        {
            // Wait until references are set before unparrenting the gameobject.
            while (ControllerReference.hand == SDK_BaseController.ControllerHand.None)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            transform.parent = null;
        }

        protected virtual void FixedUpdate()
        {
            if (targetTransform != null)
            {
                ChangeKinematicStateOfRigidbody(rigidbodySelf);
                MoveBodyToTransform(targetTransform, rigidbodySelf);
            }

            UpdateHandCollision();
        }

        protected virtual void UpdateHandCollision()
        {
            if (DisconnectDistance > MaxDisconnectDistance)
            {
                collisionToggler.EnableHandCollision(false);
            }
            else if (DisconnectDistance <= 0.01f && !collisionDetector.IsHandColliding)
            {
                collisionToggler.EnableHandCollision(true);
            }
        }

        protected virtual void ChangeKinematicStateOfRigidbody(Rigidbody body)
        {
            bool shouldBeKinematic = ShouldBodyBeKinematic(body);
            if (shouldBeKinematic && kinematicRoutine == null)
            {
                if (body.isKinematic)
                    return;

                kinematicRoutine = StartCoroutine(MakeBodyKinematic(body, TimeBeforeTurningNonKinematic));
            }
            else if (!shouldBeKinematic)
            {

                if (kinematicRoutine != null)
                {
                    StopCoroutine(kinematicRoutine);
                    kinematicRoutine = null;
                }

                if (!body.isKinematic)
                    return;

                ConnectHandWithJointToTarget(controllerScriptAlias);
                body.isKinematic = false;
                Log(body.name + " is not longer kinematic");
            }
        }

        protected virtual IEnumerator MakeBodyKinematic(Rigidbody body, float waitTime = 0.5f)
        {
            yield return new WaitForSeconds(waitTime);
            body.isKinematic = true;
            Log(body.name + " is now kinematic after " + waitTime);
            Destroy(wristJoint);

            kinematicRoutine = null;
        }

        protected virtual bool ShouldBodyBeKinematic(Rigidbody body)
        {
            return manusTouch.NumberOfFingersTouching() <= 0 && manusGrab.GetGrabbedObject() == null;
        }

        protected virtual void MoveBodyToTransform(Transform target, Rigidbody body)
        {
            if (!body.isKinematic)
            {
                MoveBodyWithVelocity(target.position, body, MaxVelocityWhenColliding, VelocityMovementMultiplier);
                RotateHandPalm(target.rotation);
            }
            else
            {
                SetBodyPosition(target.position, body);
                SetBodyRotation(target.rotation, body);
            }
        }

        protected virtual void RotateHandPalm(Quaternion targetRotation, float minAngleDeltaBeforeConnect = 0.5f)
        {
            // Rotating the hand is not required when it is attached with a joint
            if (wristJoint)
                return;

            // Connect the palm with a joint with the disconnect angle is low enough
            if (CreateJointForRotation
                && !rigidbodySelf.isKinematic
                && CurrentAngleDelta < minAngleDeltaBeforeConnect)
            {
                Log("Trying to connect the joint because the current angle delta " + CurrentAngleDelta + " is lower then de min angle delta " + minAngleDeltaBeforeConnect);
                ConnectHandWithJointToTarget(controllerScriptAlias);
            }
            else
            {
                // Rotate the palm towards the target rotation
                RotateBodyWithVelocity(targetRotation, rigidbodySelf, MaxVelocityWhenColliding);
            }
        }

        protected virtual void ConnectHandWithJointToTarget(Transform connectedBodyTarget)
        {
            if (wristJoint != null || !CreateJointForRotation)
            {
                return;
            }

            if (connectedBodyTarget == null)
            {
                Log("The palm joint will not connect since the connected body target is null");
                return;
            }

            // Try to get the rigidbody that is attached to the controller script alias.
            Rigidbody controllerRigidbody = connectedBodyTarget.GetComponent<Rigidbody>();

            if (controllerRigidbody == null)
            {
                Log("The palm joint will not connect since the rigidbody on the connected body target is null");
                return;
            }

            wristJoint = gameObject.AddComponent<ConfigurableJoint>();

            wristJoint.angularXMotion = ConfigurableJointMotion.Locked;
            wristJoint.angularYMotion = ConfigurableJointMotion.Locked;
            wristJoint.angularZMotion = ConfigurableJointMotion.Locked;

            wristJoint.connectedBody = controllerRigidbody;
            wristJoint.breakTorque = BreakTorque;

            Log("Succesfully connected " + wristJoint.name + " to " + connectedBodyTarget.name);
        }


        protected virtual void SetBodyRotation(Quaternion targetRotation, Rigidbody body)
        {
            if (!body.isKinematic)
            {
                Log("Can not set the ROTATION of " + body.name + " since the rigidbody is not kinematic");
                return;
            }
            body.MoveRotation(targetRotation);
        }

        protected virtual void SetBodyPosition(Vector3 targetPosition, Rigidbody body)
        {
            if (!body.isKinematic)
            {
                Log("Can not set the POSITION of " + body.name + " since the rigidbody is not kinematic");
                return;
            }
            body.MovePosition(Vector3.MoveTowards(body.position, targetPosition, MaxDistanceDeltaWhenKinematic));
        }

        /// <summary>
        ///     rotate the given rigidbody to the targetRotation rotation
        /// </summary>
        /// <param name="targetRotation"></param>
        /// <param name="body"></param>
        /// <param name="maxAngularVelocity"></param>
        protected virtual void RotateBodyWithVelocity(Quaternion targetRotation, Rigidbody body, float maxAngularVelocity)
        {
            if (body.isKinematic)
            {
                Log("Can not change the angular velocity of " + body.name + " since the rigidbody is kinematic");
                return;
            }

            if (wristJoint)
            {
                Log("Can not change the angular velocity since the body is connected with a joint");
                return;
            }

            var rotDelta = targetRotation * Quaternion.Inverse(body.transform.rotation);

            // Check which direction the body should rotate towards
            rotDelta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180) angle -= 360;
            var angularTarget = angle * axis;

            // Angular target is sometimes negative?
            if (angularTarget.magnitude > 0.001f)
            {
                body.angularVelocity = angularTarget;
            }

            body.maxAngularVelocity = maxAngularVelocity;
            body.angularVelocity = Vector3.ClampMagnitude(body.angularVelocity, maxAngularVelocity);
        }

        /// <summary>
        ///     Move the given rigidbody to the target location
        /// </summary>
        /// <param name="target"></param>
        /// <param name="body"></param>
        /// <param name="maxDistanceDelta"></param>
        protected virtual void MoveBodyWithVelocity(Vector3 targetPosition, Rigidbody body, float maxVelocity, float velocityMultiplier)
        {
            if (body.isKinematic)
            {
                Log("Can not change the velocity of " + body.name + " since the rigidbody is kinematic");
                return;
            }

            var velocityTarget = (targetPosition - body.position) * velocityMultiplier;

            body.velocity = Vector3.MoveTowards(body.velocity, velocityTarget, maxVelocity);
            body.velocity = Vector3.ClampMagnitude(body.velocity, maxVelocity);
        }

        protected override void Log(string log)
        {
            if (debugController)
            {
                Debug.Log(log);
            }
        }
    }
}

