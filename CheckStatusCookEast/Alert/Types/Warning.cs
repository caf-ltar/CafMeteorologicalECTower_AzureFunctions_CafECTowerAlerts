using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Alert.Types
{
    public class Warning : Basic
    {
        public Warning(
           string filename,
           string message) : base(filename, message)
        {
        }

        public override string ToString()
        {
            string s = $"[WARNING] {base.ToString()}";
            return s;
        }
    }
}
