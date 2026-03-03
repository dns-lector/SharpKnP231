using SharpKnP321.Users.Dal.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpKnP321.Users.Models
{
    internal class SignInModel
    {
        public UserData    UserData    { get; set; } = null!;
        public UserAccess  UserAccess  { get; set; } = null!;
        public AccessToken AccessToken { get; set; } = null!;
    }
}
/* Моделі - комплекси, що поєднують Сутності, 
 * а також інші дані, що передаються системою
 */