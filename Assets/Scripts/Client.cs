using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class Client : MonoBehaviour
{

    public static List<Client> clients = new List<Client>();
    private Player player;

    private float nextMoveActionTime = 0.0f;
    public string Address;

    private readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

    void Start()
    {
        if (clients.Count == 0)
        {
            Connect("ws://127.0.0.1:9000");
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
            id = "1",
            lastFireTS = 1,
            transform = new PlayerTransform()
            {
                position = new float[2] { transform.position.x, transform.position.y },
                rotation = new float[3] { transform.rotation.x, transform.rotation.y, transform.rotation.z },
            },
            turretRotation = new float[3] { turretTransform.rotation.x, turretTransform.rotation.y, turretTransform.rotation.z }
        };

        ws.Send(JsonUtility.ToJson(new JoinAction()
        {
            player = player
        }));

        ws.OnMessage += (sender, e) =>
        {
            // Debug.Log(e.Data);
            _actions.Enqueue(() => MessageHandler.hander.MatchAction(e.Data));
        };

        ws.OnError += (sender, e) => {
            ws.Close();
            ws = null;
            _actions.Enqueue(() => Destroy(gameObject));
        };
    }

    void Update()
    {
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
                rotation = new float[3] { transform.rotation.x, transform.rotation.y, transform.rotation.z },
            },
            turretRotation = turretTransform == null ? null : new float[3] { turretTransform.rotation.x, turretTransform.rotation.y, turretTransform.rotation.z }
        });

        if (Time.time >= nextMoveActionTime)
        {


            ws.Send(message);
            nextMoveActionTime = Time.time + 0.2f;
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
        Destroy(gameObject);
    }
}