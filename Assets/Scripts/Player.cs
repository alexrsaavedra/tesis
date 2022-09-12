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
            mano.Add(o.GetComponent<Ficha>());
        }
        
        Ficha move = mc.Mcts(mano, gManager);
        Debug.Log(JsonUtility.ToJson(move));
        GameObject aux = new GameObject();
        foreach (GameObject ficha in hand)
        {
            if (ficha.GetComponent<Ficha>().leftValue == move.leftValue && ficha.GetComponent<Ficha>().rightValue == move.rightValue)
            {
                aux = ficha;
            }
        }
        
        /*GameObject ficha = new GameObject();
        ficha.GetComponent<Ficha>().leftValue = move.leftValue;
        ficha.GetComponent<Ficha>().rightValue = move.rightValue;*/
        if (gManager.PlaceToken(aux))
        {
            move.GetComponent<Ficha>().owner = null;
            hand.Remove(aux);
            if (hand.Count == 0) gManager.Win(tag);
            else gManager.EndTurn(this.gameObject);
        }
    }
    /*public async void PlayAutomatic()
    {
        foreach (var token in hand)
        {
            await Task.Delay(0500);
            var result = gManager.PlaceToken(token);
            if (result == true)
            {
                token.GetComponent<Ficha>().owner = null;
                hand.Remove(token);
                if (hand.Count == 0) gManager.Win(tag);
                else gManager.EndTurn(this.gameObject); 
                break;
            }
        }
    }*/
}