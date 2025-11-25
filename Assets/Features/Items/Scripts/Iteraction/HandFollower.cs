using UnityEngine;

public class HandFollower : MonoBehaviour
{
    //Скрипт для определения позиции объекта, в который помещаются выбранные предметы
    public Transform handBone; 

    void LateUpdate()
    {
        if (handBone == null) return;

        //Копируем позицию и ротацию
        transform.position = handBone.position;
        transform.rotation = handBone.rotation;
    }
}
