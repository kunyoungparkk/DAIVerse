import tensorflow as tf
from tensorflow.keras.models import Model, load_model
from tensorflow.keras import preprocessing
from konlpy.tag import Komoran
import numpy as np

class IntentModel:
    def __init__(self, model_name):
        self.labels ={0: '0', 1: '1', 2: '2', 3: '3', 4: '4', 5: '5', 6: '6', 7: '7', 8: '8', 9: '9', 10: '10', 11: '11', 12: '12', 13: '13', 14: '14', 15: '15', 16: '16', 17: '17', 18: '18', 19: '19', 20: '20', 21: '21', 22: '22', 23: '23', 24: '24', 25: '25', 26: '26', 27: '27', 28: '28', 29: '29'}
        self.model = load_model(model_name)


    def predict_class(self, query):
        query = query.lower()
        komoran = Komoran()
        MAX_SEQ_LEN = 14
        input_text = komoran.pos(query)
        exclusion_tags=['EC', 'VX', 'XSV', 'SF', 'SP', 'SS', 'SE', 'EF']

        word_list=[]
        for i in input_text:
            if i[1] not in exclusion_tags:
                word_list.append(i[0])

        word_index_dict = {}
        with open('ContestData_word_index11.txt', 'r', encoding = 'UTF-8') as f:
            while True:
                line = f.readline()
                try:
                    word_index_dict[eval(line.split(':')[0].strip(' '))] = int(line.split(':')[1].rstrip(',\n'))
                except:
                    pass
                if not line : break

        w2i=[]
        for word in word_list:
            try:
                w2i.append(word_index_dict[word])
            except KeyError:
                pass
                
        sequences=[w2i]
        
        padded_sen=preprocessing.sequence.pad_sequences(sequences, maxlen=MAX_SEQ_LEN, padding='post')
        predict = self.model.predict(padded_sen)
        real_IDO=np.array(['포인트'])
        word_list=np.array(word_list)
        if (True in (np.in1d(word_list, real_IDO))):
            predict[0,0:22]=0
            predict[0,24:]=0

        predict_class = tf.math.argmax(predict, axis = 1)

        return predict, predict_class.numpy()[0]
