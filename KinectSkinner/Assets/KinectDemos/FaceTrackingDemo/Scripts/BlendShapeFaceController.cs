using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShapeFaceController : MonoBehaviour 
{
	public int playerIndex;
	public float smoothFactor = 10f;

	private SkinnedMeshRenderer skinnedMeshRenderer;
	private Mesh skinnedMesh;

	private int blendShapeCount;
	private string[] blendShapeNames;
	private float[] blendShapeValues;

	private KinectManager kinectManager;
	private FacetrackingManager faceManager;
	private Dictionary<KinectInterop.FaceShapeAnimations, float> dictAnimUnits = new Dictionary<KinectInterop.FaceShapeAnimations, float>();


	private static readonly Dictionary<string, KinectInterop.FaceShapeAnimations> blendShape2AnimUnit = new Dictionary<string, KinectInterop.FaceShapeAnimations>
	{
		{"Brow_Down_R", KinectInterop.FaceShapeAnimations.RighteyebrowLowerer},  // 0
		{"Cheek_Puffed_L", KinectInterop.FaceShapeAnimations.LeftcheekPuff},  // 1
		{"Jaw_L", KinectInterop.FaceShapeAnimations.JawSlideRight}, // 2
		{"Jaw_Open", KinectInterop.FaceShapeAnimations.JawOpen},  // 3
		{"Smile_R", KinectInterop.FaceShapeAnimations.LipCornerPullerRight},  // 4

		{"Brow_Up_R", KinectInterop.FaceShapeAnimations.RighteyebrowLowerer},  // 5
		{"Jaw_R", KinectInterop.FaceShapeAnimations.JawSlideRight},  // 6
		{"Eye_Closed_R", KinectInterop.FaceShapeAnimations.RighteyeClosed},  // 7
		{"Lips_Pucker", KinectInterop.FaceShapeAnimations.LipPucker},  // 8
		{"Lip_Lower_Down_R", KinectInterop.FaceShapeAnimations.LowerlipDepressorRight},  // 9

		{"Brow_Up_L", KinectInterop.FaceShapeAnimations.LefteyebrowLowerer},  // 10
		{"Smile_L", KinectInterop.FaceShapeAnimations.LipCornerPullerLeft},  // 11
		{"Cheek_Puffed_R", KinectInterop.FaceShapeAnimations.RightcheekPuff},  // 12
		{"Brow_Down_L", KinectInterop.FaceShapeAnimations.LefteyebrowLowerer},  // 13
		{"Lip_Lower_Down_L", KinectInterop.FaceShapeAnimations.LowerlipDepressorLeft},  // 14

		{"Eye_Closed_L", KinectInterop.FaceShapeAnimations.LefteyeClosed},  // 15
		{"Lips_Stretch_L", KinectInterop.FaceShapeAnimations.LipStretcherLeft},  // 16
		{"Frown_R", KinectInterop.FaceShapeAnimations.LipCornerDepressorRight},  // 17
		{"Frown_L", KinectInterop.FaceShapeAnimations.LipCornerDepressorLeft},  // 18
		{"Lips_Stretch_R", KinectInterop.FaceShapeAnimations.LipStretcherRight},  // 19
	};

	private static readonly Dictionary<string, float> blendShape2Multiplier = new Dictionary<string, float>
	{
		{"Jaw_L", -1f}, // 2
		{"Brow_Up_R", -1f},  // 5
		{"Brow_Up_L", -1f},  // 10
	};


	void Awake()
	{
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
		skinnedMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
	}


	void Start () 
	{
		// init blend shape names
		blendShapeCount = skinnedMesh.blendShapeCount;
		blendShapeNames = new string[blendShapeCount];
		blendShapeValues = new float[blendShapeCount];

		for (int i = 0; i < blendShapeCount; i++) 
		{
			blendShapeNames[i] = skinnedMesh.GetBlendShapeName(i);
		}

		// reference to KinectManager
		kinectManager = KinectManager.Instance;
	}
	
	void Update () 
	{
		// reference to face manager
		if (!faceManager) 
		{
			faceManager = FacetrackingManager.Instance;
		}

		if (kinectManager && kinectManager.IsInitialized() && faceManager && faceManager.IsFaceTrackingInitialized()) 
		{
			// check for tracked user
			long userId = kinectManager.GetUserIdByIndex(playerIndex);

			if (userId != 0 && kinectManager.IsUserTracked(userId)) 
			{
				if (faceManager.GetUserAnimUnits(userId, ref dictAnimUnits)) 
				{
					// process the blend shapes -> anim units
					for (int i = 0; i < blendShapeCount; i++) 
					{
						if (blendShape2AnimUnit.ContainsKey(blendShapeNames[i])) 
						{
							KinectInterop.FaceShapeAnimations faceAnim = blendShape2AnimUnit[blendShapeNames[i]];
							float animValue = dictAnimUnits[faceAnim];

							// check for multiplier
							float mul = 1f;
							if (blendShape2Multiplier.ContainsKey(blendShapeNames[i])) 
							{
								mul = blendShape2Multiplier[blendShapeNames[i]];
							}

							if (animValue * mul < 0f) 
							{
								animValue = 0f;
							}

							// lerp to the new value
							blendShapeValues[i] = Mathf.Lerp(blendShapeValues [i], animValue * mul * 100f, smoothFactor * Time.deltaTime);
							skinnedMeshRenderer.SetBlendShapeWeight(i, blendShapeValues[i]);
						}
					}
				}
			}
		}

	}


}
