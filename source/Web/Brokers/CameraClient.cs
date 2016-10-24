using System;
using System.Drawing;
using Microsoft.AspNet.SignalR.Hubs;
using PiCamCV.Common;
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
        
        public void SetImageTransmitPeriod(TimeSpan transmitPeriod)
        {
            _clients.All.SetImageTransmitPeriod(transmitPeriod);
        }

        public void UpdateSettings(PiSettings settings)
        {
            _clients.All.updateSettings(settings);
        }

        public void SendCommand(PanTiltSettingCommand command)
        {
            _clients.All.panTiltCommand(command);
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