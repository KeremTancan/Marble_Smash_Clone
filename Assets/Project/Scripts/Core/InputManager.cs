using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float dragPlaneZ = -2f;
    [SerializeField] private PowerUpManager powerUpManager;
    [SerializeField] private GridManager gridManager; 

    private Shape _draggedShape;
    private Vector3 _offset; 

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        // Eğer havai fişek modu aktifse, sadece o moda özel tıklamaları dinle
        if (powerUpManager.IsFireworkModeActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleFireworkClick();
            }
            return;
        }
        
        if (Input.GetMouseButtonDown(0)) HandleMouseDown();
        else if (Input.GetMouseButton(0) && _draggedShape != null) HandleMouseDrag();
        else if (Input.GetMouseButtonUp(0) && _draggedShape != null) HandleMouseUp();
    }

    private void HandleFireworkClick()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Marble marble = hit.collider.GetComponent<Marble>();
            if (marble != null && marble.ParentNode != null && marble.ParentNode.IsOccupied)
            {
                gridManager.LaunchFireworksFromNode(marble.ParentNode);
                powerUpManager.ConsumeFirework();
            }
        }
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

    private void HandleMouseDrag() { _draggedShape.OnDrag(GetMouseWorldPos() + _offset); }
    private void HandleMouseUp() { _draggedShape.OnDropped(); _draggedShape = null; }
    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mainCamera.WorldToScreenPoint(transform.position).z - dragPlaneZ;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }
}