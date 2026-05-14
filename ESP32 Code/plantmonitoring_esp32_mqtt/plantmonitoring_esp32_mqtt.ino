#include <WiFi.h>
#include <PubSubClient.h>
#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"

// ── WiFi ─────────────────────────────────────────────────────────────────────
const char* ssid         = "";
const char* wifiPassword = "";

// ── MQTT ─────────────────────────────────────────────────────────────────────
const char* mqttBroker   = "ip";
const int   mqttPort     = 1883;

// username = device ID as string, password = plainApiToken 
const char* mqttUsername = "8";
const char* mqttPassword = "token";

// Derived in setup() 
String telemetryTopic;
String commandTopic;

// ── Sensor calibration ───────────────────────────────────────────────────────
const int DRY_VALUE = 4000;
const int WET_VALUE = 535;

// ── Pin definitions ──────────────────────────────────────────────────────────
const int SENSOR_PIN = 36;
const int POWER_PIN  = 17;
const int BUTTON_PIN = 27;

// ── Timing ───────────────────────────────────────────────────────────────────
const unsigned long TIMER_DELAY = 120000;
unsigned long lastSendTime = 0;

// ── MQTT client ───────────────────────────────────────────────────────────────
WiFiClient   wifiClient;
PubSubClient mqttClient(wifiClient);

// ── Function declarations ─────────────────────────────────────────────────────
void connectWifi();
void connectMqtt();
void onCommandReceived(char* topic, byte* payload, unsigned int length);
int  getMoisturePercentage();
void sendTelemetry();

// ─────────────────────────────────────────────────────────────────────────────

void setup() {
    WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0);

    Serial.begin(115200);
    delay(1000);
    Serial.println("\n\n=== Plant Monitor — MQTT ===\n");

    // only powered for 10ms during a reading to prevent corosion
    pinMode(POWER_PIN, OUTPUT);
    digitalWrite(POWER_PIN, LOW);

    pinMode(BUTTON_PIN, INPUT_PULLUP);

    // ── Derive topics from device ID ──────────────────────────────────────────
    telemetryTopic = "devices/" + String(mqttUsername) + "/telemetry";
    commandTopic   = "devices/" + String(mqttUsername) + "/commands";

    Serial.print("Telemetry topic: "); Serial.println(telemetryTopic);
    Serial.print("Command topic:   "); Serial.println(commandTopic);

    // ── WiFi ──────────────────────────────────────────────────────────────────
    connectWifi();

    // ── MQTT ──────────────────────────────────────────────────────────────────
    mqttClient.setServer(mqttBroker, mqttPort);
    mqttClient.setCallback(onCommandReceived);
    connectMqtt();

    // send reading on boot
    sendTelemetry();
    lastSendTime = millis();
}

void loop() {
    // Maintain WiFi, reconnect if dropped
    if (WiFi.status() != WL_CONNECTED) {
        Serial.println("WiFi lost. Reconnecting...");
        connectWifi();
    }

    // Maintain MQTT, reconnect if dropped
    if (!mqttClient.connected()) {
        connectMqtt();
    }

    mqttClient.loop();

    // ── Manual button trigger ─────────────────────────────────────────────────
    if (digitalRead(BUTTON_PIN) == LOW) {
        Serial.println("\n--- Button pressed ---");
        sendTelemetry();
        lastSendTime = millis();
        delay(1000); // debounce
    }

    // ── Automatic 15-minute timer ─────────────────────────────────────────────
    if ((millis() - lastSendTime) > TIMER_DELAY) {
        sendTelemetry();
        lastSendTime = millis();
    }
}

// ─────────────────────────────────────────────────────────────────────────────

void connectWifi() {
    if (WiFi.status() == WL_CONNECTED) return;

    WiFi.begin(ssid, wifiPassword);
    Serial.print("Connecting to WiFi");

    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }

    Serial.println();
    Serial.print("Connected. IP: ");
    Serial.println(WiFi.localIP());
}

void connectMqtt() {
    while (!mqttClient.connected()) {
        Serial.print("Connecting to MQTT...");

        String clientId = "esp32-" + String(mqttUsername);

        if (mqttClient.connect(clientId.c_str(), mqttUsername, mqttPassword)) {
            Serial.println(" connected.");

            // Resubscribe every reconnect 
            mqttClient.subscribe(commandTopic.c_str());
            Serial.print("Subscribed to: ");
            Serial.println(commandTopic);
        } else {
            // rc=-4 timeout | rc=-3 connection lost | rc=-2 connect failed
            // rc= 4 bad credentials | rc= 5 not authorised
            Serial.print(" failed. rc=");
            Serial.println(mqttClient.state());
            Serial.println("Retrying in 5 seconds...");
            delay(5000);
        }
    }
}

void onCommandReceived(char* topic, byte* payload, unsigned int length) {
    String message;
    for (unsigned int i = 0; i < length; i++) {
        message += (char)payload[i];
    }

    Serial.print("Command received [");
    Serial.print(topic);
    Serial.print("]: ");
    Serial.println(message);

    if (message.indexOf("activate_pump") >= 0) {
        Serial.println("Action: activate pump");
        // TODO: drive pump relay pin HIGH here
    }
    else if (message.indexOf("request_reading") >= 0) {
        Serial.println("Action: immediate reading requested");
        sendTelemetry();
        lastSendTime = millis();
    }
    else {
        Serial.println("Unknown command — ignored.");
    }
}

int getMoisturePercentage() {
    // Power sensor for minimum time to stabilise
    digitalWrite(POWER_PIN, HIGH);
    delay(100);
    
    // taking 10 samples and average to reduce noise
    int total = 0;
    for (int i = 0; i < 10; i++) {
        total += analogRead(SENSOR_PIN);
        delay(10);
    }
    int raw = total / 10;

    Serial.print("Raw ADC: ");
    Serial.print(raw);

    int percent = map(raw, DRY_VALUE, WET_VALUE, 0, 100);
    if (percent < 0)   percent = 0;
    if (percent > 100) percent = 100;

    Serial.print(" | Moisture: ");
    Serial.print(percent);
    Serial.println("%");

    return percent;
}

void sendTelemetry() {
    if (!mqttClient.connected()) {
        connectMqtt();
    }

    int moisture = getMoisturePercentage();

    String payload = "{\"value\": " + String(moisture) + "}";

    bool published = mqttClient.publish(telemetryTopic.c_str(), payload.c_str());

    if (published) {
        Serial.print("Published to ");
        Serial.print(telemetryTopic);
        Serial.print(": ");
        Serial.println(payload);
    } else {
        Serial.println("Publish failed.");
    }
}
