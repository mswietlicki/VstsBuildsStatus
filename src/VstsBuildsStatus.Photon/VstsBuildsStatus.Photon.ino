#include "application.h"
#include "HttpClient/HttpClient.h"
#include "neopixel/neopixel.h"

#define PIXEL_PIN D2
#define PIXEL_COUNT 27
#define PIXEL_TYPE WS2812B

Adafruit_NeoPixel strip = Adafruit_NeoPixel(PIXEL_COUNT, PIXEL_PIN, PIXEL_TYPE);


unsigned int nextTime = 0;

HttpClient http;
http_header_t headers[] = {
    { "Content-Type", "application/json" },
    { "Accept" , "application/json" },
    { NULL, NULL } // NOTE: Always terminate headers with NULL
};
http_request_t request;
http_response_t response;

void setup() {
  strip.begin();
  strip.show();
  Serial.begin(9600);
}

void loop() {
    if (nextTime > millis()) {
        return;
    }

    request.hostname = "avengersbuildsstatus.azurewebsites.net";
    request.port = 80;
    request.path = "/api/softwareone-pc/PyraCloud/build/int-oci-web-api-deploy-test";

    http.get(request, response, headers);

    if(response.status != 200) {
        strip.setPixelColor(0, strip.Color(255, 0, 255));
        Serial.println(response.status);
    } else {
        Serial.println(response.body);
        if(response.body.indexOf("Failed") > 0) {
            strip.setPixelColor(0, strip.Color(255, 0, 0));
        } else if(response.body.indexOf("InProgress") > 0) {
            strip.setPixelColor(0, strip.Color(255, 255, 0));
        } else if(response.body.indexOf("Succeeded") > 0) {
            strip.setPixelColor(0, strip.Color(0, 255, 0));
        } else {
            strip.setPixelColor(0, strip.Color(20, 20, 20));
        }
    }
    strip.setBrightness(50);
    strip.show();

    nextTime = millis() + 5000;
}