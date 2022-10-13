using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

public class Player : MonoBehaviour
{
    public List<GameObject> hand;

    public bool isMyTurn;

    public GameManager gManager;

    public Transform handPosition;

    public AlgoritmoMcts mc;

    // public Player()
    // {
    //     mc = new AlgoritmoMTCS();
    // }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ReceiveToken(GameObject token)
    {
        hand.Add(token);
        token.GetComponent<Ficha>().owner = gameObject;
        token.GetComponent<Ficha>().Rotate(-90f);
        token.GetComponent<Ficha>().RenderValue();
        token.GetComponent<Transform>().position =
            new Vector3(handPosition.position.x, handPosition.position.y, handPosition.position.z);
        handPosition.position =
            new Vector3(handPosition.position.x + 1.1f, handPosition.position.y, handPosition.position.z);
    }

    public void PlaceToken(GameObject t)
    {
        if (isMyTurn)
        {
            var token = hand.Find(x => x.GetComponent<Ficha>().leftValue == t.GetComponent<Ficha>().leftValue &&
                                       x.GetComponent<Ficha>().rightValue == t.GetComponent<Ficha>().rightValue);

            if (gManager.PlaceToken((token)))
            {
                token.GetComponent<Ficha>().owner = null;
                hand.Remove(token);
                if (hand.Count == 0) gManager.Win(tag);
                else gManager.EndTurn(this.gameObject);
            }
        }
    }

    public async void PlayAutomatic()
    {
        await Task.Delay(0500);
        mc = new AlgoritmoMcts();
        List<Ficha> mano = new List<Ficha>();
        foreach (GameObject o in hand)
        {
            if (o != null)
            {
                mano.Add(o.GetComponent<Ficha>());
            }
        }
        Ficha move = mc.Mcts(gManager);
        
        //GameObject move2 = MasAcompañada();
        /*if (move == null)
        {
            gManager.EndTurn(this.gameObject);
        }
        else*/
        
            GameObject aux = new GameObject();
            foreach (GameObject ficha in hand)
            {
                if (ficha.GetComponent<Ficha>().leftValue == move.leftValue && ficha.GetComponent<Ficha>().rightValue == move.rightValue)
                {
                    aux = ficha;
                }
            }
            if (gManager.PlaceToken(aux))
            {
                aux.GetComponent<Ficha>().owner = null;
                hand.Remove(aux);
                if (hand.Count == 0) gManager.Win(tag);
                else gManager.EndTurn(this.gameObject);
            }
        
        //Debug.Log(JsonUtility.ToJson(move.GetComponent<Ficha>()));
        
    }

    /*public async void PlayAutomatic()
    {
        foreach (var token in hand)
        {
            await Task.Delay(0500);
    
            if (gManager.PlaceToken(token))
            {
                token.GetComponent<Ficha>().owner = null;
                hand.Remove(token);
                if (hand.Count == 0) gManager.Win(tag);
                else gManager.EndTurn(this.gameObject); 
                break;
            }
        }
    }*/
    //*************** Test *****************//
    public GameObject MasAcompañada()
    {
        int mas = 0;
        int masAux = 0;
        int fichAcomp = 0;
        List<GameObject> jugadasLegales1 = new List<GameObject>();
        foreach (var o in hand)
        {
            if (o.GetComponent<Ficha>().leftValue == gManager.leftSideValue ||
                o.GetComponent<Ficha>().leftValue == gManager.rightSideValue)
            {
                jugadasLegales1.Add(o);
            }

            else if (o.GetComponent<Ficha>().rightValue == gManager.leftSideValue ||
                     o.GetComponent<Ficha>().rightValue == gManager.rightSideValue)
            {
                jugadasLegales1.Add(o);
            }
        }

        if (jugadasLegales1.Count == 0)
        {
            return null;
        }
        /*for (var i = 0; i < jugadasLegales1.Count; i++)
        {
            if (jugadasLegales1[i].GetComponent<Ficha>().leftValue == gManager.leftSideValue ||
                jugadasLegales1[i].GetComponent<Ficha>().leftValue == gManager.rightSideValue)
            {
                foreach (var ficha1 in hand)
                {
                    if (ficha1.GetComponent<Ficha>().leftValue == jugadasLegales1[i].GetComponent<Ficha>().rightValue)
                    {
                        mas++;
                    }

                    if (ficha1.GetComponent<Ficha>().rightValue == jugadasLegales1[i].GetComponent<Ficha>().rightValue)
                    {
                        mas++;
                    }

                    if (mas > masAux)
                    {
                        fichAcomp = i;
                        masAux = mas;
                        mas = 0;
                    }
                    else
                    {
                        mas = 0;
                    }
                }
            }

            if (jugadasLegales1[i].GetComponent<Ficha>().rightValue == gManager.leftSideValue ||
                jugadasLegales1[i].GetComponent<Ficha>().rightValue == gManager.rightSideValue)
            {
                foreach (var ficha1 in hand)
                {
                    if (ficha1.GetComponent<Ficha>().leftValue == jugadasLegales1[i].GetComponent<Ficha>().leftValue)
                    {
                        mas++;
                    }

                    if (ficha1.GetComponent<Ficha>().rightValue == jugadasLegales1[i].GetComponent<Ficha>().leftValue)
                    {
                        mas++;
                    }

                    if (mas > masAux)
                    {
                        fichAcomp = i;
                        masAux = mas;
                        mas = 0;
                    }
                    else
                    {
                        mas = 0;
                    }
                }
            }
        }*/

        for (var i = 0; i < jugadasLegales1.Count; i++)
        {
            for (var j = 0; j < hand.Count; j++)
            {
                if (jugadasLegales1[i].GetComponent<Ficha>().rightValue == gManager.leftSideValue ||
                    jugadasLegales1[i].GetComponent<Ficha>().rightValue == gManager.rightSideValue)
                {
                    if (jugadasLegales1[i].GetComponent<Ficha>().leftValue == hand[j].GetComponent<Ficha>().rightValue ||
                        jugadasLegales1[i].GetComponent<Ficha>().leftValue == hand[j].GetComponent<Ficha>().leftValue)
                    {
                        mas++;
                    }
                }

                if (jugadasLegales1[i].GetComponent<Ficha>().leftValue == gManager.leftSideValue ||
                    jugadasLegales1[i].GetComponent<Ficha>().leftValue == gManager.rightSideValue)
                {
                    if (jugadasLegales1[i].GetComponent<Ficha>().rightValue == hand[j].GetComponent<Ficha>().rightValue ||
                        jugadasLegales1[i].GetComponent<Ficha>().rightValue == hand[j].GetComponent<Ficha>().leftValue)
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

        return jugadasLegales1[fichAcomp];
    }
}