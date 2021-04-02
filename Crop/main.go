package main

import (
	"./ProtoClasses"
	"bytes"
	"fmt"
	"github.com/golang/protobuf/proto"
	"image"
	"image/draw"
	"image/jpeg"
	"image/png"
	_ "image/png"
	"io/ioutil"
	"net/http"
	"os"
	"time"
)

func WriteResponse(w http.ResponseWriter, image []byte, ok bool, message string ) {
	respProto := ProtoClasses.Answer{Image: image, Ok: ok, Message: message}
	resp, err := proto.Marshal(&respProto)
	if err != nil{
		fmt.Println("Ошибка сериализации протобаф класса")
	}

	wroteLen, err := w.Write(resp)
	fmt.Println(wroteLen)
	fmt.Println(err)
}

func CropImage(w http.ResponseWriter, r *http.Request){
	body, err := ioutil.ReadAll(r.Body)
	if err != nil {
		WriteResponse(w, []byte{}, false, "Ошибка чтения тела запроса")
		return
	}
	if len(body) == 0 {
		WriteResponse(w, []byte{}, false, "Тело запроса пустое")
		return
	}
	var req ProtoClasses.Crop
	err = proto.Unmarshal(body, &req)
	if err != nil {
		WriteResponse(w, []byte{}, false, "Ошибка десериализации")
		return
	}

	input, imageFormat, err := image.Decode(bytes.NewReader(req.Image))
	if err != nil {
		WriteResponse(w, []byte{}, false, "Ошибка декодирования изображения")
		return
	}

	resRect := image.Rect(int(req.TopLeft.X), int(req.TopLeft.Y), int(req.BottomRight.X), int(req.BottomRight.Y))
	res := image.NewRGBA(resRect)
	draw.Draw(res, resRect, input, resRect.Min, draw.Src)

	respWriter := bytes.NewBuffer([]byte{})
	if imageFormat == "png" {
		err = png.Encode(respWriter, res)
	} else {
		err = jpeg.Encode(respWriter, res, &jpeg.Options{Quality: 100})
	}
	if err != nil {
		WriteResponse(w, []byte{}, false, "Ошибка создания изображения")
		return
	}

	WriteResponse(w, respWriter.Bytes(), true, "")
}


func main(){
	address, ok := os.LookupEnv("TRRP4_CROP_ADDR")
	if !ok {
		fmt.Println("Переменная окружения TRRP4_CROP_ADDR не задана")
		return
	}

	fmt.Println("Ожидания подключения по адресу " + address)
	mux := http.NewServeMux()
	mux.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		CropImage(w, r)
	})

	server := &http.Server{
		ReadHeaderTimeout: 5 * time.Second,
		WriteTimeout:      30 * time.Second,
		Handler:           mux,
		Addr:              address,
	}
	server.ListenAndServe()
}
