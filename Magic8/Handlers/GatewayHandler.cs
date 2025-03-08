using Magic8.Structures;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Magic8
{
    class GatewayHandler
    {
        private Uri _gatewayUrl;
        private Uri? _resumeGatewayUrl;
        public static ClientWebSocket WebSocketClient { get; private set; }
        private int _heartbeatInterval = 10;
        private Timer? _heartbeatTimer;
        private bool _heartbeatResponse = true;
        private int _lastSequenceNumber = 0;
        private bool _closedMessageRecieved = false;
        private bool _resumeSuccessful = false;
        private int _requestsIn60Seconds = 0;

        public GatewayHandler(ClientWebSocket webSocketClient)
        {
            WebSocketClient = webSocketClient;
        }

        public async Task Connect()
        {
            //Get Gateway URL 
            try
            {
                HttpResponseMessage response = await HTTPHandler.SendUnauthRequest(HttpMethod.Get, "https://discord.com/api/v10/gateway");
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(json);
                    _gatewayUrl = new Uri(doc.RootElement.GetProperty("url").GetString());// + "?v=10&encoding=json");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Connecting to web socket");
            await WebSocketClient.ConnectAsync(_gatewayUrl, CancellationToken.None);
            Console.WriteLine("Connected");

            _ = Task.Run(RecieveMessages);
        }

        //how to resume: 
        //https://discord.com/developers/docs/events/gateway#preparing-to-resume
        public async Task Reconnect()
        {
            Console.WriteLine("Reconnecting to web socket");
            await WebSocketClient.ConnectAsync(_resumeGatewayUrl, CancellationToken.None);
            _ = Task.Run(RecieveMessages);
            _resumeSuccessful = true;
            Console.WriteLine("Reconnected");

            await SendMessage(GatewayOpcodes.RESUME);
        }

        public async Task RecieveMessages()
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[16384]);

            while (WebSocketClient.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await WebSocketClient.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await HandleClose(result);
                    continue;
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    await HandleMessages(message);
                }
            }
            if (WebSocketClient.State == WebSocketState.Closed && _closedMessageRecieved) return;
            await Reconnect();
        }

        public async Task HandleMessages(string message)
        {
            JsonDocument? payload = null;
            try
            {
                payload = JsonDocument.Parse(message);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JsonException:" + ex.Message);
            }

            GatewayOpcodes opcode = (GatewayOpcodes)payload!.RootElement.GetProperty("op").GetInt32();
            Console.WriteLine("message has been recieved with opcode: " + opcode);

            switch (opcode)
            {
                case GatewayOpcodes.DISPATCH:
                    OP0DispatchEvents eventType;
                    _lastSequenceNumber = payload.RootElement.GetProperty("s").GetInt32();

                    Enum.TryParse(payload.RootElement.GetProperty("t").GetString(), out eventType);
                    await HandleOP0Message(eventType, payload.RootElement.GetProperty("d"));
                    break;

                case GatewayOpcodes.HEARTBEAT:
                    await SendMessage(GatewayOpcodes.HEARTBEAT);
                    //sent to request a heartbeat, requires immediate request 
                    break;

                case GatewayOpcodes.RECONNECT:
                    await Reconnect();
                    break;

                case GatewayOpcodes.INVALID_SESSION:
                    bool d = payload.RootElement.GetProperty("d").GetBoolean();
                    if (d) await Reconnect();
                    else await Connect();
                    break;

                case GatewayOpcodes.HELLO:
                    _heartbeatInterval = payload.RootElement.GetProperty("d").GetProperty("heartbeat_interval").GetInt32();
                    _heartbeatTimer = new Timer(async _ =>
                    {
                        await SendMessage(GatewayOpcodes.HEARTBEAT);
                    }, null, 0, _heartbeatInterval);
                    await SendMessage(GatewayOpcodes.IDENTIFY);
                    break;

                case GatewayOpcodes.HEARTBEAT_ACK:
                    _heartbeatResponse = true;
                    //if an ACK is not recieved back then terminate with any close code 
                    //then reconnect and attempt to resume 
                    break;
            }
        }

        private async Task HandleOP0Message(OP0DispatchEvents eventType, JsonElement d)
        {
            Console.WriteLine("OP0 event recieved with type: " + eventType);
            switch (eventType)
            {
                case OP0DispatchEvents.READY:
                    BotDetails botDetails = JsonSerializer.Deserialize<BotDetails>(d);
                    Application.BotRef!.BotDetails = botDetails;
                    Application.BotRef.InitializeCommands();
                    _resumeGatewayUrl = new Uri(d.GetProperty("resume_gateway_url").GetString());
                    break;
                case OP0DispatchEvents.RESUMED:
                    _resumeSuccessful = true;
                    break;
                case OP0DispatchEvents.INTERACTION_CREATE:
                    InteractionObject interactionObject = JsonSerializer.Deserialize<InteractionObject>(d);
                    if (interactionObject.Data.Options is not null)
                    {
                        JsonElement optionsArray = d.GetProperty("data").GetProperty("options");
                        for (int i = 0; i < optionsArray.GetArrayLength(); i++)
                        {
                            interactionObject.Data.Options[0].Value = optionsArray[i].GetProperty("value");
                        }
                    }
                    Application.BotCommands?.Find(c => c.Id == interactionObject.Data.Id)?.CallCommand(interactionObject);

                    break;
            }
        }

        public async Task SendMessage(GatewayOpcodes opcode)
        {
            if (_requestsIn60Seconds >= 120) { Console.WriteLine("Max Gateway Requests in 60 seconds reached"); return; }
            string payload = "";

            switch (opcode)
            {
                case GatewayOpcodes.HEARTBEAT:
                    if (_heartbeatResponse == false)
                    {
                        await CloseConnection(1001);
                        await Reconnect();
                    }
                    var heartbeat = new { op = 1, d = (int?)null };
                    payload = JsonSerializer.Serialize(heartbeat);
                    _heartbeatResponse = false;
                    break;
                case GatewayOpcodes.IDENTIFY:
                    var identify = new
                    {
                        op = 2,
                        d = new
                        {
                            token = Application.Token,
                            intents = 3276799,
                            properties = new
                            {
                                os = "windows",
                                browser = "custom-client",
                                device = "custom-client"
                            },
                            presence = new
                            {
                                status = "online",
                                activities = new object[] { },
                                afk = false
                            },
                            compress = false
                        }
                    };
                    payload = JsonSerializer.Serialize(identify);
                    break;
                case GatewayOpcodes.PRESENCE_UPDATE:
                    break;
                case GatewayOpcodes.VOICE_STATE_UPDATE:
                    break;
                case GatewayOpcodes.RESUME:
                    var resume = new
                    {
                        op = 6,
                        d = new
                        {
                            token = Application.Token,
                            session_id = Application.BotRef?.BotDetails?.SessionId,
                            seq = _lastSequenceNumber
                        }
                    };
                    payload = JsonSerializer.Serialize(resume);
                    break;
                case GatewayOpcodes.REQUEST_GUILD_MEMBERS:
                    break;
                case GatewayOpcodes.REQUEST_SOUNDBOARD_SOUNDS:
                    break;
                default:
                    Console.WriteLine("No message has been sent for: " + opcode);
                    return;
            }
            if (payload == "") return;

            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            await WebSocketClient.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

            Console.WriteLine("message has been sent with opcode: " + opcode);
            RequestCounter();
        }

        private async Task RequestCounter()
        {
            _requestsIn60Seconds++;
            Console.WriteLine("_requestin60secnds");
            await Task.Delay(60000);
            
            _requestsIn60Seconds--;
        }

        private async Task HandleClose(WebSocketReceiveResult result)
        {
            CloseEvent status = (CloseEvent)result.CloseStatus;
            bool canReconnect = false;

            switch (status)
            {
                case CloseEvent.UNKNOWN_ERROR:
                    canReconnect = true;
                    break;
                case CloseEvent.UNKNOWN_OPCODE:
                    canReconnect = true;
                    break;
                case CloseEvent.DECODE_ERROR:
                    canReconnect = true;
                    break;
                case CloseEvent.NOT_AUTHENTICATED:
                    canReconnect = true;
                    break;
                case CloseEvent.AUTHENTICATION_FAILED:
                    break;
                case CloseEvent.ALREADY_AUTHENTICATED:
                    canReconnect = true;
                    break;
                case CloseEvent.INVALID_SEQ:
                    canReconnect = true;
                    break;
                case CloseEvent.RATE_LIMITED:
                    canReconnect = true;
                    break;
                case CloseEvent.SESSION_TIMED_OUT:
                    canReconnect = true;
                    break;
                case CloseEvent.INVALID_SHARD:
                    break;
                case CloseEvent.INVALID_API_VERSION:
                    break;
                case CloseEvent.INVALID_INTENT:
                    break;
                case CloseEvent.DISALLOWED_INTENT:
                    break;

            }
            if (canReconnect) await Reconnect();
            else await Connect();
        }

        public async Task CloseConnection(int closeCode)
        {
            string reasonText = "";

            switch (closeCode)
            {
                case 1000:
                    reasonText = "Normal Closure";
                    break;
                case 1001:
                    reasonText = "Going Away";
                    break;
            }

            var close = new
            {
                op = 0,
                code = 1000,
                reason = reasonText
            };

            string payload = JsonSerializer.Serialize(close);
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            await WebSocketClient.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
