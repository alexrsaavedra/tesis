using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;


public class AlgoritmoMcts
{
    public Ficha Mcts(GameManager gameManager)
    {
        var availableTokens = gameManager.player2.
            GetComponent<Player>().hand.
            Select<GameObject, Ficha>((GameObject x) => x.GetComponent<Ficha>())
            .ToList();

        // Inicializar raíz
        NodoMCTS root = new NodoMCTS
        {
            fieldTokens = gameManager.GetFichasMCTS(),
            leftSideValue = gameManager.leftSideValue,
            rightSideValue = gameManager.rightSideValue,
            timesVisited = 0,
            hand = availableTokens
        };

        // Agregar posibles jugadas como hijos
        root.children = jugadasLegales(gameManager.leftSideValue, gameManager.rightSideValue, availableTokens)
        .Select(x => new NodoMCTS
        {
            selectedToken = x,
            wins = 0,
            timesVisited = 0,
            parent = root,
            fieldTokens = root.fieldTokens,
            hand = root.hand,
            backUpSelectedToken = x,
            leftSideValue = root.leftSideValue,
            rightSideValue = root.rightSideValue
        }).ToList();

        if (root.children.Count == 1) return root.children[0].selectedToken;

        Debug.Log(JsonUtility.ToJson(root));

        //Aqui el core de MCTS
        /**for (int i = 0; i < 2; i++)
        {
            //Selection
            NodoMCTS current = Selection(root);

            if (!current.visited)
            {
                PlaceTokenAux(current, current.selectedToken);
                current.visited = true;
                //Simulation & Backpropagation
                current.wins += Simulacion(current, gameManager);
                current.timesVisited++;
                current.parent.timesVisited++;
            }
            //Expansion
            else
            {
                if (current.timesExpanded <= 3)
                {
                    NodoMCTS expanded = Expansion(current, gameManager);
                    //Simulation
                    int simValue = Simulacion(expanded, gameManager);
                    current.timesExpanded++;
                    Backpropagation(expanded, simValue);
                }
                else if (current.timesExpanded > 3)
                {
                    NodoMCTS expanded = current.getRandomChild(current);
                    int simValue = Simulacion(expanded, gameManager);
                    Backpropagation(expanded, simValue);
                }

            }

        }*/

        // Aqui especificas la profundidad con la que quieres que se
        // realice la búsqueda
        // las propiedades timesExpanded y visited del nodo no creo que
        // sean necesarias
        RunMCTS(root, gameManager, 2);

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

        return move;
        // Se toma como mejor jugada la de mayor robustez, o sea, la más visitada.
        //return Selection(root).selectedToken;
    }

    public void RunMCTS(NodoMCTS node, GameManager gameManager, int counter)
    {
        if (counter == 0) return;

        // Corro el algoritmo siempre que el nodo actual tenga hijos
        if (node.children.Count > 0)
        {
            //Selection
            NodoMCTS current = Selection(node);

            // Expasion
            Expansion(current, gameManager);

            // Simulation
            int simValue = Simulacion(current, gameManager);

            //Backpropagation
            Backpropagation(current, simValue);

            counter--;

            // Hacer lo mismo para cada uno de los hijos del nodo, si los tubiera
            // segun sea la profundidad designada
            current.children.ForEach(x => RunMCTS(x, gameManager, counter));
        }
        else return;

    }

    public List<Ficha> jugadasLegales(int leftSideValue, int rightSideValue, List<Ficha> tokenDisponibles)
    {
        List<Ficha> jugadasLegales = new List<Ficha>();

        foreach (var ficha in tokenDisponibles)
        {
            if (ficha.leftValue == leftSideValue ||
                ficha.leftValue == rightSideValue ||
                ficha.rightValue == leftSideValue ||
                ficha.rightValue == rightSideValue)
            {
                jugadasLegales.Add(ficha);
            }
        }

        return jugadasLegales;
    }

    public NodoMCTS Selection(NodoMCTS nodo)
    {
        NodoMCTS resultado = new NodoMCTS();
        double valorUCT = 0;

        foreach (NodoMCTS item in nodo.children)
        {
            // Segun video del profesor esta tasa de exito es el resultado de la simulacion,
            // en este caso solo wins q almacena tanto exitos como lose
            double tasaExito = item.timesVisited > 0
                    ? item.wins / item.timesVisited
                    : 0;

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

        // Aquí resultado nunca será null, porque lo inicializas como una instancia
        // de NodoMCST en el inicio del método.
        // Sin embargo, si la variable valorUCT es 0 aqui, significa que no econtraste
        // ningún nodo mejor que otro
        // if (resultado == null)
        if (valorUCT == 0)
        {
            // Si antes de llamar a la simulación te aseguras de que el nodo tenga hijos, 
            // no necesitas comprobarlo aquí
            /*if (nodo.children.Any())
            {*/
            resultado = nodo.getRandomChild(nodo);
            /*}

            return null;*/
        }

        resultado.timesVisited++;
        resultado.visited = true;

        return resultado;

        // Aqí hay una posibilidad de devolver un nodo inicializado, pero si ningún
        // valor en sus propiedades, en este caso es mejor retornar null
        // return resultado;
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
            nodo.leftSideValue = nodo.fieldTokens[0].leftValue;
            // o de la siguiente forma
            // newNode.leftSideValue = nodoMcts.leftSideValue;
            nodo.rightSideValue = token.rightValue;
        }
        //si coloco la ficha a la derecha y por la derecha de la ficha
        else if (nodo.rightSideValue == token.rightValue)
        {
            nodo.fieldTokens.Add(token);
            nodo.leftSideValue = nodo.fieldTokens[0].leftValue;
            // o de la siguiente forma
            // newNode.leftSideValue = nodoMcts.leftSideValue;
            nodo.rightSideValue = token.leftValue;
        }
    }

    public int Simulacion(NodoMCTS nodoPrometedor, GameManager gameManager)
    {
        NodoMCTS simulationNode = new NodoMCTS();
        simulationNode.fieldTokens = new List<Ficha>(nodoPrometedor.fieldTokens);
        simulationNode.hand = new List<Ficha>(nodoPrometedor.hand);
        simulationNode.leftSideValue = nodoPrometedor.leftSideValue;
        simulationNode.rightSideValue = nodoPrometedor.rightSideValue;

        // Menos uno debido a que durante la expasión agregamos una jugada
        // del jugador humano
        int fichasOponente = gameManager.player1.GetComponent<Player>().hand.Count -1;
        //int fichasoposite = 20 - (nodoPrometedor.hand.Count + nodoPrometedor.fieldTokens.Count);

        // Estos son todos los tokens que no tenga la IA en su poder, 
        // O sea los que sobraron al repartir y los que debe tener el jugador,
        // porque realmente la IA no conoce cuales puedan ser las fichas que 
        // actualmente posee el jugador
        List<Ficha> tokenDisponibles = GetAvailableTokensForHuman(gameManager.salvaToken,
        gameManager.player1.GetComponent<Player>().hand
        .Select<GameObject, Ficha>(x => x.GetComponent<Ficha>())
        .ToList(),
        simulationNode.fieldTokens);

        //int fichasIA = nodoPrometedor.hand.Count;
        int fichasIA = simulationNode.hand.Count;
        //List<Ficha> tokensIA = nodoPrometedor.hand;
        List<Ficha> tokensIA = simulationNode.hand;

        while (fichasOponente > 0 && fichasIA > 0)
        {
            List<Ficha> legalPlaysIA = jugadasLegales(
                simulationNode.leftSideValue,
                simulationNode.rightSideValue,
                tokensIA);

            if (legalPlaysIA.Any())
            {
                //var index = Random.Range(0, legalPlaysIA.Count - 1);
                var index = SelectorEstrategia(legalPlaysIA);
                simulationNode.selectedToken = legalPlaysIA[index];
                //tokensIA.Remove(legalPlaysIA[index]);

                PlaceTokenAux(simulationNode, legalPlaysIA[index]);

                // Es necesario eliminar la ficha luego de colocarla
                tokensIA = tokensIA.Where(x =>
                x.rightValue != simulationNode.selectedToken.rightValue &&
                x.leftValue != simulationNode.selectedToken.leftValue).ToList();

                fichasIA--;
                //if (tokensIA.Count == 0)
                if (fichasIA == 0)
                {
                    return 1; // gana la IA
                }
            }

            // Ya tenias un metodo de obtención de jugadas legales, creo que es mejor usarlo
            // List<Ficha> legalPlays = jugadasLegalesTest(simulationNode, tokenDisponibles);

            List<Ficha> legalPlays = jugadasLegales(
                simulationNode.leftSideValue,
                simulationNode.rightSideValue,
                tokenDisponibles);

            if (legalPlays.Any())
            {
                var index = Random.Range(0, legalPlays.Count - 1);
                simulationNode.selectedToken = legalPlays[index];
                PlaceTokenAux(simulationNode, simulationNode.selectedToken);

                // Es necesario eliminar la ficha luego de colocarla
                tokenDisponibles = tokenDisponibles.Where(x =>
                x.rightValue != simulationNode.selectedToken.rightValue &&
                x.leftValue != simulationNode.selectedToken.leftValue).ToList();

                fichasOponente--;
                if (fichasOponente == 0)
                {
                    return -1; //gana humano
                }
            }

            if (!legalPlays.Any() && !legalPlaysIA.Any())
            {
                int sumH = 0;
                int sumIa = 0;

                // Aqí literalmente estas haciendo trampa XD porque estas calculando el valor
                // total de las fichas que tiene el jugador en la mano, se supone que la IA no
                // debe conocer ese valor
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

                foreach (var ficha2 in tokensIA)
                {
                    // Como las fichas usadas ya se eliminan de los mazos,
                    // no hace falta iterar para comprobar que no estén
                    /*foreach (var card2 in simulationNode.fieldTokens)
                    {
                        if (ficha2.GetComponent<Ficha>().leftValue != card2.leftValue &&
                            ficha2.GetComponent<Ficha>().rightValue != card2.rightValue)
                        {
                            sumIa += ficha2.GetComponent<Ficha>().leftValue + ficha2.GetComponent<Ficha>().rightValue;
                        }
                    }*/
                    sumIa += ficha2.GetComponent<Ficha>().leftValue + ficha2.GetComponent<Ficha>().rightValue;
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

    public List<Ficha> GetAvailableTokensForHuman(List<Ficha> notAssgnedTokens, List<Ficha> humanTokens, List<Ficha> fieldTokens)
    {
        List<Ficha> availableTokens = new List<Ficha>();

        List<Ficha> tokens = new List<Ficha>();
        tokens.AddRange(notAssgnedTokens);
        tokens.AddRange(humanTokens);

        tokens.ForEach(x =>
        {
            if (!fieldTokens.Any(t => t.leftValue == x.leftValue && t.rightValue == x.rightValue))
                availableTokens.Add(x);
        });

        return availableTokens;
    }

    // Realmente no necesitas que este algoritmo devuelva nada, ya que trabajas
    // sobre una referencia, estás modificando directamente el nodo original
    public void Expansion(NodoMCTS nodoMcts, GameManager gameManager)
    {
        List<Ficha> tokenDisponibles =
        GetAvailableTokensForHuman(gameManager.salvaToken,
        gameManager.player1.GetComponent<Player>().hand
        .Select<GameObject, Ficha>(x => x.GetComponent<Ficha>())
        .ToList(),
        nodoMcts.fieldTokens);

        /*foreach (var ficha in gameManager.salvaToken)
        {
            if (nodoMcts.fieldTokens)
            tokenDisponibles.Add(ficha);
        }*/

        List<Ficha> jugadasLegalesHumano = jugadasLegales(nodoMcts.leftSideValue, nodoMcts.rightSideValue, tokenDisponibles);

        // Mi interpretación, sería expandir el nodo agregando como hijo a todas
        // las posibles jugadas del jugador humano, ya que es a el a quien le tocaría
        // jugar supuestamente

        nodoMcts.children = new List<NodoMCTS>();

        jugadasLegalesHumano.ForEach(x =>
        {
            var nn = new NodoMCTS
            {
                selectedToken = x,
                wins = 0,
                timesVisited = 0,
                parent = nodoMcts,
                fieldTokens = nodoMcts.fieldTokens,
                hand = nodoMcts.hand,
                backUpSelectedToken = x,
                leftSideValue = nodoMcts.leftSideValue,
                rightSideValue = nodoMcts.rightSideValue
            };

            PlaceTokenAux(nn, x);
            nodoMcts.children.Add(nn);
        });


        // Al tener en cuenta todas las jugadas posibles del jugador humano, 
        // lo más probable es que nunca obtengas una sola
        // Ademas veo que sea cual sea la situación solo tomas una sola jugada,
        // Cuando habria que tenerlas en cuenta todas
        /*if (jugadasLegalesHumano.Count == 1)
        {
            NodoMCTS expandedNode = new NodoMCTS();
            expandedNode.parent = nodoMcts;
            expandedNode.fieldTokens = nodoMcts.fieldTokens;
            expandedNode.hand = nodoMcts.hand;
            expandedNode.leftSideValue = nodoMcts.leftSideValue;
            expandedNode.rightSideValue = nodoMcts.rightSideValue;
            expandedNode.timesVisited = 0;
            expandedNode.wins = 0;

            PlaceTokenAux(expandedNode, jugadasLegalesHumano[0]);

            List<Ficha> jugadasLegalesIa = jugadasLegales(expandedNode.leftSideValue, expandedNode.rightSideValue, expandedNode.hand);

            if (jugadasLegalesIa.Any())
            {
                int index = Random.Range(0, jugadasLegalesIa.Count - 1);
                PlaceTokenAux(expandedNode, jugadasLegalesIa[index]);
                expandedNode.selectedToken = jugadasLegalesIa[index];
                return expandedNode;
            }

            return nodoMcts;
        }

        if (jugadasLegalesHumano.Count > 1)
        {
            NodoMCTS expandedNode2 = new NodoMCTS();
            expandedNode2.parent = nodoMcts;
            expandedNode2.fieldTokens = nodoMcts.fieldTokens;
            expandedNode2.hand = nodoMcts.hand;
            expandedNode2.leftSideValue = nodoMcts.leftSideValue;
            expandedNode2.rightSideValue = nodoMcts.rightSideValue;
            expandedNode2.timesVisited = 0;
            expandedNode2.wins = 0;

            int index = Random.Range(0, jugadasLegalesHumano.Count - 1);
            PlaceTokenAux(expandedNode2, jugadasLegalesHumano[index]);

            List<Ficha> jugadasLegalesIa =
                jugadasLegales(expandedNode2.leftSideValue, expandedNode2.rightSideValue, expandedNode2.hand);

            // Lo q hago es, q si el humano tiene jugadas
            // disponibles pero la ia no, devuelvo el mismo
            if (jugadasLegalesIa.Any())
            {
                // nodo q m entra, pues no simulo desde un pase de la IA
                int index2 = Random.Range(0, jugadasLegalesIa.Count - 1);
                PlaceTokenAux(expandedNode2, jugadasLegalesIa[index2]);
                expandedNode2.selectedToken = jugadasLegalesIa[index2];
                return expandedNode2;
            }

            return nodoMcts;
        }

        return nodoMcts;*/


        /*Debug.Log("Este es el nodo resultante de la selección");
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
        newNode.rightSideValue = newNode.fieldTokens[newNode.fieldTokens.Count - 1].GetComponent<Ficha>().rightValue;#1#
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
            newNode.selectedToken = null;#1#*/

    }

    public void Backpropagation(NodoMCTS nodoMcts, int simulacion)
    {
        // Aqui hay un problema, estás asignando una referencia
        // Lo que significa que al final todos los nodos son el mismo
        // nodo, por lo que si cambias algo en uno, se cambia 
        // en los demás
        // NodoMCTS tempNode = nodoMcts;

        // if (nodoMcts != null)
        // {
        //     nodoMcts.visited = true;
        //     nodoMcts.timesVisited++;
        //     nodoMcts.wins += simulacion;
        //     nodoMcts = tempNode.parent;
        // }

        if (nodoMcts == null) return;

        nodoMcts.visited = true;
        nodoMcts.timesVisited++;
        nodoMcts.wins += simulacion;

        Backpropagation(nodoMcts.parent, simulacion);
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

    /////////////////////////////////////////////////////////////////////////////////////////////
    // De acá para abajo nada se usa
    /////////////////////////////////////////////////////////////////////////////////////////////

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