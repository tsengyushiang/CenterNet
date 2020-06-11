from __future__ import absolute_import
from __future__ import division
from __future__ import print_function

import _init_paths

import os
import cv2

from opts import opts
from detectors.detector_factory import detector_factory

import socket
import sys
import cv2
import pickle
import numpy as np
import struct  # new
import zlib
import threading
import json

imageQueue = []


def recvFrames(conn, data):
    while True:
        # Recieve images data from unity
        while len(data) < payload_size:
            # print("Recv: {}".format(len(data)))
            data += conn.recv(4096)

        # print("Done Recv: {}".format(len(data)))
        packed_msg_size = data[:payload_size]
        data = data[payload_size:]
        msg_size = struct.unpack(">L", packed_msg_size)[0]
        # print("{},msg_size: {}".format(packed_msg_size, msg_size))
        while len(data) < msg_size:
            data += conn.recv(4096)
        frame_data = data[:msg_size]
        data = data[msg_size:]

        nparr = np.fromstring(frame_data, np.uint8)
        img = cv2.imdecode(nparr, cv2.IMREAD_COLOR)

        imageQueue.append(img)


def processImage(conn, opt):
    time_stats = ['tot', 'load', 'pre', 'net', 'dec', 'post', 'merge']

    while True:
        if(len(imageQueue) > 0):
            # use CenterNet models
            ret = detector.run(imageQueue[0])

            data = {
                'result': []
            }
            for bbox in ret['results'][1]:
                if bbox[4] > opt.vis_thresh:
                    data['result'].append({
                        'bbox': bbox[:4],
                        'hp': bbox[5:39]
                    })

            conn.send(json.dumps(data).encode('utf-8'))
            imageQueue.pop(0)

            time_str = ''
            for stat in time_stats:
                time_str = time_str + '{} {:.3f}s |'.format(stat, ret[stat])
            print(time_str)

        if cv2.waitKey(1) == 27:
            break  # esc to quit


if __name__ == '__main__':

    # init CenterNet params
    opt = opts().init()
    os.environ['CUDA_VISIBLE_DEVICES'] = opt.gpus_str
    opt.debug = max(opt.debug, 1)
    Detector = detector_factory[opt.task]
    detector = Detector(opt)

    detector.pause = False

    # init Sockets
    HOST = '127.0.0.1'
    PORT = 8000

    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    print('Socket created')

    s.bind((HOST, PORT))
    print('Socket bind complete')
    s.listen(10)
    print('Socket now listening')

    conn, addr = s.accept()

    data = b""
    payload_size = struct.calcsize(">L")
    print("payload_size: {}".format(payload_size))

    # 建立一個子執行緒
    t = threading.Thread(target=recvFrames, args=(conn, data))

    # 執行該子執行緒
    t.start()

    processImage(conn, opt)
