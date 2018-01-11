using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Alert.Types
{
    public class Information : Basic
    {
        public Information(
            string filename,
            string message) : base(filename, message)
        {
        }

        public override string ToString()
        {
            string s = $"[I] {base.ToString()}";
            return s;
        }
    }
}
