#include <WiFi.h>
#include <HTTPClient.h>

// ================= config =================

const char* ssid = "";
const char* password = "";

const char* serverUrl = "http://serverip:5000/api/sensor";
const char* apiToken  = "token";

const int DRY_VALUE = 4095; // 0% Moisture
const int WET_VALUE = 1300; // 100% Moisture

// 4. Pin Definitions
const int SENSOR_PIN = 36;  // AO signal (VP)
const int POWER_PIN  = 17;  

// =================================================

unsigned long lastTime = 0;
// Send data every 60 seconds
unsigned long timerDelay = 10000; 

void setup() {
  Serial.begin(115200);

  pinMode(POWER_PIN, OUTPUT);
  digitalWrite(POWER_PIN, LOW); 

  // Connect to WiFi
  WiFi.begin(ssid, password);
  Serial.println("Connecting to WiFi");
  while(WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.print("Connected! IP: ");
  Serial.println(WiFi.localIP());
}

// function to read sensor ao
int getMoisturePercentage() {

  digitalWrite(POWER_PIN, HIGH);
  delay(10); // wait for electricity to stabilize
  
  int raw = analogRead(SENSOR_PIN);
  
  // stop corrosion
  digitalWrite(POWER_PIN, LOW);

  Serial.print("Raw: ");
  Serial.print(raw);

  // Mapping moisture levels
  // map(value, fromLow, fromHigh, toLow, toHigh)
  int percent = map(raw, DRY_VALUE, WET_VALUE, 0, 100);

  // sensor data can jitter, so it gets constrained to specified values
  if (percent < 0) percent = 0;
  if (percent > 100) percent = 100;

  return percent;
}

void loop() {
  // Timer check
  if ((millis() - lastTime) > timerDelay) {
    
    if(WiFi.status() == WL_CONNECTED){
      
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
    else {
      Serial.println("WiFi Disconnected");
    }
    lastTime = millis();
  }
}
