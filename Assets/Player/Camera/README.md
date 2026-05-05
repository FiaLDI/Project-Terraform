# Player Camera

This subsystem manages the player camera runtime, aim helpers, follow bindings, and camera-facing UI support.

## Main pieces

- `CameraRuntimeService` stores the active camera and handles FOV and shake requests.
- `CameraControlService` and related domain interfaces separate camera control from camera state storage.
- `CameraRegistry` and `CameraServiceProvider` expose the concrete scene camera setup.
- `PlayerCameraController`, `PlayerCameraNetAdapter`, and follow/head adapters connect runtime state to the player object.
- `AimRay` and `RayProviderExtensions` provide aim and raycast helpers.
- `CrosshairController` is the main camera-related UI element in this folder.

## Notes

This folder is about camera runtime behavior and camera-facing helpers. It does not own general movement or interaction logic, but other subsystems depend on its aim and ray providers.
