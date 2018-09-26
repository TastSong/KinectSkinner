using System;

/// <summary>
/// The face attributes class that holds Age/Gender/Head Pose/Smile/Facial Hair information.
/// </summary>
[Serializable]
public class FaceAttributes
{
	
    /// <summary>
    /// Gets or sets the age value.
    /// </summary>
    /// <value>
    /// The age value.
    /// </value>
	public float age;

    /// <summary>
    /// Gets or sets the gender.
    /// </summary>
    /// <value>
    /// The gender.
    /// </value>
	public string gender;

    /// <summary>
    /// Gets or sets the head pose.
    /// </summary>
    /// <value>
    /// The head pose.
    /// </value>
	public HeadPose headPose;

    /// <summary>
    /// Gets or sets the smile value. Represents the confidence of face is smiling.
    /// </summary>
    /// <value>
    /// The smile value.
    /// </value>
	public float smile;

    /// <summary>
    /// Gets or sets the facial hair.
    /// </summary>
    /// <value>
    /// The facial hair.
    /// </value>
	public FacialHair facialHair;

}
