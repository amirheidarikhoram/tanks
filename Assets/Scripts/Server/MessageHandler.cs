using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;

public class MessageHandler : MonoBehaviour
{

    public static MessageHandler hander;
    public GameObject HitPrefab;
    public GameObject TankPrefab;
    public Client ClientPrefab;

    void Start()
    {
        if (hander == null)
        {
            hander = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void MatchAction(string message, Client client)
    {
        Debug.Log(message);

        ActionUnion actionUnion = JsonUtility.FromJson<ActionUnion>(message);

        if (actionUnion.g_type != null)
        {
            // game action
            MatchGameAction(actionUnion, message, client);

        }
        else if (actionUnion.s_type != null)
        {
            // server action
            MatchServerAction(actionUnion, message);
        }
        else if (actionUnion.type != null)
        {
            // an action!
            MatchAction(actionUnion, message);
        }
        else
        {
            // a fuck-up!
        }
    }

    void MatchGameAction(ActionUnion action, string message, Client receiver)
    {

        if (action.g_type == "close_worlds_update")
        {
            try
            {
                CloseWorldsUpdateAction closeWorldsUpdateAction = JsonUtility.FromJson<CloseWorldsUpdateAction>(message);

                for (int i = 0; i < closeWorldsUpdateAction.worlds.Count; i++)
                {

                    World world = closeWorldsUpdateAction.worlds[i];
                    Client foundClient = Client.clients.Find((c) => c.Address == world.address);

                    if (foundClient == null)
                    {
                        // create a client for world
                        Client client = Instantiate(ClientPrefab);
                        client.gameObject.name = "Client " + client.Address;
                        client.Connect(world.address);
                    }
                }

                for (int i = 0; i < Client.clients.Count; i++)
                {
                    Client client = Client.clients[i];

                    World world = closeWorldsUpdateAction.worlds.Find((w) => w.address == client.Address);

                    if (world == null)
                    {
                        client.Close();
                    }
                }


            }
            catch (System.Exception exeption)
            {
                Debug.LogError("exception");
                Debug.LogError(exeption);
            }
        }
        else if (action.g_type == "player_join")
        {
            JoinWorldAction joinAction = JsonUtility.FromJson<JoinWorldAction>(message);

            receiver.AddPlayer(joinAction.player);
            GameObject tankObject = Instantiate(TankPrefab, new Vector3(joinAction.player.transform.position[0], joinAction.player.transform.position[1], 10), new Quaternion(0, 0, 0, 0));

            Tank tank = tankObject.GetComponent<Tank>(); 
            tank.Id = joinAction.player.id;
            tank.UpdateWithPlayer(joinAction.player);
        } else if (action.g_type == "die") {

            Debug.Log("HI");

            DieAction dieAction = JsonUtility.FromJson<DieAction>(message);

            Tank tank = FindObjectsOfType<Tank>().ToList().Find(t => t.Id == dieAction.playerId);

            if (tank != null) {
                Destroy(tank.gameObject);
                receiver.RemovePlayer(dieAction.playerId);
            }
        }
    }

    void MatchServerAction(ActionUnion action, string message)
    {
        if (action.s_type == "introduce_server")
        {
            IntroduceServerAction introduceServerAction = JsonUtility.FromJson<IntroduceServerAction>(message);

            World world = introduceServerAction.world;

            Client client = Client.clients.Find((c) => c.Address == world.address);

            if (client != null)
            {
                client.World = world;
                client.HasDrawnTheBox = false;
            }
        }
    }

    void MatchAction(ActionUnion action, string message)
    {
        if (action.type == "fire_response")
        {
            FireActionResponse response = JsonUtility.FromJson<FireActionResponse>(message);

            if (response.didHit)
            {
                Instantiate(HitPrefab, new Vector3(response.hitPosition[0], response.hitPosition[1], 10), new Quaternion(0, 0, 0, 0));
            }

            // TODO: check if the player is in this world, and yes decrease hp
        }
    }
}


