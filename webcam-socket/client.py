import cv2
import io
import socket
import struct
import time
import pickle
import zlib

payload = struct.pack(">L", 8787)
print(payload[0], payload[1], payload[2], payload[3], 8787)

client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
client_socket.connect(('127.0.0.1', 8000))
connection = client_socket.makefile('wb')

cam = cv2.VideoCapture(0)

img_counter = 0

encode_param = [int(cv2.IMWRITE_JPEG_QUALITY), 90]

while True:
    ret, frame = cam.read()
    result, frame = cv2.imencode('.jpg', frame, encode_param)
    # data = zlib.compress(pickle.dumps(frame, 0))

    data = pickle.dumps(frame, 0)

    size = len(data)

    # print("{}: {}".format(img_counter, size))
    payload = struct.pack(">L", size)
    print(payload[0], payload[1], payload[2], payload[3], size)
    client_socket.sendall(payload + data)
    img_counter += 1

cam.release()
