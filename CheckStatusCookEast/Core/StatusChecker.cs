using Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Alert;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nsar.Nodes.CafEcTower.LoggerNet.Extract;
using Nsar.Nodes.Models.LoggerNet.TOA5.DataTables;
using Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Alert.Types;
using System.Reflection;

namespace Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Core
{
    public class StatusChecker
    {
        private readonly TOA5Extractor extractor;
        private readonly ISendAlerts alerter;

        public StatusChecker(
            TOA5Extractor extractor,
            ISendAlerts alerter)
        {
            this.extractor = extractor;
            this.alerter = alerter;
        }

        public List<IAlertMessage> CheckStatus()
        {
            List<Flux> obs = extractor.GetObservations<Flux>();
            List<IAlertMessage> alerts = new List<IAlertMessage>();

            List<IAlertMessage> nullAlerts = checkNulls(obs);
            if (nullAlerts.Count > 0)
                alerts = alerts.Concat(nullAlerts).ToList();

            List<IAlertMessage> boundAlerts = checkBounds(obs);
            if (boundAlerts != null)
                alerts = alerts.Concat(boundAlerts).ToList();

            return alerts;
        }

        public bool BroadcastStatus()
        {
            List<IAlertMessage> alerts = CheckStatus();
            bool sentAlert = false;

            if(alerts.Count > 0)
            {
                alerter.SendAlert(string.Join("\r\n", alerts));
                sentAlert = true;
            }

            return sentAlert;
        }

        private List<IAlertMessage> checkNulls(List<Flux> obs)
        {
            List<string> propertiesOkToBeNull = new List<string>()
            {
                "tdr315_wc_Avg2", "profile_tdr315_wc_Avg3",
                "profile_tdr315_wc_Avg4", "profile_tdr315_wc_Avg5",
                "profile_tdr315_wc_Avg6"
            };

            List<string> propertiesWithNull = new List<string>();


            foreach (Flux ob in obs)
            {
                PropertyInfo[] properties = ob.GetType().GetProperties();

                foreach(PropertyInfo property in properties)
                {
                    if(!propertiesOkToBeNull.Contains(property.Name))
                    {
                        var val = property.GetValue(ob, null);
                        if(val == null || string.IsNullOrEmpty(val.ToString()))
                        {
                            propertiesWithNull.Add(property.Name);
                        }
                    } 
                }
            }

            List<IAlertMessage> nullAlerts = new List<IAlertMessage>();

            if(propertiesWithNull.Count > 0)
            {
                nullAlerts.Add(new Error(
                    extractor.FileName,
                    $"Null values: {String.Join(",", propertiesWithNull)}"));
            }

            return nullAlerts;
        }
        
        private List<IAlertMessage> checkBounds(List<Flux> obs)
        {
            List<IAlertMessage> boundAlerts = new List<IAlertMessage>();

            foreach (Flux ob in obs)
            {
                if (ob.CO2_sig_strgth_Min < 0.8 && ob.Precipitation_Tot == 0)
                    boundAlerts.Add(
                        new Warning(
                            extractor.FileName,
                            $"CO2_sig_strgth_Min < 0.8 ({ob.CO2_sig_strgth_Min})"));

                if (ob.H2O_sig_strgth_Min < 0.8 && ob.Precipitation_Tot == 0)
                    boundAlerts.Add(
                        new Warning(
                            extractor.FileName,
                            $"H2O_sig_strgth_Min < 0.8 ({ob.H2O_sig_strgth_Min})"));

                if (ob.batt_volt_Avg < 12.5 && ob.batt_volt_Avg >= 12.1)
                    boundAlerts.Add(
                        new Information(
                            extractor.FileName,
                            $"batt_volt_Avg low ({ob.batt_volt_Avg})"));
                
                if (ob.batt_volt_Avg < 12.1 && ob.batt_volt_Avg >= 11.6)
                    boundAlerts.Add(
                        new Warning(
                            extractor.FileName,
                            $"batt_volt_Avg low ({ob.batt_volt_Avg})"));

                if (ob.batt_volt_Avg < 11.6)
                    boundAlerts.Add(
                            new Error(
                                extractor.FileName,
                                $"batt_volt_Avg < 11.6 ({ob.batt_volt_Avg})"));

                if (ob.sonic_samples_Tot < 13500)
                    boundAlerts.Add(
                            new Warning(
                                extractor.FileName,
                                $"sonic_samples_Tot < 13500 ({ob.sonic_samples_Tot})"));

                if (ob.CO2_samples_Tot < 13500)
                    boundAlerts.Add(
                            new Warning(
                                extractor.FileName,
                                $"CO2_samples_Tot < 13500 ({ob.CO2_samples_Tot})"));

                if (ob.H2O_samples_Tot < 13500)
                    boundAlerts.Add(
                            new Warning(
                                extractor.FileName,
                                $"H2O_samples_Tot < 13500 ({ob.H2O_samples_Tot})"));

                if (ob.door_is_open_Hst > 0)
                    boundAlerts.Add(
                            new Information(
                                extractor.FileName,
                                $"door_is_open_Hst > 0 ({ob.door_is_open_Hst})"));
            }

            return boundAlerts;
        }
    }
}
