﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PiCamCV.ConsoleApp.Runners;
using PiCamCV.Interfaces;
using Raspberry.IO.Components.Controllers.Pca9685;
using RPi.Pwm;
using Emgu.CV;
using Emgu.CV.Structure;
using PiCamCV.Common;

namespace PiCamCV.ConsoleApp.Runners
{

    public  class ServoSorter : CameraConsumerRunner
    {
        private int _waitTimeMs;
        PwmControl _pwmControl;
        bool _objectCurrentlyDetected;
        private int _servoPosition;
        private Stopwatch _debounceWatch;

        public ColourDetectSettings Settings { get; set; }
        
        public ServoSorter(
            ICaptureGrab capture
            ,ConsoleOptions options) : base(capture)
        {
            _waitTimeMs = 100;
            _servoPosition = 50;

            Settings = options.ColourSettings;

            _debounceWatch = new Stopwatch();

            var deviceFactory = new Pca9685DeviceFactory();
            var device = deviceFactory.GetDevice(options.UseFakeDevice);
            SetLogLevel(device);
            
            _pwmControl = new PwmControl(device);
            _pwmControl.Init();
        }

        private void SetLogLevel(IPwmDevice device)
        {
            var stubPwmDevice = device as PwmDeviceStub;
            if (stubPwmDevice != null)
            {
                // Remove sign of life which is too chatty
                stubPwmDevice.LogChannels.Remove(PwmChannel.C0);
            }
        }

        public override void Run()
        {
            StartLedSignOfLifeAsync();
            SweeperToGreen();
            base.Run();
        }
        public override void ImageGrabbedHandler(object sender, EventArgs e)
        {
            using (var matCaptured = new Mat())
            {
                CameraCapture.Retrieve(matCaptured);
                var detector = new ColourDetector();
                var input = new ColourDetectorInput
                {
                    Captured = matCaptured
                   ,Settings = Settings
                   ,SetCapturedImage = false
                };

                var result = detector.Process(input);

                if (result.IsDetected)
                {
                    if (!_objectCurrentlyDetected)
                    {
                        _debounceWatch.Start();
                        _objectCurrentlyDetected = true;
                    }
                    SweeperToRed();
                    Log.Info(m=>m("Red detected! {0}", result));
                }
                else
                {
                    var isInDebouncePeriod = _debounceWatch.IsRunning && _debounceWatch.ElapsedMilliseconds < 800;
                    if (_objectCurrentlyDetected && !isInDebouncePeriod)
                    {
                        _debounceWatch.Reset();
                        Log.Info(m => m("Red gone"));
                        SweeperToGreen();
                        _objectCurrentlyDetected = false;
                    }
                }
            }
        }

        private void SweeperToGreen()
        {
            ServoSet(65);
        }

        private void SweeperToRed()
        {
            ServoSet(53);
        }

        private async Task StartLedSignOfLifeAsync()
        {
            int i = 0;
            Log.Info("Sign of life: on");
            while (!Stopping && i < int.MaxValue)
            {
                i++;
                var percentage = (int)(50 * (1 + Math.Sin(i / 20f)));
                _pwmControl.Led0.On(percentage);
                await Task.Delay(50).ConfigureAwait(false);
            }
            _pwmControl.Led0.Off();
            Log.Info("Sign of life: off");
        }

        private void ServoNudge(int nudge)
        {
            _servoPosition+=nudge;
            _pwmControl.Servo.MoveTo(_servoPosition);
            Log.Info(m => m("Servo={0}", _servoPosition));
        }

        private void ServoSet(int percent)
        {
            _servoPosition = percent;
            _pwmControl.Servo.MoveTo(_servoPosition);
            Log.Info(m => m("Servo={0}", _servoPosition));
        }

        //protected override void Stop()
        //{
        //    _pwmControl.
        //    base.Stop();
        //}

        public override void HandleKey(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.T:
                    _waitTimeMs -= 10;
                    Log.Info(m =>m("_waitTimeMs={0}", _waitTimeMs));
                    break;
                case ConsoleKey.RightArrow:
                    ServoNudge(1);
                    break;
                case ConsoleKey.LeftArrow:
                    ServoNudge(-1);
                    break;
                case ConsoleKey.M:
                    ServoSet(50);
                    break;
                default:
                    Log.Info(m => m("Ignoring key {0}", keyInfo.Key));
                    break;
            }
        }
    }
}

