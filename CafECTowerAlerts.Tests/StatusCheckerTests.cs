using Microsoft.Azure.WebJobs.Host;
using System.Diagnostics;
using Xunit;
using System.Collections.Generic;
using Nsar.Nodes.CafEcTower.LoggerNet.Extract;
using System.IO;
using Caf.CafMeteorologicalECTower.CafECTowerAlerts.Alert;
using Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Core;
using System.Linq;
using Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Alert;

namespace Caf.CafMeteorologicalECTower.CafECTowerAlerts
{
    public class StatusCheckerTests
    {
        private string fileWithBadNAN = 
            @"Assets/CookEastEcTower_Flux_Raw_2017_11_03_1300_badNAN.dat";
        private string fileWithNANOkLocations = 
            @"Assets/CookEastEcTower_Flux_Raw_2017_11_03_1300_okNAN.dat";
        private string fileWithBadDataAtSecondRow = 
            @"Assets/CookEastEcTower_Flux_Raw_2017_11_03_1300_2linesBadCO2.dat";
        private string fileWithBadDataAtSecondRowAndNAN = 
            @"Assets/CookEastEcTower_Flux_Raw_2017_11_03_1300_2linesBadCO2BadNAN.dat";

        [Fact]
        public void CheckStatus_HasNanAtOkLocations_NoAlert()
        {
            // Arrange
            
            var s = new FileStream(fileWithNANOkLocations, FileMode.Open);
            string contents = convertStreamToString(s);
            var e = new TOA5Extractor(
                "CookEastEcTower_Flux_Raw_2017_11_03_1300_okNAN.dat",
                contents,
                -8);
            var a = new MockTweeter();
            StatusChecker sut = new StatusChecker(e, a);
        
            // Act
            var alerts = sut.CheckStatus();
        
            // Assert
            Assert.Empty(alerts);
        }

        [Fact]
        public void CheckStatus_HasNan_CreatesSingleAlert()
        {
            // Arrange
            var s = new FileStream(fileWithBadNAN, FileMode.Open);
            string contents = convertStreamToString(s);
            var e = new TOA5Extractor(
                "CookEastEcTower_Flux_Raw_2017_11_03_1300_badNAN.dat",
                contents,
                -8);
            var a = new MockTweeter();
            StatusChecker sut = new StatusChecker(e, a);

            // Act
            var alerts = sut.CheckStatus();

            // Assert
            Assert.Single(alerts);
        }

        [Fact]
        public void CheckStatus_HasBadValueAtSecondRow_CreatesExpectedAlertString()
        {
            // Arrange
            var s = new FileStream(fileWithBadDataAtSecondRow, FileMode.Open);
            string contents = convertStreamToString(s);
            var e = new TOA5Extractor(
                "CookEastEcTower_Flux_Raw_2017_11_03_1300_2linesBadCO2.dat",
                contents,
                -8);
            var a = new MockTweeter();
            StatusChecker sut = new StatusChecker(e, a);
            string expected = "[W] CookEastEcTower_Flux_Raw_2017_11_03_1300_2linesBadCO2.dat: CO2_sig_strgth_Min < 0.8 (0.7).";

            // Act
            var alerts = sut.CheckStatus();

            // Assert
            Assert.Single(alerts);
            Assert.Equal(
                expected,
                string.Join("\r\n", alerts));
        }

        [Fact]
        public void CheckStatus_HasBadValueAtSecondRowAndNAN_CreatesExpectedAlertString()
        {
            // Arrange
            var s = new FileStream(fileWithBadDataAtSecondRowAndNAN, FileMode.Open);
            string contents = convertStreamToString(s);
            var e = new TOA5Extractor(
                "CookEastEcTower_Flux_Raw_2017_11_03_1300_2linesBadCO2BadNAN.dat",
                contents,
                -8);
            var a = new MockTweeter();
            StatusChecker sut = new StatusChecker(e, a);
            string expected = "[E] CookEastEcTower_Flux_Raw_2017_11_03_1300_2linesBadCO2BadNAN.dat: Null values > 3.\r\n[W] CookEastEcTower_Flux_Raw_2017_11_03_1300_2linesBadCO2BadNAN.dat: CO2_sig_strgth_Min < 0.8 (0.7).";

            // Act
            var alerts = sut.CheckStatus();

            // Assert
            Assert.Equal(2, alerts.Count);
            Assert.Equal(
                expected,
                string.Join("\r\n", alerts));
        }

        //[Fact]
        //public void Run_QuickTest()
        //{
        //    // Arrange
        //    var s = new FileStream(fileWithBadNAN, FileMode.Open);
        //    var t = new TraceWriterStub(TraceLevel.Verbose);
        //
        //    CookEastStatusChecker.Run(s, "CookEastEcTower_Flux_Raw_2017_11_03_1300_badNAN.dat", t);
        //}
        private string convertStreamToString(Stream stream)
        {
            string s;

            using (var reader = new StreamReader(stream, true))
            {
                s = reader.ReadToEnd();
            }

            return s;
        }


        public class TraceWriterStub : TraceWriter
        {
            protected TraceLevel _level;
            protected List<TraceEvent> _traces;
            public string TraceString { get; set; }

            public TraceWriterStub(TraceLevel level) : base(level)
            {
                _level = level;
                _traces = new List<TraceEvent>();
            }

            public override void Trace(TraceEvent traceEvent)
            {
                _traces.Add(traceEvent);
                TraceString = traceEvent.Message;
            }

            public override string ToString()
            {
                return TraceString;
            }

            public List<TraceEvent> Traces => _traces;
        }
        private class MockTweeter : ISendAlerts
        {
            public string InspectAlertString { get; private set; }

            public void SendAlert(string alertMessage)
            {
                InspectAlertString = alertMessage;
            }
        }
    }
}
