﻿namespace Oppo.ObjectModel
{
    public interface IOpcuaServerApp : IOpcuaapp
    {
        string Url { get; set; }
		string Port { get; set; }
    }


}