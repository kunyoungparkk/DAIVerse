import threading
import json
import numpy as np
from BotServer import BotServer
from IntentModel import IntentModel

intent = IntentModel(model_name='Contest_Cnn_Lstm_model11_CnnLstm.h5')

def to_client(conn, addr):
    try:
        read = conn.recv(2048)
        print("========================")
        print("Connection from: {0}".format(str(addr)))

        if read is None or not read:
            print('클라이언트 연결 끊어짐')
            exit(0)

        recv_json_data = json.loads(read.decode())
        print("데이터 수신 : ", recv_json_data)
        query = recv_json_data['Query']

        intent_prob_array, intent_predict = intent.predict_class(query)
        intent_prob = float(np.max(intent_prob_array))
        intent_name = intent.labels[intent_predict]

        send_json_data_str = {
            "Query" : query,
            "Intent" : intent_name,
            "Probability" : intent_prob
        }
        
        message = json.dumps(send_json_data_str)
        conn.send(message.encode())
    
    except Exception as ex:
        print(ex)
    
    finally:
        conn.close()

if __name__ == '__main__':
    port = 8080
    listen = 100

    # 봇 서버 동작
    bot = BotServer(port, listen)
    bot.create_sock()
    print("bot start")

    while True:
        conn, addr = bot.ready_for_client()
        client = threading.Thread(target=to_client, args=(
            conn,
            addr
        ))
        client.start()
