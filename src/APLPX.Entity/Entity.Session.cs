using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace APLPX.Entity
{
    //public class NullT { }

    //[DataContract]
    //public class Session<T> where T : class
    //{
    //    [DataMember]
    //    public User User { get; set; }
    //    [DataMember]
    //    public T Data { get; set; }
    //    [DataMember]
    //    public bool AppOnline { get; set; }
    //    [DataMember]
    //    public bool Authenticated { get; set; }
    //    [DataMember]
    //    public bool SqlAuthorization { get; set; }
    //    [DataMember]
    //    public bool WinAuthorization { get; set; }
    //    [DataMember]
    //    public bool SessionOk { get; set; }
    //    [DataMember]
    //    public int ClientCommand { get; set; }
    //    [DataMember]
    //    public string ClientMessage { get; set; }
    //    [DataMember]
    //    public string ServerMessage { get; set; }
    //    [DataMember]
    //    public string SqlKey { get; set; }
    //    [DataMember]
    //    public List<Entity.Module> Modules { get; set; }

    //    public Session<Tdata> Clone<Tdata>(Tdata data) where Tdata : class {

    //        return this.InitCommon<Tdata>(this, data);
    //    }

    //    public static Session<Tdata> Clone<Tdata>(Session<T> session, Tdata data = null) where Tdata : class {

    //        return session.InitCommon<Tdata>(session, data);
    //    }

    //    private Session<Tdata> InitCommon<Tdata>(Session<T> session, Tdata data) where Tdata : class {

    //        return new Session<Tdata> 
    //        {
    //            SessionOk = false,
    //            ClientMessage = string.Empty,
    //            ServerMessage = string.Empty,

    //            User = session.User,
    //            SqlKey = session.SqlKey,
    //            AppOnline = session.AppOnline,
    //            Authenticated = session.Authenticated,
    //            SqlAuthorization = session.SqlAuthorization,
    //            WinAuthorization = session.WinAuthorization,
    //            Data = data
    //        };
    //    }
    //}
}
