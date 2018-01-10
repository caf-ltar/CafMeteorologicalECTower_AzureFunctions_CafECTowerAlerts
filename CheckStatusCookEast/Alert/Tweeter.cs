using Caf.CafMeteorologicalECTower.CafECTowerAlerts.CheckStatusCookEast.Alert;
using System;
using TweetSharp;

namespace Caf.CafMeteorologicalECTower.CafECTowerAlerts.Alert
{
    public class Tweeter : ISendAlerts
    {
        private string consumerKey;
        private string consumerSecret;
        private string accessToken;
        private string accessTokenSecret;

        public Tweeter(
            string consumerKey,
            string consumerSecret,
            string accessToken,
            string accessTokenSecret)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.accessToken = accessToken;
            this.accessTokenSecret = accessTokenSecret;
        }

        public void SendAlert(string alertMessage)
        {
            TwitterService twitterService =
                new TwitterService(consumerKey, consumerSecret);
            twitterService.AuthenticateWith(accessToken, accessTokenSecret);

            TwitterStatus result = twitterService.SendTweet(
                new SendTweetOptions
                {
                    Status = alertMessage
                });

            var s = result;
        }
    }
}
