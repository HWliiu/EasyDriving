using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyDriving
{
    [Serializable]
    public class Effects
    {
        //Skidmarks
        [Header("Skidmarks")]
        //TODO
        //SkidSmoke
        [Header("SkidSmoke")]
        //TODO
        //ExhaustSmoke
        [Header("ExhaustSmoke")]
        // ReSharper disable once UnassignedField.Global
        public ParticleSystem[] ParticleSystems;
        [Range(0, 30)]
        public float BaseIntensity = 12f;
        [Range(0, 30)]
        public float IntensityRange = 20f;
        [Range(0, 1)]
        public float Soot = 0.4f;
        [Range(0, 1)]
        public float StartSize = 0.3f;
        [Range(0, 10)]
        public float LifeDistance = 2.5f;


        private Color _vaporColor = new Color(0.7f, 0.7f, 0.7f, 0.35f);
        private Color _sootColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        private Color _cleanColor = new Color(0f, 0f, 0f, 0f);
        private Color _idleColor;
        private float _sootIntensity;
        private VehicleController _vc;


        public void Initialize(VehicleController vc) => this._vc = vc;

        public void Update()
        {
            UpdateExhaustSmoke();
        }

        private void UpdateExhaustSmoke()
        {
            if (_vc.Active && _vc.Engine.IsRunning)
            {
                foreach (var ps in ParticleSystems)
                {
                    var emission = ps.emission;
                    if (!emission.enabled) emission.enabled = true;
                    var main = ps.main;
                    main.startSpeed = 0.3f + _vc.Engine.RpmPercent * 1f + _vc.Engine.Load;
                    main.startSize = StartSize;

                    var lifetime = Math.Abs(_vc.Speed) < 0.01f ? 0 : LifeDistance / _vc.Speed;
                    main.startLifetime = Math.Abs(LifeDistance) < 0.01f
                        ? 0
                        : Mathf.Lerp(LifeDistance, lifetime, Mathf.Clamp01(_vc.Speed / LifeDistance));

                    _idleColor = _cleanColor;
                    if (_vc.Transmission.Gear == 0)
                        _idleColor = Color.Lerp(_vaporColor, _cleanColor,
                            Mathf.Clamp01(_vc.Engine.Rpm - _vc.Engine.MinRpm) / 400f);

                    _sootIntensity = Mathf.Clamp01(_vc.Engine.Load) * Soot * 5f;

                    main.startColor = Color.Lerp(_idleColor, _sootColor, _sootIntensity);

                    var speedBias = Math.Abs(LifeDistance) < 0.01f ? 0 : Mathf.Clamp01(_vc.Speed / LifeDistance);
                    var rate = BaseIntensity + IntensityRange * _vc.Engine.RpmPercent;
                    emission.rateOverTime = rate * (1f - speedBias);
                    emission.rateOverDistance = rate * speedBias;
                }
            }
            else
            {
                foreach (var ps in ParticleSystems)
                {
                    var emission = ps.emission;
                    emission.enabled = false;
                }
            }
        }
    }
}
