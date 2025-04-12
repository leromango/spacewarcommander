import serial
import threading
import time
import websocket_server
import json

SERIAL_PORT = '/dev/cu.usbserial-10'
BAUD_RATE = 9600

WS_HOST = '0.0.0.0'  
WS_PORT = 65102

clients = []
serial_conn = None

def new_client(client, server):
    print(f"New client connected: {client['address']}")
    clients.append(client)

def client_left(client, server):
    print(f"Client disconnected: {client['address']}")
    if client in clients:
        clients.remove(client)

def message_received(client, server, message):
    print(f"[WS -> Serial] {message}")
    if serial_conn and serial_conn.is_open:
        serial_conn.write((message + "\n").encode('utf-8'))

def serial_to_ws():
    global serial_conn

    while serial_conn and serial_conn.is_open:
        try:
            if serial_conn.in_waiting > 0:
                data = serial_conn.readline().decode('utf-8', errors='ignore').strip()
                if data:
                    print(f"[Serial -> WS] {data}")
                    for client in clients:
                        server.send_message(client, data)
            else:
                time.sleep(0.01)
        except Exception as e:
            print("Error in serial_to_ws:", e)
            time.sleep(1)  

def main():
    global serial_conn, server

    try:
        serial_conn = serial.Serial(SERIAL_PORT, BAUD_RATE, timeout=1)
        print(f"Connected to serial port: {SERIAL_PORT}")
    except Exception as e:
        print("Error connecting to serial port:", e)
        return

    server = websocket_server.WebsocketServer(host=WS_HOST, port=WS_PORT)
    server.set_fn_new_client(new_client)
    server.set_fn_client_left(client_left)
    server.set_fn_message_received(message_received)

    serial_thread = threading.Thread(target=serial_to_ws, daemon=True)
    serial_thread.start()

    print(f"WebSocket server started on {WS_HOST}:{WS_PORT}")

    try:
        server.run_forever()
    except KeyboardInterrupt:
        print("Exiting program...")
    finally:
        if serial_conn and serial_conn.is_open:
            serial_conn.close()
            print("Serial connection closed")

if __name__ == '__main__':
    main()