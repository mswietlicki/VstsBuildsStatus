#include "application.h"
#include "HttpClient.h"
#include "NeoPixel.h"

#define PIXEL_PIN D2
#define PIXEL_COUNT 27
#define PIXEL_TYPE WS2812B

#define BUILD_REFRESH_DELAY 5000
#define BUILD_STATUS_UNKNOWN 0
#define BUILD_STATUS_SUCCEEDED 1
#define BUILD_STATUS_INPROGRESS 2
#define BUILD_STATUS_FAILED 3
#define BUILD_STATUS_NOTSTARTED 4
#define BUILD_STATUS_CANCELED 5

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

String builds[] = {
    "int-oci-web-api-deploy-test",
    "int-oci-web-api-deploy-qa",
    "int-oci-web-api-deploy-live",
    "int-oci-web-app-deploy-test",
    "int-oci-web-app-deploy-qa",
    "int-oci-web-app-deploy-live",
    "int-c2c-presence-web-api-test",
    "int-c2c-presence-web-api-qa",
    "int-c2c-presence-web-api-live",
    "int-c2c-presence-web-app-test",
    "int-c2c-presence-web-app-qa",
    "int-c2c-presence-web-app-live",
    "int-c2c-messaging-common-nuget",
    "int-c2c-messaging-service-test",
    "int-c2c-messaging-service-qa",
    "int-c2c-messaging-service-live",
    "int-c2c-messaging-web-api-test",
    "int-c2c-messaging-web-api-qa",
    "int-c2c-messaging-web-api-live",
    "int-c2c-messaging-web-app-test",
    "int-c2c-messaging-web-app-qa",
    "int-c2c-messaging-web-app-live",
    "ops-automation-publish",
    "int-rmr-api-test",
    "int-rmr-api-qa",
    "int-rmr-web-test",
    "int-rmr-web-qa",
};
String requestBody;


uint8_t buildsStatus[PIXEL_COUNT];
uint8_t stripTick = 0;

void showBuildsStatus() {
    if(stripTick) stripTick = 0; else stripTick = 1;
    for (int i = 0; i < PIXEL_COUNT; i++) {
        strip.setPixelColor(PIXEL_COUNT - i - 1, getBuildStatusColor(buildsStatus[i]));
    }
    strip.show();
}

uint32_t getBuildStatusColor(uint8_t status){
    switch(status)
    {
        case BUILD_STATUS_UNKNOWN:
            return strip.Color(0, 0, 0);
        case BUILD_STATUS_SUCCEEDED:
            return strip.Color(0, 50, 0);
        case BUILD_STATUS_NOTSTARTED:
            return strip.Color(50, 50, 0);
        case BUILD_STATUS_INPROGRESS:
            if(stripTick)
                return strip.Color(100, 100, 0);
            else
                return strip.Color(0, 0, 0);
        case BUILD_STATUS_FAILED:
            if(stripTick)
                return strip.Color(255, 0, 0);
            else
                return strip.Color(0, 0, 0);
        case BUILD_STATUS_CANCELED:
            return strip.Color(50, 0, 0);
        default:
            return strip.Color(0, 0, 0);
    }
}
Timer timer(500, showBuildsStatus);


void createRequestBody(){
    requestBody = "[";
    for (int i = 0; i < arraySize(builds); i++) {
        requestBody += "\"" + builds[i] + "\",";
    }
    requestBody += "]";
}


void initStrip(){
    strip.begin();
    for (int i = 0; i < PIXEL_COUNT; i++) {
        strip.setPixelColor(i, strip.Color(255, 255, 255));
    }
    strip.show();
}

uint8_t downloadBuildsStatuses(){
    request.hostname = "avengersbuildsstatus.azurewebsites.net";
    request.port = 80;
    request.path = "/api/softwareone-pc/PyraCloud/build/simple";
    request.body = requestBody;

    http.post(request, response, headers);

    if(response.status != 200) {
        RGB.color(255, 0, 0);
        Serial.printlnf("Download builds statuses failed: %d", response.status);
        return 0;
    } else {
        RGB.color(0, 0, 0);
        return 1;
    }
}

void readBuildStatusesFromResponseBody(){
    Serial.println();
    Serial.println("BuildStatuses:");
    int16_t p = 0, np = 0;
    for (int i = 0; i < arraySize(builds); i++) {
        np = response.body.indexOf(",", p + 1);

        if(response.body.lastIndexOf("Failed", np) > p)
        {
            buildsStatus[i] = BUILD_STATUS_FAILED;
            Serial.printlnf("    %s: %s", builds[i].c_str(), "FAILED");
        }
        else if(response.body.lastIndexOf("InProgress", np) > p)
        {
            buildsStatus[i] = BUILD_STATUS_INPROGRESS;
            Serial.printlnf("    %s: %s", builds[i].c_str(), "INPROGRESS");
        }
        else if(response.body.lastIndexOf("Succeeded", np) > p)
        {
            buildsStatus[i] = BUILD_STATUS_SUCCEEDED;
            Serial.printlnf("    %s: %s", builds[i].c_str(), "SUCCEEDED");
        }
        else if(response.body.lastIndexOf("NotStarted", np) > p)
        {
            buildsStatus[i] = BUILD_STATUS_NOTSTARTED;
            Serial.printlnf("    %s: %s", builds[i].c_str(), "NOTSTARTED");
        }
        else if(response.body.lastIndexOf("Canceled", np) > p)
        {
            buildsStatus[i] = BUILD_STATUS_CANCELED;
            Serial.printlnf("    %s: %s", builds[i].c_str(), "CANCELED");
        }
        else
        {
            buildsStatus[i] = BUILD_STATUS_UNKNOWN;
            Serial.printlnf("    %s: %s", builds[i].c_str(), "UNKNOWN");
        }
        p = np;
    }
}

void setup() {
    Serial.begin(57600);
    RGB.control(true);
    RGB.color(0, 0, 0);
    initStrip();
    createRequestBody();
    timer.start();
}

void loop() {
    if (nextTime > millis()) {
        return;
    }

    if(downloadBuildsStatuses()){
        readBuildStatusesFromResponseBody();
    }

    nextTime = millis() + BUILD_REFRESH_DELAY;
}
