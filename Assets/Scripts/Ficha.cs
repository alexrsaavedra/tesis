using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Ficha : MonoBehaviour
{
    // Aquí es donde se recogen todos los datos que pudieramos necesitar de una
    // ficha de dominó,  así como la gestion de las animaciones y la posición
    public int rightValue;
    public int leftValue;
    public bool isDownside;
    public float time;
    public GameObject owner;

    private Animator _animator;
    
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _animator.speed = 0;
        SetDownside();

    }
    
    // Alternar rotación de la ficha la ficha
    public void Rotate(float r)
    {
        Debug.Log("Rotar a: "+r);
        transform.Rotate(0f, 0.0f, r, Space.Self);
    }
    
    // Mover ficha
    public void Move(Transform newPosition)
    {
        transform.position = new Vector3(newPosition.position.x, newPosition.position.y, newPosition.position.z);
    }
    
    // Renderizar el valor correcto de la ficha
    public void RenderValue()
    {
        time = float.Parse("0," + rightValue.ToString() + leftValue.ToString());
        _animator.Play("value",0, time);
    }
    
    // Voltear la ficha
    public void SetDownside()
    {
        _animator.Play("value",0, 100f);
    }

    private void OnMouseDown()
    {
        Debug.Log("Token clicked");
        if (owner != null) 
        {
            owner.GetComponent<Player>().PlaceToken(gameObject);
        }
    }

    
}
