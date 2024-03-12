using UnityEngine;

namespace Cherry.Misc
{
    public class RunDisable : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(false);
        }
    }
}