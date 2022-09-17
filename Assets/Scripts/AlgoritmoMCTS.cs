using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;


public class AlgoritmoMcts
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Ficha Mcts(GameManager gameManager)
    {
        NodoMCTS root = new NodoMCTS
        {
            fieldTokens = gameManager.GetFichasMCTS(),
            leftSideValue = gameManager.leftSideValue,
            rightSideValue = gameManager.rightSideValue,
        };

        root.timesVisited = 1;
        if (root.hand == null)
        {
            root.hand = new List<Ficha>();
        }

        foreach (var gameObject in gameManager.player2.GetComponent<Player>().hand)
        {
            root.hand.Add(gameObject.GetComponent<Ficha>());
        }


        var list = jugadasLegales(root, root.hand);

        foreach (Ficha play in list) //Añado cada una de las jugadas posibles al árbol, como hijos de la raíz
        {
            NodoMCTS son = new NodoMCTS
            {
                selectedToken = play,
                wins = 0,
                timesVisited = 0,
                parent = root,
                fieldTokens = root.fieldTokens,
                hand = root.hand,
                backUpSelectedToken = play,
                leftSideValue = root.leftSideValue,
                rightSideValue = root.rightSideValue
            };
            if (root.children == null)
            {
                root.children = new List<NodoMCTS>();
            }

            root.children.Add(son);
        }

        if (root.children.Count == 0) //si no hay hijo , no hay jugadas para hacer
        {
            return null;
        }
        else if (root.children.Count == 1)
        {
            return root.children[0].selectedToken;
        }

        Debug.Log(JsonUtility.ToJson(root));
        for (int i = 0; i < 2; i++)
        {
            NodoMCTS current = Selection(root); //este current ya es la jugada posible con mejor UCT seleccionada
            Debug.Log(current.selectedToken.leftValue);
            //Debug.Log(current.selectedToken.rightValue);

            //NodoMCTS prometedor = Expansion(current);
            PlaceTokenAux(current, current.selectedToken);

            //      Simulación y Retropropagación
            current.wins += Simulacion(current, gameManager);
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

        foreach (NodoMCTS item in nodo.children)
        {
            double
                tasaExito = item.timesVisited > 0
                    ? item.wins / item.timesVisited
                    : 0; //Segun video del profesor esta tasa de exito es el resultado de la simulacion, en este caso solo wins q almacena tanto exitos como lose
            double exploración = item.timesVisited > 0
                ? Math.Sqrt(Math.Log(item.parent.timesVisited) / item.timesVisited)
                : 0;
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
        NodoMCTS simulationNode = new NodoMCTS();
        simulationNode.fieldTokens = nodoPrometedor.fieldTokens;
        simulationNode.hand = nodoPrometedor.hand;
        simulationNode.leftSideValue = nodoPrometedor.leftSideValue;
        simulationNode.rightSideValue = nodoPrometedor.rightSideValue;

        int fichasOponente = gameManager.player1.GetComponent<Player>().hand.Count;
        //int fichasoposite = 20 - (nodoPrometedor.hand.Count + nodoPrometedor.fieldTokens.Count);

        List<Ficha> tokenDisponibles = new List<Ficha>();
        foreach (var ficha in gameManager.salvaToken)
        {
            tokenDisponibles.Add(ficha);
        }

        //int fichasIA = nodoPrometedor.hand.Count;
        int fichasIA = simulationNode.hand.Count;
        //List<Ficha> tokensIA = nodoPrometedor.hand;
        List<Ficha> tokensIA = simulationNode.hand;

        while (fichasOponente > 0 && fichasIA > 0)
        {
            List<Ficha> legalPlays = jugadasLegalesTest(simulationNode, tokenDisponibles);
            if (legalPlays.Any())
            {
                var index = Random.Range(0, legalPlays.Count - 1);
                simulationNode.selectedToken = legalPlays[index];
                //tokenDisponibles.Remove(legalPlays[index]);
                PlaceTokenAux(simulationNode, simulationNode.selectedToken);
                fichasOponente--;
                if (fichasOponente == 0)
                {
                    return -1; //gana humano
                }
            }

            List<Ficha> legalPlaysIA = jugadasLegalesTest(simulationNode, tokensIA);

            if (legalPlaysIA.Any())
            {
                //var index = Random.Range(0, legalPlaysIA.Count - 1);
                var index = SelectorEstrategia(legalPlaysIA);
                simulationNode.selectedToken = legalPlaysIA[index];
                //tokensIA.Remove(legalPlaysIA[index]);

                PlaceTokenAux(simulationNode, legalPlaysIA[index]);
                fichasIA--;
                //if (tokensIA.Count == 0)
                if (fichasIA == 0)
                {
                    return 1; // gana la IA
                }
            }

            if (!legalPlays.Any() && !legalPlaysIA.Any())
            {
                int sumH = 0;
                int sumIa = 0;
                foreach (var ficha in gameManager.player1.GetComponent<Player>().hand)
                {
                    foreach (var card in simulationNode.fieldTokens)
                    {
                        if (ficha.GetComponent<Ficha>().leftValue != card.leftValue &&
                            ficha.GetComponent<Ficha>().rightValue != card.rightValue)
                        {
                            sumH += ficha.GetComponent<Ficha>().leftValue + ficha.GetComponent<Ficha>().rightValue;
                        }
                    }
                }

                foreach (var ficha2 in gameManager.player2.GetComponent<Player>().hand)
                {
                    foreach (var card2 in simulationNode.fieldTokens)
                    {
                        if (ficha2.GetComponent<Ficha>().leftValue != card2.leftValue &&
                            ficha2.GetComponent<Ficha>().rightValue != card2.rightValue)
                        {
                            sumIa += ficha2.GetComponent<Ficha>().leftValue + ficha2.GetComponent<Ficha>().rightValue;
                        }
                    }
                }

                if (sumH == sumIa)
                {
                    return 0;
                }

                return sumIa < sumH ? 1 : -1;
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
            nodo.fieldTokens = newList;
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
        //todo implementar el selector de estrategia para la simulación
    public int SelectorEstrategia(List<Ficha> fichas)   //aqui actualmente hago uso de tirar primero los dobles y luego de jugar la mayor, más abajo está jugar la más acompañada
    {
        int indexAux = 0;
        int indexDoble = -1;    //indice del doble
        int mayorAux = 0;   //indice de la mayor
        int mayorDoble = 0;
        for (var i = 0; i < fichas.Count; i++)
        {
            if (fichas[i].leftValue == fichas[i].rightValue)
            {
                if (fichas[i].leftValue + fichas[i].rightValue > mayorDoble)
                {
                    mayorDoble = fichas[i].leftValue + fichas[i].rightValue;
                    indexDoble = i;
                }
            }

            if (fichas[i].leftValue + fichas[i].rightValue > mayorAux)
            {
                mayorAux = fichas[i].leftValue + fichas[i].rightValue;
                indexAux = i;
            }
        }

        return indexDoble != -1 ? indexDoble : indexAux;
    }

    public Ficha MasAcompañada(List<Ficha> ficha, NodoMCTS simulationNode)
    {
        int mas = 0;
        int masAux = 0;
        int fichAcomp = 0;
        for (var i = 0; i < ficha.Count; i++)
        {
            foreach (var mano in simulationNode.hand)
            {
                if (ficha[i].rightValue == simulationNode.leftSideValue || 
                    ficha[i].rightValue == simulationNode.rightSideValue)
                {
                    if (ficha[i].leftValue == mano.leftValue || 
                        ficha[i].leftValue == mano.rightValue)
                    {
                        mas++;
                    }
                }

                if (ficha[i].leftValue == simulationNode.leftSideValue ||
                    ficha[i].leftValue == simulationNode.rightSideValue)
                {
                    if (ficha[i].rightValue == mano.leftValue ||
                        ficha[i].rightValue == mano.rightValue)
                    {
                        mas++;
                    }
                }
            }
            if (mas >= masAux)
            {
                masAux = mas;
                fichAcomp = i;
                mas = 0;
            }
            else
            {
                mas = 0;
            }
        }
        return ficha[fichAcomp];
    }

    public List<Ficha> jugadasLegalesTest(NodoMCTS nodo, List<Ficha> tokenDisponibles)
    {
        List<Ficha> jugadasLegales1 = new List<Ficha>();

        foreach (Ficha fichaDisponible in tokenDisponibles)
        {
            foreach (Ficha fichaMesa in nodo.fieldTokens)
            {
                if (fichaDisponible.leftValue != fichaMesa.leftValue &&
                    fichaDisponible.rightValue != fichaMesa.rightValue)
                {
                    if (fichaDisponible.leftValue == nodo.leftSideValue ||
                        fichaDisponible.leftValue == nodo.rightSideValue)
                    {
                        jugadasLegales1.Add(fichaDisponible);
                    }

                    else if (fichaDisponible.rightValue == nodo.leftSideValue ||
                             fichaDisponible.rightValue == nodo.rightSideValue)
                    {
                        jugadasLegales1.Add(fichaDisponible);
                    }
                }
            }
        }

        return jugadasLegales1;
    }
}