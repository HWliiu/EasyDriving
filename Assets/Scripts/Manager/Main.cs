using UnityEngine;

namespace EasyDriving
{
    public class Main : MonoBehaviour
    {
        public Course CurrentCourse;
        // Start is called before the first frame update
        void Start()
        {
            CurrentCourse = Course.None;
            Input.backButtonLeavesApp = true;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

    public enum Course
    {
        None,
        Dcrk,
        Ptqb,
        Zjzw,
        Qxxs,
        Cftc
    }
}
