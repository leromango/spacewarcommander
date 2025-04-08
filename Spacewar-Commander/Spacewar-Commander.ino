#include <Servo.h>

// Analog sensor inputs
int knobPin = A0;                     // Potentiometer: resource distribution
int flashlightSensorPin = A1;         // Light sensor: flashlight control
int sliderXPin = A2;                  // Slider X input
int sliderYPin = A3;                  // Slider Y input

// Button inputs
int shootButtonPin = 12;             // Shoot
int moveButtonPin = 4;               // Move 
int changeDirectionButtonPin = 5;    // Change direction (for switch)
int reloadPin = 6;                   // Reload

// Switch
int switchPin = 2;

// Indicator lights
int leftLightPin = 7;
int backLightPin = 8;
int fwdLightPin = 9;
int rightLightPin = 10;

// Servo and buzzer
int servoControlPin = 11; //PWM
Servo reloadServo;
int buzzerPin = 3; //PWM

// State variables
bool isReloading = false;
unsigned long long reloadStartTime = 0;
unsigned long long reloadDuration = 500;

unsigned long long buzzerStartTime = 0;
unsigned long long buzzerDuration = 10;

unsigned long long indicatorDuration = 50;
unsigned long long indicatorStartTime = 500;

bool bShouldBuzz = false;
bool bIsIndicatorOn = false;

enum ELightIndication {
  FORWARD,
  LEFT,
  RIGHT,
  BACK,
  ALL,
  NONE,
};

ELightIndication currentIndicator;

void setup() {
  currentIndicator = ELightIndication::NONE;
  Serial.begin(256000);
  Serial.setTimeout(50);

  // Analog inputs
  pinMode(knobPin, INPUT);
  pinMode(flashlightSensorPin, INPUT);
  pinMode(sliderXPin, INPUT);
  pinMode(sliderYPin, INPUT);

  // Buttons and switch
  pinMode(shootButtonPin, INPUT_PULLUP);
  pinMode(moveButtonPin, INPUT_PULLUP);
  pinMode(changeDirectionButtonPin, INPUT_PULLUP);
  pinMode(reloadPin, INPUT_PULLUP);
  pinMode(switchPin, INPUT_PULLUP);

  // Output pins
  pinMode(buzzerPin, OUTPUT);
  pinMode(leftLightPin, OUTPUT);
  pinMode(backLightPin, OUTPUT);
  pinMode(fwdLightPin, OUTPUT);
  pinMode(rightLightPin, OUTPUT);

  reloadServo.attach(servoControlPin);
  reloadServo.write(50); // Default position
}

void loop() {
  unsigned long long currentMillis = millis();
  int reloadValue = 0;

  while (Serial.available() > 0) {
    String incomingString = Serial.readString();

    int buzzerIndex = incomingString.indexOf("b");
    if (buzzerIndex != -1) {
      bShouldBuzz = true;
      buzzerStartTime = currentMillis;
    }

    if (incomingString.indexOf("r") != -1) {
      reloadValue = 1;
      reloadStartTime = currentMillis;
    }

    int indexLightIndication = incomingString.indexOf("a");
    if (indexLightIndication != -1 && indicatorStartTime < currentMillis) {
      currentIndicator = static_cast<ELightIndication>(String(incomingString.charAt(indexLightIndication + 1)).toInt());
      indicatorStartTime = currentMillis;
      bIsIndicatorOn = true;
    }
  }

  // Handle indicator lights
  switch (currentIndicator) {
    case ELightIndication::FORWARD:
      digitalWrite(fwdLightPin, HIGH);
      break;
    case ELightIndication::RIGHT:
      digitalWrite(rightLightPin, HIGH);
      break;
    case ELightIndication::BACK:
      digitalWrite(backLightPin, HIGH);
      break;
    case ELightIndication::LEFT:
      digitalWrite(leftLightPin, HIGH);
      break;
    case ELightIndication::ALL:
      digitalWrite(leftLightPin, HIGH);
      digitalWrite(backLightPin, HIGH);
      digitalWrite(rightLightPin, HIGH);
      digitalWrite(fwdLightPin, HIGH);
      break;
    default:
      break;
  }

  if (!bShouldBuzz) {
    noTone(buzzerPin);
  }

  if (bShouldBuzz && currentMillis - buzzerStartTime >= buzzerDuration) {
    buzzerStartTime = 0;
    bShouldBuzz = false;
    noTone(buzzerPin);
  }

  if (bIsIndicatorOn && currentMillis - indicatorStartTime >= indicatorDuration) {
    indicatorStartTime = 0;
    bIsIndicatorOn = false;
    currentIndicator = ELightIndication::NONE;
    digitalWrite(fwdLightPin, LOW);
    digitalWrite(rightLightPin, LOW);
    digitalWrite(backLightPin, LOW);
    digitalWrite(leftLightPin, LOW);
  }

  // Read analog and digital input values
  int knobValue = analogRead(knobPin);
  int flashlightValue = analogRead(flashlightSensorPin);
  int sliderXValue = analogRead(sliderXPin);
  int sliderYValue = analogRead(sliderYPin);

  int shootState = digitalRead(shootButtonPin);
  int moveState = digitalRead(moveButtonPin);
  int changeDirectionState = digitalRead(changeDirectionButtonPin);
  int switchState = digitalRead(switchPin);

  // Handle reload servo
  if (reloadValue == 1 && !isReloading) {
    isReloading = true;
    reloadStartTime = millis();
    reloadServo.write(-100);
  }

  if (isReloading && (currentMillis - reloadStartTime) >= reloadDuration) {
    reloadServo.write(50);
    isReloading = false;
    reloadStartTime = 0;
  }

  // Buzz if needed
  if (bShouldBuzz) {
    tone(buzzerPin, 100);
  }

  // Send sensor values over serial
  Serial.println(String(knobValue) + "," + 
                 String(shootState) + "," + 
                 String(moveState) + "," + 
                 String(switchState) + "," + 
                 String(changeDirectionState) + "," + 
                 String(flashlightValue) + "," + 
                 String(reloadValue) + "," + 
                 String(sliderXValue) + "," + 
                 String(sliderYValue));

  delay(100);
}