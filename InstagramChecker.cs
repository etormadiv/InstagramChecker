/**
 *   A class that allow to check whether an Instagram email/username is available for registration.
 *   Copyright (C) 2016  Etor Madiv
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU General Public License for more details.
 *
 *   You should have received a copy of the GNU General Public License
 *   along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace InstagramCheckerClient
{
	/// <summary>
	///  This class allow you to use the InstagramChecker class.
	/// </summary>
	public class Program
	{
		public static void Main()
		{
			InstagramChecker ic = new InstagramChecker();
			ic.Initialize();
			
			Console.WriteLine(ic.CheckEmail("hector@gmail.com"));
			Console.WriteLine(ic.CheckEmail("darungrim@gmail.com"));
			
			Console.WriteLine(ic.CheckUsername("hector"));
			Console.WriteLine(ic.CheckUsername("darungrim"));
		}
	}
	
	/// <summary>
	///  This class allow you to check for Instagram email/username availabilty programmatically.
	/// </summary>
	public class InstagramChecker
	{
		/// <summary>
		///  The Url of home page.
		/// </summary>
		private const string homeUrl   = "https://www.instagram.com/";
		
		/// <summary>
		///  The Url of signup.
		/// </summary>
		private const string signUpUrl = "https://www.instagram.com/accounts/emailsignup/?signupFirst=true";
		
		/// <summary>
		///  The Url that is used to check for email/username availabilty.
		/// </summary>
		private const string checkUrl  = "https://www.instagram.com/accounts/web_create_ajax/attempt/";
		
		/// <summary>
		///  The user agent string that will be used to do the request.
		/// </summary>
		private const string userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";
		
		/// <summary>
		/// A container that will hold the cookies.
		/// </summary>
		private CookieContainer cookieContainer;
		
		/// <summary>
		///  Store the value of the csrftoken cookie.
		/// </summary>
		private string csrftoken;
		
		/// <summary>
		///  Store the value of the mid cookie.
		/// </summary>
		private string mid;
		
		/// <summary>
		///  The class constructor.
		/// </summary>
		public InstagramChecker()
		{
			Initialize();
		}
		
		/// <summary>
		///  This method initialize our object to the required values.
		///  It is not intended to be called from your code directly.
		/// </summary>
		private void Initialize()
		{
			var hwr = (HttpWebRequest) WebRequest.Create(signUpUrl);
			hwr.Method 		= "GET";
			hwr.UserAgent 	= userAgent;
			
			cookieContainer     = new CookieContainer();
			hwr.CookieContainer = cookieContainer;
			
			using ( var hwResponse = hwr.GetResponse() )
			{
				CookieCollection cookieCollection = cookieContainer.GetCookies(new Uri(homeUrl));
				csrftoken = cookieCollection["csrftoken"].Value;
				mid       = cookieCollection["mid"].Value;
			}
		}
		
		/// <summary>
		///  Check for email registration availabilty in Instagram website.
		/// </summary>
		/// <param name="email"> An email address that will be checked for availabilty </param>
		/// <returns>
		///  Return true if the provided email is available for registration.
		/// </returns>
		public bool CheckEmail(string email)
		{
			var hwr = (HttpWebRequest) WebRequest.Create(checkUrl);
			hwr.Method 		= "POST";
			hwr.ContentType = "application/x-www-form-urlencoded";
			hwr.UserAgent 	= userAgent;
			hwr.Referer     = signUpUrl;
			
			hwr.Headers.Add("X-Requested-With", "XMLHttpRequest");
			hwr.Headers.Add("X-Csrftoken"     , csrftoken);
			hwr.Headers.Add("X-Instagram-Ajax", "1");
			
			hwr.CookieContainer = cookieContainer;
			
			byte[] data = Encoding.ASCII.GetBytes("email=" + email);
			
			using( var requestStream = hwr.GetRequestStream() )
			{
				requestStream.Write(data, 0, data.Length);
			}
			
			string responseRawContent = null;
			
			using ( var hwResponse = (HttpWebResponse) hwr.GetResponse() )
			{
				using( var responseStream = hwResponse.GetResponseStream() )
				{
					using( var reader = new StreamReader(responseStream) )
					{
						responseRawContent = reader.ReadToEnd();
					}
				}
			}
			
			JavaScriptSerializer deserializer = new JavaScriptSerializer();
			InstagramResponse instagramResponse = deserializer.Deserialize<InstagramResponse>(responseRawContent);
			
			return (instagramResponse.errors.email.Count == 0);
		}
		
		/// <summary>
		///  Check for username registration availabilty in Instagram website.
		/// </summary>
		/// <param name="username"> A username that will be checked for availabilty </param>
		/// <returns>
		///  Return true if the provided username is available for registration.
		/// </returns>
		public bool CheckUsername(string username)
		{
			var hwr = (HttpWebRequest) WebRequest.Create(checkUrl);
			hwr.Method 		= "POST";
			hwr.ContentType = "application/x-www-form-urlencoded";
			hwr.UserAgent 	= userAgent;
			hwr.Referer     = signUpUrl;
			
			hwr.Headers.Add("X-Requested-With", "XMLHttpRequest");
			hwr.Headers.Add("X-Csrftoken"     , csrftoken);
			hwr.Headers.Add("X-Instagram-Ajax", "1");
			
			hwr.CookieContainer = cookieContainer;
			
			byte[] data = Encoding.ASCII.GetBytes("username=" + username);
			
			using( var requestStream = hwr.GetRequestStream() )
			{
				requestStream.Write(data, 0, data.Length);
			}
			
			string responseRawContent = null;
			
			using ( var hwResponse = (HttpWebResponse) hwr.GetResponse() )
			{
				using( var responseStream = hwResponse.GetResponseStream() )
				{
					using( var reader = new StreamReader(responseStream) )
					{
						responseRawContent = reader.ReadToEnd();			
					}
				}
			}
			
			JavaScriptSerializer deserializer = new JavaScriptSerializer();
			InstagramResponse instagramResponse = deserializer.Deserialize<InstagramResponse>(responseRawContent);
			
			return (instagramResponse.errors.username.Count == 0);
		}
	}
	
	/// <summary>
	///  This class allow us to store some information about the Instagram Response.
	/// </summary>
	public class InstagramResponse
	{
		public bool dryrun_passed;
		public string status;
		List<string> username_suggestions = new List<string>();
		
		public class InstagramError
		{
			public List<string> username = new List<string>();
			public List<string> password = new List<string>();
			public List<string> email = new List<string>();
		}
		
		public InstagramError errors;
		public bool account_created;
	}
}