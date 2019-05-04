// Copyright (c) 2018 ManusVR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.ManusVR.Scripts.Factory;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace Assets.ManusVR.Scripts.PhysicalInteraction
{
    public class PhysicsFinger : Finger
    {
        private readonly HingeJoint[] _joints = new HingeJoint[4];
        private readonly Phalange[] _phalanges = new Phalange[3];

        private JointMotor _motor;
        private PhysicsHand _physicsHand;
        private Rigidbody _thumbRigidbody;
        private Rigidbody _thumbTarget;
        private float _currentTargetVelocity;

        public PhysicsFingerSettings PhysicsFingerSettings { get; set; }

        private bool collisionUpdated = false;
        private int collidingObjectsThisFrame = 0;
        private List<Collider> _collidingObjects = new List<Collider>();

        private Quaternion _initialThumbQuaternion;

        public override void Start()
        {
            base.Start();
            _initialThumbQuaternion = PhalangesGameObjects[1].transform.localRotation;
            _physicsHand = Hand as PhysicsHand;
            if (_physicsHand == null)
                Debug.LogError("PhysicsFinger needs a PhysicsHand");

            for (var i = 1; i <= 2; i++)
            {
                if (Index == FingerIndex.thumb && i == 1)
                {
                    ConstraintThumbPosition(PhalangesGameObjects[i]);
                    ConstraintThumbRotation(PhalangesGameObjects[i]);
                    continue;
                }

                GameObject holder = PhalangesGameObjects[i];

                GameObject copy = new GameObject(PhalangesGameObjects[i].name);
                //copy.transform.parent = PhalangesGameObjects[i].transform.parent;
                copy.transform.parent = Hand.transform;
                copy.transform.position = PhalangesGameObjects[i].transform.position;
                copy.transform.rotation = PhalangesGameObjects[i].transform.rotation;

                Collider targetCollider = PhalangesGameObjects[i].GetComponent<Collider>();
                Destroy(targetCollider);
                CopyCollider(targetCollider, copy);
                PhalangesGameObjects[i] = copy;

                // Get the rigidbody where it should connect to
                var connectedBody = ConnectedBody(i);

                // Add a hingejoint to the gameobject
                if (Index == FingerIndex.thumb && DeviceType != device_type_t.GLOVE_LEFT)
                    _joints[i] = AddHingeJoint(PhalangesGameObjects[i],
                        new Vector3(0, 0, 1), connectedBody);
                else if (Index == FingerIndex.thumb)
                    _joints[i] = AddHingeJoint(PhalangesGameObjects[i],
                        new Vector3(0, 0, 1), connectedBody);
                else
                    _joints[i] = AddHingeJoint(PhalangesGameObjects[i],
                        new Vector3(0, 0, 1), connectedBody);

                _phalanges[i] = HandFactory.GetPhalange(PhalangesGameObjects[i], Index, i, DeviceType);
                PhalangesGameObjects[i].GetComponent<Rigidbody>().maxAngularVelocity = 100f;

                // Add limits to the hingejoints of the finger
                if (DeviceType == device_type_t.GLOVE_RIGHT)
                    ChangeJointLimit(_joints[i], -10, 120);
                else
                    ChangeJointLimit(_joints[i], -120, 10);
                PhalangesGameObjects[i] = holder;
            }
        }

        public void FixedUpdate()
        {
            if (AmountOfCollidingObjects() > 0)
            {
                _currentTargetVelocity = PhysicsFingerSettings.MaxTargetVelocityColliding;
                return;
            }

            else if (_currentTargetVelocity < PhysicsFingerSettings.MaxTargetVelocity)
            {
                _currentTargetVelocity = _currentTargetVelocity + 10;
            }

        }

        void LateUpdate()
        {
            collisionUpdated = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            _collidingObjects.Add(collision.collider);
        }

        private void OnCollisionExit(Collision collision)
        {
            _collidingObjects.Remove(collision.collider);
        }




        /// <summary>
        ///     Check which type of rotation is required for the given phalange
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="targetRotation"></param>
        public override void RotatePhalange(int pos, Quaternion targetRotation)
        {
            if (PhysicsFingerSettings == null || !enabled)
                return;

            //PhalangesGameObjects that aren't the first or second aren't moved using physics
            if (pos < 1 || pos > 2)
            {
                base.RotatePhalange(pos, targetRotation);
                return;
            }

            //Check if Thumb
            if (Index == FingerIndex.thumb && pos == 1)
            {
                RotateThumb();
                return;
            }

            //Move if it's the first or second phalange using physics
            if (pos == 1 || pos == 2)
            {
                if (_phalanges[pos] != null)
                {
                    base.RotatePhalange(pos, targetRotation);
                    RotatePhysicsPhalange(pos, targetRotation);
                }
            }
        }


        /// <inheritdoc />
        /// <summary>
        ///     Return the amount of phalanges that are colliding with an object
        /// </summary>
        /// <returns></returns>
        public override int AmountOfCollidingObjects()
        {
            if (!collisionUpdated)
            {
                collidingObjectsThisFrame = _phalanges.Count(phalange => phalange != null && phalange.Detector != null && phalange.Detector.IsColliding);
                collisionUpdated = true;
            }

            return collidingObjectsThisFrame;
        }

        private Rigidbody ConnectedBody(int pos)
        {
            if (pos == 1 && Index == FingerIndex.thumb)
                return _thumbRigidbody;
            if (pos == 1)
                return Hand.Wrist.Rigidbody;
            if (pos == 2 && Index == FingerIndex.thumb)
                return _thumbRigidbody;
            return _phalanges[pos - 1].GetComponent<Rigidbody>();
        }

        /// <summary>
        ///     Change the jointlimit on a give Hingjoint
        /// </summary>
        /// <param name="joint"></param>
        /// <param name="minLimit"></param>
        /// <param name="maxLimit"></param>
        private void ChangeJointLimit(HingeJoint joint, float minLimit, float maxLimit)
        {
            var limit = new JointLimits();
            limit.min = minLimit;
            limit.max = maxLimit;
            joint.limits = limit;
        }

        /// <summary>
        ///     Add a hingejoint to the gameobject and provide it with the needed settings
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="axis"></param>
        /// <param name="connectedBody"></param>
        /// <returns></returns>
        private HingeJoint AddHingeJoint(GameObject gameObject, Vector3 axis, Rigidbody connectedBody)
        {
            var joint = gameObject.GetComponent<HingeJoint>();
            if (joint != null)
            {
                Debug.LogWarning(gameObject.name + " already has a hingejoint attached to it");
                return joint;
            }

            var rb = gameObject.AddComponent<Rigidbody>();

            rb.mass = 0.1f;
            joint = gameObject.AddComponent<HingeJoint>();

            joint.axis = axis;

            joint.connectedBody = connectedBody;
            joint.useMotor = true;

            return joint;
        }

        /// <summary>
        /// Rotate the phalange by using physics
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="targetRotation"></param>
        private void RotatePhysicsPhalange(int pos, Quaternion targetRotation)
        {
            var motorVelocity = AmountOfCollidingObjects() > 0
                ? PhysicsFingerSettings.VelocityMultiplierColliding
                : PhysicsFingerSettings.VelocityMultiplierNotColliding;
            // Rotate the finger with physics
            var joint = _joints[pos];
            if (joint == null)
                return;

            // calculate the angle between the rotation of the physics finger and the rotation of the target finger
            var offset = Index == FingerIndex.thumb ? new Vector3(0, 0, 180) : new Vector3(0, 0, 180);

            var jointRotation = _phalanges[pos].transform.rotation * Quaternion.Euler(offset) *
                                Quaternion.Inverse(PhalangesGameObjects[pos].transform.rotation);

            var rotDelta = jointRotation;
            float angle;
            Vector3 axis;
            rotDelta.ToAngleAxis(out angle, out axis);
            if (angle > 180)
                angle -= 360;
            if (angle > 0)
                angle -= 180;
            else
                angle = 180 + angle;
            // Let the motor move towards the target rotation     
            var targetVelocity = angle * motorVelocity;

            //joint.useLimits = targetVelocity < 1000 && targetVelocity > -1000;
            var maxTargetVelocity = AmountOfCollidingObjects() > 0
                ? PhysicsFingerSettings.MaxTargetVelocityColliding
                : PhysicsFingerSettings.MaxTargetVelocity;
            var newVelocity = Mathf.Clamp(targetVelocity, -maxTargetVelocity,
                maxTargetVelocity);


            _motor.targetVelocity = newVelocity;
            _motor.force = PhysicsFingerSettings.MotorForce;
            joint.motor = _motor;
        }


        private void RotateThumb()
        {
            if (_thumbRigidbody == null)
                return;

            _thumbTarget.MoveRotation(Quaternion.RotateTowards(_thumbTarget.rotation, Hand.ThumbRotation(), 10));
            var maxRotationDelta =
                AmountOfCollidingObjects() > 0 ? PhysicsFingerSettings.ThumbRotDeltaColliding :
                    PhysicsFingerSettings.ThumbRotDeltaNotColliding;
            _thumbTarget.MoveRotation(Quaternion.RotateTowards(_thumbTarget.rotation, Hand.WristTransform.rotation * Hand.ThumbRotation(), maxRotationDelta));

        }


        private void ConstraintThumbPosition(GameObject thumbGameObject)
        {
            _thumbRigidbody = thumbGameObject.AddComponent<Rigidbody>();
            _thumbRigidbody.mass = 1f;
            _thumbRigidbody.useGravity = false;
            _thumbRigidbody.centerOfMass = Vector3.zero;

            var thumbJoint = thumbGameObject.AddComponent<ConfigurableJoint>();
            thumbJoint.connectedBody = _physicsHand.Wrist.Rigidbody;

            thumbJoint.xMotion = ConfigurableJointMotion.Locked;
            thumbJoint.yMotion = ConfigurableJointMotion.Locked;
            thumbJoint.zMotion = ConfigurableJointMotion.Locked;
        }

        private void ConstraintThumbRotation(GameObject thumbGameobject)
        {
            var thumbJoint = thumbGameobject.AddComponent<ConfigurableJoint>();

            GameObject target = new GameObject("TargetThumb");
            target.transform.parent = Hand.transform;
            target.transform.rotation = thumbGameobject.transform.rotation;

            _thumbTarget = target.AddComponent<Rigidbody>();
            _thumbTarget.isKinematic = true;
            thumbJoint.connectedBody = _thumbTarget;
            thumbJoint.angularXMotion = ConfigurableJointMotion.Locked;
            thumbJoint.angularYMotion = ConfigurableJointMotion.Locked;
            thumbJoint.angularZMotion = ConfigurableJointMotion.Locked;
        }

        /// <summary>
        /// Make a copy of the given collider
        /// </summary>
        /// <param name="child"></param>
        private Collider CopyCollider(Collider collider, GameObject target)
        {
            // Add a extra trigger collider
            var colliderCopy = CopyComponent(collider, target) as Collider;

            if (colliderCopy is MeshCollider)
            {
                var meshCopy = colliderCopy as MeshCollider;
                // Make it convex otherwise it will not work with triggers
                meshCopy.convex = true;
                meshCopy.inflateMesh = true;
                meshCopy.skinWidth = 0.002f;
            }

            else if (colliderCopy is BoxCollider)
            {
                var boxCopy = colliderCopy as BoxCollider;
                var boxChild = collider as BoxCollider;
                boxCopy.size = boxChild.size;
                boxCopy.center = boxChild.center;
            }

            else if (colliderCopy is SphereCollider)
            {
                var sphereCopy = colliderCopy as SphereCollider;
                var sphereChild = collider as SphereCollider;
                sphereCopy.radius = sphereChild.radius;
                sphereCopy.center = sphereChild.center;
            }

            else if (colliderCopy is CapsuleCollider)
            {
                var capsuleCopy = colliderCopy as CapsuleCollider;
                var capsuleChild = collider as CapsuleCollider;
                capsuleCopy.radius = capsuleChild.radius;
                capsuleCopy.height = capsuleChild.height;
                capsuleCopy.center = capsuleChild.center;
                capsuleCopy.direction = capsuleChild.direction;
            }

            return colliderCopy;
        }

        /// <summary>
        /// Make a copy of the given component
        /// </summary>
        /// <param name="original"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        private Component CopyComponent(Component original, GameObject destination)
        {
            var type = original.GetType();
            var copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            var fields = type.GetFields();
            foreach (var field in fields)
                field.SetValue(copy, field.GetValue(original));
            return copy;
        }
    }
}