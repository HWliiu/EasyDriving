using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace EasyDriving
{
    [Serializable]
    public class Sound
    {
        [Range(0, 2)] public float MasterVolume = 1f;

        public EngineIdleSoundComponent EngineIdleComponent = new EngineIdleSoundComponent();
        public BlinkerComponent BlinkerComponent = new BlinkerComponent();
        public EngineStartComponent EngineStartComponent = new EngineStartComponent();
        public EngineStopComponent EngineStopComponent = new EngineStopComponent();
        public GearChangeComponent GearChangeComponent = new GearChangeComponent();
        public CrashComponent CrashComponent = new CrashComponent();
        public HornComponent HornComponent = new HornComponent();

        public bool InsideVehicle = true;

        [HideInInspector] public AudioMixerGroup MasterGroup;
        [HideInInspector] public AudioMixerGroup EngineMixerGroup;
        [HideInInspector] public AudioMixerGroup TransmissionMixerGroup;
        [HideInInspector] public AudioMixerGroup CrashMixerGroup;

        private float _interiorAttenuation = -7f;
        private float _lowPassFrequency = 6000f;
        private float _lowPassQ = 1f;

        private bool _wasInsideVehicle;
        private float _originalAttenuation;
        private AudioMixer _audioMixer;

        private VehicleController _vc;


        public void Initialize(VehicleController vc)
        {
            this._vc = vc;

            _audioMixer = Resources.Load("VehicleAudioMixer") as AudioMixer;
            MasterGroup = _audioMixer.FindMatchingGroups("Master")[0];
            EngineMixerGroup = _audioMixer.FindMatchingGroups("Engine")[0];
            TransmissionMixerGroup = _audioMixer.FindMatchingGroups("Transmission")[0];
            CrashMixerGroup = _audioMixer.FindMatchingGroups("Crash")[0];

            _audioMixer.GetFloat("attenuation", out _originalAttenuation);

            EngineIdleComponent.Initialize(vc, EngineMixerGroup);
            BlinkerComponent.Initialize(vc, MasterGroup);
            HornComponent.Initialize(vc, MasterGroup);
            EngineStartComponent.Initialize(vc, MasterGroup);
            EngineStopComponent.Initialize(vc, MasterGroup);
            GearChangeComponent.Initialize(vc, TransmissionMixerGroup);
            CrashComponent.Initialize(vc, CrashMixerGroup);
        }

        public void Update()
        {
            if (!_wasInsideVehicle && InsideVehicle)
            {
                _audioMixer.SetFloat("attenuation", _interiorAttenuation);
                _audioMixer.SetFloat("lowPassFrequency", _lowPassFrequency);
                _audioMixer.SetFloat("lowPassQ", _lowPassQ);
            }
            else if (_wasInsideVehicle && !InsideVehicle)
            {
                _audioMixer.SetFloat("attenuation", _originalAttenuation);
                _audioMixer.SetFloat("lowPassFrequency", 22000f);
                _audioMixer.SetFloat("lowPassQ", 1f);
            }
            _wasInsideVehicle = InsideVehicle;

            EngineIdleComponent.Update();
            BlinkerComponent.Update();
            HornComponent.Update();
            EngineStartComponent.Update();
            EngineStopComponent.Update();
            GearChangeComponent.Update();
        }

        private IEnumerable<SoundComponent> GetAllSound()
        {
            yield return EngineIdleComponent;
            yield return BlinkerComponent;
            yield return EngineStartComponent;
            yield return EngineStopComponent;
            yield return GearChangeComponent;
            yield return CrashComponent;
            yield return HornComponent;
        }

        public void EnableAllSound()
        {
            foreach (var sound in GetAllSound())
                sound.Enable();
        }

        public void DisableAllSound()
        {
            foreach (var sound in GetAllSound())
                sound.Disable();
        }
    }

    [Serializable]
    public abstract class SoundComponent
    {
        [Range(0f, 10f)]
        public float Volume = 1f;
        [Range(0f, 2f)]
        public float Pitch = 1f;
        public AudioClip Clip;
        [HideInInspector]
        public AudioSource Source;

        protected VehicleController _vc;
        protected AudioMixerGroup _audioMixerGroup;

        public void SetVolume(float volume)
        {
            if (!Source) return;
            Source.volume = volume * _vc.Sound.MasterVolume;
        }
        public void SetPitch(float pitch)
        {
            if (!Source) return;
            Source.pitch = pitch;
        }
        public float GetVolume()
        {
            if (!Source) return 0;
            return Source.volume;
        }
        public float GetPitch()
        {
            if (!Source) return 0;
            return Source.pitch;
        }
        public void Enable()
        {
            if (!Source.enabled) Source.enabled = true;
        }
        public void Disable()
        {
            if (Source.enabled) Source.enabled = false;
        }
        public void SetAudioSource(bool play = false, bool loop = false, float volume = 0f, AudioClip clip = null)
        {
            if (Source != null)
            {
                Source.playOnAwake = play;
                Source.loop = loop;
                Source.volume = volume * _vc.Sound.MasterVolume;
                Source.clip = clip;
                Source.outputAudioMixerGroup = _audioMixerGroup;

                if (play)
                    Source.Play();
                else
                    Source.Stop();
            }
        }

        public abstract void Initialize(VehicleController vc, AudioMixerGroup amg);
        public abstract void Update();
    }

    [Serializable]
    public class EngineIdleSoundComponent : SoundComponent
    {
        [Range(0, 1)]
        public float VolumeRange = 0.5f;
        [Range(0, 4)]
        public float PitchRange = 1.2f;
        [Range(0, 1)]
        public float Smoothing = 0.4f;
        [Range(0, 1)]
        public float MaxDistortion = 0.4f;
        public override void Initialize(VehicleController vc, AudioMixerGroup amg)
        {
            this._vc = vc;
            this._audioMixerGroup = amg;

            if (Clip != null)
            {
                Source = vc.gameObject.AddComponent<AudioSource>();
                SetAudioSource(false, true, 0.3f, Clip);
            }
        }

        public override void Update()
        {
            if (Source != null && Clip != null)
            {
                if (_vc.Engine.IsRunning || _vc.Engine.Starting || _vc.Engine.Stopping)
                {
                    if (!Source.isPlaying && Source.enabled) Source.Play();

                    var rpmModifier = _vc.Engine.RpmPercent;
                    var newPitch = Pitch + rpmModifier * PitchRange;
                    SetPitch(Mathf.Lerp(Source.pitch, newPitch, 1f - Smoothing));

                    var volumeModifier = 0f;
                    if (_vc.Transmission.Gear == 0)
                    {
                        volumeModifier = rpmModifier;
                    }
                    else
                    {
                        volumeModifier = rpmModifier * 0.65f + _vc.Input.Throttle * 0.3f;
                    }

                    var newVolume = (Volume + Mathf.Clamp01(volumeModifier) * VolumeRange);

                    _audioMixerGroup.audioMixer.SetFloat("engineDistortion", Mathf.Lerp(0f, MaxDistortion, volumeModifier));

                    if (_vc.Engine.Starting)
                        newVolume = _vc.Engine.StartingPercent * Volume;

                    if (_vc.Engine.Stopping)
                        newVolume = (1f - _vc.Engine.StoppingPercent) * Volume;
                    SetVolume(newVolume);
                }
                else
                {
                    if (Source.isPlaying) Source.Stop();
                    Source.volume = 0;
                    Source.pitch = 0;
                }
            }
        }
    }

    [Serializable]
    public class BlinkerComponent : SoundComponent
    {
        public override void Initialize(VehicleController vc, AudioMixerGroup amg)
        {
            this._vc = vc;
            this._audioMixerGroup = amg;

            if (Clip != null)
            {
                Source = vc.gameObject.AddComponent<AudioSource>();
                SetAudioSource(false, true, Volume, Clip);
            }
        }

        public override void Update()
        {
            if (Source != null && Clip != null)
            {
                if (_vc.Input.LeftBlinker || _vc.Input.RightBlinker)
                {
                    if (!Source.isPlaying && Source.enabled) Source.Play();
                }
                else
                {
                    if (Source.isPlaying) Source.Stop();
                }
            }
        }
    }

    [Serializable]
    public class EngineStartComponent : SoundComponent
    {
        public override void Initialize(VehicleController vc, AudioMixerGroup amg)
        {
            this._vc = vc;
            this._audioMixerGroup = amg;

            if (Clip != null)
            {
                Source = vc.gameObject.AddComponent<AudioSource>();
                SetAudioSource(false, false, Volume, Clip);
            }
        }

        public override void Update()
        {
            if (Source != null && Clip != null)
            {
                if ((_vc.Engine.Starting))
                {
                    if (!Source.isPlaying && Source.enabled)
                        Source.Play();

                    var newVolume = (1f - _vc.Engine.StartingPercent) * Volume;

                    SetVolume(newVolume);
                }
                else
                {
                    Source.volume = 0;
                    Source.Stop();
                }
            }
        }
    }

    [Serializable]
    public class EngineStopComponent : SoundComponent
    {
        public override void Initialize(VehicleController vc, AudioMixerGroup amg)
        {
            this._vc = vc;
            this._audioMixerGroup = amg;

            if (Clip != null)
            {
                Source = vc.gameObject.AddComponent<AudioSource>();
                SetAudioSource(false, false, Volume, Clip);
            }
        }

        public override void Update()
        {
            if (Source != null && Clip != null)
            {
                if ((_vc.Engine.Stopping))
                {
                    if (!Source.isPlaying && Source.enabled)
                        Source.Play();

                    var newVolume = (1f - _vc.Engine.StoppingPercent) * Volume;
                    SetVolume(newVolume);
                }
                else
                {
                    Source.volume = 0;
                    Source.Stop();
                }
            }
        }
    }

    [Serializable]
    public class GearChangeComponent : SoundComponent
    {
        private int _previousGear;

        public override void Initialize(VehicleController vc, AudioMixerGroup amg)
        {
            this._vc = vc;
            this._audioMixerGroup = amg;

            if (Clip != null)
            {
                Source = vc.gameObject.AddComponent<AudioSource>();
                SetAudioSource(false, false, Volume, Clip);
            }
        }

        public override void Update()
        {
            if (Source != null && Clip != null)
            {
                if (_previousGear != _vc.Transmission.Gear && !Source.isPlaying)
                {
                    SetVolume(Volume + Volume * Random.Range(-0.1f, 0.1f));
                    SetPitch(Pitch + Pitch * Random.Range(-0.1f, 0.1f));
                    if (Source.enabled) Source.Play();
                }

                _previousGear = _vc.Transmission.Gear;
            }
        }
    }

    [Serializable]
    public class HornComponent : SoundComponent
    {
        public override void Initialize(VehicleController vc, AudioMixerGroup amg)
        {
            this._vc = vc;
            this._audioMixerGroup = amg;

            if (Clip != null)
            {
                Source = vc.gameObject.AddComponent<AudioSource>();
                SetAudioSource(false, true, Volume, Clip);
            }
        }

        public override void Update()
        {
            if (Source != null && Clip != null)
            {
                if (_vc.Input.Horn)
                {
                    if (!Source.isPlaying && Source.enabled) Source.Play();
                }
                else
                {
                    if (Source.isPlaying) Source.Stop();
                }
            }
        }
    }

    [Serializable]
    public class CrashComponent : SoundComponent
    {
        public override void Initialize(VehicleController vc, AudioMixerGroup amg)
        {
            this._vc = vc;
            this._audioMixerGroup = amg;

            if (Clip != null)
            {
                Source = vc.gameObject.AddComponent<AudioSource>();
                SetAudioSource(false, false, Volume, Clip);
            }
        }

        public override void Update()
        {
        }

        public void Play(Collision collision)
        {
            if (Source != null && Clip != null)
            {
                if (collision != null)
                {
                    SetVolume(Mathf.Clamp01(Mathf.Clamp(_vc.SpeedKPH / 20f, 0.15f, 1f) * Volume));
                    SetPitch(Random.Range(0.6f, 1.4f));
                    if (!Source.isPlaying) Source.Play();
                }
            }
        }
    }
}
