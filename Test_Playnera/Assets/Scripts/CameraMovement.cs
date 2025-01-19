using UnityEngine;

/// <summary>
/// Реализует функцию перемещения камеры.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 50.0f;  //переменная, определяющая скорость перемещения камеры. [SerializeField] позволяет настроить значение в Инспекторе
    [SerializeField] private float xPositionMin = -1.6f, xPositionMax = 1.7f; //минимальное и максимальное значения положения камеры

    private Vector3 touchStartPosition, touchEndPosition;   //переменные типа Vector3, хранящие значения x, y, z координат для начального и конечного положения ввода

    /// <summary>
    /// Определяет значение и состояние сенсорного ввода
    /// </summary>
    public void GetTouchInput()
    {
        if (Input.touchCount > 0)   //если значений ввода больше, чем 0
        {
            Touch _touch = Input.GetTouch(0);   //получает значение первого ввода

            switch (_touch.phase)   //оценивает значение состояния ввода
            {
                case TouchPhase.Began:  //если ввод начался
                    touchStartPosition = Camera.main.ScreenToWorldPoint(_touch.position);   //получает и запоминает значение начального положения ввода
                    break;
                case TouchPhase.Moved:  //если значение ввода изменилось
                    touchEndPosition = Camera.main.ScreenToWorldPoint(_touch.position); //получает и запоминает значение конечного положения ввода
                    MoveCamera();   //вызывает функцию перемещения камеры
                    break;
                case TouchPhase.Stationary:
                    break;
                case TouchPhase.Ended:  //если ввод закончился
                    touchStartPosition = touchEndPosition;  //начальное значение ввода равно конечному 
                    break;
                case TouchPhase.Canceled:
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Перемещает камеру по оси X в соответствии с сенсорным вводом в пределах определенных значений.
    /// </summary>
    private void MoveCamera()
    {
        Vector3 _direction = touchStartPosition - touchEndPosition; //определяет направление перемещения камеры, основываясь на начальном и конечном значениях сенсорного ввода
        _direction = new Vector3(_direction.x, 0.0f, 0.0f); //ограничивает перемещение камеры по осям Y и Z

        if (this.transform.position.x <= xPositionMax && this.transform.position.x >= xPositionMin)  //если значение положения камеры по оси X находится в пределах заданных значений 
            this.transform.Translate(_direction * moveSpeed * Time.deltaTime);  //перемещает камеру в направлении _direction со скоростью moveSpeed

        ClampCameraXPosition();  //вызывает функцию ClampCameraXPosition
    }

    /// <summary>
    /// Ограничивает движение камеры в пределах обозначенных минимального и максимального значений по оси X
    /// </summary>
    private void ClampCameraXPosition() => this.transform.position = new Vector3(Mathf.Clamp(this.transform.position.x, xPositionMin, xPositionMax), this.transform.position.y, this.transform.position.z);
}
