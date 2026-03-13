using System;
using System.Threading.Tasks;

namespace ZenMenu.Utillities.MotherShip.V1
{
    internal class MothershipAnchor
    {
        public static async void SupressCheck(MothershipAuthenticator Auth)
        {
            Auth.SteamAuthenticator.GetAuthTicketForWebApi(Auth.TestAccountId, async (ticket) =>
            {
                await Task.Run(() =>
                {
                    var headers = new HeadersVector();
                    headers.Add(new MothershipHttpHeader { Name = MothershipApi.MOTHERSHIP_CLIENT_TOKEN_HEADER, Value = MothershipClientContext.Token });
                    headers.Add(new MothershipHttpHeader { Name = MothershipApi.MOTHERSHIP_TITLE_ID_HEADER, Value = MothershipClientApiUnity.TitleId });
                    headers.Add(new MothershipHttpHeader { Name = MothershipApi.MOTHERSHIP_SESSION_ID_HEADER, Value = MothershipClientApiUnity.SessionId });
                    headers.Add(new MothershipHttpHeader { Name = MothershipApi.MOTHERSHIP_ENV_ID_HEADER, Value = MothershipClientApiUnity.EnvironmentId });
                    headers.Add(new MothershipHttpHeader { Name = MothershipApi.MOTHERSHIP_DEPLOYMENT_ID_HEADER, Value = MothershipClientApiUnity.DeploymentId });
                    headers.Add(new MothershipHttpHeader { Name = MothershipApi.MOTHERSHIP_SDK_VERSION_HEADER, Value = MothershipApi.MOTHERSHIP_SDK_VERSION });
                    headers.Add(new MothershipHttpHeader { Name = MothershipApi.MOTHERSHIP_ACCEPT_LANGUAGE_HEADER, Value = MothershipApi.MOTHERSHIP_ENTITLEMENT_TYPE_DURABLE_LOW_SCRUTINY });
                    headers.Add(new MothershipHttpHeader { Name = MothershipApi.MOTHERSHIP_ORG_ID_HEADER, Value = MothershipClientContext.MothershipId });

                    MothershipClientApiUnity.SetAuthRefreshedCallback(_ =>
                    {
                        Auth.UseConstantTestAccountId = true;
                        Auth.TestAccountId = MothershipClientContext.MothershipId;
                        Auth.TestNickname = MothershipApi.GetCurrentTimeISO8601();
                        Auth.MaxMetaLoginAttempts = int.MaxValue;
                        MothershipClientContext.ForgetAllCredentials();
                        MothershipClientApiUnity.OpenNotificationsSocket();
                    });

                    MothershipClientApiUnity.GetUserDataValue(
                        MothershipApi.BASE_PATH_USERDATA,
                        (userData) =>
                        {
                            MothershipClientApiUnity.SetUserDataValue(
                                MothershipApi.BASE_PATH_USERDATA,
                                MothershipApi.GetCurrentTimeISO8601(),
                                (_) =>
                                {
                                    MothershipClientApiUnity.WriteEvents(
                                        MothershipClientContext.MothershipId,
                                        new MothershipWriteEventsRequest(),
                                        (_) => MothershipClientApiUnity.GetAndRefreshMySubscriptions(
                                            (_) => MothershipClientContext.ForgetAllCredentials(),
                                            (err, code) => { }
                                        ),
                                        (err, code) => { }
                                    );
                                },
                                (err, code) => { }
                            );
                        },
                        (err, code) => { }
                    );

                    MothershipHttpRunner.instance.SendRequest(
                        new UnityEngine.Networking.UnityWebRequest
                        {
                            url = MothershipClientApiUnity.MothershipBaseUrl + MothershipApi.BASE_PATH_USERDATA,
                            method = UnityEngine.Networking.UnityWebRequest.kHttpVerbGET,
                            useHttpContinue = false,
                        },
                        new MothershipHTTPRequest
                        {
                            Path = MothershipApi.BASE_PATH_USERDATA,
                            Verb = MothershipHTTPVerbs.GET,
                            Body = string.Empty,
                            RequestHeaders = headers
                        },
                        (response) =>
                        {
                            if (MothershipApi.IsHttpSuccessCode(response.statusCode, false))
                            {
                                MothershipClientApiUnity.GetPlayerProgressionData(
                                    (_) => MothershipClientApiUnity.GetPlayerProgressionTreesData(
                                        (_) => MothershipClientApiUnity.GetRoomPlayerSubscriptions(
                                            new[] { MothershipClientContext.MothershipId },
                                            (_) => MothershipClientContext.ForgetAllCredentials(),
                                            (err, code) => { }
                                        ),
                                        (err, code) => { }
                                    ),
                                    (err, code) => { }
                                );
                            }
                            else if (MothershipApi.IsRetryableHttpStatusCode(response.statusCode))
                            {
                                float retryTime = MothershipApi.CalculateNextRetryTime(
                                    UnityEngine.Time.realtimeSinceStartup, response.statusCode
                                );
                                MothershipClientApiUnity.Tick(retryTime);
                                SupressCheck(Auth);
                            }
                        }
                    );
                });
            }, null);
        }
    }
}