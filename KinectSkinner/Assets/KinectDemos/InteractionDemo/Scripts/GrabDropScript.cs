using UnityEngine;
using System.Collections;

public class GrabDropScript : MonoBehaviour, InteractionListenerInterface
{
	[Tooltip("List of the objects that may be dragged and dropped.")]
	public GameObject[] draggableObjects;

	[Tooltip("Material used to outline the currently selected object.")]
	public Material selectedObjectMaterial;
	
	[Tooltip("Drag speed of the selected object.")]
	public float dragSpeed = 3.0f;

	[Tooltip("Minimum Z-position of the dragged object, when moving forward and back.")]
	public float minZ = 0f;

	[Tooltip("Maximum Z-position of the dragged object, when moving forward and back.")]
	public float maxZ = 5f;

	// public options (used by the Options GUI)
	[Tooltip("Whether the objects obey gravity when released, or not. Used by the Options GUI-window.")]
	public bool useGravity = true;
	[Tooltip("Whether the objects should be put in their original positions. Used by the Options GUI-window.")]
	public bool resetObjects = false;

	[Tooltip("Camera used for screen ray-casting. This is usually the main camera.")]
	public Camera screenCamera;

	[Tooltip("UI-Text used to display information messages.")]
	public UnityEngine.UI.Text infoGuiText;

	[Tooltip("Interaction manager instance, used to detect hand interactions. If left empty, it will be the first interaction manager found in the scene.")]
	public InteractionManager interactionManager;

	// hand interaction variables
	//private bool isLeftHandDrag = false;
	private InteractionManager.HandEventType lastHandEvent = InteractionManager.HandEventType.None;

	// currently dragged object and its parameters
	private GameObject draggedObject;
	//private float draggedObjectDepth;
	private Vector3 draggedObjectOffset;
	private Material draggedObjectMaterial;
	private float draggedNormalZ;

	// initial objects' positions and rotations (used for resetting objects)
	private Vector3[] initialObjPos;
	private Quaternion[] initialObjRot;

	// normalized and pixel position of the cursor
	private Vector3 screenNormalPos = Vector3.zero;
	private Vector3 screenPixelPos = Vector3.zero;
	private Vector3 newObjectPos = Vector3.zero;


	// choose whether to use gravity or not
	public void SetUseGravity(bool bUseGravity)
	{
		this.useGravity = bUseGravity;
	}

	// request resetting of the draggable objects
	public void RequestObjectReset()
	{
		resetObjects = true;
	}


	void Start()
	{
		// by default set the main-camera to be screen-camera
		if (screenCamera == null) 
		{
			screenCamera = Camera.main;
		}

		// save the initial positions and rotations of the objects
		initialObjPos = new Vector3[draggableObjects.Length];
		initialObjRot = new Quaternion[draggableObjects.Length];

		for(int i = 0; i < draggableObjects.Length; i++)
		{
			initialObjPos[i] = screenCamera ? screenCamera.transform.InverseTransformPoint(draggableObjects[i].transform.position) : draggableObjects[i].transform.position;
			initialObjRot[i] = screenCamera ? Quaternion.Inverse(screenCamera.transform.rotation) * draggableObjects[i].transform.rotation : draggableObjects[i].transform.rotation;
		}

		// get the interaction manager instance
		if(interactionManager == null)
		{
			interactionManager = InteractionManager.Instance;
		}
	}


	void Update() 
	{
		if(interactionManager != null && interactionManager.IsInteractionInited())
		{
			if(resetObjects && draggedObject == null)
			{
				// reset the objects as needed
				resetObjects = false;
				ResetObjects ();
			}

			if(draggedObject == null)
			{
				// check if there is an underlying object to be selected
				if(lastHandEvent == InteractionManager.HandEventType.Grip && screenNormalPos != Vector3.zero)
				{
					// convert the normalized screen pos to pixel pos
					screenNormalPos = interactionManager.IsLeftHandPrimary() ? interactionManager.GetLeftHandScreenPos() : interactionManager.GetRightHandScreenPos();

					screenPixelPos.x = (int)(screenNormalPos.x * (screenCamera ? screenCamera.pixelWidth : Screen.width));
					screenPixelPos.y = (int)(screenNormalPos.y * (screenCamera ? screenCamera.pixelHeight : Screen.height));
					Ray ray = screenCamera ? screenCamera.ScreenPointToRay(screenPixelPos) : new Ray();

					// check if there is an underlying objects
					RaycastHit hit;
					if(Physics.Raycast(ray, out hit))
					{
						foreach(GameObject obj in draggableObjects)
						{
							if(hit.collider.gameObject == obj)
							{
								// an object was hit by the ray. select it and start drgging
								draggedObject = obj;
								draggedObjectOffset = hit.point - draggedObject.transform.position;
								draggedObjectOffset.z = 0; // don't change z-pos

								draggedNormalZ = (minZ + screenNormalPos.z * (maxZ - minZ)) - 
									draggedObject.transform.position.z; // start from the initial hand-z
								
								// set selection material
								draggedObjectMaterial = draggedObject.GetComponent<Renderer>().material;
								draggedObject.GetComponent<Renderer>().material = selectedObjectMaterial;

								// stop using gravity while dragging object
								draggedObject.GetComponent<Rigidbody>().useGravity = false;
								break;
							}
						}
					}
				}
				
			}
			else
			{
				// continue dragging the object
				screenNormalPos = interactionManager.IsLeftHandPrimary() ? interactionManager.GetLeftHandScreenPos() : interactionManager.GetRightHandScreenPos();
				
				// convert the normalized screen pos to 3D-world pos
				screenPixelPos.x = (int)(screenNormalPos.x * (screenCamera ? screenCamera.pixelWidth : Screen.width));
				screenPixelPos.y = (int)(screenNormalPos.y * (screenCamera ? screenCamera.pixelHeight : Screen.height));
				//screenPixelPos.z = screenNormalPos.z + draggedObjectDepth;
				screenPixelPos.z = (minZ + screenNormalPos.z * (maxZ - minZ)) - draggedNormalZ -
					(screenCamera ? screenCamera.transform.position.z : 0f);

				newObjectPos = screenCamera.ScreenToWorldPoint(screenPixelPos) - draggedObjectOffset;
				draggedObject.transform.position = Vector3.Lerp(draggedObject.transform.position, newObjectPos, dragSpeed * Time.deltaTime);
				
				// check if the object (hand grip) was released
				bool isReleased = lastHandEvent == InteractionManager.HandEventType.Release;

				if(isReleased)
				{
					// restore the object's material and stop dragging the object
					draggedObject.GetComponent<Renderer>().material = draggedObjectMaterial;

					if(useGravity)
					{
						// add gravity to the object
						draggedObject.GetComponent<Rigidbody>().useGravity = true;
					}

					draggedObject = null;
				}
			}
		}
	}


	void OnGUI()
	{
		if(infoGuiText != null && interactionManager != null && interactionManager.IsInteractionInited())
		{
			string sInfo = string.Empty;
			
			long userID = interactionManager.GetUserID();
			if(userID != 0)
			{
				if(draggedObject != null)
					sInfo = "Dragging the " + draggedObject.name + " around.";
				else
					sInfo = "Please grab and drag an object around.";
			}
			else
			{
				KinectManager kinectManager = KinectManager.Instance;

				if(kinectManager && kinectManager.IsInitialized())
				{
					sInfo = "Waiting for Users...";
				}
				else
				{
					sInfo = "Kinect is not initialized. Check the log for details.";
				}
			}
			
			infoGuiText.text = sInfo;
		}
	}


	// reset positions and rotations of the objects
	private void ResetObjects()
	{
		for(int i = 0; i < draggableObjects.Length; i++)
		{
			draggableObjects[i].GetComponent<Rigidbody>().useGravity = false;
			draggableObjects[i].GetComponent<Rigidbody>().velocity = Vector3.zero;

			draggableObjects[i].transform.position = screenCamera ? screenCamera.transform.TransformPoint(initialObjPos[i]) : initialObjPos[i];
			draggableObjects[i].transform.rotation = screenCamera ? screenCamera.transform.rotation * initialObjRot[i] : initialObjRot[i];
		}
	}


	public void HandGripDetected(long userId, int userIndex, bool isRightHand, bool isHandInteracting, Vector3 handScreenPos)
	{
		if (!isHandInteracting || !interactionManager)
			return;
		if (userId != interactionManager.GetUserID())
			return;

		lastHandEvent = InteractionManager.HandEventType.Grip;
		//isLeftHandDrag = !isRightHand;
		screenNormalPos = handScreenPos;
	}

	public void HandReleaseDetected(long userId, int userIndex, bool isRightHand, bool isHandInteracting, Vector3 handScreenPos)
	{
		if (!isHandInteracting || !interactionManager)
			return;
		if (userId != interactionManager.GetUserID())
			return;

		lastHandEvent = InteractionManager.HandEventType.Release;
		//isLeftHandDrag = !isRightHand;
		screenNormalPos = handScreenPos;
	}

	public bool HandClickDetected(long userId, int userIndex, bool isRightHand, Vector3 handScreenPos)
	{
		return true;
	}


}
