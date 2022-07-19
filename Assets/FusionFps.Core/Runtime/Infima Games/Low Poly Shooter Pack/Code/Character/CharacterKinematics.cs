using UnityEngine;
using System.Collections.Generic;

namespace InfimaGames.LowPolyShooterPack {
    /// <summary>
    /// Handles all the Inverse Kinematics needed for our Character. Very important. Uses Unity's IK code.
    /// </summary>
    public class CharacterKinematics : MonoBehaviour {
        [Header("Settings Arm Left")] [Tooltip("Left Arm Target. Determines what the IK target is.")] [SerializeField]
        private Transform armLeftTarget;

        [Range(0.0f, 1.0f)] [Tooltip("Inverse Kinematics Weight for the left arm.")] [SerializeField]
        private float armLeftWeightPosition = 1.0f;

        [Range(0.0f, 1.0f)] [Tooltip("Inverse Kinematics Weight for the left arm.")] [SerializeField]
        private float armLeftWeightRotation = 1.0f;

        [Tooltip("Left Arm Hierarchy. Root, Mid, Tip.")] [SerializeField]
        private Transform[] armLeftHierarchy;

        [Header("Settings Arm Right")] [Tooltip("Left Arm Target. Determines what the IK target is.")] [SerializeField]
        private Transform armRightTarget;

        [Range(0.0f, 1.0f)] [Tooltip("Inverse Kinematics Weight for the right arm.")] [SerializeField]
        private float armRightWeightPosition = 1.0f;

        [Range(0.0f, 1.0f)] [Tooltip("Inverse Kinematics Weight for the right arm.")] [SerializeField]
        private float armRightWeightRotation = 1.0f;

        [Tooltip("Right Arm Hierarchy. Root, Mid, Tip.")] [SerializeField]
        private Transform[] armRightHierarchy;

        [Header("Generic")] [Tooltip("Hint.")] [SerializeField]
        private Transform hint;

        [Range(0.0f, 1.0f)] [Tooltip("Hint Weight.")] [SerializeField]
        private float weightHint;

        private bool _maintainTargetPositionOffset;
        private bool _maintainTargetRotationOffset;

        private const float KSqrEpsilon = 1e-8f;

        /// <summary>
        /// Computes the Inverse Kinematics for both arms.
        /// </summary>
        public void Compute(float weightLeft = 1.0f, float weightRight = 1.0f) {
            //Compute Left Arm.
            ComputeOnce(armLeftHierarchy, armLeftTarget,
                armLeftWeightPosition * weightLeft,
                armLeftWeightRotation * weightLeft);

            //Compute Right Arm.
            ComputeOnce(armRightHierarchy, armRightTarget,
                armRightWeightPosition * weightRight,
                armRightWeightRotation * weightRight);
        }

        /// <summary>
        /// Computes the Inverse Kinematics for one arm, or hierarchy.
        /// </summary>
        /// <param name="hierarchy">Arm Hierarchy. Root, Mid, Tip.</param>
        /// <param name="target">IK Target.</param>
        /// <param name="weightPosition">Position Weight.</param>
        /// <param name="weightRotation">Rotation Weight.</param>
        private void ComputeOnce(IReadOnlyList<Transform> hierarchy, Transform target, float weightPosition = 1.0f,
            float weightRotation = 1.0f) {
            var targetOffsetPosition = Vector3.zero;
            var targetOffsetRotation = Quaternion.identity;

            if ( _maintainTargetPositionOffset )
                targetOffsetPosition = hierarchy[2].position - target.position;
            if ( _maintainTargetRotationOffset )
                targetOffsetRotation = Quaternion.Inverse(target.rotation) * hierarchy[2].rotation;

            var aPosition = hierarchy[0].position;
            var bPosition = hierarchy[1].position;
            var cPosition = hierarchy[2].position;
            var targetPos = target.position;
            var targetRot = target.rotation;
            var tPosition = Vector3.Lerp(cPosition, targetPos + targetOffsetPosition, weightPosition);
            var tRotation =
                Quaternion.Lerp(hierarchy[2].rotation, targetRot * targetOffsetRotation, weightRotation);
            var hasHint = hint != null && weightHint > 0f;

            var ab = bPosition - aPosition;
            var bc = cPosition - bPosition;
            var ac = cPosition - aPosition;
            var at = tPosition - aPosition;

            var abLen = ab.magnitude;
            var bcLen = bc.magnitude;
            var acLen = ac.magnitude;
            var atLen = at.magnitude;

            var oldAbcAngle = TriangleAngle(acLen, abLen, bcLen);
            var newAbcAngle = TriangleAngle(atLen, abLen, bcLen);

            // Bend normal strategy is to take whatever has been provided in the animation
            // stream to minimize configuration changes, however if this is collinear
            // try computing a bend normal given the desired target position.
            // If this also fails, try resolving axis using hint if provided.
            var axis = Vector3.Cross(ab, bc);
            if ( axis.sqrMagnitude < KSqrEpsilon ) {
                axis = hasHint ? Vector3.Cross(hint.position - aPosition, bc) : Vector3.zero;

                if ( axis.sqrMagnitude < KSqrEpsilon )
                    axis = Vector3.Cross(at, bc);

                if ( axis.sqrMagnitude < KSqrEpsilon )
                    axis = Vector3.up;
            }

            axis = Vector3.Normalize(axis);

            var a = 0.5f * (oldAbcAngle - newAbcAngle);
            var sin = Mathf.Sin(a);
            var cos = Mathf.Cos(a);
            var deltaR = new Quaternion(axis.x * sin, axis.y * sin, axis.z * sin, cos);
            hierarchy[1].rotation = deltaR * hierarchy[1].rotation;

            cPosition = hierarchy[2].position;
            ac = cPosition - aPosition;
            hierarchy[0].rotation = Quaternion.FromToRotation(ac, at) * hierarchy[0].rotation;

            if ( hasHint ) {
                var acSqrMag = ac.sqrMagnitude;
                if ( acSqrMag > 0f ) {
                    bPosition = hierarchy[1].position;
                    cPosition = hierarchy[2].position;
                    ab = bPosition - aPosition;
                    ac = cPosition - aPosition;

                    var acNorm = ac / Mathf.Sqrt(acSqrMag);
                    var ah = hint.position - aPosition;
                    var abProj = ab - acNorm * Vector3.Dot(ab, acNorm);
                    var ahProj = ah - acNorm * Vector3.Dot(ah, acNorm);

                    var maxReach = abLen + bcLen;
                    if ( abProj.sqrMagnitude > (maxReach * maxReach * 0.001f) && ahProj.sqrMagnitude > 0f ) {
                        var hintR = Quaternion.FromToRotation(abProj, ahProj);
                        hintR.x *= weightHint;
                        hintR.y *= weightHint;
                        hintR.z *= weightHint;
                        hintR = Quaternion.Normalize(hintR);
                        hierarchy[0].rotation = hintR * hierarchy[0].rotation;
                    }
                }
            }

            hierarchy[2].rotation = tRotation;
        }

        private static float TriangleAngle(float aLen, float aLen1, float aLen2) {
            var c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
            return Mathf.Acos(c);
        }
    }
}