using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

public class GameManager : MonoBehaviour
{
    public GameObject tokenReference;
    
    public GameObject CartelReference;

    public GameObject player1;

    public GameObject player2;

    public Transform initialPositionRef;

    public Transform middleField;
    
    public List<GameObject> fieldTokens;

    public Transform position;

    public List<GameObject> tokenDeck;
    //creo un nuevo objeto tipo lista de gameobject para guardar los token que quedan despues de repartir
    public List<GameObject> salvaToken;

    public float padding;

    public int leftSideValue;
    public int rightSideValue;


    public string texto;
    

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
        CartelReference.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        position.position = new Vector3(initialPositionRef.transform.position.x, initialPositionRef.transform.position.y, initialPositionRef.transform.position.z);
        StartCoroutine(GenerateTokens());
        
    }

    IEnumerator GenerateTokens()
    {
        var rightValue = 0;
        var leftValue = 0;

        var rowElementCount = 0;

        for (int i = 0; i < 56; i++)
        {
            var p = new Vector3(position.position.x, position.position.y, position.position.z);
            var newToken = Instantiate(tokenReference);
            
            newToken.transform.position = p;
            newToken.GetComponent<Ficha>().leftValue = leftValue;
            newToken.GetComponent<Ficha>().rightValue = rightValue;
            newToken.GetComponent<Ficha>().Rotate(90f);
            tokenDeck.Add(newToken);
            
            leftValue++;

            if (leftValue == 10 && rightValue < 9)
            {
                rightValue++;
                leftValue = rightValue;

            }
            
            rowElementCount++;
            
            if (rowElementCount % 6 == 0) position.position = new Vector3(initialPositionRef.transform.position.x, initialPositionRef.transform.position.y - (((rowElementCount / 6) * 1) + padding), initialPositionRef.transform.position.z);
            else position.position = new Vector3(position.position.x + padding + 2f, position.position.y, position.position.z);
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(2f);
        StartCoroutine(GiveTokens());
    }
    
    IEnumerator GiveTokens()       
    {
        for (int i = 0; i < 10; i++)
        {
            var index = new Random().Next(0,tokenDeck.Count -1);
            var token = tokenDeck[index];
            tokenDeck.Remove(token);
            player1.GetComponent<Player>().ReceiveToken(token);
            salvaToken.Add(token);

            index = new Random().Next(0,tokenDeck.Count -1);
            token = tokenDeck[index];
            player2.GetComponent<Player>().ReceiveToken(token);
            tokenDeck.Remove(token);
            
            yield return new WaitForSeconds(0.2f);
        }
        salvaToken.AddRange(tokenDeck);
        if (tokenDeck.Count > 0)
        {
            foreach (var item in tokenDeck)
            {
                Destroy(item);
            }
        }

        player1.GetComponent<Player>().isMyTurn = true;
    }

    public bool PlaceToken(GameObject token)
    {
        
        if (PlaceRight(token)) return true;
        if (PlaceLeft(token)) return true;
        
        return false;
    }

    public bool PlaceRight(GameObject token)
    {
        if (fieldTokens.Count == 0)
        {
            fieldTokens.Add(token);
            if (token.GetComponent<Ficha>().leftValue == token.GetComponent<Ficha>().rightValue ) {
                token.GetComponent<Transform>().position = new Vector3(middleField.transform.position.x + 1f,
                    middleField.transform.position.y, middleField.transform.position.z );
                Debug.Log("Es el primero y es doble");
            }
            else {
                token.GetComponent<Transform>().position = new Vector3(middleField.transform.position.x + 2f,
                middleField.transform.position.y, middleField.transform.position.z );
                token.GetComponent<Ficha>().Rotate(90f);
                Debug.Log("Es el primero y es normal");
            }

            leftSideValue = token.GetComponent<Ficha>().leftValue;
            rightSideValue = token.GetComponent<Ficha>().rightValue;
            

            return true;
        }
        
        var actualRightToken = fieldTokens[fieldTokens.Count - 1];
        var distance = actualRightToken.GetComponent<Ficha>().leftValue ==
                       actualRightToken.GetComponent<Ficha>().rightValue ? 1.5f : 2f;
        if (rightSideValue == token.GetComponent<Ficha>().leftValue)
        {
            fieldTokens.Add(token);
            salvaToken.Remove(token);
            if (token.GetComponent<Ficha>().leftValue == token.GetComponent<Ficha>().rightValue ) {
                token.GetComponent<Transform>().position = new Vector3(actualRightToken.transform.position.x + distance -0.5f,
                actualRightToken.transform.position.y, actualRightToken.transform.position.z );
                Debug.Log("A la derecha y es doble");
            }
            else {
                token.GetComponent<Transform>().position = new Vector3(actualRightToken.transform.position.x + distance,
                actualRightToken.transform.position.y, actualRightToken.transform.position.z );
                token.GetComponent<Ficha>().Rotate(90f);
                Debug.Log("A la derecha y es normal");
            }
            
            rightSideValue = token.GetComponent<Ficha>().rightValue;
            
            return true;
        }
        if (rightSideValue == token.GetComponent<Ficha>().rightValue)
        {
            fieldTokens.Add(token);
            salvaToken.Remove(token);
            token.GetComponent<Transform>().position = new Vector3(actualRightToken.transform.position.x + distance,
                actualRightToken.transform.position.y, actualRightToken.transform.position.z);
            
            token.GetComponent<Ficha>().Rotate(-90f);
            Debug.Log("A la derecha y es normal, hubo que girarlo");
            rightSideValue = token.GetComponent<Ficha>().leftValue;
            return true;
        }
        else return false;
    }
    
    public bool PlaceLeft(GameObject token)
    {
        var actualLeftToken = fieldTokens[0];
        var distance = actualLeftToken.GetComponent<Ficha>().leftValue ==
            actualLeftToken.GetComponent<Ficha>().rightValue ? 1.5f : 2f;
        if (leftSideValue == token.GetComponent<Ficha>().rightValue)
        {
            var newList = new List<GameObject>();
            newList.Add(token);
            newList.AddRange(fieldTokens);
            fieldTokens = newList;
            salvaToken.Remove(token);
            
            if (token.GetComponent<Ficha>().leftValue == token.GetComponent<Ficha>().rightValue ) {
                token.GetComponent<Transform>().position = new Vector3(actualLeftToken.transform.position.x - distance+0.5f,
                    actualLeftToken.transform.position.y, actualLeftToken.transform.position.z );
                Debug.Log("A la izquierda y es doble");
            }
            else {
                token.GetComponent<Transform>().position = new Vector3(actualLeftToken.transform.position.x - distance,
                    actualLeftToken.transform.position.y, actualLeftToken.transform.position.z );
                token.GetComponent<Ficha>().Rotate(90f);
                Debug.Log("A la izquierda y es normal");
            }
            leftSideValue = token.GetComponent<Ficha>().leftValue;
            return true;
        }
        else if (leftSideValue == token.GetComponent<Ficha>().leftValue)
        {
            var newList = new List<GameObject>();
            newList.Add(token);
            newList.AddRange(fieldTokens);
            fieldTokens = newList;
            salvaToken.Remove(token);
            
            token.GetComponent<Transform>().position = new Vector3(actualLeftToken.transform.position.x - distance,
                actualLeftToken.transform.position.y, actualLeftToken.transform.position.z);
            
            token.GetComponent<Ficha>().Rotate(-90f);
            
            leftSideValue = token.GetComponent<Ficha>().rightValue;
            Debug.Log("A la izquierda y es normal, hubo que girarlo");
            return true;
        }
        return false;
    }

    public void EndTurn(GameObject p)
    {
        Debug.Log("loop");
        if (!HasLegalMovements(player1.GetComponent<Player>()) && !HasLegalMovements(player2.GetComponent<Player>()))
        {
            var p1Points = 0;
            foreach (var item in player1.GetComponent<Player>().hand)
            {
                p1Points += item.GetComponent<Ficha>().leftValue;
                p1Points += item.GetComponent<Ficha>().rightValue;
            }
            
            var p2Points = 0;
            foreach (var item in player2.GetComponent<Player>().hand)
            {
                p2Points += item.GetComponent<Ficha>().leftValue;
                p2Points += item.GetComponent<Ficha>().rightValue;
            }
            
            if (p1Points < p2Points) Win(player1.tag);
            else if (p1Points > p2Points) Win(player2.tag);
            else Win("draw");
        }
        else if (p.tag == "Player_1")
        {
            player1.GetComponent<Player>().isMyTurn = false;
            player2.GetComponent<Player>().isMyTurn = true;
            if (!HasLegalMovements(player2.GetComponent<Player>())) EndTurn(player2);
            player2.GetComponent<Player>().PlayAutomatic();    //el jugador 2 juega automáticamente
        }
        else
        {
            player1.GetComponent<Player>().isMyTurn = true;
            player2.GetComponent<Player>().isMyTurn = false;
            if (!HasLegalMovements(player1.GetComponent<Player>())) EndTurn(player1);
        }
    }

    public void Win(string tag)
    {
        if (tag == "draw") Debug.Log( "La partida acabó en empate ");
        else
        {
            Debug.Log(tag + " Ganó la partida");
            if (CartelReference)
            {
                MostrarCartel(tag);  
            }

            
        }
            
        
    }

    public bool HasLegalMovements(Player p)
    {
        List<Ficha> test;
        foreach (var item in p.hand)
        {
            if (item.GetComponent<Ficha>().leftValue == leftSideValue ||
                item.GetComponent<Ficha>().rightValue == leftSideValue ||
                item.GetComponent<Ficha>().leftValue == rightSideValue ||
                item.GetComponent<Ficha>().rightValue == rightSideValue)
                return true;
        }

        return false;
    }

    public void MostrarCartel(string s)
    {
        texto = s + "Ganó la partida";
        CartelReference.SetActive(true);
        /*texto.transform.position = new Vector3(this.gameObject.transform.position.x,
            this.gameObject.transform.position.y,
            this.transform.position.z);*/
    }

    public List<Ficha> GetFichasMCTS()
    {
        List<Ficha> ft = new List<Ficha>();
        foreach (var fieldToken in fieldTokens)
        {
            ft.Add(fieldToken.GetComponent<Ficha>());
        }

        return ft;
    }
}
