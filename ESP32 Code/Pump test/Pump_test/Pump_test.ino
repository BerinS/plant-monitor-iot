const int RELAY_PIN = 26;
const int BUTTON_PIN = 18;

bool pumpOn = false;
bool lastButtonState = HIGH;

void setup() {
  Serial.begin(115200);
  
  pinMode(RELAY_PIN, OUTPUT);
  pinMode(BUTTON_PIN, INPUT_PULLUP); 
  
  digitalWrite(RELAY_PIN, HIGH); 
  
  Serial.println("Ready. Press button to toggle pump.");
}

void loop() {
  bool currentButtonState = digitalRead(BUTTON_PIN);

  // detect button press (HIGH to LOW transition)
  if (lastButtonState == HIGH && currentButtonState == LOW) {
    delay(50); // debounce
    
    pumpOn = !pumpOn;
    
    if (pumpOn) {
      digitalWrite(RELAY_PIN, LOW);  // LOW = relay ON
      Serial.println("Pump ON");
    } else {
      digitalWrite(RELAY_PIN, HIGH); // HIGH = relay OFF
      Serial.println("Pump OFF");
    }
  }

  lastButtonState = currentButtonState;
  delay(10);
}
