using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Реализует функции перемещения, масштабирования, сортировки на слоях и имитирует гравитацию.
/// </summary>
public class PlaceableObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //строковые переменные для быстрого редактирования
    private readonly string SURFACE_TAG = "surface";
    private readonly string ANIMATION_TRIGGER_PARAMETER = "bounce";

    private Camera mainCamera;    //камера 
    private CameraMovement cameraMovement; //перемещение камеры

    [SerializeField] private float fallingSpeed = 5.0f;  //скорость падения объекта. [SerializeField] позволяет настроить значение в Инспекторе
    [SerializeField] private float scaleChange = 1.2f; //значение изменения размера предмета при взаимодействии
    private float zAxis = 0.0f; //значение по оси Z

    private bool isBeingHeld = false; // логическая переменная, для определения состояния объекта - удерживается
    private bool isLayingOnSurface = false; // логическая переменная, для определения состояния объекта - лежит на поверхности

    private Vector2 originalScale; // изначальный размер объекта
    private Animator animator; // аниматор, отвечает за подпрыгивание объекта при контакте с поверхностью
    private SpriteRenderer spriteRenderer; // средство визуализации спрайтов

    private enum SortingLayerName   //перечислитель переменных для установки значения сортирующего слоя
    {
        Default,
        Front,
        None,
    }

    private void Start()
    {
        //начальные значения при запуске программы
        mainCamera = Camera.main;
        cameraMovement = mainCamera.GetComponent<CameraMovement>();

        animator = GetComponent<Animator>();
        originalScale = transform.localScale;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        AddEventSystem(); //EventSystem для обработки пользовательского ввода добавляется во время выполнения программы
    }

    private void Update()
    {
        Falling(); //вызов функции Falling
        MoveCamera(); // вызов функции MoveCamera
    }

    public void OnBeginDrag(PointerEventData eventData) //реализация встроенного Unity интерфейса IBeginDragHandler. Выполняется при начале перетаскивания объекта
    {
        isBeingHeld = true; //  предмет удерживается игроком, соответствующая переменная установлена в значение true
        zAxis = transform.position.z; // установка значения по оси Z - на случай, если предмет имеет отличное от нуля значение
        transform.position = new Vector3(transform.position.x, transform.position.y, zAxis);    //установка значения положения предмета со сбросом значения по оси Z в изначальное - на всякий случай
    }

    public void OnDrag(PointerEventData eventData)  //реализация встроенного Unity интерфейса IDragHandler. Выполняется при перетаскивании объекта
    {
        Vector3 _temp = mainCamera.ScreenToWorldPoint(eventData.position);  //временная переменная для хранения положения, преобразованного из экранного пространства в мировое
        _temp.z = zAxis;    //установка ранее полученного значения Z временной переменной - чтобы предмет не передвигался по оси Z во время перемещения
        transform.position = _temp; // установка предмета в новое положение, равное значению временной переменной
    }

    public void OnEndDrag(PointerEventData eventData) //реализация встроенного Unity интерфейса IEndDragHandler. Выполняется при завершении перетаскивания объекта
    {
        isBeingHeld = false;    //игрок больше не удерживает предмет - значение переменной установлено в false
        transform.position = new Vector3(transform.position.x, transform.position.y, zAxis);    //установка значения положения предмета со сбросом значения по оси Z в изначальное - на всякий случай
    }

    private void OnTriggerEnter2D(Collider2D collision) //вызывается, когда объект входит в контакт с коллайдером со значением isTrigger - true
    {
        if (!collision.CompareTag(SURFACE_TAG)) // если предмет столкнулся с коллайдером, отличным от указанного, дальнейший код выполнять не нужно
            return;

        if (!isBeingHeld)
            animator.SetTrigger(ANIMATION_TRIGGER_PARAMETER);   //если предмет не удерживается игроком, срабатывает триггер "bounce" аниматора и единожды проигрывается анимация подпрыгивания

        isLayingOnSurface = true;   //значение логической переменной устанавливается в true
    }

    private void OnTriggerStay2D(Collider2D collision) => SetObjectState(collision, true);  //пока объект находится в контакте с коллайдером, происходит вызов функции SetObjectState с передачей текущего коллайдера и значением true в качестве параметров

    private void OnTriggerExit2D(Collider2D collision) => SetObjectState(collision, false); //когда объект покидает коллайдер, происходит вызов функции SetObjectState с передачей текущего коллайдера и значением false в качестве параметров

    /// <summary>
    /// Устанавливает состояние "лежит на поверхности" (переданное в качестве второго аргумента функции) объекта, находящегося в контакте с коллайдером (переданного в качестве первого аргумента функции)
    /// </summary>
    /// <param name="_collision">коллайдер, с которым будет производиться сравнение меток - тегов</param>
    /// <param name="_isLayingOnSurface">значение переменной для определения состояния "лежит на поверхности" объекта</param>
    private void SetObjectState(Collider2D _collision, bool _isLayingOnSurface)
    {
        if (_collision.CompareTag(SURFACE_TAG)) //если метка переданного в качестве первого аргумента коллайдера соответствует заданной
            isLayingOnSurface = _isLayingOnSurface; //значение переменной для определения состояния "лежит на поверхности" объекта устанавливается в переданное в качестве второго аргумента значение
    }

    private void OnMouseDown() //OnMouseDown вызывается, когда пользователь нажимает на объект с коллайдером.
    {
        ChangeSize();   //Вызывает функцию ChangeSize
        spriteRenderer.sortingLayerName = SortingLayerName.Front.ToString(); // установка значения сортирующего слоя. Предмет будет находится на переднем плане, пока удерживается игроком
    }

    private void OnMouseUp()    //OnMouseDown вызывается, когда пользователь отпускает объект с коллайдером.
    {
        ResetSize();    //Вызывает функцию ResetSize
        spriteRenderer.sortingLayerName = SortingLayerName.Default.ToString();  //предмет вернется на дефолтный план и может быть заслонен другим объектом, имеющим высшее значение порядка на слое
    }

    /// <summary>
    /// Функция, имитирующая гравитацию.
    /// </summary>
    private void Falling()
    {
        if (isBeingHeld || isLayingOnSurface)   //Если предмет не удерживается игроком и не лежит на поверхности, он будет перемещаться вертикально вниз со скоростью fallingSpeed
            return;

        this.transform.Translate(Vector2.down * fallingSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Изменяет масштаб объекта в соответствии с установленным значением
    /// </summary>
    private void ChangeSize() => this.transform.localScale = originalScale * scaleChange;

    /// <summary>
    /// Восстанавливает изначальный масштаб объекта в соответствии с полученным на старте программы значением
    /// </summary>
    private void ResetSize() => this.transform.localScale = originalScale;

    #region Camera
    /// <summary>
    /// Вызывает функцию перемещения камеры.
    /// </summary>
    private void MoveCamera()
    {
        if (isBeingHeld)    //если игрок держит в руках перемещаемый предмет, функция не выполняется
            return;

        cameraMovement.GetTouchInput(); //на камере вызывается функция определения сенсорного ввода
    }
    #endregion

    /// <summary>
    /// Добавляет в текущую сцену объект с компонентами EventSystem и InputSystemUIInputModule для обработки пользовательского ввода
    /// </summary>
    private void AddEventSystem()
    {
        GameObject _eventSystem = new GameObject("EventSystem");    //создание нового игрового объекта
        _eventSystem.AddComponent<EventSystem>();   //добавление компонента EventSystem на созданный объект
        _eventSystem.AddComponent<InputSystemUIInputModule>();  //добавление компонента InputSystemUIInputModule на созданный объект
    }
}
