using UnityEngine;

namespace AlpineRush
{
    public class ChaseCamera : MonoBehaviour
    {
        public Transform Target;
        public ArcadeVehicle Vehicle;
        Vector3 smooth;
        bool ready;
        Camera cam;

        void LateUpdate()
        {
            if (Target == null) return;
            if (cam == null) cam = GetComponent<Camera>();
            Quaternion flat = Quaternion.Euler(0f, Target.eulerAngles.y, 0f);
            Vector3 desired = Target.position - flat * Vector3.forward * 7.4f + Vector3.up * 2.8f;
            Vector3 from = Target.position + Vector3.up * 1.3f;
            if (Physics.Linecast(from, desired, out RaycastHit hit, AlpineLayers.WallMask, QueryTriggerInteraction.Ignore))
                desired = hit.point + hit.normal * 0.4f;

            float blend = 1f - Mathf.Exp(-6.5f * Time.deltaTime);
            if (!ready)
            {
                smooth = desired;
                ready = true;
            }
            smooth = Vector3.Lerp(smooth, desired, blend);
            transform.position = smooth;
            Vector3 look = Target.position + Target.forward * 6f + Vector3.up * 1.15f;
            var aim = Quaternion.LookRotation(look - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, aim, 1f - Mathf.Exp(-8f * Time.deltaTime));

            if (cam != null && Vehicle != null)
            {
                float fov = Mathf.Lerp(62f, 76f, Vehicle.Speed01);
                if (Vehicle.NitroActive) fov += 7f;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, 5f * Time.deltaTime);
            }
        }
    }

    public class MinimapCamera : MonoBehaviour
    {
        public Transform Target;
        public float Height = 150f;

        void LateUpdate()
        {
            if (Target == null) return;
            transform.position = new Vector3(Target.position.x, Target.position.y + Height, Target.position.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
