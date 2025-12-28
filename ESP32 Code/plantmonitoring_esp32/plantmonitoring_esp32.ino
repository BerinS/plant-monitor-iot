#include <WiFi.h>
#include <HTTPClient.h>
#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"

// config

const char* ssid = "";
const char* password = "";

const char* serverUrl = "";
const char* apiToken  = "";

// Calibration Values
const int DRY_VALUE = 4000; // 0% Moisture
const int WET_VALUE = 1200; // 100% Moisture

// Pin Definitions
const int SENSOR_PIN = 36;  // AO signal (VP)
const int POWER_PIN  = 17;  // VCC GPIO 17
const int BUTTON_PIN = 27;  // Button connected to GPIO 27 and GND

unsigned long lastTime = 0;
// send data every 15 min (900000 ms)
unsigned long timerDelay = 900000; 

int getMoisturePercentage();
void sendSensorData();

void setup() {
  // disable brownout detector
  WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0);

  Serial.begin(115200);

  // delay to let the serial monitor start up
  delay(1000); 
  Serial.println("\n\n 30-MINUTE VERSION WITH BUTTON \n");
   
  // sensor power pin
  pinMode(POWER_PIN, OUTPUT);
  digitalWrite(POWER_PIN, LOW); 

  // HIGH by default. 
  // pressing the button connects it to GND
  pinMode(BUTTON_PIN, INPUT_PULLUP);

  // connect to WiFi
  WiFi.begin(ssid, password);
  Serial.print("Connecting to WiFi");
   
  while(WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
   
  Serial.println("");
  Serial.print("Connected! IP: ");
  Serial.println(WiFi.localIP());
   
  // runs once on startup
   
  if(WiFi.status() == WL_CONNECTED){
     sendSensorData(); 
  }

  // reset the timer 
  lastTime = millis();
}

void loop() {
  // check for button
  if (digitalRead(BUTTON_PIN) == LOW) {
    Serial.println("\n--- Button Press Detected ---");
    
    if(WiFi.status() == WL_CONNECTED){
      sendSensorData();
    }
    
    // reset timer 
    lastTime = millis();
    
    // delay to prevent detecting the same press multiple times 
    delay(1000); 
  }
  // -----------------------------------

  // timer logic
  if ((millis() - lastTime) > timerDelay) {
    
    // Check WiFi before trying to send
    if(WiFi.status() == WL_CONNECTED){
      sendSensorData();
    }
    else {
      Serial.println("WiFi Disconnected! Reconnecting...");
      WiFi.reconnect();
    }
    
    // reset timer
    lastTime = millis();
  }
}


int getMoisturePercentage() {
  digitalWrite(POWER_PIN, HIGH);
  delay(10); // wait for electricity to stabilize
   
  int raw = analogRead(SENSOR_PIN);
   
  digitalWrite(POWER_PIN, LOW);

  Serial.print("Raw: ");
  Serial.print(raw);

  // mapping moisture levels
  // map(value, fromLow, fromHigh, toLow, toHigh)
  int percent = map(raw, DRY_VALUE, WET_VALUE, 0, 100);

  // clamp
  if (percent < 0) percent = 0;
  if (percent > 100) percent = 100;

  return percent;
}

void sendSensorData() {
    int moisture = getMoisturePercentage();
    
    Serial.print(" | Sending Moisture: ");
    Serial.print(moisture);
    Serial.println("%");

    // prepare HTTP
    WiFiClient client;
    HTTPClient http;
     
    http.begin(client, serverUrl);
    http.addHeader("Content-Type", "application/json");

     // create JSON Payload
    String jsonPayload = "{\"token\": \"" + String(apiToken) + "\", \"value\": " + String(moisture) + "}";
    // send POST
    int httpResponseCode = http.POST(jsonPayload);

    if (httpResponseCode > 0) {
      Serial.print("Server response: ");
      Serial.println(httpResponseCode); 
    }
    else {
      Serial.print("Error sending POST: ");
      Serial.println(httpResponseCode);
    }
     
    http.end();
}
