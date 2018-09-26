using System;
using System.Net;
using System.Runtime.Serialization;

/// <summary>
/// Represents client error with detailed error message and error code
/// </summary>
[Serializable]
public class ClientError
{
	
	/// <summary>
	/// Gets or sets the detailed error message and error code
	/// </summary>
	public ClientExceptionMessage error;

}


/// <summary>
/// Represents detailed error message and error code
/// </summary>
[Serializable]
public class ClientExceptionMessage
{
	
	/// <summary>
	/// Gets or sets the detailed error code
	/// </summary>
	public string code;

	/// <summary>
	/// Gets or sets the detailed error message
	/// </summary>
	public string message;

}

/// <summary>
/// Represents client error with detailed error message and error code
/// </summary>
[Serializable]
public class ServiceError
{
    /// <summary>
    /// Gets or sets the detailed error message and error code
    /// </summary>
	public string statusCode;

    /// <summary>
    /// Gets or sets the detailed error message and error code
    /// </summary>
	public string message;

}

