using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Alert.Types
{
    public class Basic : IAlertMessage
    {
        private readonly string filename;
        private readonly string message;

        public Basic(string filename, string message)
        {
            this.filename = filename;
            this.message = message;
        }

        public override string ToString()
        {
            string s = $"{filename}: {message}.";
            return s;
        }
    }
}
