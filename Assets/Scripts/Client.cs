using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using WebSocketSharp;

public class Client : MonoBehaviour
{

    public static List<Client> clients = new List<Client>();
    public World World;
    public bool HasDrawnTheBox = false;
    private Player player;
    private float nextMoveActionTime = 0.0f;
    public string Address;
    private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();
    private List<Player> players = new List<Player>();

    private static string DefaultAddress = "";

    void Awake()
    {
        DefaultAddress = PlayerPrefs.GetString("address");
    }

    void Start()
    {
        if (clients.Count == 0)
        {
            Connect(DefaultAddress);
        }
    }

    WebSocket ws;
    public void Connect(string address)
    {

        if (clients.Find((Client c) => c.Address == Address) == null)
        {
            clients.Add(this);
        }
        Address = address;
        gameObject.name = Address;
        ws = new WebSocket(address);
        ws.Connect();

        GameObject tank = GameObject.Find("UserTank");
        Transform transform = tank.GetComponent<Transform>();
        Transform turretTransform = tank.GetComponentInChildren<TankTurret>().GetComponent<Transform>();

        player = new Player()
        {
            hp = 100,
            id = tank.GetComponent<Tank>().Id,
            lastFireTS = 1,
            transform = new PlayerTransform()
            {
                position = new float[2] { transform.position.x, transform.position.y },
                rotation = new float[4] { transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w },
            },
            turretRotation = new float[4] { turretTransform.rotation.x, turretTransform.rotation.y, turretTransform.rotation.z, transform.rotation.w }
        };

        ws.Send(JsonUtility.ToJson(new JoinAction()
        {
            player = player
        }));

        ws.OnMessage += (sender, e) =>
        {
            // Debug.Log(e.Data);
            _actions.Enqueue(() => MessageHandler.handler.MatchAction(e.Data, this));
        };

        ws.OnError += (sender, e) =>
        {
            ws.Close();
            ws = null;
            _actions.Enqueue(() => Destroy(gameObject));
        };
    }

    void Update()
    {

        if (World != null && !HasDrawnTheBox)
        {
            DrawWorldBox();
        }

        while (_actions.Count > 0)
        {
            if (_actions.TryDequeue(out var action))
            {
                action?.Invoke();
            }
        }
    }

    public void Send<T>(T acion)
    {
        ws.Send(JsonUtility.ToJson(acion));
    }

    public static void Emit<T>(T action)
    {
        clients.ForEach((c) =>
        {
            c.Send(action);
        });
    }

    public void SendMoveAction(Transform transform, Transform turretTransform)
    {
        if (ws == null || player == null || !ws.IsAlive)
        {
            return;
        }

        string message = JsonUtility.ToJson(new MoveAction()
        {
            playerId = player.id,
            transform = transform == null ? null : new PlayerTransform()
            {
                position = new float[2] { transform.position.x, transform.position.y },
                rotation = new float[4] { transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w },
            },
            turretRotation = turretTransform == null ? null : new float[4] { turretTransform.rotation.x, turretTransform.rotation.y, turretTransform.rotation.z, turretTransform.rotation.w }
        });

        if (Time.time >= nextMoveActionTime)
        {


            ws.Send(message);
            nextMoveActionTime = Time.time + 0.1f;
        }
    }

    public static void EmitMoveAction(Transform transform, Transform turretTransform)
    {
        clients.ForEach((c) =>
        {
            c.SendMoveAction(transform, turretTransform);
        });
    }

    public void Close()
    {
        int clientIndex = clients.FindIndex((c) => c.Address == this.Address);
        clients.RemoveAt(clientIndex);
        ws.Close();
        RemoveAllPlayersAndTanks();
        Destroy(gameObject);
    }

    private void DrawWorldBox()
    {
        HasDrawnTheBox = true;

        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 5;

        if (World.northwest.Length != 2 && World.southeast.Length != 2)
        {
            return;
        }

        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.startWidth = 0.5f;
        lineRenderer.endWidth = 0.5f;

        lineRenderer.SetPosition(0, new Vector3(World.northwest[0], World.northwest[1], 0));
        lineRenderer.SetPosition(1, new Vector3(World.southeast[0], World.northwest[1], 0));
        lineRenderer.SetPosition(2, new Vector3(World.southeast[0], World.southeast[1], 0));
        lineRenderer.SetPosition(3, new Vector3(World.northwest[0], World.southeast[1], 0));
        lineRenderer.SetPosition(4, new Vector3(World.northwest[0], World.northwest[1], 0));
    }

    public static void RemovePlayer(string playerId)
    {

        for (int i = 0; i < clients.Count; i++)
        {
            List<Player> players = clients[i].players;

            int index = players.FindIndex((p) => p.id == playerId);

            if (index > -1)
            {
                players.RemoveAt(index);
            }
        }
    }

    public bool RemoveClientPlayer(string playerId)
    {
        int index = players.FindIndex((p) => p.id == playerId);

        if (index > -1)
        {
            players.RemoveAt(index);
            return true;
        }

        return false;
    }

    public static bool CheckIfPlayerIsConnectedToAnyWorld(string playerId, Client checker)
    {

        for (int i = 0; i < clients.Count; i++)
        {

            if (checker != clients[i])
            {


                List<Player> players = clients[i].players;
                int index = players.FindIndex((p) => p.id == playerId);

                if (index > 1)
                {
                    return true;
                }
            }
        }

        return false;

    }

    public bool AddPlayer(Player player)
    {
        int index = players.FindIndex((p) => p.id == player.id);

        if (index > -1)
        {
            return false;
        }
        else
        {
            players.Add(player);
            return true;
        }
    }

    public void RemoveAllPlayersAndTanks()
    {
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];

            Tank tank = FindObjectsOfType<Tank>().ToList().Find(t => t.Id == player.id);
            if (tank != null)
            {
                Debug.Log("Should get destroyed " + player.id);
                Destroy(tank.gameObject);
            }
        }
    }
}
