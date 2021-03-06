﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Action.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetActionFromRequest<T>(HttpRequestMessage req);
        string GetTouchpointId(HttpRequestMessage req);
        string GetApimURL(HttpRequestMessage req);
    }
}