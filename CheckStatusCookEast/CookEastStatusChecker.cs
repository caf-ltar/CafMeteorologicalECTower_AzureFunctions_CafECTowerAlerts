using System.IO;
using System.Configuration;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Caf.CafMeteorologicalECTower.CafECTowerAlerts.Alert;
using Nsar.Nodes.CafEcTower.LoggerNet.Extract;
using Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Core;

namespace Caf.CafMeteorologicalECTower.CafECTowerAlerts
{
    public static class CookEastStatusChecker
    {
        [FunctionName("CheckStatusCookEast")]
        public static void Run(
            [BlobTrigger("ectower-cookeast/raw/Flux/{name}",
            Connection = "CookEastFluxConnectionString")] System.IO.Stream inBlob,
            string name,
            TraceWriter log)
        {
            string content;
            using (var reader = new StreamReader(inBlob, true))
            {
                content = reader.ReadToEnd();
            }

            TOA5Extractor extractor = new TOA5Extractor(name, content, -8);
            Tweeter tweeter = new Tweeter(
                ConfigurationManager.AppSettings["TwitterConsumerKey"],
                ConfigurationManager.AppSettings["TwitterConsumerSecret"],
                ConfigurationManager.AppSettings["TwitterAccessToken"],
                ConfigurationManager.AppSettings["TwitterAccessTokenSecret"]);

            StatusChecker statusChecker = new StatusChecker(
                extractor, tweeter);

            string alert = statusChecker.BroadcastStatus();

            if(!string.IsNullOrEmpty(alert))
                log.Info($"Alert: {alert}");
        }
    }
}
