using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// ��������� ������� �����������, ���������������, ���������� �� ����� � ��������� ����������.
/// </summary>
public class PlaceableObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //��������� ���������� ��� �������� ��������������
    private readonly string SURFACE_TAG = "surface";
    private readonly string ANIMATION_TRIGGER_PARAMETER = "bounce";

    private Camera mainCamera;    //������ 
    private CameraMovement cameraMovement; //����������� ������

    [SerializeField] private float fallingSpeed = 5.0f;  //�������� ������� �������. [SerializeField] ��������� ��������� �������� � ����������
    [SerializeField] private float scaleChange = 1.2f; //�������� ��������� ������� �������� ��� ��������������
    private float zAxis = 0.0f; //�������� �� ��� Z

    private bool isBeingHeld = false; // ���������� ����������, ��� ����������� ��������� ������� - ������������
    private bool isLayingOnSurface = false; // ���������� ����������, ��� ����������� ��������� ������� - ����� �� �����������

    private Vector2 originalScale; // ����������� ������ �������
    private Animator animator; // ��������, �������� �� ������������� ������� ��� �������� � ������������
    private SpriteRenderer spriteRenderer; // �������� ������������ ��������

    private enum SortingLayerName   //������������� ���������� ��� ��������� �������� ������������ ����
    {
        Default,
        Front,
        None,
    }

    private void Start()
    {
        //��������� �������� ��� ������� ���������
        mainCamera = Camera.main;
        cameraMovement = mainCamera.GetComponent<CameraMovement>();

        animator = GetComponent<Animator>();
        originalScale = transform.localScale;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        AddEventSystem(); //EventSystem ��� ��������� ����������������� ����� ����������� �� ����� ���������� ���������
    }

    private void Update()
    {
        Falling(); //����� ������� Falling
        MoveCamera(); // ����� ������� MoveCamera
    }

    public void OnBeginDrag(PointerEventData eventData) //���������� ����������� Unity ���������� IBeginDragHandler. ����������� ��� ������ �������������� �������
    {
        isBeingHeld = true; //  ������� ������������ �������, ��������������� ���������� ����������� � �������� true
        zAxis = transform.position.z; // ��������� �������� �� ��� Z - �� ������, ���� ������� ����� �������� �� ���� ��������
        transform.position = new Vector3(transform.position.x, transform.position.y, zAxis);    //��������� �������� ��������� �������� �� ������� �������� �� ��� Z � ����������� - �� ������ ������
    }

    public void OnDrag(PointerEventData eventData)  //���������� ����������� Unity ���������� IDragHandler. ����������� ��� �������������� �������
    {
        Vector3 _temp = mainCamera.ScreenToWorldPoint(eventData.position);  //��������� ���������� ��� �������� ���������, ���������������� �� ��������� ������������ � �������
        _temp.z = zAxis;    //��������� ����� ����������� �������� Z ��������� ���������� - ����� ������� �� ������������ �� ��� Z �� ����� �����������
        transform.position = _temp; // ��������� �������� � ����� ���������, ������ �������� ��������� ����������
    }

    public void OnEndDrag(PointerEventData eventData) //���������� ����������� Unity ���������� IEndDragHandler. ����������� ��� ���������� �������������� �������
    {
        isBeingHeld = false;    //����� ������ �� ���������� ������� - �������� ���������� ����������� � false
        transform.position = new Vector3(transform.position.x, transform.position.y, zAxis);    //��������� �������� ��������� �������� �� ������� �������� �� ��� Z � ����������� - �� ������ ������
    }

    private void OnTriggerEnter2D(Collider2D collision) //����������, ����� ������ ������ � ������� � ����������� �� ��������� isTrigger - true
    {
        if (!collision.CompareTag(SURFACE_TAG)) // ���� ������� ���������� � �����������, �������� �� ����������, ���������� ��� ��������� �� �����
            return;

        if (!isBeingHeld)
            animator.SetTrigger(ANIMATION_TRIGGER_PARAMETER);   //���� ������� �� ������������ �������, ����������� ������� "bounce" ��������� � �������� ������������� �������� �������������

        isLayingOnSurface = true;   //�������� ���������� ���������� ��������������� � true
    }

    private void OnTriggerStay2D(Collider2D collision) => SetObjectState(collision, true);  //���� ������ ��������� � �������� � �����������, ���������� ����� ������� SetObjectState � ��������� �������� ���������� � ��������� true � �������� ����������

    private void OnTriggerExit2D(Collider2D collision) => SetObjectState(collision, false); //����� ������ �������� ���������, ���������� ����� ������� SetObjectState � ��������� �������� ���������� � ��������� false � �������� ����������

    /// <summary>
    /// ������������� ��������� "����� �� �����������" (���������� � �������� ������� ��������� �������) �������, ������������ � �������� � ����������� (����������� � �������� ������� ��������� �������)
    /// </summary>
    /// <param name="_collision">���������, � ������� ����� ������������� ��������� ����� - �����</param>
    /// <param name="_isLayingOnSurface">�������� ���������� ��� ����������� ��������� "����� �� �����������" �������</param>
    private void SetObjectState(Collider2D _collision, bool _isLayingOnSurface)
    {
        if (_collision.CompareTag(SURFACE_TAG)) //���� ����� ����������� � �������� ������� ��������� ���������� ������������� ��������
            isLayingOnSurface = _isLayingOnSurface; //�������� ���������� ��� ����������� ��������� "����� �� �����������" ������� ��������������� � ���������� � �������� ������� ��������� ��������
    }

    private void OnMouseDown() //OnMouseDown ����������, ����� ������������ �������� �� ������ � �����������.
    {
        ChangeSize();   //�������� ������� ChangeSize
        spriteRenderer.sortingLayerName = SortingLayerName.Front.ToString(); // ��������� �������� ������������ ����. ������� ����� ��������� �� �������� �����, ���� ������������ �������
    }

    private void OnMouseUp()    //OnMouseDown ����������, ����� ������������ ��������� ������ � �����������.
    {
        ResetSize();    //�������� ������� ResetSize
        spriteRenderer.sortingLayerName = SortingLayerName.Default.ToString();  //������� �������� �� ��������� ���� � ����� ���� �������� ������ ��������, ������� ������ �������� ������� �� ����
    }

    /// <summary>
    /// �������, ����������� ����������.
    /// </summary>
    private void Falling()
    {
        if (isBeingHeld || isLayingOnSurface)   //���� ������� �� ������������ ������� � �� ����� �� �����������, �� ����� ������������ ����������� ���� �� ��������� fallingSpeed
            return;

        this.transform.Translate(Vector2.down * fallingSpeed * Time.deltaTime);
    }

    /// <summary>
    /// �������� ������� ������� � ������������ � ������������� ���������
    /// </summary>
    private void ChangeSize() => this.transform.localScale = originalScale * scaleChange;

    /// <summary>
    /// ��������������� ����������� ������� ������� � ������������ � ���������� �� ������ ��������� ���������
    /// </summary>
    private void ResetSize() => this.transform.localScale = originalScale;

    #region Camera
    /// <summary>
    /// �������� ������� ����������� ������.
    /// </summary>
    private void MoveCamera()
    {
        if (isBeingHeld)    //���� ����� ������ � ����� ������������ �������, ������� �� �����������
            return;

        cameraMovement.GetTouchInput(); //�� ������ ���������� ������� ����������� ���������� �����
    }
    #endregion

    /// <summary>
    /// ��������� � ������� ����� ������ � ������������ EventSystem � InputSystemUIInputModule ��� ��������� ����������������� �����
    /// </summary>
    private void AddEventSystem()
    {
        GameObject _eventSystem = new GameObject("EventSystem");    //�������� ������ �������� �������
        _eventSystem.AddComponent<EventSystem>();   //���������� ���������� EventSystem �� ��������� ������
        _eventSystem.AddComponent<InputSystemUIInputModule>();  //���������� ���������� InputSystemUIInputModule �� ��������� ������
    }
}
