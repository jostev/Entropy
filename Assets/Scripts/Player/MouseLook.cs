using System;
using Entropy.Environment;
using UnityEngine;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [Serializable]
    public class MouseLook
    {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public bool smooth;
        public float smoothTime = 5f;

        private GravityBody m_GravityBody;
        private float m_Pitch;

        public void Init(Transform character, Transform camera, GravityBody gravityBody)
        {
            m_GravityBody = gravityBody;

            m_Pitch = camera.localRotation.eulerAngles.x;
            if (m_Pitch > 180f) m_Pitch -= 360f;
        }

        public void LookRotation(Transform character, Transform camera)
        {
            Cursor.lockState = CursorLockMode.Locked;

            float yRot = Input.GetAxis("Mouse X") * XSensitivity;
            float xRot = Input.GetAxis("Mouse Y") * YSensitivity;

            Vector3 gravityUp = m_GravityBody.GetAntiGravityDirection();

            Quaternion yawRot = Quaternion.AngleAxis(yRot, gravityUp);
            Vector3 rawForward = yawRot * character.forward;

            Vector3 newForward = Vector3.ProjectOnPlane(rawForward, gravityUp).normalized;
            if (newForward.sqrMagnitude < 0.001f)
            {
                newForward = Vector3.ProjectOnPlane(Vector3.forward, gravityUp).normalized;
                if (newForward.sqrMagnitude < 0.001f)
                    newForward = Vector3.ProjectOnPlane(Vector3.right, gravityUp).normalized;
            }

            Quaternion targetCharacterRot = Quaternion.LookRotation(newForward, gravityUp);

            m_Pitch -= xRot;
            if (clampVerticalRotation)
                m_Pitch = Mathf.Clamp(m_Pitch, MinimumX, MaximumX);

            Quaternion targetCameraRot = Quaternion.AngleAxis(m_Pitch, Vector3.right);

            if (smooth)
            {
                character.rotation = Quaternion.Slerp(character.rotation, targetCharacterRot,
                    smoothTime * Time.deltaTime);
                camera.localRotation = Quaternion.Slerp(camera.localRotation, targetCameraRot,
                    smoothTime * Time.deltaTime);
            }
            else
            {
                character.rotation = targetCharacterRot;
                camera.localRotation = targetCameraRot;
            }
        }

        public void LookOveride(Transform character, Transform camera)
        {
            m_Pitch = camera.localRotation.eulerAngles.x;
            if (m_Pitch > 180f) m_Pitch -= 360f;
        }

        public void CamGoBackAll(Transform character, Transform camera)
        {
            m_Pitch = 0f;
            camera.localRotation = Quaternion.AngleAxis(m_Pitch, Vector3.right);
        }

        public void CamGoBack(Transform character, Transform camera, float speed)
        {
            m_Pitch = Mathf.MoveTowards(m_Pitch, 0f, speed * Time.deltaTime);
            camera.localRotation = Quaternion.AngleAxis(m_Pitch, Vector3.right);
        }

        public void AddPitch(float delta)
        {
            m_Pitch += delta;
            if (clampVerticalRotation)
                m_Pitch = Mathf.Clamp(m_Pitch, MinimumX, MaximumX);
        }
    }
}
