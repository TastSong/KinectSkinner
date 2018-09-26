using System;

[Serializable]
public class FacesCollection
{
	public Face[] faces;
}


/// <summary>
/// The detected face entity.
/// </summary>
[Serializable]
public class Face
{
	
    /// <summary>
    /// Gets or sets the face identifier.
    /// </summary>
    /// <value>
    /// The face identifier.
    /// </value>
	public string faceId;

    /// <summary>
    /// Gets or sets the face rectangle.
    /// </summary>
    /// <value>
    /// The face rectangle.
    /// </value>
	public FaceRectangle faceRectangle;

    /// <summary>
    /// Gets or sets the face landmarks.
    /// </summary>
    /// <value>
    /// The face landmarks.
    /// </value>
	public FaceLandmarks faceLandmarks;

    /// <summary>
    /// Gets or sets the face attributes.
    /// </summary>
    /// <value>
    /// The face attributes.
    /// </value>
	public FaceAttributes faceAttributes;

}
