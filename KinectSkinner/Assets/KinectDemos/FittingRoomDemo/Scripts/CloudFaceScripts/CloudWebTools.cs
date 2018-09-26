using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CloudWebTools 
{
	/// <summary>
	/// Gets the status code.
	/// </summary>
	/// <returns>The status code.</returns>
	/// <param name="request">Web request.</param>
	public static int GetStatusCode(WWW request)
	{
		int status = -1;

		if(request != null && request.responseHeaders != null && request.responseHeaders.ContainsKey("STATUS"))
		{
			string statusLine = request.responseHeaders["STATUS"];
			string[] statusComps = statusLine.Split(' ');

			if (statusComps.Length >= 3)
			{
				int.TryParse(statusComps[1], out status);
			}
		}

		return status;
	}

	/// <summary>
	/// Determines if the response status is error.
	/// </summary>
	/// <returns><c>true</c> if the response status is error; otherwise, <c>false</c>.</returns>
	/// <param name="request">Web request.</param>
	public static bool IsErrorStatus(WWW request)
	{
		int status = GetStatusCode(request);
		return (status >= 300);
	}

	/// <summary>
	/// Gets the status message.
	/// </summary>
	/// <returns>The status message.</returns>
	/// <param name="request">Web request.</param>
	public static string GetStatusMessage(WWW request)
	{
		string message = string.Empty;
		
		if(request != null && request.responseHeaders != null && request.responseHeaders.ContainsKey("STATUS"))
		{
			string statusLine = request.responseHeaders["STATUS"];
			string[] statusComps = statusLine.Split(' ');

			for(int i = 2; i < statusComps.Length; i++)
			{
				message += " " + statusComps[i];
			}
		}
		
		return message.Trim();
	}
	

}
