using UnityEngine;

/// <summary>
/// ��������� ������� ����������� ������.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 50.0f;  //����������, ������������ �������� ����������� ������. [SerializeField] ��������� ��������� �������� � ����������
    [SerializeField] private float xPositionMin = -1.6f, xPositionMax = 1.7f; //����������� � ������������ �������� ��������� ������

    private Vector3 touchStartPosition, touchEndPosition;   //���������� ���� Vector3, �������� �������� x, y, z ��������� ��� ���������� � ��������� ��������� �����

    /// <summary>
    /// ���������� �������� � ��������� ���������� �����
    /// </summary>
    public void GetTouchInput()
    {
        if (Input.touchCount > 0)   //���� �������� ����� ������, ��� 0
        {
            Touch _touch = Input.GetTouch(0);   //�������� �������� ������� �����

            switch (_touch.phase)   //��������� �������� ��������� �����
            {
                case TouchPhase.Began:  //���� ���� �������
                    touchStartPosition = Camera.main.ScreenToWorldPoint(_touch.position);   //�������� � ���������� �������� ���������� ��������� �����
                    break;
                case TouchPhase.Moved:  //���� �������� ����� ����������
                    touchEndPosition = Camera.main.ScreenToWorldPoint(_touch.position); //�������� � ���������� �������� ��������� ��������� �����
                    MoveCamera();   //�������� ������� ����������� ������
                    break;
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Ended:  //���� ���� ����������
                    touchStartPosition = touchEndPosition;  //��������� �������� ����� ����� ��������� 
                    break;
                case TouchPhase.Canceled:
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// ���������� ������ �� ��� X � ������������ � ��������� ������ � �������� ������������ ��������.
    /// </summary>
    private void MoveCamera()
    {
        Vector3 _direction = touchStartPosition - touchEndPosition; //���������� ����������� ����������� ������, ����������� �� ��������� � �������� ��������� ���������� �����
        _direction = new Vector3(_direction.x, 0.0f, 0.0f); //������������ ����������� ������ �� ���� Y � Z

        if (this.transform.position.x <= xPositionMax && this.transform.position.x >= xPositionMin)  //���� �������� ��������� ������ �� ��� X ��������� � �������� �������� �������� 
            this.transform.Translate(_direction * moveSpeed * Time.deltaTime);  //���������� ������ � ����������� _direction �� ��������� moveSpeed

        ClampCameraXPosition();  //�������� ������� ClampCameraXPosition
    }

    /// <summary>
    /// ������������ �������� ������ � �������� ������������ ������������ � ������������� �������� �� ��� X
    /// </summary>
    private void ClampCameraXPosition() => this.transform.position = new Vector3(Mathf.Clamp(this.transform.position.x, xPositionMin, xPositionMax), this.transform.position.y, this.transform.position.z);
}
