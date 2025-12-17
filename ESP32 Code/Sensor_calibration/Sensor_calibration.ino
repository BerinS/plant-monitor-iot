/*
 *  -- Sensor Calibration -- 
 * Dry value: Record the reading while holding the sensor in dry air.
 * Wet value:  Record the reading while submerging the sensor in water.
 * The sensor is only powered on for a brief period of time (via GPIO) during reads to prevent corosion of the probes.
*/

const int SENSOR_PIN = 36;  // AO
const int POWER_PIN = 17;   // VCC 

const int READ_DELAY = 1000; 

void setup() {
  Serial.begin(115200);
  
  pinMode(POWER_PIN, OUTPUT);
  
  // OFF by default to stop corrosion
  digitalWrite(POWER_PIN, LOW);
}

int readSoilSensor() {
  digitalWrite(POWER_PIN, HIGH);
  
  // 10ms wait for power to stabilize
  delay(10); 
  
  int val = analogRead(SENSOR_PIN);
  
  digitalWrite(POWER_PIN, LOW);
  
  return val;
}

void loop() {
  int rawValue = readSoilSensor();

  Serial.print("Raw Value: ");
  Serial.println(rawValue);

  delay(READ_DELAY);
}
