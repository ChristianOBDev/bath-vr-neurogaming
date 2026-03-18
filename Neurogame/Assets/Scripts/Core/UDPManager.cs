/*
 * Copyright (c) 2026 NeuroCONCISE
 * All rights reserved.
 *
 * Permission is hereby granted to use, copy, and modify this software
 * for personal or internal purposes, provided that this copyright
 * notice and this permission notice appear in all copies.
 *
 * Redistribution, sublicensing, or commercial use of this software,
 * in source or binary form, is prohibited without prior written
 * permission from the copyright holder.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using NeuroCONCISE;

public class UDPManager : SingletonPersistent<UDPManager>
{
    //Private
    [SerializeField] private bool enableDebugLogging = false;
    private string _remoteIp;
    private int _sendPort;
    private int _receivePort;
    
    private Socket _sendSocket;
    private UdpClient _receiveClient;
    private IPEndPoint _sendEndPoint;
    private IPEndPoint _receiveEndPoint;

    private Thread _receiveThread;
    private CancellationTokenSource _cts;
    private readonly ConcurrentQueue<Action> _mainThreadActions = new();

    private bool _isRunning;

    //Public
    public event Action<double> OnDoubleReceived;
    public event Action<float>  OnFloatReceived;
    public event Action<int>    OnIntReceived;
    public event Action<string> OnStringReceived;
    public event Action<float[]> OnFloatArrayReceived;
    public event Action<byte[]> OnRawReceived;
    
    public void Configure(string remoteIp, int sendPort, int receivePort)
    {
        if (!IPAddress.TryParse(remoteIp, out _))
            throw new ArgumentException("Invalid IP address.");

        if (sendPort <= 0 || receivePort <= 0)
            throw new ArgumentOutOfRangeException("Ports must be greater than zero.");

        _remoteIp = remoteIp;
        _sendPort = sendPort;
        _receivePort = receivePort;
    }

    public void StartUDP()
    {
        if (_isRunning)
            return;

        if (string.IsNullOrEmpty(_remoteIp))
            throw new InvalidOperationException("UDPManager not configured.");

        _sendEndPoint = new IPEndPoint(IPAddress.Parse(_remoteIp), _sendPort);
        _receiveEndPoint = new IPEndPoint(IPAddress.Any, _receivePort);

        _sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _receiveClient = new UdpClient(_receiveEndPoint);

        _cts = new CancellationTokenSource();
        _receiveThread = new Thread(() => ReceiveLoop(_cts.Token))
        {
            IsBackground = true
        };
        _receiveThread.Start();

        _isRunning = true;
    }

    public void StopUDP()
    {
        if (!_isRunning)
            return;

        _cts.Cancel();
        _receiveThread.Join();

        _receiveClient.Close();
        _sendSocket.Close();

        _receiveClient = null;
        _sendSocket = null;

        _isRunning = false;
    }
    
    private void Update()
    {
        while (_mainThreadActions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    private void OnApplicationQuit()
    {
        StopUDP();
    }

    // Send

    public void Send(double value)
        => SendTyped(MessageType.Double, BitConverter.GetBytes(value));

    public void Send(float value)
        => SendTyped(MessageType.Float, BitConverter.GetBytes(value));

    public void Send(int value)
        => SendTyped(MessageType.Int, BitConverter.GetBytes(value));

    public void Send(string value)
        => SendTyped(MessageType.String, Encoding.ASCII.GetBytes(value));
    
    public void Send(float[] values)
    {
        if (values == null || values.Length == 0)
            return;

        byte[] payload = new byte[values.Length * sizeof(float)];
        Buffer.BlockCopy(values, 0, payload, 0, payload.Length);

        SendTyped(MessageType.FloatArray, payload);
    }

    public void SendRaw(byte[] data)
        => SendTyped(MessageType.Raw, data);

    private void SendTyped(MessageType type, byte[] payload)
    {
        if (!_isRunning)
            return;

        byte[] message = new byte[payload.Length + 1];
        message[0] = (byte)type;
        Buffer.BlockCopy(payload, 0, message, 1, payload.Length);

        _sendSocket.SendTo(message, _sendEndPoint);

        if (enableDebugLogging)
        {
            string value = FormatPayloadForDebug(type, payload);
            Debug.Log($"UDP Send [{type}] {value}");
        }
    }

    // ---------------------- Receive ----------------------

    private void ReceiveLoop(CancellationToken token)
    {
        var remote = new IPEndPoint(IPAddress.Any, 0);

        while (!token.IsCancellationRequested)
        {
            try
            {
                if (_receiveClient.Available == 0)
                {
                    Thread.Sleep(1);
                    continue;
                }

                byte[] data = _receiveClient.Receive(ref remote);
                HandleReceivedData(data);
            }
            catch (SocketException)
            {
                // Socket closed during shutdown
                break;
            }
            catch (Exception e)
            {
                if (enableDebugLogging)
                    Debug.LogException(e);
            }
        }
    }

    private void HandleReceivedData(byte[] data)
    {
        if (data == null || data.Length < 1)
            return;

        MessageType type = (MessageType)data[0];

        _mainThreadActions.Enqueue(() =>
        {
            switch (type)
            {
                case MessageType.Double:
                    OnDoubleReceived?.Invoke(BitConverter.ToDouble(data, 1));
                    break;

                case MessageType.Float:
                    OnFloatReceived?.Invoke(BitConverter.ToSingle(data, 1));
                    break;

                case MessageType.Int:
                    OnIntReceived?.Invoke(BitConverter.ToInt32(data, 1));
                    break;

                case MessageType.String:
                    OnStringReceived?.Invoke(Encoding.ASCII.GetString(data, 1, data.Length - 1));
                    break;
                
                case MessageType.FloatArray:
                    OnFloatArrayReceived?.Invoke(ConvertBytesToFloatArray(data, 1));
                    break;

                default:
                    OnRawReceived?.Invoke(data);
                    break;
            }

            if (enableDebugLogging)
                Debug.Log($"UDP Received [{type}]");
        });
    }
    
    private float[] ConvertBytesToFloatArray(byte[] data, int offset)
    {
        int byteCount = data.Length - offset;

        if (byteCount % sizeof(float) != 0)
            return Array.Empty<float>();

        int floatCount = byteCount / sizeof(float);
        float[] floats = new float[floatCount];

        Buffer.BlockCopy(data, offset, floats, 0, byteCount);
        return floats;
    }
    
    private string FormatPayloadForDebug(MessageType type, byte[] payload)
    {
        switch (type)
        {
            case MessageType.Double:
                return BitConverter.ToDouble(payload, 0).ToString();

            case MessageType.Float:
                return BitConverter.ToSingle(payload, 0).ToString();

            case MessageType.Int:
                return BitConverter.ToInt32(payload, 0).ToString();

            case MessageType.String:
                return System.Text.Encoding.UTF8.GetString(payload);

            case MessageType.FloatArray:
            {
                int count = payload.Length / sizeof(float);
                float[] values = new float[count];
                for (int i = 0; i < count; i++)
                    values[i] = BitConverter.ToSingle(payload, i * sizeof(float));

                return $"[{string.Join(", ", values)}]";
            }

            case MessageType.Raw:
                return $"Raw ({payload.Length} bytes)";

            default:
                return $"Unknown ({payload.Length} bytes)";
        }
    }
}

namespace NeuroCONCISE
{
    public enum MessageType : byte
    {
        Double = 1,
        Float = 2,
        Int = 3,
        String = 4,
        FloatArray = 5,
        Raw = 255
    }
}

