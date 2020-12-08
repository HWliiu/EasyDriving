using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EasyDriving
{
    public class MobileInputManager : MonoBehaviour
    {
        public VehicleController VC;
        public SteeringWheel SteeringWheel;

        public PedalScrollbar Throttle;
        public PedalScrollbar Brake;
        public Scrollbar Clutch;
        public Toggle HandBrake;
        public Toggle[] Gears;
        public Toggle[] Blinkers;
        public EventTrigger Horn;
        public Toggle LowBeamLights;
        public Toggle FullBeamLights;

        public Toggle TopViewToggle;
        public RectTransform TopView;

        public Toggle RearViewToggle;
        public RectTransform LeftRearView;
        public RectTransform RightRearView;

        public Button ChangeCamButton;
        public Camera VehicleCameraFollow;
        public Camera VehicleCameraDriver;

        public Button MenuButton;
        public Button CanclePause;

        private WaitForSeconds _waitForRollBackGear;
        private WaitForSeconds _waitForPressClutch;

        private float _lastClutchValue;
        private bool _pressClutch;
        private float _clutchVelocity;

        // Start is called before the first frame update
        void Start()
        {
            Initialize();
            _waitForRollBackGear = new WaitForSeconds(0.2f);
            _waitForPressClutch = new WaitForSeconds(VC.Transmission.ShiftDuration / 2);
        }

        private void Initialize()
        {
            //油门
            if (Throttle == null)
                Throttle = Utility.FindChild<PedalScrollbar>(transform, nameof(Throttle)) ?? throw new ArgumentNullException(nameof(Throttle));
            Throttle.onValueChanged.AddListener(OnThrottleChanged);
            //刹车
            if (Brake == null)
                Brake = Utility.FindChild<PedalScrollbar>(transform, nameof(Brake)) ?? throw new ArgumentNullException(nameof(Brake));
            Brake.onValueChanged.AddListener(OnBrakeChanged);
            //离合
            if (Clutch == null)
                Clutch = Utility.FindChild<Scrollbar>(transform, nameof(Clutch)) ?? throw new ArgumentNullException(nameof(Clutch));
            Clutch.onValueChanged.AddListener(OnClutchChanged);
            //手刹
            if (HandBrake == null)
                HandBrake = Utility.FindChild<Toggle>(transform, nameof(HandBrake)) ?? throw new ArgumentNullException(nameof(HandBrake));
            HandBrake.onValueChanged.AddListener(OnHandBrakeChanged);
            HandBrake.isOn = true;
            //喇叭
            if (Horn == null)
            {
                var horn = Utility.FindChild<RectTransform>(transform, nameof(Horn)) ?? throw new ArgumentNullException(nameof(Horn));
                Horn = horn.gameObject.AddComponent<EventTrigger>();
                Horn.triggers = new List<EventTrigger.Entry>();

                var entry = new EventTrigger.Entry();
                var callback = new EventTrigger.TriggerEvent();
                callback.AddListener(TurnOnHorn);
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback = callback;
                Horn.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                callback = new EventTrigger.TriggerEvent();
                callback.AddListener(TurnOffHorn);
                entry.eventID = EventTriggerType.PointerUp;
                entry.callback = callback;
                Horn.triggers.Add(entry);
            }
            //换挡
            Gears = new Toggle[7];
            for (var i = -1; i <= 5; i++)
            {
                var str = "Gear" + i.ToString();
                Gears[i + 1] = Utility.FindChild<Toggle>(transform, str) ?? throw new ArgumentNullException(str);
                var gear = i;
                Gears[i + 1].onValueChanged.AddListener(value => { if (value) ShiftInto(gear); });
            }
            //转向灯
            Blinkers = new Toggle[3];
            for (var i = 0; i < 3; i++)
            {
                var str = "Blinker" + i.ToString();
                Blinkers[i] = Utility.FindChild<Toggle>(transform, str) ?? throw new ArgumentNullException(str);
                var blinker = i;
                Blinkers[i].onValueChanged.AddListener(value => { if (value) TurnBlinker(blinker); });
            }
            //远近光灯
            if (LowBeamLights == null)
                LowBeamLights = Utility.FindChild<Toggle>(transform, nameof(LowBeamLights)) ?? throw new ArgumentNullException(nameof(LowBeamLights));
            if (FullBeamLights == null)
                FullBeamLights = Utility.FindChild<Toggle>(transform, nameof(FullBeamLights)) ?? throw new ArgumentNullException(nameof(FullBeamLights));
            LowBeamLights.onValueChanged.AddListener(TurnLowBeamLights);
            FullBeamLights.onValueChanged.AddListener(TurnFullBeamLights);
            //方向盘
            if (SteeringWheel == null)
                SteeringWheel = Utility.FindChild<SteeringWheel>(transform, nameof(SteeringWheel)) ?? throw new ArgumentNullException(nameof(SteeringWheel));

            //切换顶视图
            if (TopViewToggle == null)
                TopViewToggle = Utility.FindChild<Toggle>(transform, nameof(TopViewToggle)) ?? throw new ArgumentNullException(nameof(TopViewToggle));
            if (TopView == null)
                TopView = Utility.FindChild<RectTransform>(transform, nameof(TopView)) ?? throw new ArgumentNullException(nameof(TopView));
            TopViewToggle.onValueChanged.AddListener(OnTopViewToggleChanged);
            //切换后视镜视图
            if (RearViewToggle == null)
                RearViewToggle = Utility.FindChild<Toggle>(transform, nameof(RearViewToggle)) ?? throw new ArgumentNullException(nameof(RearViewToggle));
            if (LeftRearView == null)
                LeftRearView = Utility.FindChild<RectTransform>(transform, nameof(LeftRearView)) ?? throw new ArgumentNullException(nameof(LeftRearView));
            if (RightRearView == null)
                RightRearView = Utility.FindChild<RectTransform>(transform, nameof(RightRearView)) ?? throw new ArgumentNullException(nameof(RightRearView));
            RearViewToggle.onValueChanged.AddListener(OnRearViewToggleChanged);
            //更换摄像机
            if (ChangeCamButton == null)
                ChangeCamButton = Utility.FindChild<Button>(transform, nameof(ChangeCamButton)) ?? throw new ArgumentNullException(nameof(ChangeCamButton));
            if (VehicleCameraFollow == null)
                VehicleCameraFollow = Utility.FindChild<Camera>(VC.transform, nameof(VehicleCameraFollow)) ?? throw new ArgumentNullException(nameof(VehicleCameraFollow));
            if (VehicleCameraDriver == null)
                VehicleCameraDriver = Utility.FindChild<Camera>(VC.transform, nameof(VehicleCameraDriver)) ?? throw new ArgumentNullException(nameof(VehicleCameraDriver));
            ChangeCamButton.onClick.AddListener(ChangeCamera);
            //菜单
            if (MenuButton == null)
                MenuButton = Utility.FindChild<Button>(transform, nameof(MenuButton)) ?? throw new ArgumentNullException(nameof(MenuButton));
            MenuButton.onClick.AddListener(OnPause);
            if (CanclePause == null)
                CanclePause = Utility.FindChild<Button>(transform, nameof(CanclePause)) ?? throw new ArgumentNullException(nameof(CanclePause));
            CanclePause.onClick.AddListener(OnCanclePause);
        }

        // Update is called once per frame
        void Update()
        {
            UpdateSteering();
            UpdateClutch();
        }

        private void UpdateSteering()
        {
            if (SteeringWheel != null && VC != null)
                VC.Input.Steering = SteeringWheel.Value;
        }

        private void UpdateClutch()
        {
            //刹车时自动更新离合器
            if (VC.SpeedKPH < 5f && !VC.Transmission.Shifting && Brake.value > 0.05f && VC.Transmission.Gear != 0)
            {
                Clutch.value = Mathf.MoveTowards(Clutch.value, Brake.value, 0.1f);
            }
            //换挡时自动更新离合器
            if (VC.Transmission.Shifting)
            {
                if (_pressClutch)
                {
                    if (Clutch.value < 1f) Clutch.value += Time.deltaTime * 5f;
                }
                else
                {
                    if (Clutch.value > _lastClutchValue) Clutch.value -= Time.deltaTime * 5f;
                }
            }
        }

        private void OnThrottleChanged(float value)
        {
            VC.Input.Throttle = value;
            if (!value.IsDeadzoneZero() && HandBrake.isOn && VC.Transmission.Gear != 0)
                HandBrake.isOn = false;
        }

        private void OnBrakeChanged(float value)
        {
            VC.Input.Brake = value;
        }

        private void OnClutchChanged(float value) => VC.Input.Clutch = value;
        private void OnHandBrakeChanged(bool value)
        {
            var released = Utility.FindChild<Image>(HandBrake.transform, "Released") ?? throw new ArgumentNullException("Released image not found");
            released.enabled = !value;
            VC.Input.Handbrake = value;
        }

        private void ShiftInto(int gear)
        {
            if (!VC.Transmission.ShiftInto(gear, out var prevGear))
            {
                //换挡失败时回退
                StartCoroutine(rollBackGear());
                return;
            }
            //自动踩离合器
            if (VC.SpeedKPH > 0.1f)
                StartCoroutine(autoClutch());

            IEnumerator rollBackGear()
            {
                yield return _waitForRollBackGear;
                Gears[prevGear + 1].isOn = true;
            }
            IEnumerator autoClutch()
            {
                _pressClutch = true;
                _lastClutchValue = Clutch.value;
                yield return _waitForPressClutch;
                _pressClutch = false;
            }
        }

        private void TurnBlinker(int i)
        {
            switch (i)
            {
                case 0:
                    VC.Input.LeftBlinker = false;
                    VC.Input.RightBlinker = false;
                    break;
                case 1:
                    VC.Input.LeftBlinker = true;
                    break;
                case 2:
                    VC.Input.RightBlinker = true;
                    break;
                default:
                    return;
            }
        }

        private void TurnOnHorn(BaseEventData eventData) => VC.Input.Horn = true;

        private void TurnOffHorn(BaseEventData eventData) => VC.Input.Horn = false;

        private void TurnLowBeamLights(bool value)
        {
            if (!value && FullBeamLights.isOn) FullBeamLights.isOn = false;
            VC.Input.LowBeamLights = value;
        }
        private void TurnFullBeamLights(bool value)
        {
            if (value && !LowBeamLights.isOn) LowBeamLights.isOn = true;
            VC.Input.FullBeamLights = value;
        }

        private void OnTopViewToggleChanged(bool value) => TopView.gameObject.SetActive(value);
        private void OnRearViewToggleChanged(bool value)
        {
            LeftRearView.gameObject.SetActive(value);
            RightRearView.gameObject.SetActive(value);
        }

        private void ChangeCamera()
        {
            if (VehicleCameraFollow.gameObject.activeSelf)
            {
                VehicleCameraDriver.gameObject.SetActive(true);
                VehicleCameraFollow.gameObject.SetActive(false);
                VC.Sound.InsideVehicle = true;
            }
            else if (VehicleCameraDriver.gameObject.activeSelf)
            {
                VehicleCameraDriver.gameObject.SetActive(false);
                VehicleCameraFollow.gameObject.SetActive(true);
                VC.Sound.InsideVehicle = false;
            }
        }

        private void OnPause()
        {
            Time.timeScale = 0f;
            CanclePause.gameObject.SetActive(true);
            VC.Sound.DisableAllSound();
        }

        private void OnCanclePause()
        {
            Time.timeScale = 1f;
            CanclePause.gameObject.SetActive(false);
            VC.Sound.EnableAllSound();
        }
    }
}
