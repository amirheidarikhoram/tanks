using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MessageHandler : MonoBehaviour
{

    public static MessageHandler handler;
    public GameObject FirePrefab;
    public GameObject HitPrefab;
    public GameObject TankPrefab;
    public Client ClientPrefab;

    void Start()
    {
        if (handler == null)
        {
            handler = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void MatchAction(string message, Client client)
    {
        // Debug.Log(message);

        ActionUnion actionUnion = JsonUtility.FromJson<ActionUnion>(message);

        if (actionUnion.g_type != null)
        {
            // game action
            MatchGameAction(actionUnion, message, client);

        }
        else if (actionUnion.s_type != null)
        {
            // server action
            MatchServerAction(actionUnion, message, client);
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

            if (receiver.AddPlayer(joinAction.player))
            {

                if (!Client.CheckIfPlayerIsConnectedToAnyWorld(joinAction.player.id, receiver))
                {
                    CreateTank(joinAction.player);
                }
            }
        }
        else if (action.g_type == "player_disconnect")
        {
            PlayerDisconnectAction disconnectAction = JsonUtility.FromJson<PlayerDisconnectAction>(message);
            Tank tank = FindObjectsOfType<Tank>().ToList().Find(t => t.Id == disconnectAction.playerId);

            if (receiver.RemoveClientPlayer(disconnectAction.playerId))
            {

                if (!Client.CheckIfPlayerIsConnectedToAnyWorld(disconnectAction.playerId, receiver))
                {
                    Destroy(tank.gameObject);
                }
            }
        }
        else if (action.g_type == "die")
        {

            DieAction dieAction = JsonUtility.FromJson<DieAction>(message);

            Tank tank = FindObjectsOfType<Tank>().ToList().Find(t => t.Id == dieAction.playerId);
            if (tank != null)
            {

                if (tank.name == "UserTank")
                {
                    SceneManager.LoadScene("DieScene");
                }

                Destroy(tank.gameObject);
                Client.RemovePlayer(dieAction.playerId);
            }
        }
        else if (action.g_type == "player_state_update")
        {
            PlayerStateUpdateAction updateAction = JsonUtility.FromJson<PlayerStateUpdateAction>(message);

            Tank tank = FindObjectsOfType<Tank>().ToList().Find(t => t.Id == updateAction.player.id);
            if (tank != null)
            {
                tank.UpdateWithPlayer(updateAction.player);
            }
        }
    }

    void MatchServerAction(ActionUnion action, string message, Client receiver)
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

            if (introduceServerAction.players != null)
            {
                for (int i = 0; i < introduceServerAction.players.Count; i++)
                {
                    Player player = introduceServerAction.players[i];

                    if (!tankExists(player.id))
                    {
                        receiver.AddPlayer(player);
                        CreateTank(introduceServerAction.players[i]);
                    }
                }
            }
        }
    }

    void MatchAction(ActionUnion action, string message)
    {
        if (action.type == "fire")
        {
            Debug.Log(message);

            FireAction fireAction = JsonUtility.FromJson<FireAction>(message);
            Vector2 direction2D = new Vector2(fireAction.fireDirection[0], fireAction.fireDirection[1]);
            direction2D.Normalize();
            float angle = (Mathf.Atan2(direction2D.y, direction2D.x) * Mathf.Rad2Deg) - 90;
            Instantiate(FirePrefab, new Vector3(fireAction.firePosition[0], fireAction.firePosition[1]), Quaternion.Euler(0, 0, angle));
        }
        else if (action.type == "fire_response")
        {
            FireActionResponse response = JsonUtility.FromJson<FireActionResponse>(message);

            if (response.didHit)
            {
                Debug.Log(message);
                Instantiate(HitPrefab, new Vector3(response.hitPosition[0], response.hitPosition[1], 0), new Quaternion(0, 0, 0, 0));
            }
        }
    }

    void CreateTank(Player player)
    {
        GameObject tankObject = Instantiate(TankPrefab, new Vector3(player.transform.position[0], player.transform.position[1], 10), new Quaternion(0, 0, 0, 0));
        Tank tank = tankObject.GetComponent<Tank>();
        tank.Id = player.id;
        tank.UpdateWithPlayer(player);
    }

    bool tankExists(string playerId)
    {
        Tank tank = FindObjectsOfType<Tank>().ToList().Find(t => t.Id == playerId);
        return tank != null;
    }
}


