using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

public class NodoMCTS
{
    public List<NodoMCTS> children;
    public List<Ficha> hand;
    public List<Ficha> fieldTokens;
    public int leftSideValue;
    public int rightSideValue;
    public Ficha selectedToken;
    public int timesVisited;
    public NodoMCTS parent;
    public Player oponente;
    public int wins;
    public bool expanded = false;

    /*public double GetResult()  
    {
        //para estrategia botagorda
        int puntosFicha = 0;
        int fichasOponente = 0;
        double a=0;
        
        if (oponente.GetComponent<Player>().tag == "Player_1")
        {
            fichasOponente = oponente.GetComponent<Player>().hand.Count;
        }

        int fichaE = selectedToken.GetComponent<Ficha>().leftValue + selectedToken.GetComponent<Ficha>().rightValue;
        foreach (var suma in hand)
        { 
            puntosFicha = suma.GetComponent<Ficha>().leftValue + suma.GetComponent<Ficha>().rightValue;
            if (fichaE>puntosFicha)
            {
                a += 0.5;
            }
        }
        
        if (hand.Count<fichasOponente)
        {
            a += 1;
        }

        a = a / hand.Count;
        
        
        return a;
    }*/

    public NodoMCTS getRandomChild()
    {
        var index = Random.Range(0, children.Count - 1);
        return children[index];
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    /*public Node encontrarMejorNodoConUCT(Node raíz)
    {
        int np = 1;                                                 //el número de veces que ha sido visitado el padre
        int ni = 1;                                                  //el número de veces que ha sido visitado el hijo
        double tasaExitoI = 0;                                       // tasa de exito del hijo, corresponde a la explotación, //representa cuantas de las jugadas realizadas desde esta posición resultaron en exito
        int C = 1;                                                          // constante para ajustar la cantidad de exploración e incorpora raiz de UCT, corresponde a la exploracion, // Se escoge C=1 para un equilibrio entre exploracion y explotacion
        double valorUCT /*= tasaExitoI + C * Math.Sqrt(Math.Log(np) / ni)#1#;
        int bestIndex = 0;

        for (int i = 0; i < raíz.getHijos().Count; i++)
        {
            if (raíz.getHijos()[i].GetCantVisitas() / raiz.getHijos()[i].getExito > tasaExitoI)
            {
                tasaExitoI = raíz.getHijos()[i].GetCantVisitas() / raiz.getHijos()[i].getExito;
                valorUCT = tasaExitoI + C * Math.Sqrt(Math.Log(np) / ni);
                bestIndex = i;
            }
        }

        Node mejor = raiz.getHijos()[bestIndex];
        return mejor;
    }*/
}