using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Cherry.Cam
{
    public class MCamera : IMCamera
    {
        public const string PlayerCameraName = "PlayerCamera";

        private readonly Dictionary<string, Camera> _nameToCamera = new();

        private Plane[] _mainCameraPlanes;

        public Transform PlayerCameraRootTnf { get; private set; }
        private Transform _playerCameraVerticalTnf;

        public Camera PlayerCamera { get; private set; }

        public Camera Main { get; private set; }

        public bool IsOverUGUI()
        {
            if (isOverUGUI(Input.mousePosition)) return true;

            foreach (var touch in Input.touches)
                if (isOverUGUI(touch.position))
                    return true;

            return false;
        }

        public Vector3 GetTouchPos()
        {
            if (Input.touchCount > 0) return Input.GetTouch(0).position;

            return Input.mousePosition;
        }

        public bool IsTouchBegan()
        {
            return (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began) ||
                   Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
        }

        public bool IsTouched()
        {
            return Input.touchCount > 0 || Input.GetMouseButton(0) || Input.GetMouseButton(1);
        }

        public bool IsTouchEnded()
        {
            return (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended) ||
                   Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1);
        }

        public bool SetMainCamera(string name)
        {
            if (!_nameToCamera.TryGetValue(name, out var camera)) return false;

            Main = camera;
            _mainCameraPlanes = GeometryUtility.CalculateFrustumPlanes(Main);
            return true;
        }

        public void AddCamera(string name, Camera camera, bool main = false)
        {
            _nameToCamera.Add(name, camera);
            if (main) SetMainCamera(name);
        }

        public void RemoveCamera(string name)
        {
            if (!_nameToCamera.TryGetValue(name, out var cam)) return;
            _nameToCamera.Remove(name);
            if (cam.GetInstanceID() != Main.GetInstanceID()) return;
            Main = null;
            _mainCameraPlanes = null;
        }

        public Camera GetCamera(string name = null)
        {
            if (string.IsNullOrEmpty(name)) return Main;
            if (_nameToCamera.TryGetValue(name, out var camera)) return camera;
            Game.Log.Error($"not exist camera {name}");
            return null;
        }

        public Camera CreatePlayerCamera(float horizontal, float vertical, Transform root = null, float distance = 10,
            float fov = 60, bool mainCamera = true)
        {
            if (PlayerCamera != null)
            {
                Game.Log.Error("exist player camera");
                return PlayerCamera;
            }

            PlayerCameraRootTnf = new GameObject(PlayerCameraName).transform;
            _playerCameraVerticalTnf = new GameObject("Vertical").transform;
            _playerCameraVerticalTnf.SetParent(PlayerCameraRootTnf);
            var camTnf = new GameObject("Camera").transform;
            camTnf.SetParent(_playerCameraVerticalTnf);
            camTnf.localPosition = new Vector3(0, 0, -distance);
            PlayerCamera = camTnf.gameObject.AddComponent<Camera>();
            PlayerCamera.fieldOfView = Mathf.Clamp(fov, 40, 80);

            if (root != null)
            {
                PlayerCameraRootTnf.transform.SetParent(root);
                PlayerCameraRootTnf.localPosition = Vector3.zero;
            }
            PlayerCameraRootTnf.localEulerAngles = new Vector3(0, horizontal, 0);
            _playerCameraVerticalTnf.localEulerAngles = new Vector3(vertical, 0, 0);
            AddCamera(PlayerCameraName, PlayerCamera);
            return PlayerCamera;
        }

        public void DestroyPlayerCamera()
        {
            if (PlayerCamera == null) return;

            Object.Destroy(PlayerCameraRootTnf);
            PlayerCameraRootTnf = null;
            _playerCameraVerticalTnf = null;
            PlayerCamera = null;
        }

        public void RotatePlayerCameraHorizontal(float horizontal)
        {
            PlayerCameraRootTnf.localEulerAngles += new Vector3(0, horizontal, 0);
        }

        public void RotatePlayerCameraVertical(float vertical)
        {
            _playerCameraVerticalTnf.localEulerAngles += new Vector3(vertical, 0, 0);
        }

        public void RotatePlayerCamera(Vector2 val)
        {
            PlayerCameraRootTnf.localEulerAngles += new Vector3(0, val.x, 0);
            _playerCameraVerticalTnf.localEulerAngles += new Vector3(val.y, 0, 0);
        }

        public void SetPlayerCameraDistance(float distance)
        {
            PlayerCamera.transform.localPosition = new Vector3(0, 0, -distance);
        }

        public void MovePlayerCameraDistance(float val)
        {
            PlayerCamera.transform.localPosition -= new Vector3(0, 0, val);
        }

        public bool TouchHit(out RaycastHit hit, int mask, float dis = Mathf.Infinity)
        {
            if (!IsOverUGUI()) Physics.Raycast(Main.ScreenPointToRay(GetTouchPos()), out hit, dis, mask);
            hit = new RaycastHit();
            return false;
        }

        public bool TouchHit(Action<RaycastHit> action, int mask, float dis = Mathf.Infinity)
        {
            if (!TouchHit(out var hit, mask, dis)) return false;
            action(hit);
            return true;
        }

        public bool TouchHit(out RaycastHit hit, float dis = Mathf.Infinity,
            params string[] layers)
        {
            return TouchHit(out hit, LayerMask.GetMask(layers), dis);
        }

        public bool TouchHit(Action<RaycastHit> action, float dis = Mathf.Infinity,
            params string[] layers)
        {
            if (!TouchHit(out var hit, dis, layers)) return false;
            action(hit);
            return true;
        }

        public bool TouchHit(out RaycastHit hit, float dis = Mathf.Infinity)
        {
            if (!IsOverUGUI()) return Physics.Raycast(Main.ScreenPointToRay(GetTouchPos()), out hit, dis);
            hit = new RaycastHit();
            return false;
        }

        public bool TouchHit(Action<RaycastHit> action, float dis = Mathf.Infinity)
        {
            if (!TouchHit(out var hit, dis)) return false;
            action(hit);
            return true;
        }

        public bool TouchHit(out RaycastHit hit, params string[] layers)
        {
            return TouchHit(out hit, LayerMask.GetMask(layers));
        }

        public bool TouchHit(Action<RaycastHit> action, params string[] layers)
        {
            if (!TouchHit(out var hit, layers)) return false;
            action(hit);
            return true;
        }

        public bool Hit(out RaycastHit hit, int mask, float dis = Mathf.Infinity)
        {
            return Physics.Raycast(new Ray(Main.transform.position, Main.transform.forward), out hit, dis, mask);
        }

        public bool Hit(Action<RaycastHit> action, int mask, float dis = Mathf.Infinity)
        {
            if (!Hit(out var hit, mask, dis)) return false;
            action(hit);
            return true;
        }

        public bool Hit(out RaycastHit hit, float dis = Mathf.Infinity, params string[] layers)
        {
            return Hit(out hit, LayerMask.GetMask(layers), dis);
        }

        public bool Hit(Action<RaycastHit> action, float dis = Mathf.Infinity,
            params string[] layers)
        {
            if (!Hit(out var hit, dis, layers)) return false;
            action(hit);
            return true;
        }

        public bool Hit(out RaycastHit hit, float dis = Mathf.Infinity)
        {
            return Physics.Raycast(new Ray(Main.transform.position, Main.transform.forward), out hit, dis);
        }

        public bool Hit(Action<RaycastHit> action, float dis = Mathf.Infinity)
        {
            if (!Hit(out var hit, dis)) return false;
            action(hit);
            return true;
        }

        public bool Hit(out RaycastHit hit, params string[] layers)
        {
            return Hit(out hit, Mathf.Infinity, layers);
        }

        public bool Hit(Action<RaycastHit> action, params string[] layers)
        {
            if (!Hit(out var hit, layers)) return false;
            action(hit);
            return true;
        }

        public void Capture(int width, int height, Action<Texture2D> onTexture)
        {
            Game.StartCo(_CaptureCamera(Main, width, height, onTexture));
        }

        public bool IsInViewport(Vector3 position)
        {
            var pos = Main.WorldToViewportPoint(position);
            return !(pos.x < 0) && !(pos.x > 1) && !(pos.y < 0) && !(pos.y > 1);
        }

        public bool IsInViewport(Bounds bounds)
        {
            if (_mainCameraPlanes == null) return false;
            return GeometryUtility.TestPlanesAABB(_mainCameraPlanes, bounds);
        }

        private bool isOverUGUI(Vector3 pos)
        {
            var eventData = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(pos.x, pos.y)
            };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 0;
        }

        private IEnumerator _CaptureCamera(Camera camera, int width, int height, Action<Texture2D> onTexture)
        {
            yield return new WaitForEndOfFrame();

            var rt = new RenderTexture(width, height, 0);
            camera.targetTexture = rt;
            camera.Render();
            RenderTexture.active = rt;
            var screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();

            camera.targetTexture = null;
            RenderTexture.active = null;
            Object.Destroy(rt);

            onTexture(screenShot);
        }
    }
}