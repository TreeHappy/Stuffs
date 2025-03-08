(Due to technical issues, the search service is temporarily unavailable.)

Here's an extended example that includes JSON parsing, message routing, and handling different message types:

### **Enhanced Server with JSON Processing & Message Dispatching**
```gdscript
extends Node

var _server := TCPServer.new()
var _client_connection : StreamPeerTCP
var _read_buffer := PackedByteArray()

func _ready():
    if _server.listen(8080, "127.0.0.1") != OK:
        push_error("Failed to start server")
        return
    print("Server listening...")

func _process(delta):
    # Accept new connections
    if _server.is_connection_available():
        _client_connection = _server.take_connection()
        print("Client connected")

    # Process existing connection
    if _client_connection and _client_connection.get_status() == StreamPeerTCP.STATUS_CONNECTED:
        _handle_incoming_data()

func _handle_incoming_data():
    # Read all available bytes
    var bytes = _client_connection.get_available_bytes()
    if bytes > 0:
        _read_buffer += _client_connection.get_data(bytes)[1]
    
    # Split messages using newline delimiter
    var str_buffer = _read_buffer.get_string_from_utf8()
    var messages = str_buffer.split("\n", false)
    
    # Process complete messages (leave incomplete in buffer)
    for i in range(messages.size() - 1):
        var msg = messages[i]
        _process_message(msg)
    
    # Update buffer with remaining data
    if messages.size() > 0:
        var last_msg = messages[-1]
        _read_buffer = last_msg.to_utf8_buffer()

func _process_message(raw_message: String):
    var json = JSON.new()
    var error = json.parse(raw_message)
    
    if error != OK:
        push_error("JSON parse error: ", json.get_error_message())
        return
    
    var msg = json.get_data()
    if typeof(msg) != TYPE_DICTIONARY:
        push_error("Invalid message format")
        return
    
    # Route message based on type
    var msg_type = msg.get("type", "")
    match msg_type:
        "chat":
            _handle_chat_message(msg)
        "movement":
            _handle_movement(msg)
        "ping":
            _handle_ping(msg)
        _:
            push_error("Unknown message type: ", msg_type)

func _handle_chat_message(msg: Dictionary):
    var sender = msg.get("sender", "unknown")
    var text = msg.get("text", "")
    print("[Chat] %s: %s" % [sender, text])
    # Add your chat handling logic here

func _handle_movement(msg: Dictionary):
    var x = msg.get("x", 0)
    var y = msg.get("y", 0)
    print("[Movement] Received coordinates: (%d, %d)" % [x, y])
    # Add movement logic here

func _handle_ping(msg: Dictionary):
    print("[Ping] Received ping request")
    # Send pong response
    var response = {
        "type": "pong",
        "timestamp": Time.get_unix_time_from_system()
    }
    _client_connection.put_utf8_string(JSON.stringify(response) + "\n")
```

### **Enhanced Client with JSON Sending**
```gdscript
extends Node

var _client := StreamPeerTCP.new()
var _read_buffer := PackedByteArray()

func _ready():
    var error = _client.connect_to_host("127.0.0.1", 8080)
    if error != OK:
        push_error("Connection failed: ", error)
    print("Connecting...")

func _process(delta):
    match _client.get_status():
        StreamPeerTCP.STATUS_CONNECTED:
            _handle_server_communication()
        StreamPeerTCP.STATUS_CONNECTING:
            pass  # Still connecting
        StreamPeerTCP.STATUS_ERROR:
            push_error("Connection error")

func _handle_server_communication():
    # Example: Send a chat message
    if Input.is_action_just_pressed("ui_accept"):
        var chat_msg = {
            "type": "chat",
            "sender": "client",
            "text": "Hello from client!"
        }
        _send_json(chat_msg)
    
    # Read server responses
    var bytes = _client.get_available_bytes()
    if bytes > 0:
        _read_buffer += _client.get_data(bytes)[1]
        var str_data = _read_buffer.get_string_from_utf8()
        var messages = str_data.split("\n", false)
        
        for i in range(messages.size() - 1):
            _process_server_message(messages[i])
        
        if messages.size() > 0:
            _read_buffer = messages[-1].to_utf8_buffer()

func _send_json(data: Dictionary):
    var json_str = JSON.stringify(data)
    _client.put_utf8_string(json_str + "\n")

func _process_server_message(raw_message: String):
    var json = JSON.new()
    if json.parse(raw_message) != OK:
        push_error("Invalid server response")
        return
    
    var msg = json.get_data()
    match msg.get("type", ""):
        "pong":
            print("Received pong response at ", msg.get("timestamp", 0))
        "server_message":
            print("Server notification: ", msg.get("text", ""))
        _:
            print("Unknown server message: ", raw_message)
```

### Key Features:
1. **Message Delimiters**: Uses newline `\n` to separate messages
2. **JSON Parsing**: Robust error handling for invalid JSON
3. **Message Routing**:
   - Uses `match` statement to handle different message types
   - Separate handler functions for different message categories
4. **Bidirectional Communication**: Both client and server can send/receive
5. **Buffer Management**: Handles partial messages between frames

### Message Format Example:
```javascript
// Client -> Server
{
    "type": "movement",
    "x": 100,
    "y": 200
}

// Server -> Client
{
    "type": "pong",
    "timestamp": 1690834567
}
```

### Usage Tips:
1. **Message Structure**: Always include a "type" field in your JSON
2. **Error Handling**: Add more validation for message fields
3. **Performance**: For high-frequency messages (like position updates), consider binary protocols instead of JSON
4. **Security**: Add authentication and validation for production use
5. **Extensions**: Add more message types and handlers as needed for your application

This implementation provides a foundation you can build upon for networked applications in Godot using proper message handling patterns.

