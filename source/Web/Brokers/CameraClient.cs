using System;
using System.Drawing;
using Microsoft.AspNet.SignalR.Hubs;
using PiCamCV.Common.PanTilt.Controllers;
using PiCamCV.ConsoleApp.Runners.PanTilt;

namespace Web
{
    public class CameraClient : ICameraClient
    {
        private readonly IHubConnectionContext<dynamic> _clients;

        public CameraClient(IHubConnectionContext<dynamic> clients)
        {
            _clients = clients;
        }

        public void MoveRelative(PanTiltSetting setting)
        {
            _clients.All.MoveRelative(setting);
        }

        public void MoveAbsolute(PanTiltSetting setting)
        {
            _clients.All.MoveAbsolute(setting);
        }

        public void SetImageTransmitPeriod(TimeSpan transmitPeriod)
        {
            _clients.All.SetImageTransmitPeriod(transmitPeriod);
        }

        public void SetRegionOfInterest(Rectangle roi)
        {
            _clients.All.SetRegionOfInterest(roi);
        }

        public void SetMode(ProcessingMode mode)
        {
            _clients.All.SetMode(mode);
        }
    }
}