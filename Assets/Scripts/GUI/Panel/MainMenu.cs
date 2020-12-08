using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace EasyDriving
{
    public class MainMenu : MonoBehaviour
    {
        public int OffsetX = 30;
        public int OffsetY = 0;
        public float Scale = 0.8f;
        public float TweenTime = 0.5f;
        public RectTransform[] List;

        private int _left;
        private int _right;
        private int _listCount;
        private RectTransform[] _sortArray;

        private Vector2 _firstMousePos;
        private Vector2 _secondMousePos;

        private float _lastMoveTime;

        private Main _main;
        // Start is called before the first frame update
        void Start()
        {
            Initialize();
            _main = UnitySingleton<Main>.GetInstance();
        }

        private void Initialize()
        {
            if (List == null && List.Length % 2 == 0)
                Debug.LogError("list count must be obb number");

            _left = 0;
            _listCount = List.Length;
            _right = _listCount - 1;
            _sortArray = new RectTransform[_listCount];

            var halfAmount = _listCount / 2;
            var index = 0;
            for (var i = halfAmount; i >= 1; i--)
            {
                var pos = new Vector2(i * OffsetX, i * OffsetY) * -1;
                InitializeItem(index++, pos, Mathf.Pow(Scale, i));
            }

            InitializeItem(index++, Vector3.zero, 1);

            for (var i = 1; i <= halfAmount; i++)
            {
                var pos = new Vector2(i * OffsetX, i * -OffsetY);
                InitializeItem(index++, pos, Mathf.Pow(Scale, i));
            }

            Sort();
            ReSetEnable();
        }

        private void InitializeItem(int index, Vector2 pos, float scale)
        {
            var go = List[index];
            go.anchorMin = go.anchorMax = go.pivot = Vector2.one / 2f;
            go.anchoredPosition = pos;
            go.localScale *= scale;

            InsertToSortArray(go, go.localScale.x);
        }

        private void InsertToSortArray(RectTransform go, float localScaleX)
        {
            var depth = CalculateDepth(localScaleX);
            depth = _listCount / 2 - depth;

            if (depth == _listCount / 2)
                _sortArray[depth * 2] = go;
            else if (_sortArray[depth] == null)
                _sortArray[depth] = go;
            else
                _sortArray[depth + _listCount / 2] = go;
        }

        private int CalculateDepth(float scaleNum)
        {
            var i = 0;
            while (true)
            {
                var num = Mathf.Pow(Scale, i);
                if (Math.Abs(num - scaleNum) > 0.01f) i++;
                else break;
            }
            return i;
        }

        private void Sort()
        {
            for (var i = 0; i < _listCount; i++)
            {
                _sortArray[i].SetSiblingIndex(i);
            }

            for (var i = 0; i < _sortArray.Length; i++)
            {
                _sortArray[i] = null;
            }
        }


        public void Move(int direction)
        {
            if (direction == -1)//向左滑动
            {
                var startIndex = _left;
                var lastIndex = _right;
                var lastPos = List[lastIndex].anchoredPosition;

                InsertToSortArray(List[startIndex], List[startIndex].localScale.x);

                for (var i = 0; i < _listCount - 1; i++)
                {
                    var index = (lastIndex - i + _listCount) % _listCount;
                    var preIndex = (index - 1 + _listCount) % _listCount;
                    List[index].DOAnchorPos(List[preIndex].anchoredPosition, TweenTime);
                    List[index].DOScale(List[preIndex].localScale, TweenTime);

                    InsertToSortArray(List[index], List[preIndex].localScale.x);
                }

                List[startIndex].DOAnchorPos(lastPos, TweenTime);

                _left = (_left + 1) % _listCount;
                _right = (_right + 1) % _listCount;
            }
            else if (direction == 1)//向右滑动
            {
                var startIndex = _right;
                var lastIndex = _left;
                var lastPos = List[lastIndex].anchoredPosition;

                InsertToSortArray(List[startIndex], List[startIndex].localScale.x);

                for (var i = 0; i < _listCount - 1; i++)
                {
                    var index = (lastIndex + i + _listCount) % _listCount;
                    var preIndex = (index + 1 + _listCount) % _listCount;
                    List[index].DOAnchorPos(List[preIndex].anchoredPosition, TweenTime);
                    List[index].DOScale(List[preIndex].localScale, TweenTime);

                    InsertToSortArray(List[index], List[preIndex].localScale.x);
                }

                List[startIndex].DOAnchorPos(lastPos, TweenTime);

                _left = (_left - 1 + _listCount) % _listCount;
                _right = (_right - 1 + _listCount) % _listCount;
            }

            Sort();
        }

        private void ReSetEnable()
        {
            var buttons = transform.GetComponentsInChildren<Button>();
            for (int i = 0; i < _listCount - 1; i++)
            {
                buttons[i].interactable = false;
            }
            buttons[_listCount - 1].interactable = true;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                _firstMousePos = Event.current.mousePosition;
            }
            if (Event.current.type == EventType.MouseUp)
            {
                _secondMousePos = Event.current.mousePosition;

                if (_firstMousePos.x - _secondMousePos.x > 50f && Time.realtimeSinceStartup - _lastMoveTime > TweenTime * 2f)
                {
                    Move(-1);
                    ReSetEnable();
                    _lastMoveTime = Time.realtimeSinceStartup;
                }
                if (_firstMousePos.x - _secondMousePos.x < -50f && Time.realtimeSinceStartup - _lastMoveTime > TweenTime * 2f)
                {
                    Move(1);
                    ReSetEnable();
                    _lastMoveTime = Time.realtimeSinceStartup;
                }
                _firstMousePos = _secondMousePos;
            }
        }

        public void OnClickDCRK()
        {
            _main.CurrentCourse = Course.Dcrk;
            Utility.LoadScene(2);
        }

        public void OnClickCFTC()
        {
            _main.CurrentCourse = Course.Cftc;
            Utility.LoadScene(2);
        }

        public void OnClickPTQB()
        {
            _main.CurrentCourse = Course.Ptqb;
            Utility.LoadScene(2);
        }

        public void OnClickQXXS()
        {
            _main.CurrentCourse = Course.Qxxs;
            Utility.LoadScene(2);
        }

        public void OnClickZJZW()
        {
            _main.CurrentCourse = Course.Zjzw;
            Utility.LoadScene(2);
        }
    }
}
