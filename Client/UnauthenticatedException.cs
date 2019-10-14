using System;
using System.Net.Http;

namespace Client
{
	public class UnauthenticatedException : HttpRequestException
	{
		public UnauthenticatedException(string message, Exception inner) :
			base(message, inner)
		{
		}

		public UnauthenticatedException(string message) :
			base(message)
		{
		}

		public UnauthenticatedException()
		{
		}
	}
}