using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float dragPlaneZ = -1f;

    private Shape _draggedShape;
    private Vector3 _offset; 

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) HandleMouseDown();
        else if (Input.GetMouseButton(0) && _draggedShape != null) HandleMouseDrag();
        else if (Input.GetMouseButtonUp(0) && _draggedShape != null) HandleMouseUp();
    }

    private void HandleMouseDown()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Shape shape = hit.collider.GetComponentInParent<Shape>();
            if (shape != null && !shape.IsPlaced)
            {
                _draggedShape = shape;
                _offset = _draggedShape.transform.position - GetMouseWorldPos();
                _draggedShape.OnSelected();
            }
        }
    }

    private void HandleMouseDrag()
    {
        _draggedShape.OnDrag(GetMouseWorldPos() + _offset);
    }

    private void HandleMouseUp()
    {
        _draggedShape.OnDropped();
        _draggedShape = null;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mainCamera.WorldToScreenPoint(transform.position).z - dragPlaneZ;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }
}