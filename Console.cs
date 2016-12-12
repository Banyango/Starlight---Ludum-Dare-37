using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

public class Console : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {

	public Canvas CanvasToControl;
	public GraphicRaycaster RayCaster;

	public float MaxCursorLockDistance;
	public Image MouseCursor;

	private bool _isTracking = false;
	private bool _canTrack;
	[SerializeField] private Transform _player;
	[SerializeField] private DialogueImplementation _dialog;

	// Use this for initialization
	void Start () {
		EventSystem.GetSystem (0).SetSelectedGameObject (null);
	}

	#region IPointerUpHandler implementation

	public void OnPointerUp (PointerEventData eventData)
	{
		
	}

	#endregion

	#region IPointerDownHandler implementation
	public void OnPointerDown (PointerEventData eventData)
	{
		
	}
	#endregion

	#region IPointerClickHandler implementation

	public void OnPointerClick (PointerEventData eventData)
	{
		if (_isTracking) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			
			Debug.DrawRay (ray.origin, ray.direction);
			
			if (Physics.Raycast (ray, out hit) && hit.collider.gameObject.Equals (this.gameObject)) {
			
				Vector2 uvCoords = hit.textureCoord;
			
				eventData.position = CanvasToControl.worldCamera.ViewportToScreenPoint (uvCoords);
			
				List<RaycastResult> resultList = new List<RaycastResult> ();
			
				RayCaster.Raycast (eventData, resultList);
			
				for (int i = 0; i < resultList.Count; i++) {
					var raycastResult = resultList [i];
					eventData.pointerPressRaycast = raycastResult;
					ExecuteEvents.Execute (raycastResult.gameObject, new BaseEventData (EventSystem.GetSystem (0)), ExecuteEvents.submitHandler);
				}
			}
		}
	}

	#endregion

	public void FixedUpdate() {

		if (_player != null && Vector3.Distance (_player.position, this.transform.position) < MaxCursorLockDistance) {			
			_isTracking = true;
			RayCaster.enabled = true;
			if (_dialog != null) {
				_dialog.Paused = false;
			}
		} else {
			if (_dialog != null) {
				_dialog.Paused = true;
			}
			_isTracking = false;
			RayCaster.enabled = false;
		}

		if (_isTracking) {
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			Debug.DrawRay (ray.origin, ray.direction);

			if (Physics.Raycast (ray, out hit) && hit.collider.gameObject.Equals (this.gameObject)) {

				Vector2 uvCoords = hit.textureCoord;

				var screenPosition = CanvasToControl.worldCamera.ViewportToScreenPoint (uvCoords);

				Vector2 pos;

				RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasToControl.transform as RectTransform, screenPosition, CanvasToControl.worldCamera, out pos);

				MouseCursor.transform.position = CanvasToControl.transform.TransformPoint(pos);
			}
		}
	}
}
