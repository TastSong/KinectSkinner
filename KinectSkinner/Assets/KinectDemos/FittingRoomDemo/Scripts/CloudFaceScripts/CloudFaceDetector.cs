using UnityEngine;
using System.Collections;

/// <summary>
/// This interface needs to be implemented by all cloud-face listeners
/// </summary>
public interface CloudFaceListenerInterface
{
	/// <summary>
	/// User face gets detected.
	/// </summary>
	/// <param name="userIndex">Player index of the user.</param>
	/// <param name="gender">User's gender.</param>
	/// <param name="age">User's age in years.</param>
	/// <param name="smile">Smile percentage.</param>
	void UserFaceDetected(int userIndex, UserGender gender, float age, float smile);
}

public enum UserGender : int { Unisex = 0, Male = 1, Female = 2 };


public class CloudFaceDetector : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("UI-text used for information messages.")]
	public UnityEngine.UI.Text infoText;

	[HideInInspector]
	public string userGender;  // detected user gender

	[HideInInspector]
	public float userAge;  // detected user age

	[HideInInspector]
	public float userSmile;  // detected user smile percentage

	// last detected userId;
	private long lastUserId = 0;

	private static CloudFaceDetector instance = null;


	void Awake()
	{
		instance = this;
	}


	/// <summary>
	/// Gets the CloudFaceDetector instance.
	/// </summary>
	/// <value>The CloudFaceDetector instance.</value>
	public static CloudFaceDetector Instance
	{
		get
		{
			return instance;
		}
	}


	void Update () 
	{
		KinectManager manager = KinectManager.Instance;

		if(manager && manager.IsInitialized ()) 
		{
			long userId = manager.GetUserIdByIndex(playerIndex);

			if (userId != 0) 
			{
				if (lastUserId != userId) 
				{
					lastUserId = userId;

					Texture2D texImage = manager.GetUsersClrTex2D();
					Vector3 texScale = manager.GetColorImageScale();
					Texture2D texClipped = ClipUserImage(userId, texImage, texScale);

					StartCoroutine(DoFaceDetection(texClipped));
				}
			}

			if(userId == 0 && userId != lastUserId)
			{
				lastUserId = userId;

				if(infoText != null)
				{
					infoText.text = string.Empty;
				}
			}
		}
	}


	// clips color camera image to the dimensions of the user
	private Texture2D ClipUserImage(long userId, Texture2D texImage, Vector3 texScale)
	{
		BodySlicer slicer = BodySlicer.Instance;

		if (slicer && texImage) 
		{
			if (slicer.getCalibratedUserId () != userId) 
			{
				slicer.OnCalibrationSuccess (userId);
			}

			BodySliceData sliceH = slicer.getBodySliceData(BodySlice.HEIGHT);
			BodySliceData sliceW = slicer.getBodySliceData(BodySlice.WIDTH);

			if (sliceH.isSliceValid && sliceW.isSliceValid) 
			{
				int rectX = (int)sliceW.startColorPoint.x;
				int rectW = sliceW.colorPointsLength;

				int rectY = (int)sliceH.startColorPoint.y;
				int rectH = sliceH.colorPointsLength;

				Texture2D texClipped = new Texture2D(rectW, rectH, TextureFormat.ARGB32, false);
				texClipped.SetPixels(texImage.GetPixels(rectX, rectY, rectW, rectH));

				return texClipped;
			}
		}

		return texImage;
	}



	// performs face detection
	private IEnumerator DoFaceDetection(Texture2D texImage)
	{
		// get the face manager instance
		CloudFaceManager faceManager = CloudFaceManager.Instance;

		if(texImage && faceManager)
		{
			Texture2D texFlipped = FlipTextureV(texImage);
			yield return faceManager.DetectFaces(texFlipped);

			if(faceManager.faces != null && faceManager.faces.Length > 0)
			{
				Face face = faceManager.faces[0];

				// set user data
				userGender = face.faceAttributes.gender;
				userAge = face.faceAttributes.age;
				userSmile = face.faceAttributes.smile;

				string sMessage = string.Format("{0}, Age: {1:F1}", userGender.ToUpper(), userAge);
				Debug.Log(string.Format("CloudFaceDetector found " + sMessage));

				if(infoText != null)
				{
					infoText.text = sMessage;
				}

				// convert to gender enum
				UserGender gender = userGender.ToLower () == "male" ? UserGender.Male : UserGender.Female;

				// invoke UserDataDetected() of the category selector(s) related to the same user
				MonoBehaviour[] monoScripts = FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];
				foreach(MonoBehaviour monoScript in monoScripts)
				{
					if((monoScript is CloudFaceListenerInterface) && monoScript.enabled)
					{
						CloudFaceListenerInterface userFaceListener = (CloudFaceListenerInterface)monoScript;
						userFaceListener.UserFaceDetected(playerIndex, gender, userAge, userSmile);
					}
				}

			}
		}

		yield return null;
	}

	// flips texture vertically
	private Texture2D FlipTextureV(Texture2D original)
	{
		Texture2D flipped = new Texture2D(original.width, original.height, TextureFormat.ARGB32, false);

		int xN = original.width;
		int yN = original.height;

		for(int i = 0; i < xN; i++)
		{
			for(int j = 0; j < yN; j++) 
			{
				flipped.SetPixel(i, yN - j - 1, original.GetPixel(i,j));
			}
		}

		flipped.Apply();

//		SavePngFile (original, "original.png");
//		SavePngFile (flipped, "flipped.png");

		return flipped;
	}

//	// saves texture as png-file
//	private void SavePngFile(Texture2D tex, string sFileName)
//	{
//		if (tex) 
//		{
//			byte[] btTexPng = tex.EncodeToPNG();
//			System.IO.File.WriteAllBytes (sFileName, btTexPng);
//		}
//	}

}
