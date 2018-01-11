using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Alert.Types
{
    public interface IAlertMessage
    {
        string ToString();
        string Filename { get; }
        string Message { get; }
    }
}
