using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Alert
{
    public interface ISendAlerts
    {
        void SendAlert(string alertMessage);
    }
}
