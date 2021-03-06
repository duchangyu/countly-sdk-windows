﻿using CountlySDK.CountlyCommon.Server;
using CountlySDK.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CountlySDK
{
    internal class Api : ApiBase
    {
        //==============SINGLETON============
        //fourth version from:
        //http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly Api instance = new Api();
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit    
        static Api() { }
        internal Api() { }
        public static Api Instance { get { return instance; } }
        //-------------SINGLETON-----------------

        protected override async Task<T> Call<T>(string address, Stream imageData = null)
        {
            return await Task.Run<T>(async () =>
            {
                return await CallJob<T>(address, imageData);
            }).ConfigureAwait(false);
        }      

        protected override async Task<string> RequestAsync(string address, String requestData = null, Stream imageData = null)
        {
            try
            {
                UtilityHelper.CountlyLogging("POST " + address);
                
                //make sure stream is at start
                imageData?.Seek(0, SeekOrigin.Begin);
                HttpContent httpContent = (imageData != null) ? new StreamContent(imageData) : null;

                if(requestData != null)
                {
                    //if there is request data to stream, that means it was too long
                    String[] pairsS = UtilityHelper.DecodeDataForURL(requestData).Split('&');

                    KeyValuePair<string, string>[] pairs = new KeyValuePair<string, string>[pairsS.Length];
                    for(int a = 0; a < pairsS.Length; a++)
                    {
                        String[] splitPair = pairsS[a].Split('=');
                        pairs[a] = new KeyValuePair<string, string>(splitPair[0], splitPair[1]);
                    }
                    
                    httpContent = new FormUrlEncodedContent(pairs);
                }

                System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient();
                System.Net.Http.HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(address, httpContent);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return await httpResponseMessage.Content.ReadAsStringAsync();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                UtilityHelper.CountlyLogging("Encountered a exception while making a POST request, " + ex.ToString());
                return null;
            }
        }

        protected override async Task DoSleep(int sleepTime)
        {
            System.Threading.Tasks.Task.Delay(DeviceMergeWaitTime).Wait();
        }
    }
}
