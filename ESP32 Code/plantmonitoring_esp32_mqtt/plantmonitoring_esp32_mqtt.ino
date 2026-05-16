#include <WiFi.h>
#include <PubSubClient.h>
#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"

// ── WiFi ─────────────────────────────────────────────────────────────────────
const char* ssid         = "wifi";
const char* wifiPassword = "pass";

// ── MQTT ─────────────────────────────────────────────────────────────────────
const char* mqttBroker   = "ip";
const int   mqttPort     = 1883;

const char* mqttUsername = "8";
const char* mqttPassword = "your-plain-token";

String telemetryTopic;
String commandTopic;

// ── Sensor calibration ───────────────────────────────────────────────────────
const int DRY_VALUE = 4095;
const int WET_VALUE = 535;

// ── Pin definitions ──────────────────────────────────────────────────────────
const int SENSOR_PIN = 36;
const int POWER_PIN  = 17;
const int BUTTON_PIN = 27;
const int PUMP_PIN   = 25; // Relay IN 

// ── Timing ───────────────────────────────────────────────────────────────────
const unsigned long TIMER_DELAY        = 900000; // 15 minutes
const unsigned long MAX_PUMP_DURATION  = 8000;  // 8 seconds hard limit for pump running
                                                  
unsigned long lastSendTime  = 0;
unsigned long pumpStartTime = 0;
unsigned long pumpDuration = 0;

bool pumpRunning   = false;

// ── MQTT client ───────────────────────────────────────────────────────────────
WiFiClient   wifiClient;
PubSubClient mqttClient(wifiClient);

// ── Function declarations ─────────────────────────────────────────────────────
void connectWifi();
void connectMqtt();
void onCommandReceived(char* topic, byte* payload, unsigned int length);
int  getMoisturePercentage();
void sendTelemetry();
void activatePump(unsigned long durationMs);
void deactivatePump();

// ─────────────────────────────────────────────────────────────────────────────

void setup() {
    WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0);

    Serial.begin(115200);
    delay(1000);
    Serial.println("\n\n=== Plant Monitor — MQTT ===\n");

    // Sensor power — LOW by default to prevent corrosion
    pinMode(POWER_PIN, OUTPUT);
    digitalWrite(POWER_PIN, LOW);

    pinMode(BUTTON_PIN, INPUT_PULLUP);

    // Pump relay — LOW by default (pump off)
    // This is critical — on boot the pin floats briefly before pinMode.
    // Setting LOW immediately ensures the pump doesn't fire on startup.
    pinMode(PUMP_PIN, OUTPUT);
    digitalWrite(PUMP_PIN, LOW);

    // Derive topics from device ID — single source of truth
    telemetryTopic = "devices/" + String(mqttUsername) + "/telemetry";
    commandTopic   = "devices/" + String(mqttUsername) + "/commands";

    Serial.print("Telemetry: "); Serial.println(telemetryTopic);
    Serial.print("Commands:  "); Serial.println(commandTopic);

    connectWifi();

    mqttClient.setServer(mqttBroker, mqttPort);
    mqttClient.setCallback(onCommandReceived);

    mqttClient.setBufferSize(512);

    connectMqtt();

    sendTelemetry();
    lastSendTime = millis();
}

void loop() {
    if (WiFi.status() != WL_CONNECTED) {
        Serial.println("WiFi lost. Reconnecting...");
        connectWifi();
    }

    if (!mqttClient.connected()) {
        connectMqtt();
    }

    // Must run every loop, handles incoming messages
    mqttClient.loop();

    if (pumpRunning && (millis() - pumpStartTime) >= pumpDuration) {
        Serial.println("Pump cycle complete.");
        deactivatePump();
    }

    // ── Button trigger ────────────────────────────────────────────────────────
    if (digitalRead(BUTTON_PIN) == LOW) {
        Serial.println("\n--- Button pressed ---");
        sendTelemetry();
        lastSendTime = millis();
        delay(1000);
    }

    // ── Automatic timer ───────────────────────────────────────────────────────
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
            mqttClient.subscribe(commandTopic.c_str());
            Serial.print("Subscribed to: ");
            Serial.println(commandTopic);

        } else {
            Serial.print(" failed. rc=");
            Serial.println(mqttClient.state());
            Serial.println("Retrying in 5 seconds...");
            delay(5000);
        }
    }
}

// Fires when a message arrives on devices/{id}/commands
// Payload format: {"action":"activate_pump","duration_seconds":10}
void onCommandReceived(char* topic, byte* payload, unsigned int length) {
    String message;
    for (unsigned int i = 0; i < length; i++) {
        message += (char)payload[i];
    }

    Serial.print("Command [");
    Serial.print(topic);
    Serial.print("]: ");
    Serial.println(message);

    if (message.indexOf("activate_pump") >= 0) {
        // Extract duration_seconds from payload
        unsigned long duration = 3000; // default 3 seconds

        int idx = message.indexOf("duration_seconds");
        if (idx >= 0) {
            // number after "duration_seconds":
            int colonIdx = message.indexOf(':', idx);
            if (colonIdx >= 0) {
                int seconds = message.substring(colonIdx + 1).toInt();
                if (seconds > 0) {
                    duration = (unsigned long)seconds * 1000;
                }
            }
        }

        activatePump(duration);
    }
    else if (message.indexOf("deactivate_pump") >= 0) {
        // Explicit stop pump command 
        deactivatePump();
    }
    else if (message.indexOf("request_reading") >= 0) {
        Serial.println("Immediate reading requested.");
        sendTelemetry();
        lastSendTime = millis();
    }
    else {
        Serial.println("Unknown command — ignored.");
    }
}

void activatePump(unsigned long durationMs) {
    if (durationMs > MAX_PUMP_DURATION) {
        durationMs = MAX_PUMP_DURATION;
        Serial.println("Duration clamped to safety cap.");
    }

    Serial.print("Pump ON for ");
    Serial.print(durationMs / 1000);
    Serial.println(" seconds.");

    digitalWrite(PUMP_PIN, HIGH);
    pumpRunning   = true;
    pumpStartTime = millis();
    pumpDuration  = durationMs; 
}

void deactivatePump() {
    if (!pumpRunning) return;

    digitalWrite(PUMP_PIN, LOW);
    pumpRunning = false;
    pumpDuration = 0;
    Serial.println("Pump OFF.");
}

int getMoisturePercentage() {
    digitalWrite(POWER_PIN, HIGH);
    delay(100);

    int total = 0;
    for (int i = 0; i < 10; i++) {
        total += analogRead(SENSOR_PIN);
        delay(10);
    }
    int raw = total / 10;

    digitalWrite(POWER_PIN, LOW);

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
    if (!mqttClient.connected()) connectMqtt();

    int moisture = getMoisturePercentage();

    String payload = "{\"value\": " + String(moisture) + "}";
    bool published = mqttClient.publish(telemetryTopic.c_str(), payload.c_str());

    if (published) {
        Serial.print("Published: ");
        Serial.println(payload);
    } else {
        Serial.println("Publish failed.");
    }
}
