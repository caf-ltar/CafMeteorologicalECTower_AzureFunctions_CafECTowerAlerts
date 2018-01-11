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

        const int TWITTER_CHAR_LIMIT = 280;

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

        public string BroadcastStatus()
        {
            List<IAlertMessage> alerts = CheckStatus();
            string alertString = "";

            if(alerts.Count > 0)
            {
                string alert = string.Join("\r\n", alerts);
                if (alert.Length > TWITTER_CHAR_LIMIT)
                {
                    alert = getExceedsCharMessage(alerts);
                }
                alerter.SendAlert(alert);
                alertString = alert;
            }

            return alertString;
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
                if (propertiesWithNull.Count > 3)
                {
                    nullAlerts.Add(new Error(
                        extractor.FileName,
                        $"Null values > 3"));
                }
                else
                {
                    nullAlerts.Add(new Error(
                        extractor.FileName,
                        $"Null values: {String.Join(",", propertiesWithNull)}"));
                }
            }

            return nullAlerts;
        }
        
        private List<IAlertMessage> checkBounds(List<Flux> obs)
        {
            List<IAlertMessage> boundAlerts = new List<IAlertMessage>();

            foreach (Flux ob in obs)
            {
                if (ob.CO2_sig_strgth_Min < 0.8 && ob.RH_probe_Avg < 90)
                    boundAlerts.Add(
                        new Warning(
                            extractor.FileName,
                            $"CO2_sig_strgth_Min < 0.8 ({getShortString(ob.CO2_sig_strgth_Min)})"));

                if (ob.H2O_sig_strgth_Min < 0.8 && ob.RH_probe_Avg < 90)
                    boundAlerts.Add(
                        new Warning(
                            extractor.FileName,
                            $"H2O_sig_strgth_Min < 0.8 ({getShortString(ob.H2O_sig_strgth_Min)})"));

                if (ob.batt_volt_Avg < 12.5 && ob.batt_volt_Avg >= 12.1)
                    boundAlerts.Add(
                        new Information(
                            extractor.FileName,
                            $"batt_volt_Avg low ({getShortString(ob.batt_volt_Avg)})"));
                
                if (ob.batt_volt_Avg < 12.1 && ob.batt_volt_Avg >= 11.6)
                    boundAlerts.Add(
                        new Warning(
                            extractor.FileName,
                            $"batt_volt_Avg low ({getShortString(ob.batt_volt_Avg)})"));

                if (ob.batt_volt_Avg < 11.6)
                    boundAlerts.Add(
                            new Error(
                                extractor.FileName,
                                $"batt_volt_Avg < 11.6 ({getShortString(ob.batt_volt_Avg)})"));

                if (ob.sonic_samples_Tot < 13500)
                    boundAlerts.Add(
                            new Warning(
                                extractor.FileName,
                                $"sonic_samples_Tot < 13500 ({getShortString(ob.sonic_samples_Tot)})"));

                if (ob.CO2_samples_Tot < 13500)
                    boundAlerts.Add(
                            new Warning(
                                extractor.FileName,
                                $"CO2_samples_Tot < 13500 ({getShortString(ob.CO2_samples_Tot)})"));

                if (ob.H2O_samples_Tot < 13500)
                    boundAlerts.Add(
                            new Warning(
                                extractor.FileName,
                                $"H2O_samples_Tot < 13500 ({getShortString(ob.H2O_samples_Tot)})"));

                if (ob.door_is_open_Hst > 0)
                    boundAlerts.Add(
                            new Information(
                                extractor.FileName,
                                $"door_is_open_Hst > 0 ({getShortString(ob.door_is_open_Hst)})"));
            }

            return boundAlerts;
        }

        private string getShortString(double? val)
        {
            return String.Format("{0:0.0}", val);
        }
        private string getExceedsCharMessage(List<IAlertMessage> alerts)
        {
            int numFiles = 0;
            string currFilename = "";
            foreach(IAlertMessage m in alerts)
            {
                if(currFilename != m.Filename)
                {
                    numFiles++;
                    currFilename = m.Filename;
                }
            }
            return $"{alerts.Count} error(s) from {numFiles} file(s) exceeds Twitters char limit";
        }
    }
}
