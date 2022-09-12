using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;


public class AlgoritmoMcts
{
    //public GameManager gameManager;
    /*public GameObject playerIA;*/

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }


    public Ficha Mcts(List<Ficha> hand, GameManager gameManager)
    {
        NodoMCTS root = new NodoMCTS
        {
            fieldTokens = gameManager.GetFichasMCTS(),
            leftSideValue = gameManager.leftSideValue,
            rightSideValue = gameManager.rightSideValue
        };

        root.timesVisited = 1;
        if (root.hand == null)
        {
            root.hand = new List<Ficha>();
        }

        root.hand.AddRange(hand);

        var list = jugadasLegales(root, root.hand);

        foreach (var play in list) //Añado cada una de las jugadas posibles al árbol, como hijos de la raíz
        {
            NodoMCTS son = new NodoMCTS
            {
                selectedToken = play,
                wins = 0,
                timesVisited = 0,
                parent = root,
                fieldTokens = root.fieldTokens,
                hand = root.hand,
                backUpSelectedToken = play
            };
            if (root.children == null)
            {
                root.children = new List<NodoMCTS>();
            }

            root.children.Add(son);
        }

        Debug.Log(JsonUtility.ToJson(root));
        for (int i = 0; i < 2; i++)
        {
            NodoMCTS current = Selection(root); //este current ya es la jugada posible con mejor UCT seleccionada
            Debug.Log(current.selectedToken.leftValue);
            Debug.Log(current.selectedToken.rightValue);
            
            NodoMCTS prometedor = Expansion(current);

            //      Simulación y Retropropagación
            current.wins += Simulacion(prometedor, gameManager);
            current.timesVisited++;
            current.parent.timesVisited++;
            //current = current.parent;             Preguntar esto a rafael, lo hacen en otros ejemplos de codigo
        }

        var visits = 0;
        Ficha move = new Ficha();
        foreach (var finalMove in root.children)
        {
            if (finalMove.timesVisited > visits)
            {
                visits = finalMove.timesVisited;
                move = finalMove.backUpSelectedToken;
            }
        }

        return move; // Se toma como mejor jugada la de mayor robustez, o sea, la más visitada.
                                                       //return Selection(root).selectedToken;
    }


    public NodoMCTS Selection(NodoMCTS nodo)
    {
        NodoMCTS resultado = new NodoMCTS();
        double valorUCT = 0;

        foreach (var item in nodo.children)
        {
            Debug.Log(item.selectedToken.leftValue);
            Debug.Log(item.selectedToken.rightValue);
            double tasaExito = item.timesVisited > 0 ? item.wins / item.timesVisited : 0;   //Segun video del profesor esta tasa de exito es el resultado de la simulacion, en este caso solo wins q almacena tanto exitos como lose
            double exploración = item.timesVisited > 0 ? Math.Sqrt(Math.Log(item.parent.timesVisited) / item.timesVisited) : 0;
            double valorAux = tasaExito + exploración;
            if (valorAux >= valorUCT)
            {
                valorUCT = valorAux;
                resultado = item;
            }
        }

        if (resultado == null)
        {
            if (nodo.children.Any())
            {
                return nodo.getRandomChild();
            }

            return nodo;
        }

        return resultado;
    }

    public NodoMCTS Expansion(NodoMCTS nodoMcts)
    {
        Debug.Log("Este es el nodo resultante de la selección");
        Debug.Log(JsonUtility.ToJson(nodoMcts));
        
        //si coloco la ficha a la izquierda y por la izquierda de la ficha

        if (nodoMcts.fieldTokens[0].leftValue == nodoMcts.selectedToken.leftValue)
        {
            var newList = new List<Ficha>();
            newList.Add(nodoMcts.selectedToken);
            newList.AddRange(nodoMcts.fieldTokens);
            nodoMcts.fieldTokens = newList;
            nodoMcts.leftSideValue = nodoMcts.selectedToken.rightValue;
            nodoMcts.rightSideValue =
                nodoMcts.fieldTokens[nodoMcts.fieldTokens.Count - 1].rightValue;
            nodoMcts.hand.Remove(nodoMcts.selectedToken);
        }
        //si coloco la ficha a la izquierda y por la derecha de la ficha
        else if (nodoMcts.fieldTokens[0].leftValue == nodoMcts.selectedToken.rightValue)
        {
            var newList = new List<Ficha>();
            newList.Add(nodoMcts.selectedToken);
            newList.AddRange(nodoMcts.fieldTokens);
            nodoMcts.fieldTokens = newList;
            nodoMcts.leftSideValue = nodoMcts.selectedToken.leftValue;
            nodoMcts.rightSideValue =
                nodoMcts.fieldTokens[nodoMcts.fieldTokens.Count - 1].rightValue;
            nodoMcts.hand.Remove(nodoMcts.selectedToken);
            //newNode.rightSideValue = nodoMcts.rightSideValue;
        }
        //si coloco la ficha a la derecha y x la izquierda de la ficha
        else if (nodoMcts.fieldTokens[nodoMcts.fieldTokens.Count - 1].rightValue == nodoMcts.selectedToken.leftValue)
        {
            nodoMcts.fieldTokens.Add(nodoMcts.selectedToken);
            nodoMcts.leftSideValue = nodoMcts.fieldTokens[0].leftValue; // o de la siguiente forma
            //newNode.leftSideValue = nodoMcts.leftSideValue;
            nodoMcts.rightSideValue =
                nodoMcts.fieldTokens[nodoMcts.fieldTokens.Count - 1].rightValue;
            nodoMcts.hand.Remove(nodoMcts.selectedToken);
        }
        //si coloco la ficha a la derecha y por la derecha de la ficha
        else if (nodoMcts.fieldTokens[nodoMcts.fieldTokens.Count - 1].rightValue == nodoMcts.selectedToken.rightValue)
        {
            nodoMcts.fieldTokens.Add(nodoMcts.selectedToken);
            nodoMcts.leftSideValue = nodoMcts.fieldTokens[0].leftValue; // o de la siguiente forma
            //newNode.leftSideValue = nodoMcts.leftSideValue;
            nodoMcts.rightSideValue = nodoMcts.fieldTokens[nodoMcts.fieldTokens.Count - 1].leftValue;
            nodoMcts.hand.Remove(nodoMcts.selectedToken);
        }

        /*newNode.fieldTokens.Add(nodoMcts.selectedToken);
        newNode.leftSideValue = newNode.fieldTokens[0].GetComponent<Ficha>().leftValue;
        newNode.rightSideValue = newNode.fieldTokens[newNode.fieldTokens.Count - 1].GetComponent<Ficha>().rightValue;*/
/*List<GameObject> jugadasLegales = new List<GameObject>();
            
        foreach (var ficha in newNode.hand)
        {
            if (ficha.GetComponent<Ficha>().leftValue==nodoMcts.leftSideValue || ficha.GetComponent<Ficha>().leftValue == nodoMcts.rightSideValue)
            {
                jugadasLegales.Add(ficha);
            }
            else if (ficha.GetComponent<Ficha>().rightValue == nodoMcts.leftSideValue || ficha.GetComponent<Ficha>().rightValue == nodoMcts.rightSideValue)
            {
                jugadasLegales.Add(ficha);
            }
        }

        if (jugadasLegales.Any())
        {
            var index = Random.Range(0, jugadasLegales.Count - 1);
            newNode.selectedToken = jugadasLegales[index];
        }
        else
            newNode.selectedToken = null;*/

        return nodoMcts;
    }


    public int Simulacion(NodoMCTS nodoPrometedor, GameManager gameManager)
    {
        Debug.Log("Este es el nodo que viene de la exapansión");
        Debug.Log(JsonUtility.ToJson(nodoPrometedor));
        int fichasOponente = 20 - (nodoPrometedor.hand.Count + nodoPrometedor.fieldTokens.Count);
        Debug.Log("Cantidad de fichas que tiene el oponente");
        Debug.Log(fichasOponente);
        Debug.Log("Cantidad de fichas que tiene la IA");
        Debug.Log(nodoPrometedor.hand.Count);
        Debug.Log("Cant de fichas que hay en el campo");
        Debug.Log(nodoPrometedor.fieldTokens.Count);
        
        List<Ficha> tokenDisponibles = new List<Ficha>();
        foreach (var ficha in gameManager.salvaToken)
        {
            tokenDisponibles.Add(ficha);
            Debug.Log(JsonUtility.ToJson(ficha));
        }
        

        List<Ficha> tokensIA = nodoPrometedor.hand;

        while (fichasOponente > 0 && tokensIA.Count > 0)
        {
            List<Ficha> legalPlays = jugadasLegales(nodoPrometedor, tokenDisponibles);
            if (legalPlays.Any())
            {
                var index = Random.Range(0, legalPlays.Count - 1);
                nodoPrometedor.selectedToken = legalPlays[index];
                tokenDisponibles.Remove(legalPlays[index]);
                PlaceTokenAux(nodoPrometedor, nodoPrometedor.selectedToken);
                fichasOponente--;
                if (fichasOponente == 0)
                {
                    return -1; //gana humano
                }
            }

            List<Ficha> legalPlaysIA = jugadasLegales(nodoPrometedor, tokensIA);
            if (legalPlaysIA.Any())
            {
                var index = Random.Range(0, legalPlaysIA.Count - 1);
                nodoPrometedor.selectedToken = legalPlaysIA[index];
                tokensIA.Remove(legalPlaysIA[index]);
                PlaceTokenAux(nodoPrometedor, legalPlaysIA[index]);
                if (tokensIA.Count == 0)
                {
                    return 1; // gana la IA
                }
            }

            if (!legalPlays.Any() && !legalPlaysIA.Any())
            {
                if (tokensIA.Count < fichasOponente)
                {
                    return 1;
                }

                if (tokensIA.Count > fichasOponente)
                {
                    return -1;
                }

                return 0;
            }
        }

        return 0; //empate
    }

    public void RetroPropagación()
    {
    }


    public List<Ficha> jugadasLegales(NodoMCTS nodo, List<Ficha> tokenDisponibles)
    {
        List<Ficha> jugadasLegales1 = new List<Ficha>();

        foreach (var ficha in tokenDisponibles)
        {
            if (ficha.leftValue == nodo.leftSideValue ||
                ficha.leftValue == nodo.rightSideValue)
            {
                jugadasLegales1.Add(ficha);
            }

            else if (ficha.rightValue == nodo.leftSideValue ||
                     ficha.rightValue == nodo.rightSideValue)
            {
                jugadasLegales1.Add(ficha);
            }
        }

        return jugadasLegales1;
    }

    public void PlaceTokenAux(NodoMCTS nodo, Ficha token)
    {
        //si coloco la ficha a la izquierda y por la izquierda de la ficha
        if (nodo.leftSideValue == token.leftValue)
        {
            var newList = new List<Ficha>();
            newList.Add(token);
            newList.AddRange(nodo.fieldTokens);
            nodo.fieldTokens = newList;
            nodo.leftSideValue = token.rightValue;
            nodo.rightSideValue = nodo.fieldTokens[nodo.fieldTokens.Count - 1].rightValue;
        }
        //si coloco la ficha a la izquierda y por la derecha de la ficha
        else if (nodo.leftSideValue == token.rightValue)
        {
            var newList = new List<Ficha>();
            newList.Add(token);
            newList.AddRange(nodo.fieldTokens);
            nodo.leftSideValue = token.leftValue;
            nodo.rightSideValue = nodo.fieldTokens[nodo.fieldTokens.Count - 1].rightValue;
            //newNode.rightSideValue = nodoMcts.rightSideValue;
        }
        //si coloco la ficha a la derecha y x la izquierda de la ficha
        else if (nodo.rightSideValue == token.leftValue)
        {
            nodo.fieldTokens.Add(token);
            nodo.leftSideValue = nodo.fieldTokens[0].leftValue; // o de la siguiente forma
            //newNode.leftSideValue = nodoMcts.leftSideValue;
            nodo.rightSideValue = token.rightValue;
        }
        //si coloco la ficha a la derecha y por la derecha de la ficha
        else if (nodo.rightSideValue == token.rightValue)
        {
            nodo.fieldTokens.Add(token);
            nodo.leftSideValue = nodo.fieldTokens[0].leftValue; // o de la siguiente forma
            //newNode.leftSideValue = nodoMcts.leftSideValue;
            nodo.rightSideValue = token.leftValue;
        }
    }
}