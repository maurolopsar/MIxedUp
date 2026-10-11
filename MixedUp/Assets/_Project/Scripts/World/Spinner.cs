using UnityEngine;

namespace MixedUp
{
    /// <summary>Turns something round and round at a steady speed: a lighthouse beam, a weather vane.</summary>
    public class Spinner : MonoBehaviour
    {
        public Vector3 degreesPerSecond = new Vector3(0f, 40f, 0f);

        void Update() => transform.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
    }
}
