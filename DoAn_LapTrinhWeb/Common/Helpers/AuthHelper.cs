using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace DoAn_LapTrinhWeb.Common.Helpers
{
	public static class AuthHelper
	{
		public static string GetName(this IIdentity identity)
		{
			return identity.GetUserData().Name;
		}
		public static string GetAvartar(this IIdentity identity)
		{
			return identity.GetUserData().Avatar;
		}
		public static int GetUserId(this IIdentity identity)
		{
			return identity.GetUserData().UserId;
		}
		public static string GetEmail(this IIdentity identity)
		{
			return identity.GetUserData().Email;
		}
		public static string GetAddress(this IIdentity identity)
		{
			return identity.GetUserData().Address;
		}
		public static string GetPhoneNumber(this IIdentity identity)
		{
			return identity.GetUserData().PhoneNumber;
		}
		public static string GetRole(this IIdentity identity)
		{
			return identity.GetUserData().RoleCode;
		}
		public static LoggedUserData GetUserData(this IIdentity identity)
		{
			var jsonUserData = HttpContext.Current.User.Identity.Name;
			var userData = JsonConvert.DeserializeObject<LoggedUserData>(jsonUserData);
			return userData;
		}
	}
}