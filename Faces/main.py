import cv2
import requests
from flask import Flask, request
from mimetypes import guess_type
from time import time_ns
import os
import faces_pb2
import base64

app = Flask(__name__)


def DeserializeMessage(message):
    answer = faces_pb2.Answer()
    answer.ParseFromString(message)
    return answer


def DetectFaces(fileName):
    img = cv2.imread(fileName)
    gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

    face_cascade = cv2.CascadeClassifier('haarcascade_frontalface_default.xml')
    faces = face_cascade.detectMultiScale(gray, 1.1, 4)

    return faces


@app.route('/', methods=['GET', 'POST'])
def UploadImage():
    if 'image' not in request.files:
        print("Параметр определен неверно")
        return 'Параметр определен неверно', 400

    image = request.files['image']

    if image.filename == '':
        print("Пустое изображение")
        return 'Пустое изображение', 400

    mime = guess_type(image.filename)[0] or ''
    if not mime.startswith('image'):
        print("Некорректный тип файла")
        return 'Некорректный тип файла', 400

    ext = image.filename.rpartition('.')[-1]
    fileName = f'./{time_ns()}.{ext}'
    image.save(fileName)

    faces = []
    with open(fileName, 'rb') as image:
        img = bytes(image.read())

        for (x, y, w, h) in DetectFaces(fileName):
            p1 = faces_pb2.Point(x=x, y=y)
            p2 = faces_pb2.Point(x=x+w, y=y+h)
            req = faces_pb2.Crop(image=img, topLeft=p1, bottomRight=p2)
            req = req.SerializeToString()

            response = requests.post(os.getenv('TRRP4_CROP_ADDR'), data=req)

            face = DeserializeMessage(response.content)
            faces.append(base64.b64encode(face.image).decode('ascii'))

    os.remove(fileName)

    return {'faces': faces}


if __name__ == '__main__':
    app.run()
