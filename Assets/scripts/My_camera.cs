using UnityEngine;
using System.Collections;

public class My_camera : MonoBehaviour {
	
	public static float longitude;

	public float zoomSpeed, rotationSpeed, radiusMin, radiusMax, colatitudeMin, colatitudeMax;

	private Transform _transform, _map;
	private float _radius, _colatitude;
	
	void Start () {
		
		_transform = transform;
		_map = GameObject.Find("map").transform;
		_radius = radiusMax;
		_colatitude = 45f;
		longitude = 45f;
	}

	void Update () {

		if (Manager.current_state == Manager.STATE_MENU) {
			longitude += rotationSpeed * 0.02f * Time.deltaTime;
		}

		_radius -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime;
		_radius = Mathf.Clamp(_radius, radiusMin, radiusMax);

		_colatitude += Input.GetAxis("Vertical") * rotationSpeed * Time.deltaTime;
		longitude -= Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;

		if (Input.GetAxis("Fire2") != 0) { // right click
			_colatitude -= Input.GetAxis("Mouse Y") * rotationSpeed * 2 * Time.deltaTime;
			longitude += Input.GetAxis("Mouse X") * rotationSpeed * 2 * Time.deltaTime;
		}

		_colatitude = clamp_angle(_colatitude, colatitudeMin, colatitudeMax);

		_transform.rotation = Quaternion.Euler(_colatitude, longitude, 0);
		_transform.position = _transform.rotation * new Vector3(0, 0, -_radius) + _map.position;
	}

	float clamp_angle (float angle, float min, float max) {

		angle += angle<-360 ? 360 : angle>360 ? -360 : 0;
		return Mathf.Clamp(angle, min, max);
	}
}
