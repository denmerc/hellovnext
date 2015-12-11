using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace APLPX.Entity
{
    [DataContract]
    public class User
    {
        [DataMember]
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        //[DataMember]
        //public UserCredential Credential { get; set; }

        [DataMember]
        public string Email { get; set; } //TODO: not storing this?
        //[DataMember]
        //public string Name { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Greeting { get; set; }
        [DataMember]
        public DateTime LoginDate { get; set; }
        [DataMember]
        public DateTime CreatedDate { get; set; }
        [DataMember]
        public bool IsActive { get; set; }        
        public List<UserRoleType> Roles { get; set; }
        public List<Folder> Folders { get; set; }
        public List<Module> Modules { get; set; }

    }

    public class Folder
    {
        public int ModuleFeatureId { get; set; }
        public int FolderId { get; set; }
        public string Name { get; set; }
    }

    //[DataContract]
    //public class UserRole
    //{
    //    [DataMember]
    //    public int Id { get; set; }
    //    [DataMember]
    //    public string Name { get; set; }
    //    [DataMember]
    //    public string Description { get; set; }
    //}

    [DataContract]
    public class UserCredential
    {
        [DataMember]
        public string Login { get; set; }
        [DataMember]
        public string OldPassword { get; set; }
        [DataMember]
        public string NewPassword { get; set; }
    }
}
