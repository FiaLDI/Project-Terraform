public interface IScannable
{
    /// <summary>
    /// Вызывается сканером, когда объект попадает в зону сканирования.
    /// scanSpeed — скорость заполнения прогресса.
    /// </summary>
    void OnScanned(float scanSpeed);
}
