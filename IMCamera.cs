using System;
using UnityEngine;

namespace Cherry
{
    public interface IMCamera
    {
        Camera Main { get; }
        bool IsOverUGUI();
        Vector3 GetTouchPos();
        bool IsTouchBegan();
        bool IsTouched();
        bool IsTouchEnded();
        bool SetMainCamera(string name);
        void AddCamera(string name, Camera camera, bool main = false);
        void RemoveCamera(string name);
        Camera GetCamera(string name = null);

        Camera CreatePlayerCamera(float horizontal, float vertical, Transform root = null, float distance = 10,
            float fov = 60, bool mainCamera = true);

        void DestroyPlayerCamera();
        void RotatePlayerCameraHorizontal(float horizontal);
        void RotatePlayerCameraVertical(float vertical);
        void RotatePlayerCamera(Vector2 val);
        void SetPlayerCameraDistance(float distance);
        void MovePlayerCameraDistance(float val);
        bool TouchHit(out RaycastHit hit, int mask, float dis = Mathf.Infinity);
        bool TouchHit(Action<RaycastHit> action, int mask, float dis = Mathf.Infinity);

        bool TouchHit(out RaycastHit hit, float dis = Mathf.Infinity,
            params string[] layers);

        bool TouchHit(Action<RaycastHit> action, float dis = Mathf.Infinity,
            params string[] layers);

        bool TouchHit(out RaycastHit hit, float dis = Mathf.Infinity);
        bool TouchHit(Action<RaycastHit> action, float dis = Mathf.Infinity);
        bool TouchHit(out RaycastHit hit, params string[] layers);
        bool TouchHit(Action<RaycastHit> action, params string[] layers);
        bool Hit(out RaycastHit hit, int mask, float dis = Mathf.Infinity);
        bool Hit(Action<RaycastHit> action, int mask, float dis = Mathf.Infinity);
        bool Hit(out RaycastHit hit, float dis = Mathf.Infinity, params string[] layers);

        bool Hit(Action<RaycastHit> action, float dis = Mathf.Infinity,
            params string[] layers);

        bool Hit(out RaycastHit hit, float dis = Mathf.Infinity);
        bool Hit(Action<RaycastHit> action, float dis = Mathf.Infinity);
        bool Hit(out RaycastHit hit, params string[] layers);
        bool Hit(Action<RaycastHit> action, params string[] layers);
        void Capture(int width, int height, Action<Texture2D> onTexture);
        bool IsInViewport(Vector3 position);
        bool IsInViewport(Bounds bounds);
    }
}