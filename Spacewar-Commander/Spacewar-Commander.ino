#include <Servo.h>

// COMMENTS FOR PIN DEFINITIONS ARE AI BEWARE
// --- Pin Definitions ---
// Analog sensor inputs
const int knobPin = A0;             // Potentiometer: resource distribution
const int flashlightSensorPin = A1; // Light sensor: flashlight control
const int sliderXPin = A2;          // Slider X input
const int sliderYPin = A3;          // Slider Y input

// Button inputs (Using INPUT_PULLUP, so LOW means pressed)
const int shootButtonPin = 12;      // Shoot
const int moveButtonPin = 4;        // Move (Used for Reload in Godot)
const int reloadPin = 5;            // Reload Button (Used for Rotation update in Godot)

// Indicator lights
const int leftLightPin = 7;
const int backLightPin = 8;
const int fwdLightPin = 9;
const int rightLightPin = 10;

// Servo and buzzer
const int servoControlPin = 11; // PWM pin for Servo
Servo reloadServo;
const int buzzerPin = 3;       // PWM pin for Buzzer

// --- State Variables ---
// Servo Reload Sequence
bool isReloading = false;         // Is the servo currently in the reload motion?
unsigned long reloadStartTime = 0; // When did the reload motion start?
const unsigned long reloadDuration = 150; // Duration servo stays in reload position (ms)
const int servoReloadPosition = 25; // Angle for "up" or reload position
const int servoDefaultPosition = 100;// Angle for "down" or default position

bool bShouldBuzz = false;         
unsigned long buzzerStartTime = 0;
const unsigned long buzzerOnDuration = 150;

bool bIsIndicatorOn = false;            
unsigned long indicatorStartTime = 0; 
const unsigned long indicatorOnDuration = 300;

enum ELightIndication {
  FORWARD = 0, 
  LEFT = 1,
  RIGHT = 2,
  BACK = 3,
  ALL = 4,
  NONE 
};

ELightIndication currentIndicator = ELightIndication::NONE;

// --- Setup Function ---
void setup() {
  currentIndicator = ELightIndication::NONE;
  Serial.begin(9600);
  Serial.setTimeout(10);

  // Initialize Inputs
  pinMode(knobPin, INPUT);
  pinMode(flashlightSensorPin, INPUT);
  pinMode(sliderXPin, INPUT);
  pinMode(sliderYPin, INPUT);

  pinMode(shootButtonPin, INPUT_PULLUP);
  pinMode(moveButtonPin, INPUT_PULLUP);
  pinMode(reloadPin, INPUT_PULLUP);

  // Initialize Outputs
  pinMode(buzzerPin, OUTPUT);
  pinMode(leftLightPin, OUTPUT);
  pinMode(backLightPin, OUTPUT);
  pinMode(fwdLightPin, OUTPUT);
  pinMode(rightLightPin, OUTPUT);
  digitalWrite(leftLightPin, LOW); 
  digitalWrite(backLightPin, LOW);
  digitalWrite(fwdLightPin, LOW);
  digitalWrite(rightLightPin, LOW);
  noTone(buzzerPin); 

  // Initialize Servo
  reloadServo.attach(servoControlPin);
  reloadServo.write(servoDefaultPosition); // Start servo in default position
  delay(50); // Small delay to allow servo to reach position
}

// --- Main Loop ---
void loop() {
  unsigned long currentMillis = millis(); // Get current time once per loop

  // --- Process Incoming Serial Commands from Godot ---
  if (Serial.available() > 0) {
    String incomingString = Serial.readString(); 

    // Buzzer trigger ('b')
    if (incomingString.indexOf('b') != -1) {
      bShouldBuzz = true;
      buzzerStartTime = currentMillis; 
      tone(buzzerPin, 100); 
    }

    // Reload command ('r') - Triggers servo sequence
    if (incomingString.indexOf('r') != -1) {
      if (!isReloading) { // Only start if not already reloading
          isReloading = true;
          reloadServo.write(servoReloadPosition); // Move servo to reload (up) position
          reloadStartTime = currentMillis; 
      }
    }

    int indexLightStart = incomingString.indexOf('a');
    int indexLightEnd = incomingString.lastIndexOf('a'); // Find the second 'a'
    if (indexLightStart != -1 && indexLightEnd > indexLightStart) {
       String numStr = incomingString.substring(indexLightStart + 1, indexLightEnd);
       if (numStr.length() > 0) { 
           int indicatorValue = numStr.toInt(); 
           currentIndicator = static_cast<ELightIndication>(indicatorValue);
           indicatorStartTime = currentMillis;
           bIsIndicatorOn = true; 
           digitalWrite(fwdLightPin, LOW);
           digitalWrite(rightLightPin, LOW);
           digitalWrite(backLightPin, LOW);
           digitalWrite(leftLightPin, LOW);
       }
    }
  } 

  // --- Update Servo State ---
  // If servo is in reload position and duration has passed, return to default
  if (isReloading && (currentMillis - reloadStartTime >= reloadDuration)) {
    reloadServo.write(servoDefaultPosition); // Return servo to default (down) position
    isReloading = false; // End the reloading state
  }

  // --- Update Buzzer State ---
  if (bShouldBuzz && (currentMillis - buzzerStartTime >= buzzerOnDuration)) {
    bShouldBuzz = false;
    noTone(buzzerPin); // Stop the tone
  }

  // --- Update Indicator Light State ---
  if (bIsIndicatorOn) {
    switch (currentIndicator) {
      case ELightIndication::FORWARD: digitalWrite(fwdLightPin, HIGH); break;
      case ELightIndication::RIGHT:   digitalWrite(rightLightPin, HIGH); break;
      case ELightIndication::BACK:    digitalWrite(backLightPin, HIGH); break;
      case ELightIndication::LEFT:    digitalWrite(leftLightPin, HIGH); break;
      case ELightIndication::ALL:
        digitalWrite(leftLightPin, HIGH);
        digitalWrite(backLightPin, HIGH);
        digitalWrite(rightLightPin, HIGH);
        digitalWrite(fwdLightPin, HIGH);
        break;
      default:
        break; 
    }

    if (currentMillis - indicatorStartTime >= indicatorOnDuration) {
      bIsIndicatorOn = false;
      currentIndicator = ELightIndication::NONE;
      digitalWrite(fwdLightPin, LOW);
      digitalWrite(rightLightPin, LOW);
      digitalWrite(backLightPin, LOW);
      digitalWrite(leftLightPin, LOW);
    }
  }

  // --- Read Input Values ---
  int knobValue = analogRead(knobPin);
  int flashlightValue = analogRead(flashlightSensorPin);
  int sliderXValue = analogRead(sliderXPin);
  int sliderYValue = analogRead(sliderYPin);

  // Read buttons (LOW means pressed due to INPUT_PULLUP)
  // Send 0 for pressed, 1 for not pressed to match Godot crap
  int shootState = digitalRead(shootButtonPin) == LOW ? 0 : 1;
  int moveState = digitalRead(moveButtonPin) == LOW ? 0 : 1;    // Corresponds to Reload in Godot
  int reloadState = digitalRead(reloadPin) == LOW ? 0 : 1;      // Corresponds to Rotation Update trigger in Godot

  // --- Send Sensor Values Over Serial ---
  Serial.print(knobValue); Serial.print(",");
  Serial.print(shootState); Serial.print(",");
  Serial.print(moveState); Serial.print(","); // Was move button, now used for reload
  Serial.print(flashlightValue); Serial.print(",");
  Serial.print(reloadState); Serial.print(","); // Was reload button, now used for rotation update
  Serial.print(sliderXValue); Serial.print(",");
  Serial.println(sliderYValue); 

  Serial.flush();

  delay(100);
}