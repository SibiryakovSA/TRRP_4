FROM python:3.8

ARG PORT=5000

COPY . .

ENV FLASK_ENV production
ENV FLASK_APP main
ENV FLASK_RUN_PORT $PORT
ENV TRRP4_CROP_ADDR http://10.99.254.247:3000

EXPOSE $PORT

RUN apt-get update && apt-get install -y python3-opencv
RUN pip install -r requirements.txt

CMD ["flask", "run", "--host", "0.0.0.0"]
